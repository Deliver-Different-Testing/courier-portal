import { CheckCircle, Clock } from 'lucide-react'
import type { PortalStep } from '../../services/np_stepApi'

interface Props {
  form: Record<string, any>
  steps: PortalStep[]
  files: Record<string, File | null>
  uploadedDocs: Set<number>
  stepFieldData: Record<number, Record<string, any>>
  onBack: () => void
  onSubmit: () => void
  title: string
  description?: string
  isPreview?: boolean
}

function ReviewSection({ title, children }: { title: string; children: React.ReactNode }) {
  return <div className="mb-6"><h3 className="text-xs font-bold text-gray-500 uppercase tracking-wide mb-3 border-b pb-2">{title}</h3><div className="space-y-1.5">{children}</div></div>
}

function ReviewRow({ label, value }: { label: string; value: string }) {
  return <div className="flex justify-between text-sm"><span className="text-gray-500">{label}</span><span className="font-medium text-[#0d0c2c]">{value || '—'}</span></div>
}

export default function ReviewStep({ form, steps, files, uploadedDocs, stepFieldData, onBack, onSubmit, title, description }: Props) {
  return (
    <div className="bg-white rounded-2xl p-8 shadow-sm border border-gray-100">
      <h2 className="text-2xl font-bold text-[#0d0c2c] mb-1">{title}</h2>
      {description && <p className="text-gray-500 mb-6">{description}</p>}

      <ReviewSection title="Personal Details">
        <ReviewRow label="Name" value={`${form.firstName || ''} ${form.lastName || ''}`} />
        <ReviewRow label="Email" value={form.email} />
        <ReviewRow label="Phone" value={form.phone} />
        {form.dateOfBirth && <ReviewRow label="Date of Birth" value={form.dateOfBirth} />}
        {form.address && <ReviewRow label="Address" value={[form.address, form.city, form.region, form.postcode].filter(Boolean).join(', ')} />}
      </ReviewSection>

      {steps.some(s => s.stepType === 'vehicle') && (
        <ReviewSection title="Vehicle & License">
          <ReviewRow label="Vehicle" value={[form.vehicleMake, form.vehicleModel, form.vehicleYear].filter(Boolean).join(' ') || form.vehicleType || '—'} />
          <ReviewRow label="Registration" value={form.registrationPlate || '—'} />
          <ReviewRow label="Own Vehicle" value={form.hasOwnVehicle ? 'Yes' : 'No'} />
          <ReviewRow label="License" value={form.licenseType || '—'} />
          {form.licenseNumber && <ReviewRow label="License #" value={form.licenseNumber} />}
          {form.licenseExpiry && <ReviewRow label="Expiry" value={form.licenseExpiry} />}
        </ReviewSection>
      )}

      <ReviewSection title="Documents & Steps">
        {steps.filter(s => !['details', 'vehicle', 'review', 'quiz'].includes(s.stepType)).map(s => {
          const hasFile = s.documentTypeId ? (uploadedDocs.has(s.documentTypeId) || !!files[`doc-${s.documentTypeId}`]) : !!files[`step-${s.id}`]
          const fieldData = stepFieldData[s.id]
          return (
            <div key={s.id} className="py-1.5">
              <div className="flex items-center justify-between text-sm">
                <span className="text-gray-600">{s.title} {s.isRequired && <span className="text-red-400 text-xs">*</span>}</span>
                {hasFile ? (
                  <span className="flex items-center gap-1 text-green-600 text-xs font-medium"><CheckCircle size={14} /> Uploaded</span>
                ) : (
                  <span className="flex items-center gap-1 text-gray-400 text-xs"><Clock size={14} /> {s.isRequired ? 'Missing' : 'Skipped'}</span>
                )}
              </div>
              {fieldData && Object.entries(fieldData).length > 0 && (
                <div className="ml-4 mt-1 space-y-0.5">
                  {Object.entries(fieldData).map(([k, v]) => (
                    <div key={k} className="text-xs text-gray-500">{k}: <span className="font-medium">{String(v)}</span></div>
                  ))}
                </div>
              )}
            </div>
          )
        })}
      </ReviewSection>

      <div className="flex gap-4 mt-8">
        <button onClick={onBack} className="flex-1 border border-gray-300 py-3.5 rounded-xl font-medium hover:bg-gray-50">← Back</button>
        <button onClick={onSubmit}
          className="flex-1 bg-[#FFD200] text-[#0d0c2c] py-3.5 rounded-xl font-bold text-lg hover:bg-[#E87C1E] hover:text-white transition-colors">
          Submit Application ✓
        </button>
      </div>
    </div>
  )
}
