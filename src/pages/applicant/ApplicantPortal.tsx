import { useState, useRef, useCallback, useEffect, useMemo } from 'react';
import { useParams } from 'react-router-dom';
import { resolveTenant, DEFAULT_DOC_REQUIREMENTS } from '@/lib/tenants';
import type { TenantDocRequirement } from '@/lib/tenants';
import ScanToFill, { AiConfidenceBadge } from '@/components/common/ScanToFill';

// ═══════════════════════════════════════════════════════
// DFRNT Courier Portal — Dynamic Page-Per-Document Architecture
// ═══════════════════════════════════════════════════════

// ── Country Detection ──────────────────────────────────

type Country = 'NZ' | 'US';

function detectCountry(): Country {
  try {
    const tz = Intl.DateTimeFormat().resolvedOptions().timeZone || '';
    if (tz.startsWith('Pacific/Auckland') || tz.startsWith('NZ')) return 'NZ';
    const locale = navigator.language || '';
    if (locale.includes('NZ') || locale === 'en-NZ') return 'NZ';
  } catch { /* fall through */ }
  return 'US';
}

function detectCountryFromPhone(phone: string): Country | null {
  const cleaned = phone.replace(/\s/g, '');
  if (cleaned.startsWith('+64') || cleaned.startsWith('0064')) return 'NZ';
  if (cleaned.startsWith('+1') || cleaned.startsWith('001')) return 'US';
  return null;
}

// ── Role Profiles ──────────────────────────────────────

interface RoleProfile {
  id: string;
  name: string;
  description: string;
  requiresDG: boolean;
  vehicleTypes: string[];
  icon: string;
}

const ROLE_PROFILES: Record<string, RoleProfile> = {
  'general': { id: 'general', name: 'Courier Driver', description: 'General delivery and courier services', requiresDG: false, vehicleTypes: ['Car', 'Van', 'Truck'], icon: '🚚' },
  'medical': { id: 'medical', name: 'Medical Courier', description: 'Healthcare and pharmaceutical delivery', requiresDG: true, vehicleTypes: ['Car', 'Van'], icon: '🏥' },
  'dg': { id: 'dg', name: 'Dangerous Goods Driver', description: 'Hazardous materials transport', requiresDG: true, vehicleTypes: ['Van', 'Truck'], icon: '⚠️' },
  'freight': { id: 'freight', name: 'Freight Driver', description: 'Large item and pallet delivery', requiresDG: false, vehicleTypes: ['Truck', 'Trailer'], icon: '📦' },
  'express': { id: 'express', name: 'Express Courier', description: 'Same-day and urgent deliveries', requiresDG: false, vehicleTypes: ['Car', 'Motorcycle', 'Van'], icon: '⚡' },
};

// ── Document Types ─────────────────────────────────────

interface PortalDocType {
  id: number;
  name: string;
  mandatory: boolean;
  instructions: string;
  countriesOnly?: Country[];
  rolesOnly?: string[];
  extractableFields?: { key: string; label: string; type: 'text' | 'date' | 'select'; required?: boolean }[];
}

function buildDocTypes(tenantDocs?: TenantDocRequirement[]): PortalDocType[] {
  const docs = tenantDocs || DEFAULT_DOC_REQUIREMENTS;
  return docs.map(d => ({ ...d }));
}

// ── Vehicle Photo Slots ────────────────────────────────

const VEHICLE_PHOTO_SLOTS = [
  { key: 'front', label: 'Front' },
  { key: 'side', label: 'Side' },
  { key: 'rear', label: 'Rear' },
  { key: 'cargo', label: 'Cargo Area' },
] as const;

// ── Local Storage ──────────────────────────────────────

const STORAGE_KEY = 'dfrnt_portal_application';

interface UploadedDoc {
  docTypeId: number;
  fileName: string;
  status: 'uploaded' | 'processing' | 'verified';
}

interface SavedApplication {
  step: number;
  form: Record<string, string>;
  uploadedDocs: UploadedDoc[];
  country: Country;
  tenantSlug: string;
  savedAt: string;
}

function saveProgress(data: SavedApplication) {
  try { localStorage.setItem(STORAGE_KEY, JSON.stringify(data)); } catch { /* quota */ }
}

function loadProgress(tenantSlug: string): SavedApplication | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const saved = JSON.parse(raw) as SavedApplication;
    if (saved.tenantSlug !== tenantSlug) return null;
    if (Date.now() - new Date(saved.savedAt).getTime() > 7 * 24 * 60 * 60 * 1000) return null;
    return saved;
  } catch { return null; }
}

function clearProgress() {
  try { localStorage.removeItem(STORAGE_KEY); } catch { /* */ }
}

// ═══════════════════════════════════════════════════════
// MAIN COMPONENT
// ═══════════════════════════════════════════════════════

