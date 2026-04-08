import { useEffect, useState } from 'react'
import * as api from '../../services/np_stepApi'
import { STEP_TYPES, getStepType } from '../../lib/stepTypes'
import { Save, Eye, Plus, Edit2, Trash2, ChevronRight, ChevronUp, ChevronDown, X } from 'lucide-react'

export default function FlowBuilder() {
  const [settings, setSettings] = useState<any>(null)
  const [steps, setSteps] = useState<api.PortalStep[]>([])
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(false)
  const [showAddModal, setShowAddModal] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [editForm, setEditForm] = useState({ title: '', description: '' })
  const [deleteConfirm, setDeleteConfirm] = useState<number | null>(null)

  useEffect(() => {
    api.getSettings().then(setSettings)
    api.getPortalSteps().then(setSteps)
  }, [])

  const handleSaveAll = async () => {
    setSaving(true); setSaved(false)
    try {
      // Save settings (branding)
      await api.updateSettings(settings)
      // Save step order
      await api.reorderPortalSteps(steps.map((s, i) => ({ id: s.id, sortOrder: i + 1 })))
      // Update each step
      for (const step of steps) {
        await api.updatePortalStep(step.id, step)
      }
      setSaved(true)
      setTimeout(() => setSaved(false), 3000)
    } catch { alert('Error saving') }
    finally { setSaving(false) }
  }

  const addStep = async (typeKey: string) => {
    const type = getStepType(typeKey)
    if (!type) return
    try {
      const newStep = await api.createPortalStep({
        stepType: typeKey,
        title: type.label,
        description: type.description,
        isRequired: true,
        isActive: true,
        sortOrder: steps.length + 1,
      })
      setSteps([...steps, newStep])
      setShowAddModal(false)
    } catch (e: any) { alert('Error adding step: ' + e.message) }
  }

  const moveStep = (index: number, direction: -1 | 1) => {
    const newSteps = [...steps]
    const target = index + direction
    if (target < 0 || target >= newSteps.length) return
    ;[newSteps[index], newSteps[target]] = [newSteps[target], newSteps[index]]
    setSteps(newSteps)
  }

  const toggleRequired = (id: number) => {
    setSteps(steps.map(s => s.id === id ? { ...s, isRequired: !s.isRequired } : s))
  }

  const toggleActive = (id: number) => {
    setSteps(steps.map(s => s.id === id ? { ...s, isActive: !s.isActive } : s))
  }

  const startEdit = (step: api.PortalStep) => {
    setEditingId(step.id)
    setEditForm({ title: step.title, description: step.description || '' })
  }

  const saveEdit = () => {
    setSteps(steps.map(s => s.id === editingId ? { ...s, title: editForm.title, description: editForm.description } : s))
    setEditingId(null)
  }

  const handleDelete = async (id: number) => {
    try {
      await api.deletePortalStep(id)
      setSteps(steps.filter(s => s.id !== id))
      setDeleteConfirm(null)
    } catch { alert('Error deleting step') }
  }

  if (!settings) return <div className="text-gray-400 flex items-center justify-center h-64">Loading...</div>

  const activeSteps = steps.filter(s => s.isActive)

  // Determine which singleton types are already in use
  const usedTypes = new Set(steps.map(s => s.stepType))

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-[#0d0c2c]">Flow Builder</h1>
          <p className="text-sm text-gray-500 mt-1">Design the applicant portal flow — drag steps to reorder</p>
        </div>
        <div className="flex items-center gap-3">
          <a href="/apply/form?preview=true" target="_blank" className="flex items-center gap-2 text-sm text-gray-600 border rounded-lg px-4 py-2 hover:bg-gray-50">
            <Eye size={16} /> Preview Portal
          </a>
          <button onClick={handleSaveAll} disabled={saving}
            className="flex items-center gap-2 bg-[#FFD200] text-[#0d0c2c] px-4 py-2 rounded-lg font-semibold hover:bg-[#E87C1E] hover:text-white disabled:opacity-50 transition-colors">
            <Save size={16} /> {saving ? 'Saving...' : saved ? 'Saved ✓' : 'Save Changes'}
          </button>
        </div>
      </div>

      {/* Branding */}
      <div className="bg-white rounded-xl p-6 shadow-sm mb-6">
        <h2 className="font-semibold mb-4">Branding</h2>
        <div className="grid grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Company Name</label>
            <input className="w-full border rounded-lg p-2.5 text-sm" value={settings.companyName || ''}
              onChange={e => setSettings((s: any) => ({ ...s, companyName: e.target.value }))} />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Welcome Title</label>
            <input className="w-full border rounded-lg p-2.5 text-sm" value={settings.welcomeTitle || ''}
              onChange={e => setSettings((s: any) => ({ ...s, welcomeTitle: e.target.value }))} />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Welcome Subtitle</label>
            <input className="w-full border rounded-lg p-2.5 text-sm" value={settings.welcomeSubtitle || ''}
              onChange={e => setSettings((s: any) => ({ ...s, welcomeSubtitle: e.target.value }))} />
          </div>
        </div>
      </div>

      {/* Flow Preview */}
      <div className="bg-[#0d0c2c] rounded-xl p-5 mb-6">
        <h3 className="text-xs font-bold text-gray-400 uppercase tracking-wide mb-3">Applicant Flow Preview</h3>
        <div className="flex items-center flex-wrap gap-1">
          {activeSteps.map((s, i) => (
            <div key={s.id} className="flex items-center">
              <div className="flex items-center gap-1.5">
                <span className="w-6 h-6 rounded-full bg-[#FFD200] text-[#0d0c2c] flex items-center justify-center text-xs font-bold">{i + 1}</span>
                <span className="text-sm text-white">{s.title}</span>
              </div>
              {i < activeSteps.length - 1 && <ChevronRight size={14} className="text-gray-500 mx-1" />}
            </div>
          ))}
        </div>
      </div>

      {/* Steps List */}
      <div className="space-y-3 mb-6">
        {steps.map((step, index) => {
          const type = getStepType(step.stepType)
          const isEditing = editingId === step.id
          return (
            <div key={step.id} className={`bg-white rounded-xl shadow-sm border ${!step.isActive ? 'opacity-60' : ''}`}>
              <div className="flex items-center gap-4 p-4">
                {/* Reorder */}
                <div className="flex flex-col gap-0.5">
                  <button onClick={() => moveStep(index, -1)} disabled={index === 0} className="text-gray-400 hover:text-gray-600 disabled:opacity-30"><ChevronUp size={16} /></button>
                  <button onClick={() => moveStep(index, 1)} disabled={index === steps.length - 1} className="text-gray-400 hover:text-gray-600 disabled:opacity-30"><ChevronDown size={16} /></button>
                </div>

                {/* Step number */}
                <span className="w-8 h-8 rounded-full bg-[#FFD200] text-[#0d0c2c] flex items-center justify-center text-sm font-bold shrink-0">
                  {step.isActive ? activeSteps.indexOf(step) + 1 || '—' : '—'}
                </span>

                {/* Icon + Title */}
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <span className="text-lg">{type?.icon || '📄'}</span>
                    <span className="font-semibold text-[#0d0c2c]">{step.title}</span>
                    <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">{step.stepType}</span>
                  </div>
                  {step.description && <p className="text-xs text-gray-500 mt-0.5 truncate">{step.description}</p>}
                </div>

                {/* Required toggle */}
                <div className="flex items-center gap-2">
                  <span className="text-xs text-gray-500">{step.isRequired ? 'Required' : 'Optional'}</span>
                  <button onClick={() => toggleRequired(step.id)}
                    className={`w-10 h-6 rounded-full transition-colors relative ${step.isRequired ? 'bg-red-400' : 'bg-gray-300'}`}>
                    <span className={`absolute top-0.5 w-5 h-5 bg-white rounded-full shadow transition-transform ${step.isRequired ? 'left-4' : 'left-0.5'}`} />
                  </button>
                </div>

                {/* Active toggle */}
                <div className="flex items-center gap-2">
                  <span className="text-xs text-gray-500">{step.isActive ? 'Active' : 'Hidden'}</span>
                  <button onClick={() => toggleActive(step.id)}
                    className={`w-10 h-6 rounded-full transition-colors relative ${step.isActive ? 'bg-green-500' : 'bg-gray-300'}`}>
                    <span className={`absolute top-0.5 w-5 h-5 bg-white rounded-full shadow transition-transform ${step.isActive ? 'left-4' : 'left-0.5'}`} />
                  </button>
                </div>

                {/* Edit */}
                <button onClick={() => isEditing ? setEditingId(null) : startEdit(step)} className="text-gray-400 hover:text-[#0d0c2c] p-1"><Edit2 size={16} /></button>

                {/* Delete */}
                {deleteConfirm === step.id ? (
                  <div className="flex items-center gap-1">
                    <button onClick={() => handleDelete(step.id)} className="text-xs bg-red-500 text-white px-2 py-1 rounded">Delete</button>
                    <button onClick={() => setDeleteConfirm(null)} className="text-xs text-gray-500 px-2 py-1">Cancel</button>
                  </div>
                ) : (
                  <button onClick={() => setDeleteConfirm(step.id)} className="text-gray-400 hover:text-red-500 p-1"><Trash2 size={16} /></button>
                )}
              </div>

              {/* Edit panel */}
              {isEditing && (
                <div className="border-t p-4 bg-yellow-50">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-xs font-medium text-gray-700 mb-1">Title</label>
                      <input className="w-full border rounded-lg p-2.5 text-sm" value={editForm.title}
                        onChange={e => setEditForm(f => ({ ...f, title: e.target.value }))} />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-700 mb-1">Description</label>
                      <input className="w-full border rounded-lg p-2.5 text-sm" value={editForm.description}
                        onChange={e => setEditForm(f => ({ ...f, description: e.target.value }))} />
                    </div>
                  </div>
                  <button onClick={saveEdit} className="mt-3 bg-[#0d0c2c] text-white px-4 py-2 rounded-lg text-sm font-semibold">Apply</button>
                </div>
              )}
            </div>
          )
        })}
      </div>

      {/* Add Step Button */}
      <button onClick={() => setShowAddModal(true)}
        className="w-full border-2 border-dashed border-gray-300 rounded-xl p-4 text-gray-500 hover:border-[#FFD200] hover:text-[#0d0c2c] transition-colors flex items-center justify-center gap-2 font-medium">
        <Plus size={20} /> Add Step
      </button>

      {/* Add Step Modal */}
      {showAddModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-2xl p-6 w-[600px] max-h-[80vh] overflow-y-auto shadow-2xl">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-bold text-[#0d0c2c]">Add Step</h2>
              <button onClick={() => setShowAddModal(false)} className="text-gray-400 hover:text-gray-600"><X size={20} /></button>
            </div>
            <div className="grid grid-cols-2 gap-3">
              {STEP_TYPES.map(type => {
                const disabled = type.singleton && usedTypes.has(type.key)
                return (
                  <button key={type.key} onClick={() => !disabled && addStep(type.key)} disabled={disabled}
                    className={`text-left p-4 rounded-xl border transition-colors ${disabled ? 'opacity-40 cursor-not-allowed bg-gray-50' : 'hover:border-[#FFD200] hover:bg-yellow-50 cursor-pointer'}`}>
                    <div className="flex items-center gap-2 mb-1">
                      <span className="text-lg">{type.icon}</span>
                      <span className="font-semibold text-sm text-[#0d0c2c]">{type.label}</span>
                    </div>
                    <p className="text-xs text-gray-500">{type.description}</p>
                    {disabled && <p className="text-xs text-orange-500 mt-1">Already added</p>}
                  </button>
                )
              })}
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
