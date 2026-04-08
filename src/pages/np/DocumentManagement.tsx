import { useEffect, useState } from 'react'
import * as api from '../lib/api'
import { useStore } from '../store'
import { Plus, Edit2, Save, FileCheck, CheckCircle, XCircle, Clock, Eye } from 'lucide-react'

type Tab = 'types' | 'review' | 'checklist'

export default function DocumentManagement() {
  const [tab, setTab] = useState<Tab>('types')

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-[#0d0c2c]">Document Management</h1>
      </div>
      <div className="flex gap-2 mb-6">
        {([['types', 'Document Types'], ['review', 'Review Queue'], ['checklist', 'Applicant Checklist']] as const).map(([id, label]) => (
          <button key={id} onClick={() => setTab(id)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${tab === id ? 'bg-[#3bc7f4] text-white' : 'bg-white hover:bg-gray-50'}`}>
            {label}
          </button>
        ))}
      </div>

      {tab === 'types' && <DocumentTypesTab />}
      {tab === 'review' && <ReviewQueueTab />}
      {tab === 'checklist' && <ChecklistTab />}
    </div>
  )
}

function DocumentTypesTab() {
  const documentTypes = useStore(s => s.documentTypes)
  const setDocumentTypes = useStore(s => s.setDocumentTypes)
  const [editing, setEditing] = useState<number | null>(null)
  const [showNew, setShowNew] = useState(false)
  const [form, setForm] = useState({ name: '', description: '', isRequired: false, appliesTo: 'Applicant', validityMonths: '', sortOrder: 0, isActive: true })

  useEffect(() => { api.getDocumentTypes().then(setDocumentTypes) }, [])

  const resetForm = () => setForm({ name: '', description: '', isRequired: false, appliesTo: 'Applicant', validityMonths: '', sortOrder: 0, isActive: true })

  const handleCreate = async () => {
    const result = await api.createDocumentType({ ...form, validityMonths: form.validityMonths ? parseInt(form.validityMonths) : undefined })
    setDocumentTypes([...documentTypes, result])
    setShowNew(false)
    resetForm()
  }

  const handleUpdate = async (id: number) => {
    const result = await api.updateDocumentType(id, { ...form, validityMonths: form.validityMonths ? parseInt(form.validityMonths) : undefined })
    setDocumentTypes(documentTypes.map(d => d.id === id ? result : d))
    setEditing(null)
    resetForm()
  }

  const startEdit = (dt: api.DocumentType) => {
    setForm({ name: dt.name, description: dt.description || '', isRequired: dt.isRequired, appliesTo: dt.appliesTo, validityMonths: dt.validityMonths?.toString() || '', sortOrder: dt.sortOrder, isActive: dt.isActive })
    setEditing(dt.id)
  }

  return (
    <div>
      <div className="flex justify-end mb-4">
        <button onClick={() => { setShowNew(true); resetForm() }} className="flex items-center gap-2 bg-[#3bc7f4] text-white px-4 py-2 rounded-lg">
          <Plus size={18} /> Add Type
        </button>
      </div>

      {(showNew || editing !== null) && (
        <div className="bg-white rounded-xl p-6 shadow-sm mb-6">
          <h3 className="font-semibold mb-4">{editing !== null ? 'Edit' : 'New'} Document Type</h3>
          <div className="grid grid-cols-2 gap-4">
            <input className="border rounded-lg p-3" placeholder="Name *" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} />
            <input className="border rounded-lg p-3" placeholder="Description" value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} />
            <select className="border rounded-lg p-3" value={form.appliesTo} onChange={e => setForm(f => ({ ...f, appliesTo: e.target.value }))}>
              <option value="Applicant">Applicant</option>
              <option value="ActiveCourier">Active Courier</option>
              <option value="Both">Both</option>
            </select>
            <input className="border rounded-lg p-3" placeholder="Validity (months)" type="number" value={form.validityMonths} onChange={e => setForm(f => ({ ...f, validityMonths: e.target.value }))} />
            <label className="flex items-center gap-2 p-3"><input type="checkbox" checked={form.isRequired} onChange={e => setForm(f => ({ ...f, isRequired: e.target.checked }))} /> Required</label>
            <label className="flex items-center gap-2 p-3"><input type="checkbox" checked={form.isActive} onChange={e => setForm(f => ({ ...f, isActive: e.target.checked }))} /> Active</label>
          </div>
          <div className="flex gap-2 mt-4">
            <button onClick={editing !== null ? () => handleUpdate(editing) : handleCreate} className="bg-[#3bc7f4] text-white px-6 py-2 rounded-lg flex items-center gap-2"><Save size={16} /> Save</button>
            <button onClick={() => { setEditing(null); setShowNew(false) }} className="border px-6 py-2 rounded-lg">Cancel</button>
          </div>
        </div>
      )}

      <div className="bg-white rounded-xl shadow-sm overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50 border-b">
            <tr>
              <th className="text-left p-4 text-sm font-semibold">Name</th>
              <th className="text-left p-4 text-sm font-semibold">Applies To</th>
              <th className="text-left p-4 text-sm font-semibold">Required</th>
              <th className="text-left p-4 text-sm font-semibold">Validity</th>
              <th className="text-left p-4 text-sm font-semibold">Status</th>
              <th className="text-left p-4 text-sm font-semibold">Actions</th>
            </tr>
          </thead>
          <tbody>
            {documentTypes.map(dt => (
              <tr key={dt.id} className="border-b">
                <td className="p-4 text-sm font-medium">{dt.name}</td>
                <td className="p-4 text-sm text-gray-600">{dt.appliesTo}</td>
                <td className="p-4 text-sm">{dt.isRequired ? '✅' : '—'}</td>
                <td className="p-4 text-sm text-gray-600">{dt.validityMonths ? `${dt.validityMonths} mo` : '—'}</td>
                <td className="p-4"><span className={`text-xs px-2 py-1 rounded-full ${dt.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'}`}>{dt.isActive ? 'Active' : 'Inactive'}</span></td>
                <td className="p-4"><button onClick={() => startEdit(dt)} className="text-[#3bc7f4] hover:underline text-sm"><Edit2 size={16} /></button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

function ReviewQueueTab() {
  const [applicants, setApplicants] = useState<api.Applicant[]>([])
  const [allDocs, setAllDocs] = useState<(api.ApplicantDocument & { applicantName: string })[]>([])
  const [filter, setFilter] = useState<'Pending' | 'Verified' | 'Rejected' | ''>('Pending')
  const [rejectModal, setRejectModal] = useState<{ docId: number; reason: string } | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api.getApplicants().then(async apps => {
      setApplicants(apps)
      const docs: (api.ApplicantDocument & { applicantName: string })[] = []
      for (const a of apps) {
        try {
          const aDocs = await api.getApplicantDocuments(a.id)
          aDocs.forEach(d => docs.push({ ...d, applicantName: `${a.firstName} ${a.lastName}` }))
        } catch {}
      }
      setAllDocs(docs)
      setLoading(false)
    })
  }, [])

  const filtered = filter ? allDocs.filter(d => d.status === filter) : allDocs

  const handleVerify = async (id: number, approved: boolean, reason?: string) => {
    await api.verifyDocument(id, approved, reason)
    setAllDocs(prev => prev.map(d => d.id === id ? { ...d, status: approved ? 'Verified' : 'Rejected', rejectionReason: reason } : d))
    setRejectModal(null)
  }

  if (loading) return <div className="text-gray-400 text-center py-8">Loading documents...</div>

  return (
    <div>
      <div className="flex gap-2 mb-4">
        {(['', 'Pending', 'Verified', 'Rejected'] as const).map(f => (
          <button key={f} onClick={() => setFilter(f)}
            className={`px-3 py-1.5 rounded-lg text-sm ${filter === f ? 'bg-[#3bc7f4] text-white' : 'bg-white'}`}>
            {f || 'All'} {f === 'Pending' && `(${allDocs.filter(d => d.status === 'Pending').length})`}
          </button>
        ))}
      </div>

      <div className="bg-white rounded-xl shadow-sm overflow-hidden">
        <table className="w-full">
          <thead className="bg-gray-50 border-b">
            <tr>
              <th className="text-left p-4 text-sm font-semibold">Applicant</th>
              <th className="text-left p-4 text-sm font-semibold">Document</th>
              <th className="text-left p-4 text-sm font-semibold">File</th>
              <th className="text-left p-4 text-sm font-semibold">Uploaded</th>
              <th className="text-left p-4 text-sm font-semibold">Status</th>
              <th className="text-left p-4 text-sm font-semibold">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filtered.map(d => (
              <tr key={d.id} className="border-b">
                <td className="p-4 text-sm font-medium">{d.applicantName}</td>
                <td className="p-4 text-sm text-gray-600">{d.documentType?.name || `Type #${d.documentTypeId}`}</td>
                <td className="p-4 text-sm text-gray-500">{d.fileName} <span className="text-xs">({(d.fileSize / 1024).toFixed(0)} KB)</span></td>
                <td className="p-4 text-sm text-gray-500">{new Date(d.uploadedDate).toLocaleDateString()}</td>
                <td className="p-4">
                  <span className={`text-xs px-2 py-1 rounded-full ${
                    d.status === 'Verified' ? 'bg-green-100 text-green-800' :
                    d.status === 'Rejected' ? 'bg-red-100 text-red-800' :
                    'bg-yellow-100 text-yellow-800'
                  }`}>{d.status}</span>
                </td>
                <td className="p-4">
                  {d.status === 'Pending' && (
                    <div className="flex gap-2">
                      <button onClick={() => handleVerify(d.id, true)}
                        className="text-green-600 hover:bg-green-50 p-1.5 rounded-lg" title="Approve">
                        <CheckCircle size={18} />
                      </button>
                      <button onClick={() => setRejectModal({ docId: d.id, reason: '' })}
                        className="text-red-600 hover:bg-red-50 p-1.5 rounded-lg" title="Reject">
                        <XCircle size={18} />
                      </button>
                    </div>
                  )}
                </td>
              </tr>
            ))}
            {filtered.length === 0 && (
              <tr><td colSpan={6} className="p-8 text-center text-gray-400">No documents found</td></tr>
            )}
          </tbody>
        </table>
      </div>

      {rejectModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <div className="fixed inset-0 bg-black/30" onClick={() => setRejectModal(null)} />
          <div className="bg-white rounded-xl p-6 shadow-xl relative z-10 w-96">
            <h3 className="font-semibold mb-4">Rejection Reason</h3>
            <textarea className="w-full border rounded-lg p-3 text-sm" rows={3} placeholder="Why is this document being rejected?"
              value={rejectModal.reason} onChange={e => setRejectModal({ ...rejectModal, reason: e.target.value })} />
            <div className="flex gap-2 mt-4">
              <button onClick={() => handleVerify(rejectModal.docId, false, rejectModal.reason)}
                className="bg-red-500 text-white px-4 py-2 rounded-lg text-sm">Reject</button>
              <button onClick={() => setRejectModal(null)} className="border px-4 py-2 rounded-lg text-sm">Cancel</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

function ChecklistTab() {
  const [applicants, setApplicants] = useState<api.Applicant[]>([])
  const [docTypes, setDocTypes] = useState<api.DocumentType[]>([])
  const [selectedId, setSelectedId] = useState<number | null>(null)
  const [docs, setDocs] = useState<api.ApplicantDocument[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    Promise.all([api.getApplicants(), api.getDocumentTypes()]).then(([apps, types]) => {
      setApplicants(apps)
      setDocTypes(types.filter(t => t.isActive))
      setLoading(false)
    })
  }, [])

  useEffect(() => {
    if (selectedId) {
      api.getApplicantDocuments(selectedId).then(setDocs)
    }
  }, [selectedId])

  if (loading) return <div className="text-gray-400 text-center py-8">Loading...</div>

  const requiredTypes = docTypes.filter(t => t.isRequired)

  return (
    <div className="grid grid-cols-3 gap-6">
      <div className="col-span-1">
        <h3 className="font-semibold text-sm mb-3">Select Applicant</h3>
        <div className="bg-white rounded-xl shadow-sm max-h-[600px] overflow-y-auto">
          {applicants.map(a => (
            <button key={a.id} onClick={() => setSelectedId(a.id)}
              className={`w-full text-left p-3 border-b text-sm hover:bg-gray-50 ${selectedId === a.id ? 'bg-[#3bc7f4]/5 border-l-2 border-l-[#3bc7f4]' : ''}`}>
              <p className="font-medium">{a.firstName} {a.lastName}</p>
              <p className="text-xs text-gray-500">{a.email}</p>
            </button>
          ))}
        </div>
      </div>
      <div className="col-span-2">
        {selectedId ? (
          <div>
            <h3 className="font-semibold text-sm mb-3">Document Checklist</h3>
            <div className="space-y-2">
              {docTypes.map(dt => {
                const doc = docs.find(d => d.documentTypeId === dt.id)
                return (
                  <div key={dt.id} className={`bg-white rounded-lg p-4 shadow-sm border flex items-center justify-between ${
                    doc?.status === 'Verified' ? 'border-green-200' :
                    doc?.status === 'Rejected' ? 'border-red-200' :
                    doc ? 'border-yellow-200' : 'border-gray-200'
                  }`}>
                    <div className="flex items-center gap-3">
                      {doc?.status === 'Verified' ? <CheckCircle size={18} className="text-green-500" /> :
                       doc?.status === 'Rejected' ? <XCircle size={18} className="text-red-500" /> :
                       doc ? <Clock size={18} className="text-yellow-500" /> :
                       <div className="w-[18px] h-[18px] rounded-full border-2 border-gray-300" />}
                      <div>
                        <p className="text-sm font-medium">{dt.name} {dt.isRequired && <span className="text-red-500 text-xs">required</span>}</p>
                        {doc && <p className="text-xs text-gray-500">{doc.fileName}</p>}
                      </div>
                    </div>
                    <span className={`text-xs px-2 py-1 rounded-full ${
                      doc?.status === 'Verified' ? 'bg-green-100 text-green-800' :
                      doc?.status === 'Rejected' ? 'bg-red-100 text-red-800' :
                      doc ? 'bg-yellow-100 text-yellow-800' : 'bg-gray-100 text-gray-500'
                    }`}>{doc?.status || 'Missing'}</span>
                  </div>
                )
              })}
            </div>
            <div className="mt-4 bg-gray-50 rounded-lg p-4">
              <p className="text-sm text-gray-600">
                <span className="font-medium">{docs.filter(d => d.status === 'Verified').length}</span> verified · 
                <span className="font-medium ml-1">{docs.filter(d => d.status === 'Pending').length}</span> pending · 
                <span className="font-medium ml-1">{requiredTypes.filter(t => !docs.find(d => d.documentTypeId === t.id)).length}</span> missing required
              </p>
            </div>
          </div>
        ) : (
          <div className="flex items-center justify-center h-64 text-gray-400">Select an applicant to view their document checklist</div>
        )}
      </div>
    </div>
  )
}
