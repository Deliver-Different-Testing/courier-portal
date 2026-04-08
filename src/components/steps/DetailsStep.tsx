import { useState, useRef } from 'react'
import { Camera, CheckCircle, Loader2, RefreshCw } from 'lucide-react'
import { Field, NavButtons, StepWrapper } from './shared'
import { scanDocument } from '../../services/np_stepApi'

interface Props {
  form: Record<string, any>
  update: (field: string, value: string | boolean) => void
  onNext: () => void
  saving: boolean
  title: string
  description?: string
}

interface AiField { value: string | null; confidence: 'high' | 'medium' | 'low' }

export default function DetailsStep({ form, update, onNext, saving, title, description }: Props) {
  const [scanning, setScanning] = useState(false)
  const [scanResult, setScanResult] = useState<Record<string, AiField> | null>(null)
  const [scanFileName, setScanFileName] = useState('')
  const [scanFile, setScanFile] = useState<File | null>(null)
  const inputRef = useRef<HTMLInputElement>(null)

  const handleScan = async (file: File) => {
    setScanFile(file)
    setScanFileName(file.name)
    setScanning(true)
    try {
      const result = await scanDocument(file, 'driver_license')
      if (result.success && result.fields) {
        setScanResult(result.fields)
        // Auto-populate form fields from AI extraction
        const f = result.fields as Record<string, AiField>
        if (f.firstName?.value) update('firstName', f.firstName.value)
        if (f.lastName?.value) update('lastName', f.lastName.value)
        if (f.dateOfBirth?.value) update('dateOfBirth', f.dateOfBirth.value)
        if (f.address?.value) update('address', f.address.value)
        // Try to extract city from address
        if (f.city?.value) update('city', f.city.value)
        if (f.licenseNumber?.value) update('licenseNumber', f.licenseNumber.value)
        if (f.licenseExpiry?.value) update('licenseExpiry', f.licenseExpiry.value)
      }
    } catch (e) {
      console.error('Scan failed:', e)
    } finally {
      setScanning(false)
    }
  }

  const fieldCount = scanResult ? Object.values(scanResult).filter(f => f.value).length : 0

  return (
    <StepWrapper title={title} description={description || "Scan your driver's license to auto-fill, or enter details manually."}>
      {/* License Scan Zone */}
      <div className="mb-6">
        {scanning ? (
          <div className="border-2 border-dashed border-[#3bc7f4]/40 rounded-xl p-8 text-center bg-blue-50">
            <Loader2 size={32} className="mx-auto mb-3 text-[#3bc7f4] animate-spin" />
            <p className="font-semibold text-sm text-[#3bc7f4]">Scanning document...</p>
            <p className="text-xs text-gray-500 mt-1">AI is extracting your details</p>
          </div>
        ) : scanResult ? (
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
            <input ref={inputRef} type="file" className="hidden" accept=".jpg,.jpeg,.png,.heic"
              onChange={e => { if (e.target.files?.[0]) handleScan(e.target.files[0]) }} />
          </div>
        ) : (
          <label className="block border-2 border-dashed border-[#3bc7f4]/40 rounded-xl p-8 text-center cursor-pointer hover:border-[#FFD200] hover:bg-yellow-50 transition-colors">
            <Camera size={32} className="mx-auto mb-3 text-[#3bc7f4]" />
            <p className="font-semibold text-sm text-[#3bc7f4]">📷 Scan Driver's License (front & back)</p>
            <p className="text-xs text-gray-500 mt-1">Upload or take photos of both front and back</p>
            <input type="file" className="hidden" accept=".jpg,.jpeg,.png,.heic" capture="environment"
              onChange={e => { if (e.target.files?.[0]) handleScan(e.target.files[0]) }} />
          </label>
        )}
      </div>

      <div className="space-y-5">
        <div className="grid grid-cols-2 gap-4">
          <AiField label="First Name *" placeholder="John" value={form.firstName || ''} onChange={v => update('firstName', v)}
            aiStatus={scanResult?.firstName} />
          <AiField label="Last Name *" placeholder="Smith" value={form.lastName || ''} onChange={v => update('lastName', v)}
            aiStatus={scanResult?.lastName} />
        </div>
        <Field label="Email *" placeholder="john@example.com" type="email" value={form.email || ''} onChange={v => update('email', v)} />
        <Field label="Mobile Number *" placeholder="+64 21 123 4567" value={form.phone || ''} onChange={v => update('phone', v)} />
        <AiField label="Date of Birth" type="date" value={form.dateOfBirth || ''} onChange={v => update('dateOfBirth', v)}
          aiStatus={scanResult?.dateOfBirth} />
        <AiField label="Address" placeholder="Street address" value={form.address || ''} onChange={v => update('address', v)}
          aiStatus={scanResult?.address} />
        <div className="grid grid-cols-3 gap-4">
          <Field label="City" placeholder="Auckland" value={form.city || ''} onChange={v => update('city', v)} />
          <div>
            <label className="block text-xs font-bold text-gray-700 uppercase tracking-wide mb-1.5">Region</label>
            <select className="w-full border border-gray-300 rounded-lg p-3 text-sm focus:ring-2 focus:ring-[#FFD200] focus:border-[#FFD200] outline-none"
              value={form.region || ''} onChange={e => update('region', e.target.value)}>
              <option value="">Select</option>
              {['Auckland','Wellington','Canterbury','Waikato','Bay of Plenty','Otago','Manawatu-Whanganui',"Hawke's Bay",'Northland','Taranaki','Southland','Nelson','Marlborough','West Coast','Gisborne','Tasman'].map(r =>
                <option key={r} value={r}>{r}</option>)}
            </select>
          </div>
          <Field label="Postcode" placeholder="1010" value={form.postcode || ''} onChange={v => update('postcode', v)} />
        </div>
        <div>
          <label className="block text-xs font-bold text-gray-700 uppercase tracking-wide mb-1.5">How did you hear about us?</label>
          <select className="w-full border border-gray-300 rounded-lg p-3 text-sm focus:ring-2 focus:ring-[#FFD200] focus:border-[#FFD200] outline-none"
            value={form.source || ''} onChange={e => update('source', e.target.value)}>
            <option value="">Select...</option>
            <option value="seek">Seek</option><option value="trademe">Trade Me Jobs</option><option value="website">Urgent Couriers Website</option>
            <option value="referral">Referral from existing driver</option><option value="facebook">Facebook</option>
            <option value="linkedin">LinkedIn</option><option value="walkin">Walk-in</option><option value="other">Other</option>
          </select>
        </div>
      </div>
      <NavButtons onNext={onNext} disabled={!form.firstName || !form.lastName || !form.email || !form.phone || saving} nextLabel={saving ? 'Saving...' : undefined} />
    </StepWrapper>
  )
}

// Field with AI badge indicator
function AiField({ label, placeholder, type = 'text', value, onChange, aiStatus }: {
  label: string; placeholder?: string; type?: string; value: string; onChange: (v: string) => void
  aiStatus?: AiField | null
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
