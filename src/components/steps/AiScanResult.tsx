import { CheckCircle } from 'lucide-react'

export interface AiFieldData {
  [key: string]: { value: string | null; confidence: string }
}

export function AiScanResult({ data, fileName, onRescan }: { data: AiFieldData; fileName: string; onRescan: () => void }) {
  const fieldCount = Object.values(data).filter(f => f.value).length
  if (fieldCount === 0) return null
  return (
    <div className="border-2 border-green-200 bg-green-50 rounded-xl p-4 mb-6">
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center gap-2">
          <CheckCircle size={18} className="text-green-600" />
          <span className="font-semibold text-sm text-green-800">{fieldCount} fields extracted from {fileName}</span>
        </div>
        <button onClick={onRescan} className="text-sm text-blue-600 hover:underline">Re-scan</button>
      </div>
      <div className="grid grid-cols-2 gap-2">
        {Object.entries(data).filter(([, v]) => v.value).map(([key, val]) => (
          <div key={key} className="flex items-center gap-2 text-sm">
            <span className={`text-xs px-1.5 py-0.5 rounded font-medium ${val.confidence === 'high' ? 'bg-green-100 text-green-700' : 'bg-orange-100 text-orange-700'}`}>
              AI {val.confidence === 'high' ? '✓' : '~'}
            </span>
            <span className="text-gray-600">{key}:</span>
            <span className="font-medium">{val.value}</span>
          </div>
        ))}
      </div>
    </div>
  )
}
