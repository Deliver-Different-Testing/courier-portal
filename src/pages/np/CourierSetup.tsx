import { useState, useEffect } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import { courierService } from '@/services/np_courierService';
import FormField from '@/components/common/FormField';
import DocumentUpload from '@/components/common/DocumentUpload';
import { useDocumentTypes, useCourierDocuments, useComplianceSummary } from '@/hooks/useDocuments';
import type { Courier, DocumentStatus } from '@/types';

const tabKeys = ['profile', 'vehicle', 'licensing', 'insurance', 'financial', 'device', 'documents', 'notes'] as const;
const tabLabels = ['Profile', 'Vehicle', 'Licensing', 'Insurance', 'Financial', 'Device & Settings', 'Documents', 'Notes & Audit'];

interface Props {
  onSelectCourier: (id: number) => void;
}

function isExpiringSoon(dateStr: string): boolean {
  if (!dateStr) return false;
  const d = new Date(dateStr);
  const now = new Date('2026-02-28');
  const diff = (d.getTime() - now.getTime()) / (1000 * 60 * 60 * 24);
  return diff > 0 && diff <= 30;
}

function isExpired(dateStr: string): boolean {
  if (!dateStr) return false;
  return new Date(dateStr) < new Date('2026-02-28');
}

export default function CourierSetup({ onSelectCourier }: Props) {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const initialTab = (searchParams.get('tab') as typeof tabKeys[number]) || 'profile';
  const [tab, setTab] = useState<typeof tabKeys[number]>(tabKeys.includes(initialTab as any) ? initialTab : 'profile');
  const [courier, setCourier] = useState<Courier | undefined>();
  const [masters, setMasters] = useState<Courier[]>([]);
  const [masterName, setMasterName] = useState<Courier | null | undefined>(null);

  useEffect(() => {
    if (id) {
      courierService.getById(Number(id)).then(c => {
        setCourier(c);
        if (c) onSelectCourier(c.id);
      });
    }
  }, [id, onSelectCourier]);

  useEffect(() => {
    courierService.getMasters().then(setMasters);
  }, []);

  useEffect(() => {
    if (courier?.master) {
      courierService.getById(courier.master).then(m => setMasterName(m ?? null));
    }
  }, [courier?.master]);

  if (!courier) {
    return (
      <div className="bg-blue-50 border border-blue-200 text-blue-600 rounded-lg px-4 py-3.5 text-sm flex items-center gap-2.5">
        Select a courier from Fleet Overview to view their setup.
      </div>
    );
  }

  const c = courier;

  return (
    <>
      {/* Header */}
      <div className="flex items-center gap-4 mb-5">
        <div className="w-12 h-12 rounded-full bg-brand-cyan flex items-center justify-center text-xl font-bold text-white">
          {c.firstName[0]}{c.surName[0]}
        </div>
        <div>
          <h2 className="text-lg font-bold">{c.firstName} {c.surName}</h2>
          <div className="text-[13px] text-text-secondary">{c.code} · {c.type} Courier · {c.location}</div>
        </div>
        <span className={`ml-auto px-3 py-1 rounded-xl text-xs border ${
          c.status === 'active'
            ? 'bg-green-50 text-success border-green-200'
            : 'bg-red-50 text-error border-[#991b1b]'
        }`}>
          ● {c.status === 'active' ? 'Active' : 'Inactive'}
        </span>
      </div>

      {/* Tabs */}
      <div className="flex gap-0 border-b border-border mb-5 overflow-x-auto">
        {tabKeys.map((t, i) => (
          <div
            key={t}
            onClick={() => setTab(t)}
            className={`px-4 py-2.5 text-[13px] cursor-pointer whitespace-nowrap border-b-2 transition-all ${
              tab === t ? 'text-brand-cyan border-brand-cyan' : 'text-text-secondary border-transparent hover:text-text-primary'
            }`}
          >
            {tabLabels[i]}
          </div>
        ))}
      </div>

      {/* Tab Content */}
      <div className="bg-white border border-border rounded-lg p-5">
        {tab === 'profile' && (
          <>
            <Section title="Identity" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="First Name" value={c.firstName} />
              <FormField label="Surname" value={c.surName} />
              <FormField label="Code" value={c.code} readonly />
              <FormField label="Courier Type" type="select" value={c.type} options={['Master', 'Sub']} />
              {c.type === 'Sub' && (
                <FormField label="Master Courier" type="select" value={masterName ? `${masterName.firstName} ${masterName.surName}` : ''} options={masters.map(m => `${m.firstName} ${m.surName}`)} />
              )}
              <FormField label="Gender" type="select" value={c.gender} options={['Male', 'Female', 'Other']} />
              <FormField label="Date of Birth" type="date" value={c.dob} />
              <FormField label="Start Date" type="date" value={c.startDate} />
              <FormField label="Finish Date" type="date" value={c.finishDate} />
            </div>
            <Section title="Contact" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Personal Mobile" value={c.phone} />
              <FormField label="Urgent Mobile (Company)" value={c.urgentMobile} />
              <FormField label="Email" value={c.email} />
              <FormField label="Home Phone" value={c.homePhone} />
              <FormField label="Address" value={c.address} full />
            </div>
            <Section title="Emergency / Next of Kin" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Doctor" value={c.doctor} />
              <FormField label="Doctor Phone" value={c.doctorPhone} />
              <FormField label="Next of Kin" value={c.nextOfKin} />
              <FormField label="Relationship" value={c.nokRelationship} />
              <FormField label="Next of Kin Address" value={c.nokAddress} full />
              <FormField label="Next of Kin Phone" value={c.nokPhone} />
            </div>
          </>
        )}

        {tab === 'vehicle' && (
          <>
            <Section title="Vehicle Details" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Vehicle Type" type="select" value={c.vehicle} options={['Car', 'Van', 'Truck', 'Motorcycle', 'Bicycle', 'Other']} />
              <FormField label="Make" type="select" value={c.make} options={['Toyota', 'Ford', 'Mercedes', 'Volkswagen', 'Isuzu', 'Fiat', 'Subaru', 'Honda', 'Mazda', 'Hyundai', 'Yamaha', 'Other']} />
              <FormField label="Model" value={c.model} />
              <FormField label="Year" value={String(c.year)} />
              <FormField label="License Plate" value={c.rego} />
            </div>
            <div className="my-2">
              <FormField label="Low Emission Vehicle" type="checkbox" checked={c.lowEmission} />
            </div>
            <Section title="Dimensions & Weight" />
            <div className="grid grid-cols-3 gap-4">
              <FormField label="Max Pallets" value={String(c.maxPallets)} />
              <FormField label="Tare Weight (kg)" value={String(c.tareWeight)} />
              <FormField label="Max Carrying Weight (kg)" value={String(c.maxCarry)} />
              <FormField label="RUC Weight" value={String(c.rucWeight)} />
              <FormField label="RUC Kms" value={String(c.rucKms)} />
              <FormField label="RUC Payload" value={String(c.rucPayload)} />
              <FormField label="Height (m)" value={String(c.height)} />
              <FormField label="Width (m)" value={String(c.width)} />
              <FormField label="Length (m)" value={String(c.length)} />
            </div>
            <Section title="Compliance Dates" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Vehicle Inspection Expiry" type="date" value={c.inspectionExpiry} warning={isExpiringSoon(c.inspectionExpiry) ? 'Expiring soon!' : undefined} />
              <FormField label="Registration Expiry" type="date" value={c.regoExpiry} warning={isExpiringSoon(c.regoExpiry) ? 'Expiring soon!' : undefined} />
            </div>
          </>
        )}

        {tab === 'licensing' && (
          <>
            <Section title="Driver's License" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Driver's License No" value={c.dlNo} />
              <FormField label="Driver's License Expiry" type="date" value={c.dlExpiry} warning={isExpiringSoon(c.dlExpiry) ? 'Expiring soon!' : undefined} />
            </div>
            {isExpired(c.dlExpiry) && (
              <div className="bg-amber-50 border border-[#854d0e] text-[#92400e] rounded-lg px-4 py-3.5 text-sm flex items-center gap-2.5 mt-3">
                ⚠️ Driver's license has EXPIRED. Courier must not operate until renewed.
              </div>
            )}
            <Section title="Endorsements" />
            <FormField label="Dangerous Goods" type="checkbox" checked={c.dangerousGoods} />
            {c.dangerousGoods && (
              <div className="grid grid-cols-2 gap-4 mt-2">
                <FormField label="DG Certificate Expiry" type="date" value={c.dgExpiry} warning={isExpiringSoon(c.dgExpiry) ? 'Expiring soon!' : undefined} />
              </div>
            )}
            <FormField label="Heavy Transport Endorsement" type="checkbox" checked={c.hte} />
            <Section title="DOT Number" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="DOT Number" value={c.tslNo} />
            </div>
          </>
        )}

        {tab === 'insurance' && (
          <>
            <Section title="Insurance Details" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Policy Number" value={c.policyNo} />
              <FormField label="Insurance Company" type="select" value={c.insuranceCo} options={['State Farm', 'Progressive', 'GEICO', 'Allstate', 'Liberty Mutual', 'Other']} />
              <FormField label="Carrier Liability ($)" value={c.carrierLiab.toLocaleString()} />
              <FormField label="Public Liability ($)" value={c.publicLiab.toLocaleString()} />
            </div>
            <FormField label="Commercial Insurance" type="checkbox" checked={c.commercialIns} />
          </>
        )}

        {tab === 'financial' && (
          <>
            <div className="bg-blue-50 border border-blue-200 text-blue-600 rounded-lg px-4 py-3.5 text-sm flex items-center gap-2.5 mb-4">
              💡 These are YOUR rates to this courier. Your customers cannot see these.
            </div>
            <Section title="Tax & Banking" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Tax ID (EIN)" value={c.taxId} />
              <FormField label="Federal Withholding %" value={String(c.wht)} />
              <FormField label="Bank Account (Routing / Account)" value={c.bankAcct} />
            </div>
            <Section title="Pay Rates" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Pay Percentage (%)" value={String(c.payPct)} />
              <FormField label="Bonus Percentage (%)" value={String(c.bonusPct)} />
            </div>
            <FormField label="Payroll Registration" type="checkbox" checked={c.paydayReg} />
            <Section title="Compliance Dates" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Contract Signed Date" type="date" value={c.contractSigned} />
              <FormField label="Security Check Date" type="date" value={c.securityCheck} />
            </div>
          </>
        )}

        {tab === 'device' && (
          <>
            <Section title="Communication Channel" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Channel" type="select" value={c.channel} options={['App', 'SMS', 'Radio']} />
              <FormField label="Device Type" value={c.deviceType} />
            </div>
            <FormField label="Device Admin" type="checkbox" checked={c.deviceAdmin} />
            <Section title="SMS & Network" />
            <FormField label="Carrier Network" type="checkbox" checked={c.vodafone} />
            <FormField label="Send Job via SMS" type="checkbox" checked={c.smsJob} />
            <FormField label="Send Alert SMS" type="checkbox" checked={c.smsAlert} />
            <Section title="Web & Display" />
            <FormField label="Web Enabled" type="checkbox" checked={c.webEnabled} />
            <FormField label="Auto Despatch" type="checkbox" checked={c.autoDispatch} />
            <FormField label="Show Client Phone" type="checkbox" checked={c.showClientPhone} />
            <FormField label="Mobile Advert Courier" type="checkbox" checked={c.mobileAdvert} />
            <FormField label="Display on Web" type="checkbox" checked={c.displayWeb} />
            <Section title="Security & POD" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Password" type="password" value="••••••••" />
            </div>
            <FormField label="POD Required" type="checkbox" checked={c.podRequired} />
            <Section title="Working Hours" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Expected Start Time" type="time" value={c.startTime} />
              <FormField label="Expected End Time" type="time" value={c.endTime} />
            </div>
          </>
        )}

        {tab === 'documents' && (
          <CourierDocumentsTab courierId={c.id} />
        )}

        {tab === 'notes' && (
          <>
            <Section title="Notes" />
            <FormField label="Notes" type="textarea" value={c.notes} full rows={5} />
            <Section title="Training" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Training Hours (Initial)" value={String(c.trainingInit)} />
              <FormField label="Training Hours (Follow-up)" value={String(c.trainingFollow)} />
            </div>
            <Section title="Audit Trail" />
            <div className="grid grid-cols-2 gap-4">
              <FormField label="Created" value={c.created} readonly />
              <FormField label="Created By" value={c.createdBy} readonly />
              <FormField label="Last Modified" value={c.modified} readonly />
              <FormField label="Last Modified By" value={c.modifiedBy} readonly />
            </div>
          </>
        )}
      </div>

      {/* Action Buttons */}
      <div className="flex gap-2.5 mt-4">
        <button className="bg-brand-cyan text-brand-dark border-none font-medium px-4 py-2 rounded-md text-sm hover:shadow-cyan-glow">
          Save Changes
        </button>
        <button
          onClick={() => navigate('/fleet')}
          className="bg-transparent border border-border text-text-primary px-4 py-2 rounded-md text-sm hover:border-brand-cyan hover:text-brand-cyan transition-all"
        >
          Back to Fleet
        </button>
      </div>
    </>
  );
}