export default function ApplicantPortal() {
  const { tenantSlug, roleId } = useParams<{ tenantSlug: string; roleId?: string }>();
  const tenant = useMemo(() => resolveTenant(tenantSlug), [tenantSlug]);
  const roleProfile = useMemo(() => ROLE_PROFILES[roleId || 'general'] || ROLE_PROFILES['general'], [roleId]);
  const [country, setCountry] = useState<Country>(tenant.country || detectCountry());
  const [step, setStep] = useState(-1); // -1 = landing
  const [submitted, setSubmitted] = useState(false);
  const [resumePrompt, setResumePrompt] = useState(false);

  // ── Form State ─────────────────────────────────────
  const [form, setForm] = useState<Record<string, string>>({
    email: '', firstName: '', lastName: '', mobile: '', dateOfBirth: '',
    address: '', city: '', state: '', postcode: '',
    vehicleType: '', vehicleMake: '', vehicleModel: '', vehicleYear: '', vehiclePlate: '',
  });

  const [uploadedDocs, setUploadedDocs] = useState<UploadedDoc[]>([]);
  const [aiFields, setAiFields] = useState<Record<string, number>>({});
  // Doc extracted fields: docId -> { fieldKey -> value }
  const [docExtractedFields, setDocExtractedFields] = useState<Record<number, Record<string, string>>>({});
  const [docExtractedConfidence, setDocExtractedConfidence] = useState<Record<number, Record<string, number>>>({});
  const [vehiclePhotos, setVehiclePhotos] = useState<Record<string, { file: File; url: string }>>({});

  const [signed, setSigned] = useState(false);
  const [declarationName, setDeclarationName] = useState('');
  const [agreedToTerms, setAgreedToTerms] = useState(false);
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});

  const canvasRef = useRef<HTMLCanvasElement>(null);
  const drawingRef = useRef(false);

  // ── Available Doc Types for Country/Role ───────────
  const allDocTypes = useMemo(() => buildDocTypes(tenant.docRequirements), [tenant]);
  const docTypes = useMemo(() => allDocTypes.filter(d =>
    (!d.countriesOnly || d.countriesOnly.includes(country)) &&
    (!d.rolesOnly || d.rolesOnly.includes(roleProfile.id))
  ), [allDocTypes, country, roleProfile.id]);

  // ── Dynamic Steps ──────────────────────────────────
  // Step -1: Landing
  // Step 0: Your Details (fixed)
  // Step 1: Your Vehicle (fixed)
  // Step 2..2+N-1: Dynamic doc pages
  // Step 2+N: Review & Submit
  const totalSteps = 2 + docTypes.length + 1; // Details + Vehicle + N docs + Review
  const reviewStep = 2 + docTypes.length;

  const stepLabels = useMemo(() => {
    const labels = ['Details', 'Vehicle'];
    for (const dt of docTypes) labels.push(dt.name);
    labels.push('Review');
    return labels;
  }, [docTypes]);

  // Get doc type for a dynamic step (step >= 2 and step < reviewStep)
  const getDocForStep = (s: number): PortalDocType | null => {
    if (s < 2 || s >= reviewStep) return null;
    return docTypes[s - 2] || null;
  };

  // ── Resume Detection ───────────────────────────────
  useEffect(() => {
    const saved = loadProgress(tenant.slug);
    if (saved && saved.step > 0) setResumePrompt(true);
  }, [tenant.slug]);

  const handleResume = () => {
    const saved = loadProgress(tenant.slug);
    if (saved) {
      setForm(prev => ({ ...prev, ...saved.form }));
      setUploadedDocs(saved.uploadedDocs);
      setStep(saved.step);
      setCountry(saved.country);
    }
    setResumePrompt(false);
  };

  // ── Auto-save ──────────────────────────────────────
  useEffect(() => {
    if (step > 0 && !submitted) {
      saveProgress({ step, form, uploadedDocs, country, tenantSlug: tenant.slug, savedAt: new Date().toISOString() });
    }
  }, [step, form, uploadedDocs, country, tenant.slug, submitted]);

  // ── Country Detection from Phone ───────────────────
  useEffect(() => {
    const detected = detectCountryFromPhone(form.mobile);
    if (detected) setCountry(detected);
  }, [form.mobile]);

  // ── Form Helpers ───────────────────────────────────
  const u = (field: string, value: string) => {
    setForm(f => ({ ...f, [field]: value }));
    if (validationErrors[field]) setValidationErrors(v => { const n = { ...v }; delete n[field]; return n; });
  };

  const inputClass = "w-full px-3 py-3 text-[15px] border border-gray-200 rounded-xl bg-white focus:outline-none focus:ring-2 focus:ring-brand-cyan/40 focus:border-brand-cyan transition-all text-brand-dark placeholder:text-gray-400";
  const labelClass = "text-xs font-semibold text-brand-dark/60 uppercase tracking-wider mb-1.5 block";
  const errorClass = "text-xs text-red-500 mt-1";
  const sectionClass = "bg-white rounded-2xl border border-gray-200 p-5 sm:p-6 space-y-4 shadow-sm ring-1 ring-brand-dark/5";

  // ── Signature Pad ──────────────────────────────────
  const startDraw = useCallback((e: React.MouseEvent<HTMLCanvasElement> | React.TouchEvent<HTMLCanvasElement>) => {
    drawingRef.current = true;
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    const rect = canvas.getBoundingClientRect();
    const x = 'touches' in e ? e.touches[0].clientX - rect.left : e.clientX - rect.left;
    const y = 'touches' in e ? e.touches[0].clientY - rect.top : e.clientY - rect.top;
    ctx.beginPath();
    ctx.moveTo(x * (canvas.width / rect.width), y * (canvas.height / rect.height));
  }, []);

  const draw = useCallback((e: React.MouseEvent<HTMLCanvasElement> | React.TouchEvent<HTMLCanvasElement>) => {
    if (!drawingRef.current) return;
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    const rect = canvas.getBoundingClientRect();
    const x = 'touches' in e ? e.touches[0].clientX - rect.left : e.clientX - rect.left;
    const y = 'touches' in e ? e.touches[0].clientY - rect.top : e.clientY - rect.top;
    ctx.lineTo(x * (canvas.width / rect.width), y * (canvas.height / rect.height));
    ctx.strokeStyle = '#0d0c2c';
    ctx.lineWidth = 2.5;
    ctx.lineCap = 'round';
    ctx.stroke();
  }, []);

  const stopDraw = useCallback(() => { drawingRef.current = false; setSigned(true); }, []);

  const clearSignature = () => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    setSigned(false);
  };

  // ── Validation ─────────────────────────────────────
  const validateStep = (s: number): boolean => {
    const errors: Record<string, string> = {};
    if (s === 0) {
      if (!form.firstName.trim()) errors.firstName = 'Required';
      if (!form.lastName.trim()) errors.lastName = 'Required';
      if (!form.email.trim()) errors.email = 'Required';
      else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) errors.email = 'Invalid email';
      if (!form.mobile.trim()) errors.mobile = 'Mobile number required';
    }
    if (s === 1) {
      if (!form.vehicleType) errors.vehicleType = 'Required';
      if (!form.vehiclePlate.trim()) errors.vehiclePlate = 'Required';
    }
    // Doc steps: no hard validation (skip allowed for non-mandatory)
    if (s === reviewStep) {
      if (!agreedToTerms) errors.agree = 'You must agree to the declaration';
      if (!declarationName.trim()) errors.declarationName = 'Full name required';
      if (!signed) errors.signature = 'Signature required';
    }
    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleNext = () => {
    if (!validateStep(step)) return;
    if (step < reviewStep) setStep(s => s + 1);
  };

  const handleBack = () => {
    if (step > 0) setStep(s => s - 1);
  };

  // ── Vehicle Photo Upload ───────────────────────────
  const handleVehiclePhoto = (slotKey: string, file: File) => {
    const url = URL.createObjectURL(file);
    setVehiclePhotos(prev => {
      if (prev[slotKey]?.url) URL.revokeObjectURL(prev[slotKey].url);
      return { ...prev, [slotKey]: { file, url } };
    });
  };

  const removeVehiclePhoto = (slotKey: string) => {
    setVehiclePhotos(prev => {
      if (prev[slotKey]?.url) URL.revokeObjectURL(prev[slotKey].url);
      const next = { ...prev };
      delete next[slotKey];
      return next;
    });
  };

  // ── Submit ─────────────────────────────────────────
  const handleSubmit = () => {
    if (!validateStep(reviewStep)) return;
    console.log('[Portal] Application submitted:', {
      tenant: tenant.slug, email: form.email,
      name: `${form.firstName} ${form.lastName}`,
      phone: form.mobile, vehicleType: form.vehicleType,
      documentsUploaded: uploadedDocs.length,
      vehiclePhotos: Object.keys(vehiclePhotos).length,
    });
    clearProgress();
    setSubmitted(true);
    setStep(reviewStep + 1);
  };

  const countryFlag = country === 'NZ' ? '🇳🇿' : '🇺🇸';

  // ═════════════════════════════════════════════════════
  // RENDER
  // ═════════════════════════════════════════════════════

  return (
    <div className="min-h-screen bg-brand-light" style={{ fontFamily: "'Inter', -apple-system, sans-serif" }}>
      {/* ── Resume Prompt ────────────────────────────── */}
      {resumePrompt && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-xl max-w-sm w-full p-6 space-y-4">
            <h3 className="text-lg font-bold text-brand-dark">Resume Application?</h3>
            <p className="text-sm text-gray-600">We found a saved application. Would you like to continue where you left off?</p>
            <div className="flex gap-3">
              <button onClick={handleResume} className="flex-1 bg-brand-cyan text-white py-2.5 rounded-xl font-medium text-sm hover:bg-brand-cyan/90">Resume</button>
              <button onClick={() => { clearProgress(); setResumePrompt(false); }} className="flex-1 border border-gray-300 text-gray-600 py-2.5 rounded-xl font-medium text-sm hover:bg-gray-50">Start Fresh</button>
            </div>
          </div>
        </div>
      )}

      {/* ── Header ────────────────────────────────────── */}
      <header className="text-white safe-area-top" style={{ backgroundColor: tenant.primaryColor }}>
        <div className="max-w-3xl mx-auto px-4 py-4 sm:py-5">
          <div className="flex items-center gap-3">
            {tenant.logoUrl ? (
              <img src={tenant.logoUrl} alt={tenant.name} className="h-10 sm:h-12 w-auto max-w-[180px] object-contain" />
            ) : (
              <div className="w-11 h-11 rounded-xl flex items-center justify-center text-lg font-bold text-white" style={{ backgroundColor: tenant.accentColor }}>
                {tenant.name.charAt(0)}
              </div>
            )}
            <div className="flex-1">
              <div className="flex items-center gap-2 flex-wrap">
                {!tenant.logoUrl && <h1 className="text-lg sm:text-xl font-bold tracking-tight">{tenant.name}</h1>}
                {roleProfile.id !== 'general' && (
                  <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium" style={{ backgroundColor: tenant.accentColor + '33', color: 'white' }}>
                    {roleProfile.icon} {roleProfile.name}
                  </span>
                )}
              </div>
              <p className="text-xs text-white/50 mt-0.5">{tenant.tagline} {countryFlag}</p>
            </div>
          </div>
          <div className="h-0.5 mt-4 rounded-full" style={{ background: `linear-gradient(to right, ${tenant.accentColor}, ${tenant.accentColor}80, transparent)` }} />
        </div>
      </header>

      {/* ── Landing Hero (step -1) ────────────────────── */}
      {step === -1 && (
        <div className="max-w-3xl mx-auto px-4 py-8 sm:py-12">
          <div className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden">
            <div className="p-6 sm:p-10 text-center sm:text-left" style={{ backgroundColor: tenant.primaryColor }}>
              {tenant.logoUrl && (
                <img src={tenant.logoUrl} alt={tenant.name} className="h-12 sm:h-16 w-auto max-w-[250px] object-contain mb-4 mx-auto sm:mx-0" />
              )}
              <h2 className="text-2xl sm:text-3xl font-bold text-white mb-2">
                {roleProfile.id === 'general'
                  ? (tenant.logoUrl ? 'Join Our Team' : `Join ${tenant.name}`)
                  : `Become a ${roleProfile.name}`}
              </h2>
              <p className="text-white/70 sm:text-lg">
                {country === 'NZ'
                  ? `We're looking for reliable courier drivers across New Zealand.`
                  : `We're looking for reliable courier drivers across the United States.`}
              </p>
            </div>

            <div className="p-6 sm:p-10 space-y-6">
              <h3 className="text-lg font-bold text-brand-dark">What you'll need to apply</h3>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {docTypes.map(doc => {
                  const icon = doc.name.includes('License') ? '🪪'
                    : doc.name.includes('Registration') || doc.name.includes('WOF') ? '🚗'
                    : doc.name.includes('Insurance') || doc.name.includes('Liability') ? '🛡️'
                    : doc.name.includes('Dangerous') || doc.name.includes('DG') ? '⚠️'
                    : doc.name.includes('DOT') || doc.name.includes('TSL') ? '🏛️'
                    : doc.name.includes('HIPAA') ? '🏥'
                    : '📋';
                  return (
                    <div key={doc.id} className="flex items-start gap-3 p-3 bg-brand-light rounded-xl">
                      <span className="text-xl">{icon}</span>
                      <div>
                        <div className="font-medium text-brand-dark text-sm">
                          {doc.name}
                          {doc.mandatory && <span className="text-red-400 ml-1 text-xs">Required</span>}
                        </div>
                        <div className="text-xs text-gray-500">{doc.instructions}</div>
                      </div>
                    </div>
                  );
                })}
                {country === 'NZ' && (
                  <div className="flex items-start gap-3 p-3 bg-brand-light rounded-xl">
                    <span className="text-xl">🏛️</span>
                    <div>
                      <div className="font-medium text-brand-dark text-sm">IRD Number</div>
                      <div className="text-xs text-gray-500">For tax and payment setup</div>
                    </div>
                  </div>
                )}
                {country === 'US' && (
                  <div className="flex items-start gap-3 p-3 bg-brand-light rounded-xl">
                    <span className="text-xl">🏛️</span>
                    <div>
                      <div className="font-medium text-brand-dark text-sm">SSN or EIN</div>
                      <div className="text-xs text-gray-500">For tax withholding and 1099 reporting</div>
                    </div>
                  </div>
                )}
              </div>

              <div className="flex items-center gap-3 p-4 bg-green-50 rounded-xl border border-green-100">
                <span className="text-2xl">📱</span>
                <div>
                  <div className="font-medium text-green-800 text-sm">Quick & Easy — About 5 minutes</div>
                  <div className="text-xs text-green-600">Take photos of your documents with your phone camera. Our AI reads them automatically.</div>
                </div>
              </div>

              <button
                onClick={() => setStep(0)}
                className="w-full sm:w-auto px-8 py-3.5 rounded-xl text-white font-semibold text-base shadow-sm hover:opacity-90 transition-all"
                style={{ backgroundColor: tenant.accentColor }}
              >
                Start Application →
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── Progress Bar ─────────────────────────────── */}
      {step >= 0 && step <= reviewStep && !submitted && (
        <div className="bg-white border-b border-gray-200 sticky top-0 z-30">
          <div className="max-w-3xl mx-auto px-4 py-3">
            {/* Mobile: simple progress bar */}
            <div className="sm:hidden">
              <div className="flex items-center justify-between mb-2">
                <span className="text-xs font-semibold text-brand-dark">{stepLabels[step]}</span>
                <span className="text-xs text-gray-400">Step {step + 1} of {totalSteps}</span>
              </div>
              <div className="h-1.5 bg-gray-200 rounded-full overflow-hidden">
                <div className="h-full bg-brand-cyan rounded-full transition-all duration-500" style={{ width: `${((step + 1) / totalSteps) * 100}%` }} />
              </div>
            </div>

            {/* Desktop: step indicators */}
            <div className="hidden sm:flex items-center gap-1">
              {stepLabels.map((label, i) => (
                <div key={i} className="flex-1 flex items-center gap-1.5 min-w-0">
                  <button
                    onClick={() => i < step && setStep(i)}
                    disabled={i > step}
                    className={`w-7 h-7 rounded-full flex items-center justify-center text-[10px] font-bold flex-shrink-0 transition-all ${
                      i < step ? 'bg-brand-cyan text-white cursor-pointer hover:bg-brand-cyan/80'
                        : i === step ? 'bg-brand-dark text-white'
                        : 'bg-gray-200 text-gray-400'
                    }`}
                  >
                    {i < step ? '✓' : i + 1}
                  </button>
                  <span className={`text-[10px] whitespace-nowrap truncate ${i === step ? 'text-brand-dark font-semibold' : 'text-gray-400'}`}>
                    {label}
                  </span>
                  {i < stepLabels.length - 1 && (
                    <div className={`flex-1 h-0.5 rounded ${i < step ? 'bg-brand-cyan' : 'bg-gray-200'}`} />
                  )}
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* ── Content ──────────────────────────────────── */}
      <div className="max-w-3xl mx-auto px-4 py-6 sm:py-8 pb-32">

        {/* ═══ STEP 0: Your Details ═══ */}
        {step === 0 && (
          <div className="space-y-5">
            <div className={sectionClass}>
              <h2 className="text-lg font-bold text-brand-dark">Your Details</h2>
              <p className="text-sm text-gray-500">Scan your driver's license to auto-fill, or enter details manually.</p>

              <ScanToFill
                label="📸 Scan Driver's License (front & back)"
                docTypeName="Driver's License"
                accent={tenant.accentColor}
                onExtracted={(fields) => {
                  const mapping: Record<string, string> = {
                    firstName: 'firstName', lastName: 'lastName', dob: 'dateOfBirth',
                    address: 'address', licenseNumber: 'driversLicenceNo', licenseExpiry: 'dlExpiry',
                  };
                  const newAi: Record<string, number> = {};
                  for (const [key, { value, confidence }] of Object.entries(fields)) {
                    const formKey = mapping[key] || key;
                    u(formKey, value);
                    newAi[formKey] = confidence;
                  }
                  setAiFields(prev => ({ ...prev, ...newAi }));
                }}
              />

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <div className="flex items-center gap-1.5 mb-1">
                    <label className={labelClass + ' mb-0'}>First Name *</label>
                    {aiFields.firstName && <AiConfidenceBadge confidence={aiFields.firstName} />}
                  </div>
                  <input className={inputClass} value={form.firstName} onChange={e => u('firstName', e.target.value)} placeholder="John" />
                  {validationErrors.firstName && <p className={errorClass}>{validationErrors.firstName}</p>}
                </div>
                <div>
                  <div className="flex items-center gap-1.5 mb-1">
                    <label className={labelClass + ' mb-0'}>Last Name *</label>
                    {aiFields.lastName && <AiConfidenceBadge confidence={aiFields.lastName} />}
                  </div>
                  <input className={inputClass} value={form.lastName} onChange={e => u('lastName', e.target.value)} placeholder="Smith" />
                  {validationErrors.lastName && <p className={errorClass}>{validationErrors.lastName}</p>}
                </div>
              </div>

              <div>
                <label className={labelClass}>Email *</label>
                <input type="email" className={inputClass} value={form.email} onChange={e => u('email', e.target.value)} placeholder="john@example.com" />
                {validationErrors.email && <p className={errorClass}>{validationErrors.email}</p>}
              </div>

              <div>
                <label className={labelClass}>Mobile Number *</label>
                <input type="tel" className={inputClass} value={form.mobile} onChange={e => {
                  let v = e.target.value.replace(/[^\d+\s()-]/g, '');
                  if (country === 'NZ') {
                    const digits = v.replace(/\D/g, '');
                    if (digits.startsWith('64') && digits.length > 2) {
                      v = '+64 ' + digits.slice(2, 4) + (digits.length > 4 ? ' ' + digits.slice(4, 7) : '') + (digits.length > 7 ? ' ' + digits.slice(7, 11) : '');
                    } else if (digits.startsWith('0') && digits.length > 1) {
                      v = digits.slice(0, 3) + (digits.length > 3 ? ' ' + digits.slice(3, 6) : '') + (digits.length > 6 ? ' ' + digits.slice(6, 10) : '');
                    }
                  }
                  if (country === 'US') {
                    const digits = v.replace(/\D/g, '');
                    const d = digits.startsWith('1') ? digits.slice(1) : digits;
                    if (d.length > 0) {
                      v = '+1 (' + d.slice(0, 3) + (d.length > 3 ? ') ' + d.slice(3, 6) : '') + (d.length > 6 ? '-' + d.slice(6, 10) : '');
                    }
                  }
                  u('mobile', v);
                }} placeholder={country === 'NZ' ? '+64 21 123 4567' : '+1 (555) 123-4567'} />
                {validationErrors.mobile && <p className={errorClass}>{validationErrors.mobile}</p>}
              </div>

              <div>
                <div className="flex items-center gap-1.5 mb-1.5">
                  <label className={labelClass + ' mb-0'}>Date of Birth</label>
                  {aiFields.dateOfBirth && <AiConfidenceBadge confidence={aiFields.dateOfBirth} />}
                </div>
                <input type="date" className={inputClass} value={form.dateOfBirth} onChange={e => u('dateOfBirth', e.target.value)} />
              </div>

              <div>
                <div className="flex items-center gap-1.5 mb-1.5">
                  <label className={labelClass + ' mb-0'}>Address</label>
                  {aiFields.address && <AiConfidenceBadge confidence={aiFields.address} />}
                </div>
                <input className={inputClass} value={form.address} onChange={e => u('address', e.target.value)} placeholder="Street address" />
              </div>

              <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                <div>
                  <label className={labelClass}>City</label>
                  <input className={inputClass} value={form.city} onChange={e => u('city', e.target.value)} />
                </div>
                <div>
                  <label className={labelClass}>{country === 'NZ' ? 'Region' : 'State'}</label>
                  <input className={inputClass} value={form.state} onChange={e => u('state', e.target.value)} />
                </div>
                <div>
                  <label className={labelClass}>{country === 'NZ' ? 'Postcode' : 'ZIP Code'}</label>
                  <input className={inputClass} value={form.postcode} onChange={e => u('postcode', e.target.value)} />
                </div>
              </div>

              {/* Country indicator */}
              <div className="flex items-center gap-2 px-3 py-2 bg-gray-50 rounded-xl text-xs text-gray-500">
                <span className="text-base">{countryFlag}</span>
                <span>Detected region: <strong className="text-gray-700">{country === 'NZ' ? 'New Zealand' : 'United States'}</strong></span>
                <button onClick={() => setCountry(c => c === 'NZ' ? 'US' : 'NZ')} className="ml-auto text-brand-cyan font-medium hover:underline">
                  Switch to {country === 'NZ' ? 'US' : 'NZ'}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* ═══ STEP 1: Your Vehicle ═══ */}
        {step === 1 && (
          <div className="space-y-5">
            <div className={sectionClass}>
              <h2 className="text-lg font-bold text-brand-dark">Your Vehicle</h2>
              <p className="text-sm text-gray-500">Details of the vehicle you'll use for deliveries.</p>

              <ScanToFill
                label="Scan Vehicle Registration to auto-fill"
                docTypeName="Vehicle Registration"
                accent={tenant.accentColor}
                onExtracted={(fields) => {
                  const mapping: Record<string, string> = {
                    plateNumber: 'vehiclePlate', make: 'vehicleMake', model: 'vehicleModel',
                    year: 'vehicleYear', regoExpiry: 'vehicleRegoExpiry', vehicleType: 'vehicleType',
                  };
                  const newAi: Record<string, number> = {};
                  for (const [key, { value, confidence }] of Object.entries(fields)) {
                    const formKey = mapping[key] || key;
                    u(formKey, value);
                    newAi[formKey] = confidence;
                  }
                  setAiFields(prev => ({ ...prev, ...newAi }));
                }}
              />

              <div>
                <div className="flex items-center gap-1.5 mb-1.5">
                  <label className={labelClass + ' mb-0'}>Vehicle Type *</label>
                  {aiFields.vehicleType && <AiConfidenceBadge confidence={aiFields.vehicleType} />}
                </div>
                <select className={inputClass} value={form.vehicleType} onChange={e => u('vehicleType', e.target.value)}>
                  <option value="">Select type...</option>
                  <option value="Hatchback">Hatchback</option>
                  <option value="Sedan">Sedan</option>
                  <option value="SUV">SUV</option>
                  <option value="Station Wagon">Station Wagon</option>
                  <option value="Van">Van</option>
                  <option value="Truck">Truck</option>
                  <option value="Motorcycle">Motorcycle</option>
                  <option value="E-Bike">E-Bike</option>
                </select>
                {validationErrors.vehicleType && <p className={errorClass}>{validationErrors.vehicleType}</p>}
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <div className="flex items-center gap-1.5 mb-1.5">
                    <label className={labelClass + ' mb-0'}>Make</label>
                    {aiFields.vehicleMake && <AiConfidenceBadge confidence={aiFields.vehicleMake} />}
                  </div>
                  <input className={inputClass} value={form.vehicleMake} onChange={e => u('vehicleMake', e.target.value)} placeholder="Toyota" />
                </div>
                <div>
                  <div className="flex items-center gap-1.5 mb-1.5">
                    <label className={labelClass + ' mb-0'}>Model</label>
                    {aiFields.vehicleModel && <AiConfidenceBadge confidence={aiFields.vehicleModel} />}
                  </div>
                  <input className={inputClass} value={form.vehicleModel} onChange={e => u('vehicleModel', e.target.value)} placeholder="Corolla" />
                </div>
                <div>
                  <div className="flex items-center gap-1.5 mb-1.5">
                    <label className={labelClass + ' mb-0'}>Year</label>
                    {aiFields.vehicleYear && <AiConfidenceBadge confidence={aiFields.vehicleYear} />}
                  </div>
                  <input type="number" className={inputClass} value={form.vehicleYear} onChange={e => u('vehicleYear', e.target.value)} placeholder="2022" />
                </div>
                <div>
                  <div className="flex items-center gap-1.5 mb-1.5">
                    <label className={labelClass + ' mb-0'}>Registration Plate *</label>
                    {aiFields.vehiclePlate && <AiConfidenceBadge confidence={aiFields.vehiclePlate} />}
                  </div>
                  <input className={inputClass} value={form.vehiclePlate} onChange={e => u('vehiclePlate', e.target.value.toUpperCase())} placeholder="ABC123" />
                  {validationErrors.vehiclePlate && <p className={errorClass}>{validationErrors.vehiclePlate}</p>}
                </div>
              </div>
            </div>

            {/* Vehicle Photos */}
            <div className={sectionClass}>
              <h3 className="text-base font-bold text-brand-dark">Vehicle Photos</h3>
              <p className="text-sm text-gray-500">Upload clear photos of your vehicle (helps verify vehicle details)</p>
              <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                {VEHICLE_PHOTO_SLOTS.map(slot => {
                  const photo = vehiclePhotos[slot.key];
                  return (
                    <div key={slot.key} className="relative">
                      {photo ? (
                        <div className="relative w-full aspect-square rounded-xl border-2 border-green-400 overflow-hidden bg-gray-100">
                          <img src={photo.url} alt={slot.label} className="w-full h-full object-cover" />
                          <button
                            onClick={() => removeVehiclePhoto(slot.key)}
                            className="absolute top-1 right-1 w-6 h-6 bg-red-500 text-white rounded-full flex items-center justify-center text-xs font-bold hover:bg-red-600 shadow"
                          >
                            ×
                          </button>
                          <div className="absolute bottom-0 left-0 right-0 bg-black/50 text-white text-[10px] text-center py-0.5 font-medium">{slot.label}</div>
                        </div>
                      ) : (
                        <label className="flex flex-col items-center justify-center w-full aspect-square rounded-xl border-2 border-dashed border-gray-300 cursor-pointer hover:border-brand-cyan transition-colors bg-gray-50">
                          <input type="file" accept="image/*" className="hidden" onChange={e => { const f = e.target.files?.[0]; if (f) handleVehiclePhoto(slot.key, f); e.target.value = ''; }} />
                          <svg className="w-8 h-8 text-gray-300 mb-1" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                            <path strokeLinecap="round" strokeLinejoin="round" d="M6.827 6.175A2.31 2.31 0 015.186 7.23c-.38.054-.757.112-1.134.175C2.999 7.58 2.25 8.507 2.25 9.574V18a2.25 2.25 0 002.25 2.25h15A2.25 2.25 0 0021.75 18V9.574c0-1.067-.75-1.994-1.802-2.169a47.865 47.865 0 00-1.134-.175 2.31 2.31 0 01-1.64-1.055l-.822-1.316a2.192 2.192 0 00-1.736-1.039 48.774 48.774 0 00-5.232 0 2.192 2.192 0 00-1.736 1.039l-.821 1.316z" />
                            <path strokeLinecap="round" strokeLinejoin="round" d="M16.5 12.75a4.5 4.5 0 11-9 0 4.5 4.5 0 019 0z" />
                          </svg>
                          <span className="text-[11px] font-medium text-gray-400">{slot.label}</span>
                        </label>
                      )}
                    </div>
                  );
                })}
              </div>
              <p className="text-xs text-gray-400 italic">📸 Photos help speed up your approval</p>
            </div>
          </div>
        )}

        {/* ═══ DYNAMIC DOC STEPS (2..reviewStep-1) ═══ */}
        {step >= 2 && step < reviewStep && (() => {
          const doc = getDocForStep(step);
          if (!doc) return null;
          const isLicense = doc.name.toLowerCase().includes('license') || doc.name.toLowerCase().includes('licence');
          const isRego = doc.name.toLowerCase().includes('registration');
          const isWof = doc.name.toLowerCase().includes('wof') || doc.name.toLowerCase().includes('warrant');
          const isUploaded = uploadedDocs.some(d => d.docTypeId === doc.id);
          const extracted = docExtractedFields[doc.id] || {};
          const confidence = docExtractedConfidence[doc.id] || {};

          // Check if this doc was already partially captured in a fixed step
          const dlAlreadyScannedFront = isLicense && !!(form.firstName && aiFields.firstName);
          const regoAlreadyScanned = isRego && !!(form.plateNumber || form.licensePlate);

          return (
            <div className="space-y-5">
              <div className={sectionClass}>
                <div className="flex items-center gap-3">
                  <h2 className="text-lg font-bold text-brand-dark flex-1">{doc.name}</h2>
                  {doc.mandatory ? (
                    <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold bg-red-50 text-red-600 border border-red-200">Required</span>
                  ) : (
                    <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold bg-gray-100 text-gray-500 border border-gray-200">Optional</span>
                  )}
                </div>

                {/* Already captured notice */}
                {dlAlreadyScannedFront && (
                  <div className="flex items-center gap-2 p-3 bg-green-50 border border-green-200 rounded-xl">
                    <span>✅</span>
                    <div>
                      <span className="text-sm font-medium text-green-700">Front already captured in Your Details</span>
                      <p className="text-xs text-green-600 mt-0.5">Now scan the <strong>back</strong> of your license for classes and endorsements.</p>
                    </div>
                  </div>
                )}

                {regoAlreadyScanned && !isUploaded && (
                  <div className="flex items-center gap-2 p-3 bg-green-50 border border-green-200 rounded-xl">
                    <span>✅</span>
                    <div>
                      <span className="text-sm font-medium text-green-700">Vehicle details already captured</span>
                      <p className="text-xs text-green-600 mt-0.5">Scan or upload the registration document as proof if not already done.</p>
                    </div>
                  </div>
                )}

                {doc.instructions && !dlAlreadyScannedFront && !regoAlreadyScanned && (
                  <p className="text-sm text-gray-500">{doc.instructions}</p>
                )}

                {isWof && (
                  <p className="text-sm text-gray-500">Upload a photo of your WOF sticker or certificate as proof of expiry date.</p>
                )}

                <ScanToFill
                  label={dlAlreadyScannedFront ? '📸 Scan back of license (classes & endorsements)' : isLicense ? `📸 Scan ${doc.name} (front & back)` : isWof ? `📸 Photo of WOF sticker or certificate` : `📸 Scan ${doc.name}`}
                  docTypeName={doc.name}
                  accent={tenant.accentColor}
                  onExtracted={(fields) => {
                    const newExtracted: Record<string, string> = {};
                    const newConfidence: Record<string, number> = {};
                    for (const [key, { value, confidence: conf }] of Object.entries(fields)) {
                      newExtracted[key] = value;
                      newConfidence[key] = conf;
                    }
                    setDocExtractedFields(prev => ({ ...prev, [doc.id]: { ...prev[doc.id], ...newExtracted } }));
                    setDocExtractedConfidence(prev => ({ ...prev, [doc.id]: { ...prev[doc.id], ...newConfidence } }));
                    setUploadedDocs(docs => [
                      ...docs.filter(d => d.docTypeId !== doc.id),
                      { docTypeId: doc.id, fileName: 'scanned', status: 'verified' },
                    ]);
                  }}
                />

                {/* Extractable Fields */}
                {doc.extractableFields && doc.extractableFields.length > 0 && (
                  <div className="space-y-3">
                    <h3 className="text-sm font-semibold text-brand-dark/70">Document Details</h3>
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                      {doc.extractableFields.map(field => (
                        <div key={field.key}>
                          <div className="flex items-center gap-1.5 mb-1.5">
                            <label className={labelClass + ' mb-0'}>
                              {field.label}{field.required ? ' *' : ''}
                            </label>
                            {confidence[field.key] && <AiConfidenceBadge confidence={confidence[field.key]} />}
                          </div>
                          <input
                            type={field.type === 'date' ? 'date' : 'text'}
                            className={inputClass}
                            value={extracted[field.key] || ''}
                            onChange={e => setDocExtractedFields(prev => ({
                              ...prev,
                              [doc.id]: { ...prev[doc.id], [field.key]: e.target.value },
                            }))}
                            placeholder={field.label}
                          />
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {isUploaded && (
                  <div className="flex items-center gap-2 p-3 bg-green-50 border border-green-200 rounded-xl">
                    <span>✅</span>
                    <span className="text-sm font-medium text-green-700">Document scanned successfully</span>
                  </div>
                )}
              </div>

              {/* Skip button for non-mandatory */}
              {!doc.mandatory && (
                <button
                  onClick={handleNext}
                  className="w-full text-center text-sm text-gray-400 hover:text-gray-600 py-2"
                >
                  Skip this document →
                </button>
              )}
            </div>
          );
        })()}

        {/* ═══ REVIEW & SUBMIT ═══ */}
        {step === reviewStep && (
          <div className="space-y-5">
            <div className={sectionClass}>
              <h2 className="text-lg font-bold text-brand-dark">Review Your Application</h2>

              <div className="divide-y divide-gray-100 text-sm">
                <div className="py-2 flex justify-between">
                  <span className="text-gray-500">Name</span>
                  <span className="font-medium text-brand-dark">{form.firstName} {form.lastName}</span>
                </div>
                <div className="py-2 flex justify-between">
                  <span className="text-gray-500">Email</span>
                  <span className="font-medium text-brand-dark">{form.email}</span>
                </div>
                <div className="py-2 flex justify-between">
                  <span className="text-gray-500">Phone</span>
                  <span className="font-medium text-brand-dark">{form.mobile || '—'}</span>
                </div>
                <div className="py-2 flex justify-between">
                  <span className="text-gray-500">Location</span>
                  <span className="font-medium text-brand-dark">{form.city}{form.state ? `, ${form.state}` : ''} {countryFlag}</span>
                </div>
                <div className="py-2 flex justify-between">
                  <span className="text-gray-500">Vehicle</span>
                  <span className="font-medium text-brand-dark">{form.vehicleType} {form.vehicleMake} {form.vehicleModel}</span>
                </div>
                <div className="py-2 flex justify-between">
                  <span className="text-gray-500">Plate</span>
                  <span className="font-medium text-brand-dark">{form.vehiclePlate || '—'}</span>
                </div>
                <div className="py-2 flex justify-between">
                  <span className="text-gray-500">Vehicle Photos</span>
                  <span className="font-medium text-brand-dark">{Object.keys(vehiclePhotos).length} of {VEHICLE_PHOTO_SLOTS.length}</span>
                </div>
              </div>

              {/* Document status */}
              <h3 className="text-sm font-bold text-brand-dark pt-2">Documents</h3>
              <div className="space-y-2">
                {docTypes.map(dt => {
                  const uploaded = uploadedDocs.some(d => d.docTypeId === dt.id);
                  return (
                    <div key={dt.id} className="flex items-center justify-between py-2 text-sm">
                      <div className="flex items-center gap-2">
                        <span>{uploaded ? '✅' : '⬜'}</span>
                        <span className={uploaded ? 'text-brand-dark font-medium' : 'text-gray-400'}>{dt.name}</span>
                      </div>
                      {dt.mandatory && !uploaded && (
                        <span className="text-xs text-red-400 font-medium">Missing</span>
                      )}
                    </div>
                  );
                })}
              </div>

              <button onClick={() => setStep(0)} className="text-sm text-brand-cyan font-medium hover:underline">
                ← Edit details
              </button>
            </div>

            {/* Declaration */}
            <div className={sectionClass}>
              <h2 className="text-lg font-bold text-brand-dark">Declaration</h2>

              <div className="bg-gray-50 rounded-xl p-4 text-xs text-gray-600 leading-relaxed max-h-40 overflow-y-auto">
                <p className="mb-2">I hereby declare that all information provided in this application is true and correct to the best of my knowledge. I understand that any false or misleading statements may result in the termination of my engagement as a courier.</p>
                <p className="mb-2">I agree to comply with all applicable laws, regulations, and company policies in the performance of courier services. I acknowledge that I am responsible for maintaining valid insurance, registration, and licenses for my vehicle.</p>
                <p>I consent to background checks and verification of the information provided herein. I understand this is an independent contractor arrangement and not an employment relationship.</p>
              </div>

              <label className="flex items-start gap-3 cursor-pointer">
                <input type="checkbox" checked={agreedToTerms} onChange={e => setAgreedToTerms(e.target.checked)} className="mt-0.5 w-5 h-5 rounded border-gray-300 text-brand-cyan focus:ring-brand-cyan" />
                <span className="text-sm text-gray-700">I agree to the terms and conditions above</span>
              </label>
              {validationErrors.agree && <p className={errorClass}>{validationErrors.agree}</p>}

              <div>
                <label className={labelClass}>Full Name *</label>
                <input className={inputClass} value={declarationName} onChange={e => setDeclarationName(e.target.value)} placeholder="Type your full legal name" />
                {validationErrors.declarationName && <p className={errorClass}>{validationErrors.declarationName}</p>}
              </div>

              <div>
                <label className={labelClass}>Signature *</label>
                <canvas
                  ref={canvasRef} width={600} height={180}
                  className="w-full border border-gray-300 rounded-xl cursor-crosshair bg-white touch-none"
                  style={{ height: '120px' }}
                  onMouseDown={startDraw} onMouseMove={draw} onMouseUp={stopDraw} onMouseLeave={stopDraw}
                  onTouchStart={startDraw} onTouchMove={draw} onTouchEnd={stopDraw}
                />
                <button onClick={clearSignature} className="mt-2 text-xs text-brand-cyan font-medium hover:underline">Clear signature</button>
                {validationErrors.signature && <p className={errorClass}>{validationErrors.signature}</p>}
              </div>
            </div>
          </div>
        )}

        {/* ═══ CONFIRMATION ═══ */}
        {step === reviewStep + 1 && submitted && (
          <div className={sectionClass + ' text-center'}>
            <div className="w-20 h-20 rounded-full bg-green-100 flex items-center justify-center mx-auto mb-4">
              <svg className="w-10 h-10 text-green-600" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
                <path d="M20 6L9 17l-5-5" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-brand-dark mb-2">Application Submitted!</h2>
            <p className="text-sm text-gray-500 max-w-md mx-auto mb-6">
              Thank you for applying to <strong>{tenant.name}</strong>. Our team will review your application and be in touch shortly.
            </p>
            <div className="bg-gray-50 rounded-xl px-4 py-3 text-sm text-gray-600 inline-block">
              Confirmation sent to <strong className="text-brand-dark">{form.email}</strong>
            </div>
          </div>
        )}
      </div>

      {/* ── Bottom Navigation ─────────────────────────── */}
      {step >= 0 && step <= reviewStep && !submitted && (
        <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 safe-area-bottom z-40">
          <div className="max-w-3xl mx-auto px-4 py-3 flex items-center justify-between">
            {step > 0 ? (
              <button onClick={handleBack} className="flex items-center gap-1 px-5 py-3 text-sm font-medium text-gray-600 hover:text-gray-800 active:bg-gray-50 rounded-xl">
                <svg className="w-4 h-4" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10l3.293 3.293a1 1 0 01-1.414 1.414l-4-4a1 1 0 010-1.414l4-4a1 1 0 011.414 0z" /></svg>
                Back
              </button>
            ) : <div />}

            {step < reviewStep ? (
              <button
                onClick={handleNext}
                className="flex items-center gap-1 px-6 py-3 text-sm font-semibold text-white rounded-xl hover:opacity-90 active:opacity-80 disabled:opacity-40 shadow-sm transition-all"
                style={{ backgroundColor: tenant.accentColor }}
              >
                Continue
                <svg className="w-4 h-4" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" /></svg>
              </button>
            ) : (
              <button
                onClick={handleSubmit}
                className="flex items-center gap-2 px-6 py-3 text-sm font-semibold bg-green-600 text-white rounded-xl hover:bg-green-700 active:bg-green-800 disabled:opacity-40 shadow-sm transition-all"
              >
                Submit Application
                <svg className="w-4 h-4" viewBox="0 0 20 20" fill="currentColor"><path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L7 12.586l7.293-7.293a1 1 0 011.414 0z" /></svg>
              </button>
            )}
          </div>
        </div>
      )}

      {/* ── Footer ────────────────────────────────────── */}
      <footer className="bg-brand-dark text-white/40 safe-area-bottom">
        <div className="max-w-3xl mx-auto px-4 py-6 flex items-center justify-center gap-3">
          <img src={import.meta.env.BASE_URL + 'auto-mate-icon.png'} alt="Auto-Mate" className="h-6 w-6 object-contain opacity-50" />
          <span className="text-xs">Powered by <strong className="text-white/60">DFRNT</strong> · Deliver Different</span>
        </div>
      </footer>
    </div>
  );
}
