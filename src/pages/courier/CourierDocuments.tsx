import { useState, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { resolveTenant, DEFAULT_DOC_REQUIREMENTS } from '@/lib/tenants';

// ── Types ──
type DocStatus = 'current' | 'expiring' | 'expired' | 'superseded';

interface CourierDocument {
  id: number;
  docTypeId: number;
  docTypeName: string;
  fileName: string;
  fileSize: number;
  uploadedDate: string;
  expiryDate: string | null;
  status: DocStatus;
  aiConfidence?: number;
  humanVerified: boolean;
  supersededBy?: number;
}

// ── Mock Data ──
const MOCK_DOCS: CourierDocument[] = [
  { id: 1, docTypeId: 1, docTypeName: "Driver's License", fileName: 'drivers-license-front.jpg', fileSize: 245000, uploadedDate: '2026-01-15', expiryDate: '2027-08-15', status: 'current', aiConfidence: 96, humanVerified: true },
  { id: 2, docTypeId: 2, docTypeName: 'Vehicle Registration', fileName: 'rego-2026.pdf', fileSize: 180000, uploadedDate: '2026-02-01', expiryDate: '2026-04-10', status: 'expiring', aiConfidence: 91, humanVerified: false },
  { id: 3, docTypeId: 3, docTypeName: 'Vehicle Insurance', fileName: 'insurance-cert.pdf', fileSize: 320000, uploadedDate: '2025-06-10', expiryDate: '2026-02-28', status: 'expired', aiConfidence: 88, humanVerified: true },
  { id: 4, docTypeId: 4, docTypeName: 'WOF Certificate', fileName: 'wof-sept-2025.jpg', fileSize: 150000, uploadedDate: '2025-09-20', expiryDate: '2026-09-20', status: 'current', aiConfidence: 94, humanVerified: true },
  // Superseded history
  { id: 5, docTypeId: 3, docTypeName: 'Vehicle Insurance', fileName: 'insurance-2024.pdf', fileSize: 290000, uploadedDate: '2024-06-01', expiryDate: '2025-06-10', status: 'superseded', aiConfidence: 85, humanVerified: true, supersededBy: 3 },
  { id: 6, docTypeId: 1, docTypeName: "Driver's License", fileName: 'dl-old-scan.jpg', fileSize: 210000, uploadedDate: '2024-08-20', expiryDate: '2026-01-10', status: 'superseded', aiConfidence: 82, humanVerified: true, supersededBy: 1 },
];

// ── Helpers ──
function statusConfig(status: DocStatus) {
  const map = {
    current: { label: 'Current', bg: 'bg-green-100', text: 'text-green-700', icon: '✅', border: 'border-green-200' },
    expiring: { label: 'Expiring Soon', bg: 'bg-amber-100', text: 'text-amber-700', icon: '⚠️', border: 'border-amber-200' },
    expired: { label: 'Expired', bg: 'bg-red-100', text: 'text-red-700', icon: '❌', border: 'border-red-200' },
    superseded: { label: 'Superseded', bg: 'bg-gray-100', text: 'text-gray-500', icon: '📁', border: 'border-gray-200' },
  };
  return map[status];
}

function daysUntil(dateStr: string | null): number | null {
  if (!dateStr) return null;
  const d = new Date(dateStr);
  const now = new Date('2026-03-07');
  return Math.ceil((d.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
}

function formatFileSize(bytes: number): string {
  return bytes >= 1048576 ? `${(bytes / 1048576).toFixed(1)} MB` : `${(bytes / 1024).toFixed(0)} KB`;
}

// ── Mock AI extraction ──
function mockExtract(docTypeName: string) {
  return new Promise<{ fields: { name: string; value: string; confidence: number }[]; confidence: number }>(resolve => {
    setTimeout(() => {
      resolve({
        confidence: 89 + Math.floor(Math.random() * 10),
        fields: [
          { name: 'Document Type', value: docTypeName, confidence: 95 },
          { name: 'Expiry Date', value: '2027-03-15', confidence: 88 },
        ],
      });
    }, 1800);
  });
}

export default function CourierDocuments() {
  const { tenantSlug } = useParams<{ tenantSlug: string }>();
  const tenant = resolveTenant(tenantSlug);
  const country = tenant.country;

  const [documents, setDocuments] = useState<CourierDocument[]>(MOCK_DOCS);
  const [showHistory, setShowHistory] = useState(false);
  const [uploadingTypeId, setUploadingTypeId] = useState<number | null>(null);
  const [processingTypeId, setProcessingTypeId] = useState<number | null>(null);
  const [expandedId, setExpandedId] = useState<number | null>(null);
  const fileRef = useRef<HTMLInputElement>(null);

  const docRequirements = tenant.docRequirements || DEFAULT_DOC_REQUIREMENTS;
  const applicableTypes = docRequirements.filter(dt =>
    !dt.countriesOnly || dt.countriesOnly.includes(country)
  );

  // Current (non-superseded) docs
  const currentDocs = documents.filter(d => d.status !== 'superseded');
  const historyDocs = documents.filter(d => d.status === 'superseded');

  // Compliance stats
  const mandatoryTypes = applicableTypes.filter(dt => dt.mandatory);
  const compliantCount = mandatoryTypes.filter(dt =>
    currentDocs.some(d => d.docTypeId === dt.id && d.status === 'current')
  ).length;
  const expiringCount = currentDocs.filter(d => d.status === 'expiring').length;
  const expiredCount = currentDocs.filter(d => d.status === 'expired').length;
  const missingCount = mandatoryTypes.filter(dt =>
    !currentDocs.some(d => d.docTypeId === dt.id)
  ).length;
  const compliancePercent = mandatoryTypes.length > 0
    ? Math.round((compliantCount / mandatoryTypes.length) * 100)
    : 100;

  const triggerUpload = (docTypeId: number) => {
    setUploadingTypeId(docTypeId);
    fileRef.current?.click();
  };

  const handleFile = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file || uploadingTypeId == null) return;
    e.target.value = '';

    const docType = applicableTypes.find(dt => dt.id === uploadingTypeId);
    if (!docType) return;

    setProcessingTypeId(uploadingTypeId);

    const extraction = await mockExtract(docType.name);
    const expiryField = extraction.fields.find(f => f.name.toLowerCase().includes('expiry'));

    // Supersede existing current doc for this type
    setDocuments(prev => {
      const updated = prev.map(d =>
        d.docTypeId === uploadingTypeId && d.status !== 'superseded'
          ? { ...d, status: 'superseded' as DocStatus }
          : d
      );
      const newId = Math.max(...updated.map(d => d.id), 0) + 1;
      return [
        ...updated,
        {
          id: newId,
          docTypeId: uploadingTypeId,
          docTypeName: docType.name,
          fileName: file.name,
          fileSize: file.size,
          uploadedDate: '2026-03-07',
          expiryDate: expiryField?.value || null,
          status: 'current' as DocStatus,
          aiConfidence: extraction.confidence,
          humanVerified: false,
        },
      ];
    });
    setProcessingTypeId(null);
    setUploadingTypeId(null);
  };

  return (
    <div className="px-4 pt-4 pb-4">
      <input ref={fileRef} type="file" className="hidden" accept="image/*,.pdf" onChange={handleFile} />

      <h1 className="text-xl font-bold text-gray-900 mb-4">My Documents</h1>

      {/* Compliance Overview Card */}
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-4 mb-4">
        <div className="flex items-center justify-between mb-3">
          <span className="text-sm font-semibold text-gray-900">Compliance Status</span>
          <span className={`text-xs font-bold px-2.5 py-1 rounded-full ${
            compliancePercent === 100 ? 'bg-green-100 text-green-700'
              : compliancePercent >= 70 ? 'bg-amber-100 text-amber-700'
              : 'bg-red-100 text-red-700'
          }`}>
            {compliancePercent}%
          </span>
        </div>

        {/* Progress bar */}
        <div className="h-2 bg-gray-100 rounded-full overflow-hidden mb-3">
          <div
            className="h-full rounded-full transition-all duration-500"
            style={{
              width: `${compliancePercent}%`,
              backgroundColor: compliancePercent === 100 ? '#16a34a' : compliancePercent >= 70 ? '#d97706' : '#dc2626',
            }}
          />
        </div>

        {/* Stat pills */}
        <div className="flex gap-2 flex-wrap">
          <span className="text-xs px-2.5 py-1 rounded-full bg-green-50 text-green-700 border border-green-200">
            ✅ {compliantCount} Current
          </span>
          {expiringCount > 0 && (
            <span className="text-xs px-2.5 py-1 rounded-full bg-amber-50 text-amber-700 border border-amber-200">
              ⚠️ {expiringCount} Expiring
            </span>
          )}
          {expiredCount > 0 && (
            <span className="text-xs px-2.5 py-1 rounded-full bg-red-50 text-red-700 border border-red-200">
              ❌ {expiredCount} Expired
            </span>
          )}
          {missingCount > 0 && (
            <span className="text-xs px-2.5 py-1 rounded-full bg-gray-100 text-gray-600 border border-gray-200">
              📄 {missingCount} Missing
            </span>
          )}
        </div>
      </div>

      {/* Urgent actions */}
      {(expiredCount > 0 || expiringCount > 0) && (
        <div className={`rounded-xl px-4 py-3 mb-4 text-xs ${
          expiredCount > 0 ? 'bg-red-50 border border-red-200 text-red-700' : 'bg-amber-50 border border-amber-200 text-amber-700'
        }`}>
          {expiredCount > 0
            ? `⚠ ${expiredCount} expired document${expiredCount > 1 ? 's' : ''} — upload updated proof to stay active.`
            : `${expiringCount} document${expiringCount > 1 ? 's' : ''} expiring soon — renew before they expire.`
          }
        </div>
      )}

      {/* Document Cards */}
      <div className="space-y-3">
        {applicableTypes.map(dt => {
          const doc = currentDocs.find(d => d.docTypeId === dt.id);
          const isProcessing = processingTypeId === dt.id;
          const isExpanded = expandedId === dt.id;
          const days = doc ? daysUntil(doc.expiryDate) : null;
          const sc = doc ? statusConfig(doc.status) : null;
          const typeHistory = historyDocs.filter(d => d.docTypeId === dt.id);

          return (
            <div key={dt.id} className={`bg-white rounded-2xl shadow-sm border overflow-hidden transition-all ${
              doc ? (doc.status === 'expired' ? 'border-red-200' : doc.status === 'expiring' ? 'border-amber-200' : 'border-gray-100') : 'border-dashed border-gray-300'
            }`}>
              {/* Card header */}
              <div
                className="flex items-center gap-3 px-4 py-3.5 cursor-pointer active:bg-gray-50"
                onClick={() => setExpandedId(isExpanded ? null : dt.id)}
              >
                <span className="text-xl">
                  {isProcessing ? '⏳' : doc ? (sc?.icon || '📄') : '📄'}
                </span>
                <div className="flex-1 min-w-0">
                  <div className="text-sm font-semibold text-gray-900 flex items-center gap-2">
                    {dt.name}
                    {dt.mandatory && (
                      <span className="text-[9px] px-1.5 py-0 rounded bg-red-50 text-red-600 border border-red-200 uppercase font-bold">Req</span>
                    )}
                  </div>
                  {isProcessing && (
                    <div className="text-xs mt-0.5 flex items-center gap-1.5" style={{ color: tenant.accentColor }}>
                      <span className="inline-block w-3 h-3 border-2 border-t-transparent rounded-full animate-spin" style={{ borderColor: `${tenant.accentColor} transparent ${tenant.accentColor} ${tenant.accentColor}` }} />
                      Analysing with AI…
                    </div>
                  )}
                  {doc && !isProcessing && (
                    <div className="text-xs text-gray-500 mt-0.5">
                      {doc.fileName} · {formatFileSize(doc.fileSize)}
                    </div>
                  )}
                  {!doc && !isProcessing && (
                    <div className="text-xs text-gray-400 mt-0.5">Not uploaded</div>
                  )}
                </div>

                {/* Status badge or Upload button */}
                {doc && sc && !isProcessing && (
                  <span className={`text-[10px] font-semibold px-2 py-0.5 rounded-full ${sc.bg} ${sc.text}`}>
                    {sc.label}
                  </span>
                )}
                {!doc && !isProcessing && (
                  <button
                    onClick={e => { e.stopPropagation(); triggerUpload(dt.id); }}
                    className="text-xs font-semibold px-3 py-1.5 rounded-lg text-white shrink-0"
                    style={{ backgroundColor: tenant.accentColor }}
                  >
                    Upload
                  </button>
                )}

                <svg
                  className={`w-4 h-4 text-gray-400 transition-transform shrink-0 ${isExpanded ? 'rotate-180' : ''}`}
                  viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"
                >
                  <polyline points="6 9 12 15 18 9" />
                </svg>
              </div>

              {/* Expanded details */}
              {isExpanded && (
                <div className="border-t border-gray-100 px-4 py-3 bg-gray-50/50">
                  {doc && (
                    <>
                      {/* Expiry info */}
                      {doc.expiryDate && (
                        <div className="flex items-center gap-2 text-xs mb-3">
                          <span className="text-gray-500">📅 Expires:</span>
                          <span className={`font-medium ${
                            days != null && days < 0 ? 'text-red-600' : days != null && days <= 30 ? 'text-amber-600' : 'text-gray-900'
                          }`}>
                            {new Date(doc.expiryDate).toLocaleDateString('en-NZ', { day: 'numeric', month: 'short', year: 'numeric' })}
                            {days != null && (
                              <span className="ml-1 text-gray-400">
                                ({days < 0 ? `${Math.abs(days)} days ago` : `${days} days`})
                              </span>
                            )}
                          </span>
                        </div>
                      )}

                      {/* Upload info */}
                      <div className="flex items-center gap-2 text-xs text-gray-500 mb-3">
                        <span>Uploaded {new Date(doc.uploadedDate).toLocaleDateString('en-NZ', { day: 'numeric', month: 'short', year: 'numeric' })}</span>
                        {doc.aiConfidence != null && (
                          <span className={`px-1.5 py-0.5 rounded text-[10px] font-medium border ${
                            doc.aiConfidence >= 95 ? 'bg-green-50 text-green-600 border-green-200'
                              : doc.aiConfidence >= 70 ? 'bg-amber-50 text-amber-600 border-amber-200'
                              : 'bg-red-50 text-red-600 border-red-200'
                          }`}>
                            AI {doc.aiConfidence}%
                          </span>
                        )}
                        {doc.humanVerified && (
                          <span className="text-green-600 font-medium">✓ Verified</span>
                        )}
                      </div>

                      {/* Actions */}
                      <div className="flex gap-2">
                        <button
                          onClick={() => triggerUpload(dt.id)}
                          className="flex-1 text-xs font-semibold py-2.5 rounded-xl text-white"
                          style={{ backgroundColor: tenant.accentColor }}
                        >
                          {doc.status === 'expired' ? '📤 Upload Renewed Document' : '📤 Upload New Version'}
                        </button>
                        <button className="text-xs text-gray-500 px-3 py-2.5 rounded-xl border border-gray-200 bg-white active:bg-gray-50">
                          👁 View
                        </button>
                      </div>

                      {/* History for this doc type */}
                      {typeHistory.length > 0 && (
                        <div className="mt-3 pt-3 border-t border-gray-200">
                          <div className="text-[11px] font-semibold text-gray-500 uppercase tracking-wide mb-2">
                            📁 Previous Versions ({typeHistory.length})
                          </div>
                          {typeHistory.map(h => (
                            <div key={h.id} className="flex items-center gap-2 text-xs text-gray-400 py-1.5 border-b border-gray-100 last:border-b-0">
                              <span className="flex-1 truncate">{h.fileName}</span>
                              <span>{new Date(h.uploadedDate).toLocaleDateString('en-NZ', { day: 'numeric', month: 'short', year: 'numeric' })}</span>
                              {h.expiryDate && (
                                <span className="text-gray-300">exp {new Date(h.expiryDate).toLocaleDateString('en-NZ', { day: 'numeric', month: 'short', year: 'numeric' })}</span>
                              )}
                              <button className="text-gray-400 hover:text-gray-600">👁</button>
                            </div>
                          ))}
                        </div>
                      )}
                    </>
                  )}

                  {!doc && (
                    <div className="text-center py-4">
                      <p className="text-sm text-gray-500 mb-3">{dt.instructions}</p>
                      <button
                        onClick={() => triggerUpload(dt.id)}
                        className="text-sm font-semibold px-6 py-2.5 rounded-xl text-white"
                        style={{ backgroundColor: tenant.accentColor }}
                      >
                        📸 Upload or Take Photo
                      </button>
                    </div>
                  )}
                </div>
              )}
            </div>
          );
        })}
      </div>

      {/* History toggle */}
      {historyDocs.length > 0 && (
        <button
          onClick={() => setShowHistory(!showHistory)}
          className="w-full mt-4 text-xs font-medium text-gray-500 py-2 text-center"
        >
          {showHistory ? '▲ Hide' : '▼ Show'} All Document History ({historyDocs.length} archived)
        </button>
      )}

      {showHistory && historyDocs.length > 0 && (
        <div className="mt-2 bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
          <div className="px-4 py-3 border-b border-gray-100 bg-gray-50">
            <span className="text-sm font-semibold text-gray-700">📁 Compliance History</span>
            <p className="text-[11px] text-gray-400 mt-0.5">Previous documents kept as proof of compliance history</p>
          </div>
          {historyDocs.map(doc => (
            <div key={doc.id} className="flex items-center gap-3 px-4 py-3 border-b border-gray-50 last:border-b-0">
              <span className="text-gray-300 text-lg">📁</span>
              <div className="flex-1 min-w-0">
                <div className="text-xs font-medium text-gray-600">{doc.docTypeName}</div>
                <div className="text-[11px] text-gray-400 truncate">{doc.fileName} · {formatFileSize(doc.fileSize)}</div>
              </div>
              <div className="text-right shrink-0">
                <div className="text-[11px] text-gray-400">{new Date(doc.uploadedDate).toLocaleDateString('en-NZ', { day: 'numeric', month: 'short', year: 'numeric' })}</div>
                {doc.expiryDate && (
                  <div className="text-[10px] text-gray-300">exp {new Date(doc.expiryDate).toLocaleDateString('en-NZ', { day: 'numeric', month: 'short', year: 'numeric' })}</div>
                )}
              </div>
              <button className="text-gray-400 text-xs">👁</button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
