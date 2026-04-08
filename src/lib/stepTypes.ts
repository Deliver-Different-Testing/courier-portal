export const STEP_TYPES = [
  { key: 'details', label: 'Personal Details', icon: '📋', description: 'Name, email, phone, DOB, address', singleton: true },
  { key: 'vehicle', label: 'Vehicle Details', icon: '🚗', description: 'Vehicle type, make/model/year, registration plate, vehicle photos', singleton: true },
  { key: 'driver_license', label: "Driver's License", icon: '🪪', description: 'Front & back scan, license number, expiry, class, endorsements', singleton: true },
  { key: 'vehicle_registration', label: 'Vehicle Registration', icon: '📄', description: 'Upload + plate number, registration expiry', singleton: false },
  { key: 'vehicle_insurance', label: 'Vehicle Insurance', icon: '🛡️', description: 'Upload + policy number, insurer, coverage amount, expiry', singleton: false },
  { key: 'wof_certificate', label: 'WOF Certificate', icon: '📋', description: 'Upload + WOF expiry date', singleton: false },
  { key: 'tsl_certificate', label: 'TSL Certificate', icon: '📋', description: 'Upload + TSL expiry date', singleton: false },
  { key: 'generic_document', label: 'Document Upload', icon: '📎', description: 'Generic file upload step', singleton: false },
  { key: 'quiz', label: 'Assessment Quiz', icon: '📝', description: 'Knowledge assessment quiz', singleton: true },
  { key: 'review', label: 'Review & Submit', icon: '✅', description: 'Summary and submission', singleton: true },
]

export function getStepType(key: string) {
  return STEP_TYPES.find(s => s.key === key)
}
