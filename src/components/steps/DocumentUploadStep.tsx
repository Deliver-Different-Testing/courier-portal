// @ts-nocheck
import { useState, useRef } from 'react'
import { CheckCircle, Loader2, RefreshCw, Camera } from 'lucide-react'
import { Field, NavButtons, StepWrapper } from './shared'
import { scanDocument } from '../../services/np_stepApi'

interface Props {
  stepType: string; title: string; description?: string; isRequired: boolean
  file: File | null; uploading: boolean; uploaded: boolean
  onFile: (f: File) => void
  fields: Record<string, any>; updateField: (key: string, value: string) => void
  onNext: () => void; onBack: () => void
}

interface AiFieldValue { value: string | null; confidence: 'high' | 'medium' | 'low' }

const STEP_FIELDS: Record<string, { key: string; label: string; type?: string; placeholder?: string; required?: boolean }[]> = {
  vehicle_registration: [
    { key: 'plateNumber', label: 'Plate Number *', placeholder: 'ABC123', required: true },
    { key: 'regoExpiry', label: 'Registration Expiry *', type: 'date', required: true },
  ],
  vehicle_insurance: [
    { key: 'policyNumber', label: 'Policy Number *', placeholder: 'POL-12345', required: true },
    { key: 'insurerName', label: 'Insurer Name *', placeholder: 'e.g. AA Insurance', required: true },
    { key: 'coverageAmount', label: 'Coverage Amount', placeholder: '$10,000' },
    { key: 'expiryDate', label: 'Expiry Date *', type: 'date', required: true },
  ],
  wof_certificate: [
    { key: 'wofExpiry', label: 'WOF Expiry *', type: 'date', required: true },
  ],
  tsl_certificate: [
    { key: 'tslNumber', label: 'TSL Number', placeholder: 'TSL-12345' },
    { key: 'tslExpiry', label: 'TSL Expiry *', type: 'date', required: true },
  ],
}

// Step types that support AI scanning
const SCAN_ENABLED = ['vehicle_registration', 'vehicle_insurance', 'wof_certificate', 'tsl_certificate']