function StatusBadgeDoc({ status }: { status: DocumentStatus }) {
  const map: Record<DocumentStatus, { icon: string; label: string; bg: string; color: string }> = {
    Current: { icon: '✅', label: 'Current', bg: 'bg-green-50', color: 'text-green-700' },
    ExpiringSoon: { icon: '⚠️', label: 'Expiring Soon', bg: 'bg-amber-50', color: 'text-amber-700' },
    Expired: { icon: '❌', label: 'Expired', bg: 'bg-red-50', color: 'text-red-700' },
    Superseded: { icon: '📁', label: 'Superseded', bg: 'bg-gray-50', color: 'text-gray-500' },
  };
  const s = map[status] || map.Current;
  return <span className={`text-xs px-2.5 py-0.5 rounded-lg border ${s.bg} ${s.color}`}>{s.icon} {s.label}</span>;
}

function CourierDocumentsTab({ courierId }: { courierId: number }) {
  const { types } = useDocumentTypes();
  const { documents, upload, deleteDoc, verify, getDownloadUrl } = useCourierDocuments(courierId);
  const summary = useComplianceSummary(types, documents);
  const [showUpload, setShowUpload] = useState(false);
  const [uploadTypeId, setUploadTypeId] = useState<number | undefined>();

  const activeTypes = types.filter(dt => dt.active && (dt.appliesTo === 'ActiveCourier' || dt.appliesTo === 'Both'));

  const handleDownload = async (docId: number) => {
    const url = await getDownloadUrl(docId);
    if (url) window.open(url, '_blank');
  };

  return (
    <>
      {/* Compliance summary bar */}
      <div className="bg-surface-light border border-border rounded-lg px-4 py-3 mb-4 flex items-center gap-3 text-sm">
        <span className="font-medium text-brand-dark">Compliance:</span>
        <span className="text-green-600">{summary.current} current</span>
        <span className="text-text-secondary">·</span>
        {summary.expiring > 0 && <><span className="text-amber-600">{summary.expiring} expiring</span><span className="text-text-secondary">·</span></>}
        {summary.expired > 0 && <><span className="text-red-600">{summary.expired} expired</span><span className="text-text-secondary">·</span></>}
        {summary.missing > 0 && <span className="text-red-500">{summary.missing} missing</span>}
        {summary.missing === 0 && summary.expired === 0 && summary.expiring === 0 && (
          <span className="text-green-600">All documents current ✅</span>
        )}
        <span className="ml-auto text-text-secondary">{summary.current + summary.expiring}/{summary.total} mandatory</span>
      </div>

      <div className="flex justify-between items-center mb-4">
        <h3 className="font-bold">Documents</h3>
        <button
          onClick={() => { setUploadTypeId(undefined); setShowUpload(true); }}
          className="bg-brand-cyan text-brand-dark border-none font-medium px-4 py-2 rounded-md text-sm hover:shadow-cyan-glow"
        >
          Upload Document
        </button>
      </div>

      {/* Document type rows */}
      {activeTypes.map((dt) => {
        const doc = documents.find(d => d.documentTypeId === dt.id && d.status !== 'Superseded');
        return (
          <div key={dt.id} className="flex items-center gap-3 py-3 border-b border-border last:border-b-0">
            <div className="flex-1 min-w-0">
              <div className="text-sm font-medium text-brand-dark flex items-center gap-2">
                {dt.name}
                {dt.mandatory && <span className="text-[10px] px-1.5 py-0 rounded bg-red-50 text-red-600 border border-red-200 uppercase">Required</span>}
              </div>
              {doc ? (
                <div className="text-xs text-text-secondary mt-0.5 flex items-center gap-2 flex-wrap">
                  <span>{doc.fileName}</span>
                  <span>· Uploaded {new Date(doc.uploadedDate).toLocaleDateString()}</span>
                  {doc.expiryDate && <span>· Expires {new Date(doc.expiryDate).toLocaleDateString()}</span>}
                  {doc.aiConfidence != null && (
                    <span className={doc.aiConfidence >= 95 ? 'text-green-600' : doc.aiConfidence >= 70 ? 'text-amber-600' : 'text-red-500'}>
                      AI {doc.aiConfidence.toFixed(0)}%
                    </span>
                  )}
                  {doc.humanVerified && <span className="text-green-600">✓ Verified</span>}
                </div>
              ) : (
                <div className="text-xs text-red-400 mt-0.5">
                  {dt.mandatory ? '⚠ Not uploaded — required' : 'Not uploaded'}
                </div>
              )}
            </div>

            {doc ? (
              <div className="flex items-center gap-2 shrink-0">
                <StatusBadgeDoc status={doc.status} />
                <button onClick={() => handleDownload(doc.id)} className="text-xs text-brand-cyan hover:underline">Download</button>
                {!doc.humanVerified && (
                  <button onClick={() => verify(doc.id)} className="text-xs text-green-600 hover:underline">Verify</button>
                )}
                <button onClick={() => { if (confirm('Delete this document?')) deleteDoc(doc.id); }} className="text-xs text-red-500 hover:underline">Delete</button>
              </div>
            ) : (
              <button
                onClick={() => { setUploadTypeId(dt.id); setShowUpload(true); }}
                className="text-xs bg-brand-cyan text-brand-dark px-3 py-1.5 rounded-md font-medium hover:shadow-cyan-glow shrink-0"
              >
                Upload
              </button>
            )}
          </div>
        );
      })}

      {showUpload && (
        <DocumentUpload
          documentTypes={activeTypes}
          selectedTypeId={uploadTypeId}
          onUpload={upload}
          onClose={() => setShowUpload(false)}
        />
      )}
    </>
  );
}

function Section({ title }: { title: string }) {
  return (
    <div className="text-sm font-bold text-brand-cyan mt-5 mb-3 pb-1.5 border-b border-border first:mt-0">
      {title}
    </div>
  );
}
