import { useRef } from 'react'
import { CheckCircle, Camera, ChevronRight, Loader2 } from 'lucide-react'

export function Field({ label, placeholder, type = 'text', value, onChange }: {
  label: string; placeholder?: string; type?: string; value: string; onChange: (v: string) => void
}) {
  return (
    <div>
      <label className="block text-xs font-bold text-gray-700 uppercase tracking-wide mb-1.5">{label}</label>
      <input className="w-full border border-gray-300 rounded-lg p-3 text-sm focus:ring-2 focus:ring-[#FFD200] focus:border-[#FFD200] outline-none"
        type={type} placeholder={placeholder} value={value} onChange={e => onChange(e.target.value)} />
    </div>
  )
}

export function NavButtons({ onBack, onNext, disabled, nextLabel, onSkip }: {
  onBack?: () => void; onNext: () => void; disabled?: boolean; nextLabel?: string; onSkip?: () => void
}) {
  return (
    <div className="flex gap-4 mt-8">
      {onBack && <button onClick={onBack} className="flex-1 border border-gray-300 py-3.5 rounded-xl font-medium hover:bg-gray-50 transition-colors">← Back</button>}
      {onSkip && (
        <button onClick={onSkip} className="flex-1 bg-gray-100 text-gray-700 py-3.5 rounded-xl font-medium hover:bg-gray-200 transition-colors">
          Skip this document →
        </button>
      )}
      <button onClick={onNext} disabled={disabled}
        className="flex-1 bg-[#0d0c2c] text-white py-3.5 rounded-xl font-semibold hover:bg-[#1a1950] disabled:opacity-50 transition-colors flex items-center justify-center gap-2">
        {nextLabel || <>Next <ChevronRight size={18} /></>}
      </button>
    </div>
  )
}

export function UploadZone({ label, description, file, uploading, onFile }: {
  label: string; description?: string; file: File | null; uploading: boolean; onFile: (f: File) => void
}) {
  const inputRef = useRef<HTMLInputElement>(null)
  return (
    <div>
      {file ? (
        <div className="border-2 border-green-200 bg-green-50 rounded-xl p-5 flex items-center gap-3">
          <CheckCircle size={24} className="text-green-600 shrink-0" />
          <div className="min-w-0 flex-1">
            <p className="font-medium text-sm text-green-800 truncate">{file.name}</p>
            <p className="text-xs text-green-600">{(file.size / 1024).toFixed(0)} KB</p>
          </div>
          <button onClick={() => inputRef.current?.click()} className="text-xs text-green-700 underline">Replace</button>
          <input ref={inputRef} type="file" className="hidden" accept=".pdf,.jpg,.jpeg,.png,.doc,.docx,.heic"
            onChange={e => { if (e.target.files?.[0]) onFile(e.target.files[0]) }} />
        </div>
      ) : (
        <label className={`block border-2 border-dashed rounded-xl p-8 text-center cursor-pointer transition-colors
          ${uploading ? 'border-gray-300 bg-gray-50' : 'border-[#3bc7f4]/40 hover:border-[#FFD200] hover:bg-yellow-50'}`}>
          {uploading ? (
            <div className="flex items-center justify-center gap-2 text-gray-500"><Loader2 size={20} className="animate-spin" /> Uploading...</div>
          ) : (
            <>
              <Camera size={32} className="mx-auto mb-3 text-[#3bc7f4]" />
              <p className="font-semibold text-sm text-[#3bc7f4]">{label}</p>
              {description && <p className="text-xs text-gray-500 mt-1">{description}</p>}
            </>
          )}
          <input type="file" className="hidden" accept=".pdf,.jpg,.jpeg,.png,.doc,.docx,.heic" capture="environment"
            onChange={e => { if (e.target.files?.[0]) onFile(e.target.files[0]) }} />
        </label>
      )}
    </div>
  )
}

export function StepWrapper({ title, description, isRequired, children }: {
  title: string; description?: string; isRequired?: boolean; children: React.ReactNode
}) {
  return (
    <div className="bg-white rounded-2xl p-8 shadow-sm border border-gray-100">
      <div className="flex items-center gap-2 mb-1">
        <h2 className="text-2xl font-bold text-[#0d0c2c]">{title}</h2>
        {isRequired !== undefined && (
          <span className={`text-xs font-semibold px-2 py-0.5 rounded-full ${isRequired ? 'text-red-500 bg-red-50' : 'text-gray-500 bg-gray-100'}`}>
            {isRequired ? 'Required' : 'Optional'}
          </span>
        )}
      </div>
      {description && <p className="text-gray-500 mb-6">{description}</p>}
      {children}
    </div>
  )
}
