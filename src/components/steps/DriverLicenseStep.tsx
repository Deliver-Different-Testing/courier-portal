// @ts-nocheck
import { useState, useRef } from 'react'
import { CheckCircle, Loader2, RefreshCw, Camera } from 'lucide-react'
import { Field, NavButtons, StepWrapper } from './shared'
import { scanDocument } from '../../services/np_stepApi'

interface Props {
  title: string; description?: string; isRequired: boolean
  form: Record<string, any>; update: (field: string, value: string) => void
  files: Record<string, File | null>; onFile: (key: string, f: File) => void
  uploading: boolean
  onNext: () => void; onBack: () => void
  onUpload?: (f: File) => void
}

interface AiFieldValue { value: string | null; confidence: 'high' | 'medium' | 'low' }

export default function DriverLicenseStep({ title, description, isRequired, form, update, files, onFile, uploading, onNext, onBack, onUpload }: Props) {
  const [scanning, setScanning] = useState(false)
  const [scanResult, setScanResult] = useState<Record<string, AiFieldValue> | null>(null)
  const [scanFileName, setScanFileName] = useState('')
  const frontRef = useRef<HTMLInputElement>(null)

  const handleFrontScan = async (file: File) => {
    onFile('license_front', file)
    if (onUpload) onUpload(file)

    setScanFileName(file.name)
    setScanning(true)
    try {
      const result = await scanDocument(file, 'driver_license')
      if (result.success && result.fields) {
        const f = result.fields as Record<string, AiFieldValue>
        setScanResult(f)
        if (f.licenseNumber?.value) update('licenseNumber', f.licenseNumber.value)
        if (f.licenseExpiry?.value) update('licenseExpiry', f.licenseExpiry.value)
        if (f.licenseClass?.value) update('licenseClass', f.licenseClass.value)
        if (f.endorsements?.value) update('endorsements', f.endorsements.value)
      }
    } catch (e) {
      console.error('Scan failed:', e)
    } finally {
      setScanning(false)
    }
  }

  const fieldCount = scanResult ? Object.values(scanResult).filter(f => f.value).length : 0
  const frontFile = files['license_front']
  const backFile = files['license_back']
  const hasFront = !!frontFile

  return (
    <StepWrapper title={title} description={description} isRequired={isRequired}>
      <div className="space-y-5">
        {/* Front scan with AI */}
        {scanning ? (
          <div className="border-2 border-dashed border-[#3bc7f4]/40 rounded-xl p-8 text-center bg-blue-50">
            <Loader2 size={32} className="mx-auto mb-3 text-[#3bc7f4] animate-spin" />
            <p className="font-semibold text-sm text-[#3bc7f4]">Scanning license...</p>
            <p className="text-xs text-gray-500 mt-1">AI is extracting your license details</p>
          </div>
        ) : scanResult && frontFile ? (
          <div>
            {hasFront && (
              <div className="border-2 border-green-200 bg-green-50 rounded-xl p-4 mb-3">
                <div className="flex items-center gap-2 mb-2">
                  <CheckCircle size={18} className="text-green-600" />
                  <span className="text-sm font-medium text-green-800">Front already captured in Your Details</span>
                </div>
                <p className="text-sm text-green-700">Now scan the <strong>back</strong> of your license for classes and endorsements.</p>
              </div>
            )}
            {/* AI Results */}
            <div className="border-2 border-green-200 bg-green-50 rounded-xl p-4 mb-3">
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                  <CheckCircle size={18} className="text-green-600" />
                  <span className="font-semibold text-sm text-green-800">{fieldCount} fields extracted from {scanFileName}</span>
                </div>
                <button onClick={() => frontRef.current?.click()} className="flex items-center gap-1 text-sm text-blue-600 hover:underline">
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
            <input ref={frontRef} type="file" className="hidden" accept=".jpg,.jpeg,.png,.heic"
              onChange={e => { if (e.target.files?.[0]) handleFrontScan(e.target.files[0]) }} />
          </div>
        ) : (
          <label className="block border-2 border-dashed border-[#3bc7f4]/40 rounded-xl p-8 text-center cursor-pointer hover:border-[#FFD200] hover:bg-yellow-50 transition-colors">
            <Camera size={32} className="mx-auto mb-3 text-[#3bc7f4]" />
            <p className="font-semibold text-sm text-[#3bc7f4]">📷 Scan front of license</p>
            <p className="text-xs text-gray-500 mt-1">Upload or take a photo</p>
            <input type="file" className="hidden" accept=".jpg,.jpeg,.png,.heic" capture="environment"
              onChange={e => { if (e.target.files?.[0]) handleFrontScan(e.target.files[0]) }} />
          </label>
        )}

        {/* Back upload (no AI scan, just upload) */}
        <div>
          {backFile ? (
            <div className="border-2 border-green-200 bg-green-50 rounded-xl p-4 flex items-center gap-3">
              <CheckCircle size={18} className="text-green-600 shrink-0" />
              <div className="min-w-0 flex-1">
                <p className="font-medium text-sm text-green-800 truncate">License back: {backFile.name}</p>
              </div>
              <label className="text-xs text-green-700 underline cursor-pointer">
                Replace
                <input type="file" className="hidden" accept=".jpg,.jpeg,.png,.heic"
                  onChange={e => { if (e.target.files?.[0]) onFile('license_back', e.target.files[0]) }} />
              </label>
            </div>
          ) : (
            <label className="block border-2 border-dashed border-gray-300 rounded-xl p-6 text-center cursor-pointer hover:border-[#FFD200] hover:bg-yellow-50 transition-colors">
              <Camera size={24} className="mx-auto mb-2 text-gray-400" />
              <p className="font-semibold text-sm text-gray-500">📷 Scan back of license (classes & endorsements)</p>
              <p className="text-xs text-gray-400 mt-1">Upload or take a photo</p>
              <input type="file" className="hidden" accept=".jpg,.jpeg,.png,.heic" capture="environment"
                onChange={e => { if (e.target.files?.[0]) onFile('license_back', e.target.files[0]) }} />
            </label>
          )}
        </div>

        {/* Document Details fields with AI badges */}
        <p className="text-sm font-semibold text-gray-700 pt-2">Document Details</p>
        <AiField label="License Number *" placeholder="AB123456" value={form.licenseNumber || ''} onChange={v => update('licenseNumber', v)} aiStatus={scanResult?.licenseNumber} />
        <AiField label="Expiry Date *" type="date" value={form.licenseExpiry || ''} onChange={v => update('licenseExpiry', v)} aiStatus={scanResult?.licenseExpiry} />
        <AiField label="License Class" placeholder="Class 1 Full" value={form.licenseClass || ''} onChange={v => update('licenseClass', v)} aiStatus={scanResult?.licenseClass} />
        <AiField label="Endorsements" placeholder="e.g. P, D, I" value={form.endorsements || ''} onChange={v => update('endorsements', v)} aiStatus={scanResult?.endorsements} />
      </div>
      <NavButtons onBack={onBack} onNext={onNext}
        disabled={isRequired && (!form.licenseNumber || !form.licenseExpiry)}
        onSkip={!isRequired ? onNext : undefined} />
    </StepWrapper>
  )
}

function AiField({ label, placeholder, type = 'text', value, onChange, aiStatus }: {
  label: string; placeholder?: string; type?: string; value: string; onChange: (v: string) => void
  aiStatus?: { value: string | null; confidence: string } | null
}) {
  return (
    <div>
      <div className="flex items-center gap-2 mb-1.5">
        <label className="block text-xs font-bold text-gray-700 uppercase tracking-wide">{label}</label>
        {aiStatus?.value && (
          <span className={`text-xs px-1.5 py-0.5 rounded font-medium ${
            aiStatus.confidence === 'high' ? 'bg-green-100 text-green-700' : 'bg-orange-100 text-orange-700'
          }`}>
            AI {aiStatus.confidence === 'high' ? '✓' : '~'}
          </span>
        )}
      </div>
      <input className="w-full border border-gray-300 rounded-lg p-3 text-sm focus:ring-2 focus:ring-[#FFD200] focus:border-[#FFD200] outline-none"
        type={type} placeholder={placeholder} value={value} onChange={e => onChange(e.target.value)} />
    </div>
  )
}
