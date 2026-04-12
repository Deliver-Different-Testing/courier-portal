import { useState, useRef, useCallback } from 'react';

// ── Confidence Badge ────────────────────────────────────

export function AiConfidenceBadge({ confidence }: { confidence: number }) {
  if (confidence >= 95)
    return <span className="inline-flex items-center gap-0.5 text-[10px] px-1.5 py-0.5 rounded-full bg-green-50 text-green-700 border border-green-200 font-medium">AI ✓</span>;
  if (confidence >= 70)
    return <span className="inline-flex items-center gap-0.5 text-[10px] px-1.5 py-0.5 rounded-full bg-amber-50 text-amber-700 border border-amber-200 font-medium">AI ~</span>;
  return <span className="inline-flex items-center gap-0.5 text-[10px] px-1.5 py-0.5 rounded-full bg-red-50 text-red-700 border border-red-200 font-medium">Review</span>;
}

// ── Mock Extraction Data ────────────────────────────────

const MOCK_EXTRACTIONS: Record<string, Record<string, { value: string; confidence: number }>> = {
  "Driver's License": {
    firstName:    { value: 'James',           confidence: 98 },
    lastName:     { value: 'Wilson',          confidence: 97 },
    dob:          { value: '1990-04-22',      confidence: 93 },
    address:      { value: '42 Queen Street, Auckland 1010', confidence: 88 },
    licenseNumber:{ value: 'DL-29384756',     confidence: 96 },
    licenseExpiry:{ value: '2027-08-15',      confidence: 95 },
    gender:       { value: 'Male',            confidence: 85 },
    licenseClass: { value: 'Class 1, Class 2', confidence: 91 },
    endorsements: { value: 'D, W',            confidence: 87 },
  },
  "Vehicle Registration": {
    plateNumber:  { value: 'KXR-902',         confidence: 95 },
    make:         { value: 'Toyota',          confidence: 92 },
    model:        { value: 'Hiace',           confidence: 90 },
    year:         { value: '2021',            confidence: 88 },
    regoExpiry:   { value: '2026-11-30',      confidence: 91 },
    vehicleType:  { value: 'Van',             confidence: 80 },
  },
};

function getMockExtraction(docTypeName: string): Record<string, { value: string; confidence: number }> {
  for (const [key, fields] of Object.entries(MOCK_EXTRACTIONS)) {
    if (docTypeName.toLowerCase().includes(key.toLowerCase().split(' ')[0].toLowerCase())) {
      return fields;
    }
  }
  return MOCK_EXTRACTIONS[docTypeName] || { documentType: { value: docTypeName, confidence: 72 } };
}

// ── ScanToFill Component ────────────────────────────────

interface ScanToFillProps {
  label: string;
  docTypeName: string;
  onExtracted: (fields: Record<string, { value: string; confidence: number }>) => void;
  accent?: string;
}

type ScanState = 'idle' | 'processing' | 'done';

export default function ScanToFill({ label, docTypeName, onExtracted, accent = '#3bc7f4' }: ScanToFillProps) {
  const [state, setState] = useState<ScanState>('idle');
  const [fileName, setFileName] = useState('');
  const [extractedFields, setExtractedFields] = useState<Record<string, { value: string; confidence: number }> | null>(null);
  const fileRef = useRef<HTMLInputElement>(null);

  const handleFile = useCallback((file: File) => {
    setFileName(file.name);
    setState('processing');

    setTimeout(() => {
      const fields = getMockExtraction(docTypeName);
      setExtractedFields(fields);
      setState('done');
      onExtracted(fields);
    }, 1800);
  }, [docTypeName, onExtracted]);

  const reset = () => {
    setState('idle');
    setFileName('');
    setExtractedFields(null);
  };

  return (
    <div className="mb-5">
      <input
        ref={fileRef}
        type="file"
        accept="image/*,.pdf"
        multiple
        className="hidden"
        onChange={e => {
          const files = e.target.files;
          if (files && files.length > 0) handleFile(files[0]);
          e.target.value = '';
        }}
      />

      {state === 'idle' && (
        <div
          onClick={() => fileRef.current?.click()}
          onDragOver={e => { e.preventDefault(); e.stopPropagation(); }}
          onDrop={e => { e.preventDefault(); const f = e.dataTransfer.files[0]; if (f) handleFile(f); }}
          className="border-2 border-dashed rounded-2xl p-5 text-center cursor-pointer hover:border-opacity-100 transition-colors"
          style={{ borderColor: accent + '66' }}
        >
          <div className="text-3xl mb-2">📸</div>
          <div className="text-sm font-semibold" style={{ color: accent }}>
            {label}
          </div>
          <div className="text-xs text-gray-400 mt-1">
            {docTypeName.toLowerCase().includes('license') || docTypeName.toLowerCase().includes('licence')
              ? 'Upload or take photos of both front and back'
              : 'Upload photo or PDF to auto-fill fields below'
            }
          </div>
        </div>
      )}

      {state === 'processing' && (
        <div className="border rounded-2xl p-5 text-center" style={{ borderColor: accent + '44', backgroundColor: accent + '08' }}>
          <div className="flex items-center justify-center gap-2">
            <span className="inline-block w-4 h-4 border-2 border-t-transparent rounded-full animate-spin" style={{ borderColor: accent, borderTopColor: 'transparent' }} />
            <span className="text-sm font-medium" style={{ color: accent }}>Analysing with AI…</span>
          </div>
          <div className="text-xs text-gray-400 mt-1">{fileName}</div>
        </div>
      )}

      {state === 'done' && extractedFields && (
        <div className="border border-green-300 bg-green-50/50 rounded-2xl p-4">
          <div className="flex items-center justify-between mb-2">
            <div className="flex items-center gap-2">
              <span className="text-sm">✅</span>
              <span className="text-xs font-semibold text-green-700">
                {Object.keys(extractedFields).length} fields extracted from {fileName}
              </span>
            </div>
            <button onClick={reset} className="text-xs font-medium hover:underline" style={{ color: accent }}>
              Re-scan
            </button>
          </div>
          <div className="grid grid-cols-2 gap-x-3 gap-y-1">
            {Object.entries(extractedFields).map(([key, { value, confidence }]) => (
              <div key={key} className="flex items-center gap-1.5 text-xs py-0.5">
                <AiConfidenceBadge confidence={confidence} />
                <span className="text-gray-500 truncate">{key}:</span>
                <span className="font-medium text-gray-900 truncate">{value}</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