export default function DocumentUploadStep({ stepType, title, description, isRequired, file, uploading, uploaded, onFile, fields, updateField, onNext, onBack }: Props) {
  const [scanning, setScanning] = useState(false)
  const [scanResult, setScanResult] = useState<Record<string, AiFieldValue> | null>(null)
  const [scanFileName, setScanFileName] = useState('')
  const inputRef = useRef<HTMLInputElement>(null)

  const canScan = SCAN_ENABLED.includes(stepType)
  const stepFields = STEP_FIELDS[stepType] || []
  const requiredFieldsFilled = stepFields.filter(f => f.required).every(f => fields[f.key])
  const canProceed = isRequired ? (uploaded || !!file) && (stepFields.length === 0 || requiredFieldsFilled) : true

  const handleFileWithScan = async (f: File) => {
    onFile(f) // trigger upload
    if (!canScan) return

    setScanFileName(f.name)
    setScanning(true)
    try {
      const result = await scanDocument(f, stepType)
      if (result.success && result.fields) {
        const aiFields = result.fields as Record<string, AiFieldValue>
        setScanResult(aiFields)
        // Auto-populate form fields
        for (const sf of stepFields) {
          const aiVal = aiFields[sf.key]
          if (aiVal?.value) updateField(sf.key, aiVal.value)
        }
      }
    } catch (e) {
      console.error('Scan failed:', e)
    } finally {
      setScanning(false)
    }
  }

  const fieldCount = scanResult ? Object.values(scanResult).filter(f => f.value).length : 0

  return (
    <StepWrapper title={title} description={description} isRequired={isRequired}>
      <div className="space-y-5">
        {/* Upload + AI Scan Zone */}
        {scanning ? (
          <div className="border-2 border-dashed border-[#3bc7f4]/40 rounded-xl p-8 text-center bg-blue-50">
            <Loader2 size={32} className="mx-auto mb-3 text-[#3bc7f4] animate-spin" />
            <p className="font-semibold text-sm text-[#3bc7f4]">Scanning document...</p>
            <p className="text-xs text-gray-500 mt-1">AI is extracting details from your upload</p>
          </div>
        ) : scanResult && file ? (
          <div>
            {/* File confirmation */}
            <div className="border-2 border-green-200 bg-green-50 rounded-xl p-5 flex items-center gap-3 mb-3">
              <CheckCircle size={24} className="text-green-600 shrink-0" />
              <div className="min-w-0 flex-1">
                <p className="font-medium text-sm text-green-800 truncate">{file.name}</p>
                <p className="text-xs text-green-600">{(file.size / 1024).toFixed(0)} KB</p>
              </div>
              <button onClick={() => inputRef.current?.click()} className="text-xs text-green-700 underline">Replace</button>
              <input ref={inputRef} type="file" className="hidden" accept=".pdf,.jpg,.jpeg,.png,.heic"
                onChange={e => { if (e.target.files?.[0]) handleFileWithScan(e.target.files[0]) }} />
            </div>
            {/* AI Results */}
            <div className="border-2 border-green-200 bg-green-50 rounded-xl p-4">
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                  <CheckCircle size={18} className="text-green-600" />
                  <span className="font-semibold text-sm text-green-800">{fieldCount} fields extracted from {scanFileName}</span>
                </div>
                <button onClick={() => inputRef.current?.click()} className="flex items-center gap-1 text-sm text-blue-600 hover:underline">
                  <RefreshCw size={14} /> Re-scan
                </button>
              </div>
              <div className="grid grid-cols-2 gap-2">
                {Object.entries(scanResult).filter(([, v]) => v.value).map(([key, val]) => (
                  <div key={key} className="flex items-center gap-2 text-sm">
                    <span className={`text-xs px-1.5 py-0.5 rounded font-medium ${
                      val.confidence === 'high' ? 'bg-green-100 text-green-700' : 'bg-orange-100 text-orange-700'
                    }`}>
                      AI {val.confidence === 'high' ? '✓' : '~'}
                    </span>
                    <span className="text-gray-600">{key}:</span>
                    <span className="font-medium">{val.value}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        ) : file ? (
          <div className="border-2 border-green-200 bg-green-50 rounded-xl p-5 flex items-center gap-3">
            <CheckCircle size={24} className="text-green-600 shrink-0" />
            <div className="min-w-0 flex-1">
              <p className="font-medium text-sm text-green-800 truncate">{file.name}</p>
              <p className="text-xs text-green-600">{(file.size / 1024).toFixed(0)} KB</p>
            </div>
            <button onClick={() => inputRef.current?.click()} className="text-xs text-green-700 underline">Replace</button>
            <input ref={inputRef} type="file" className="hidden" accept=".pdf,.jpg,.jpeg,.png,.heic"
              onChange={e => { if (e.target.files?.[0]) handleFileWithScan(e.target.files[0]) }} />
          </div>
        ) : (
          <label className={`block border-2 border-dashed rounded-xl p-8 text-center cursor-pointer transition-colors
            ${uploading ? 'border-gray-300 bg-gray-50' : 'border-[#3bc7f4]/40 hover:border-[#FFD200] hover:bg-yellow-50'}`}>
            {uploading ? (
              <div className="flex items-center justify-center gap-2 text-gray-500"><Loader2 size={20} className="animate-spin" /> Uploading...</div>
            ) : (
              <>
                <Camera size={32} className="mx-auto mb-3 text-[#3bc7f4]" />
                <p className="font-semibold text-sm text-[#3bc7f4]">📷 Upload {title}</p>
                <p className="text-xs text-gray-500 mt-1">Take a photo or upload a file</p>
              </>
            )}
            <input type="file" className="hidden" accept=".pdf,.jpg,.jpeg,.png,.heic" capture="environment"
              onChange={e => { if (e.target.files?.[0]) handleFileWithScan(e.target.files[0]) }} />
          </label>
        )}

        {/* Document-specific fields with AI badges */}
        <p className="text-sm font-semibold text-gray-700 pt-2">Document Details</p>
        {stepFields.map(f => (
          <div key={f.key}>
            <div className="flex items-center gap-2 mb-1.5">
              <label className="block text-xs font-bold text-gray-700 uppercase tracking-wide">{f.label}</label>
              {scanResult?.[f.key]?.value && (
                <span className={`text-xs px-1.5 py-0.5 rounded font-medium ${
                  scanResult[f.key].confidence === 'high' ? 'bg-green-100 text-green-700' : 'bg-orange-100 text-orange-700'
                }`}>
                  AI {scanResult[f.key].confidence === 'high' ? '✓' : '~'}
                </span>
              )}
            </div>
            <input className="w-full border border-gray-300 rounded-lg p-3 text-sm focus:ring-2 focus:ring-[#FFD200] focus:border-[#FFD200] outline-none"
              type={f.type || 'text'} placeholder={f.placeholder} value={fields[f.key] || ''} onChange={e => updateField(f.key, e.target.value)} />
          </div>
        ))}
      </div>
      <NavButtons onBack={onBack} onNext={onNext} disabled={!canProceed}
        onSkip={!isRequired ? onNext : undefined} />
    </StepWrapper>
  )
}
