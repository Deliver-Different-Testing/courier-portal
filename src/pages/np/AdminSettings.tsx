// @ts-nocheck
import { useEffect, useState } from 'react'
import * as api from '../../lib/api'
import { Plus, Users, Eye, EyeOff, Copy, CheckCircle, Link, KeyRound } from 'lucide-react'

export default function AdminSettings() {
  const [users, setUsers] = useState<api.AdminUserInfo[]>([])
  const [showInvite, setShowInvite] = useState(false)
  const [form, setForm] = useState({ email: '', displayName: '', role: 'Reviewer' })
  const [inviting, setInviting] = useState(false)
  const [inviteLink, setInviteLink] = useState<string | null>(null)
  const [copied, setCopied] = useState(false)
  const [resetLink, setResetLink] = useState<{ userId: number; link: string } | null>(null)
  const [resetCopied, setResetCopied] = useState(false)

  useEffect(() => { api.getUsers().then(setUsers).catch(() => {}) }, [])

  const handleInvite = async () => {
    setInviting(true)
    try {
      const result = await api.inviteUser({ email: form.email, displayName: form.displayName, role: form.role })
      setUsers([...users, result.user])
      setInviteLink(result.inviteLink)
    } catch (e: any) { alert('Error: ' + e.message) }
    finally { setInviting(false) }
  }

  const copyLink = () => {
    if (inviteLink) {
      navigator.clipboard.writeText(inviteLink)
      setCopied(true)
      setTimeout(() => setCopied(false), 3000)
    }
  }

  const closeInvite = () => {
    setShowInvite(false)
    setInviteLink(null)
    setForm({ email: '', displayName: '', role: 'Reviewer' })
    setCopied(false)
  }

  const toggleActive = async (user: api.AdminUserInfo) => {
    try {
      const updated = await api.updateUser(user.id, { isActive: !user.isActive })
      setUsers(users.map(u => u.id === user.id ? updated : u))
    } catch { alert('Error updating user') }
  }

  const handleResetLink = async (user: api.AdminUserInfo) => {
    try {
      const result = await api.generateResetLink(user.id)
      setResetLink({ userId: user.id, link: result.resetLink })
      setResetCopied(false)
    } catch { alert('Error generating reset link') }
  }

  const copyResetLink = () => {
    if (resetLink) {
      navigator.clipboard.writeText(resetLink.link)
      setResetCopied(true)
      setTimeout(() => setResetCopied(false), 3000)
    }
  }

  const changeRole = async (user: api.AdminUserInfo, role: string) => {
    try {
      const updated = await api.updateUser(user.id, { role })
      setUsers(users.map(u => u.id === user.id ? updated : u))
    } catch { alert('Error updating role') }
  }

  const getStatusBadge = (user: api.AdminUserInfo) => {
    if (!user.isActive) return { label: 'Inactive', color: 'bg-red-100 text-red-700' }
    if (user.hasInvite) return { label: 'Invited', color: 'bg-blue-100 text-blue-700' }
    return { label: 'Active', color: 'bg-green-100 text-green-700' }
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-[#0d0c2c]">Settings</h1>
          <p className="text-sm text-gray-500 mt-1">Manage admin users and system settings</p>
        </div>
      </div>

      <div className="bg-white rounded-xl shadow-sm">
        <div className="flex items-center justify-between p-6 border-b">
          <div className="flex items-center gap-3">
            <Users size={20} className="text-[#0d0c2c]" />
            <div>
              <h2 className="font-semibold">Admin Users</h2>
              <p className="text-sm text-gray-500">Invite users to access the admin panel</p>
            </div>
          </div>
          <button onClick={() => { setShowInvite(true); setInviteLink(null) }}
            className="flex items-center gap-2 bg-[#FFD200] text-[#0d0c2c] px-4 py-2 rounded-lg text-sm font-semibold hover:bg-[#E87C1E] hover:text-white transition-colors">
            <Plus size={16} /> Invite User
          </button>
        </div>

        {showInvite && (
          <div className="p-6 bg-yellow-50 border-b">
            {inviteLink ? (
              /* Invite link generated — show copy UI */
              <div>
                <div className="flex items-center gap-2 mb-3">
                  <CheckCircle size={20} className="text-green-600" />
                  <h3 className="font-semibold text-green-800">Invite Created!</h3>
                </div>
                <p className="text-sm text-gray-600 mb-3">
                  Share this link with <strong>{form.email}</strong> to set up their account. The link expires in 7 days.
                </p>
                <div className="flex items-center gap-2 bg-white border rounded-lg p-3">
                  <Link size={16} className="text-gray-400 shrink-0" />
                  <input className="flex-1 text-sm text-gray-700 bg-transparent outline-none" readOnly value={inviteLink} />
                  <button onClick={copyLink}
                    className={`flex items-center gap-1 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                      copied ? 'bg-green-100 text-green-700' : 'bg-[#FFD200] text-[#0d0c2c] hover:bg-[#E87C1E] hover:text-white'
                    }`}>
                    {copied ? <><CheckCircle size={14} /> Copied!</> : <><Copy size={14} /> Copy Link</>}
                  </button>
                </div>
                <div className="mt-4">
                  <button onClick={closeInvite} className="border px-4 py-2 rounded-lg text-sm">Done</button>
                </div>
              </div>
            ) : (
              /* Invite form */
              <div>
                <h3 className="font-semibold mb-3">Invite New User</h3>
                <p className="text-sm text-gray-500 mb-4">They'll receive a link to set up their own password.</p>
                <div className="grid grid-cols-3 gap-4">
                  <div>
                    <label className="block text-xs font-medium text-gray-700 mb-1">Email *</label>
                    <input className="w-full border rounded-lg p-2.5 text-sm" type="email" placeholder="john@example.com" value={form.email}
                      onChange={e => setForm(f => ({ ...f, email: e.target.value }))} />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-700 mb-1">Display Name</label>
                    <input className="w-full border rounded-lg p-2.5 text-sm" placeholder="John Smith" value={form.displayName}
                      onChange={e => setForm(f => ({ ...f, displayName: e.target.value }))} />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-700 mb-1">Role</label>
                    <select className="w-full border rounded-lg p-2.5 text-sm" value={form.role}
                      onChange={e => setForm(f => ({ ...f, role: e.target.value }))}>
                      <option value="Admin">Admin</option>
                      <option value="Reviewer">Reviewer</option>
                    </select>
                  </div>
                </div>
                <div className="flex gap-2 mt-4">
                  <button onClick={handleInvite} disabled={!form.email || inviting}
                    className="bg-[#0d0c2c] text-white px-4 py-2 rounded-lg text-sm font-semibold disabled:opacity-50">
                    {inviting ? 'Creating Invite...' : 'Generate Invite Link'}
                  </button>
                  <button onClick={closeInvite} className="border px-4 py-2 rounded-lg text-sm">Cancel</button>
                </div>
              </div>
            )}
          </div>
        )}

        <table className="w-full">
          <thead className="bg-gray-50 border-b">
            <tr>
              <th className="text-left p-4 text-xs font-semibold text-gray-600 uppercase">Name</th>
              <th className="text-left p-4 text-xs font-semibold text-gray-600 uppercase">Email</th>
              <th className="text-left p-4 text-xs font-semibold text-gray-600 uppercase">Role</th>
              <th className="text-left p-4 text-xs font-semibold text-gray-600 uppercase">Created</th>
              <th className="text-center p-4 text-xs font-semibold text-gray-600 uppercase">Status</th>
              <th className="text-right p-4 text-xs font-semibold text-gray-600 uppercase">Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.map(user => {
              const status = getStatusBadge(user)
              return (
                <tr key={user.id} className={`border-b hover:bg-gray-50 ${!user.isActive ? 'opacity-50' : ''}`}>
                  <td className="p-4 text-sm font-medium">{user.displayName || user.username || '—'}</td>
                  <td className="p-4 text-sm text-gray-600">{user.email}</td>
                  <td className="p-4">
                    <select className="text-xs border rounded px-2 py-1" value={user.role}
                      onChange={e => changeRole(user, e.target.value)}>
                      <option value="Admin">Admin</option>
                      <option value="Reviewer">Reviewer</option>
                    </select>
                  </td>
                  <td className="p-4 text-sm text-gray-500">{new Date(user.createdDate).toLocaleDateString('en-NZ')}</td>
                  <td className="p-4 text-center">
                    <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${status.color}`}>{status.label}</span>
                  </td>
                  <td className="p-4 text-right">
                    <div className="flex items-center justify-end gap-1">
                      <button onClick={() => handleResetLink(user)}
                        className="text-gray-400 hover:text-[#E87C1E] p-1" title="Generate password reset link">
                        <KeyRound size={16} />
                      </button>
                      <button onClick={() => toggleActive(user)}
                        className="text-gray-400 hover:text-[#0d0c2c] p-1" title={user.isActive ? 'Deactivate' : 'Activate'}>
                        {user.isActive ? <EyeOff size={16} /> : <Eye size={16} />}
                      </button>
                    </div>
                    {resetLink?.userId === user.id && (
                      <div className="mt-2 flex items-center gap-1 bg-yellow-50 border border-yellow-200 rounded-lg p-2">
                        <input className="flex-1 text-xs text-gray-600 bg-transparent outline-none min-w-0" readOnly value={resetLink.link} />
                        <button onClick={copyResetLink}
                          className={`shrink-0 flex items-center gap-1 px-2 py-1 rounded text-xs font-medium transition-colors ${
                            resetCopied ? 'bg-green-100 text-green-700' : 'bg-[#FFD200] text-[#0d0c2c] hover:bg-[#E87C1E] hover:text-white'
                          }`}>
                          {resetCopied ? <><CheckCircle size={12} /> Copied</> : <><Copy size={12} /> Copy</>}
                        </button>
                        <button onClick={() => setResetLink(null)} className="text-gray-400 hover:text-gray-600 text-xs px-1">✕</button>
                      </div>
                    )}
                  </td>
                </tr>
              )
            })}
            {users.length === 0 && (
              <tr><td colSpan={6} className="p-8 text-center text-gray-400">No users yet. Invite one above.</td></tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
