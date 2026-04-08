import { Field, NavButtons, StepWrapper, UploadZone } from './shared'

interface Props {
  form: Record<string, any>
  update: (field: string, value: string | boolean) => void
  onNext: () => void
  onBack: () => void
  title: string
  description?: string
  vehiclePhotos: Record<string, File | null>
  onVehiclePhoto: (key: string, file: File) => void
}

export default function VehicleStep({ form, update, onNext, onBack, title, description, vehiclePhotos, onVehiclePhoto }: Props) {
  return (
    <StepWrapper title={title} description={description}>
      <div className="space-y-5">
        <div>
          <label className="block text-xs font-bold text-gray-700 uppercase tracking-wide mb-1.5">Vehicle Type *</label>
          <select className="w-full border border-gray-300 rounded-lg p-3 text-sm focus:ring-2 focus:ring-[#FFD200] focus:border-[#FFD200] outline-none"
            value={form.vehicleType || ''} onChange={e => update('vehicleType', e.target.value)}>
            <option value="">Select vehicle type</option>
            <option value="car">Car / Sedan</option><option value="suv">SUV / Station Wagon</option>
            <option value="van">Van</option><option value="ute">Ute</option><option value="truck">Truck (Class 2+)</option>
            <option value="bike">Motorbike</option><option value="ebike">E-Bike / Bicycle</option>
            <option value="none">No vehicle — need company vehicle</option>
          </select>
        </div>
        <div className="grid grid-cols-3 gap-4">
          <Field label="Make" placeholder="Toyota" value={form.vehicleMake || ''} onChange={v => update('vehicleMake', v)} />
          <Field label="Model" placeholder="Hiace" value={form.vehicleModel || ''} onChange={v => update('vehicleModel', v)} />
          <Field label="Year" placeholder="2020" value={form.vehicleYear || ''} onChange={v => update('vehicleYear', v)} />
        </div>
        <Field label="Registration Plate" placeholder="ABC123" value={form.registrationPlate || ''} onChange={v => update('registrationPlate', v)} />
        <label className="flex items-center gap-3 p-4 bg-gray-50 rounded-xl cursor-pointer border border-gray-200">
          <input type="checkbox" checked={form.hasOwnVehicle || false} onChange={e => update('hasOwnVehicle', e.target.checked)} className="w-5 h-5 accent-[#FFD200]" />
          <div><span className="font-medium text-sm">I own this vehicle</span><p className="text-xs text-gray-500">Available for courier work</p></div>
        </label>
        <div>
          <label className="block text-xs font-bold text-gray-700 uppercase tracking-wide mb-1.5">Driver's License Class *</label>
          <select className="w-full border border-gray-300 rounded-lg p-3 text-sm focus:ring-2 focus:ring-[#FFD200] focus:border-[#FFD200] outline-none"
            value={form.licenseType || ''} onChange={e => update('licenseType', e.target.value)}>
            <option value="">Select license class</option>
            <option value="Class 1 - Full">Class 1 — Full (Car)</option><option value="Class 1 - Restricted">Class 1 — Restricted</option>
            <option value="Class 2 - Full">Class 2 — Full (Medium rigid)</option><option value="Class 2 - Restricted">Class 2 — Restricted</option>
            <option value="Class 3 - Full">Class 3 — Full</option><option value="Class 4 - Full">Class 4 — Full</option>
            <option value="Class 5 - Full">Class 5 — Full</option><option value="Overseas">Overseas License</option>
          </select>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <Field label="License Number" placeholder="AB123456" value={form.licenseNumber || ''} onChange={v => update('licenseNumber', v)} />
          <Field label="License Expiry *" type="date" value={form.licenseExpiry || ''} onChange={v => update('licenseExpiry', v)} />
        </div>
        <div>
          <h3 className="text-xs font-bold text-gray-700 uppercase tracking-wide mb-3">Vehicle Photos</h3>
          <div className="grid grid-cols-2 gap-3">
            {['Front', 'Side', 'Rear', 'Cargo Area'].map(label => (
              <UploadZone key={label} label={`📷 ${label}`} description={`Photo of vehicle ${label.toLowerCase()}`}
                file={vehiclePhotos[label] || null} uploading={false}
                onFile={f => onVehiclePhoto(label, f)} />
            ))}
          </div>
        </div>
      </div>
      <NavButtons onBack={onBack} onNext={onNext} disabled={!form.vehicleType || !form.licenseType} />
    </StepWrapper>
  )
}
