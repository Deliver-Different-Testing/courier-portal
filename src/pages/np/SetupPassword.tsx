import { useState } from 'react'
import { useSearchParams, useNavigate } from 'react-router-dom'
import { CheckCircle, AlertCircle } from 'lucide-react'

export default function SetupPassword() {
  const [params] = useSearchParams()
  const token = params.get('token')
  const navigate = useNavigate()
  const [password, setPassword] = useState('')
  const [confirm, setConfirm] = useState('')
  const [saving, setSaving] = useState(false)
  const [done, setDone] = useState(false)
  const [error, setError] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (password.length < 6) { setError('Password must be at least 6 characters'); return }
    if (password !== confirm) { setError('Passwords do not match'); return }
    setSaving(true); setError('')
    try {
      const res = await fetch('/api/users/setup-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ token, password })
      })
      if (!res.ok) {
        const text = await res.text()
        throw new Error(text || 'Failed to set password')
      }
      setDone(true)
    } catch (e: any) { setError(e.message || 'Error setting password') }
    finally { setSaving(false) }
  }

  if (!token) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#0d0c2c]">
        <div className="bg-white p-8 rounded-xl shadow-lg w-96 text-center">
          <AlertCircle size={48} className="mx-auto mb-4 text-red-500" />
          <h1 className="text-xl font-bold mb-2">Invalid Link</h1>
          <p className="text-gray-500 text-sm">This invite link is missing or invalid. Please ask your admin for a new invite.</p>
        </div>
      </div>
    )
  }

  if (done) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#0d0c2c]">
        <div className="bg-white p-8 rounded-xl shadow-lg w-96 text-center">
          <CheckCircle size={48} className="mx-auto mb-4 text-green-500" />
          <h1 className="text-xl font-bold mb-2">Password Set!</h1>
          <p className="text-gray-500 text-sm mb-6">Your account is ready. You can now log in.</p>
          <button onClick={() => navigate('/admin/login')}
            className="w-full bg-[#FFD200] text-[#0d0c2c] py-3 rounded-lg font-semibold hover:bg-[#E87C1E] hover:text-white transition-colors">
            Go to Login
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-[#0d0c2c]">
      <form onSubmit={handleSubmit} className="bg-white p-8 rounded-xl shadow-lg w-96">
        <h1 className="text-2xl font-bold mb-2 text-[#0d0c2c]">Set Your Password</h1>
        <p className="text-gray-500 text-sm mb-6">Choose a password for your admin account.</p>
        {error && <div className="flex items-center gap-2 text-red-500 text-sm mb-4 bg-red-50 p-3 rounded-lg"><AlertCircle size={16} />{error}</div>}
        <div className="mb-4">
          <label className="block text-xs font-bold text-gray-700 uppercase tracking-wide mb-1.5">New Password</label>
          <input className="w-full border rounded-lg p-3" type="password" placeholder="At least 6 characters"
            value={password} onChange={e => setPassword(e.target.value)} />
        </div>
        <div className="mb-6">
          <label className="block text-xs font-bold text-gray-700 uppercase tracking-wide mb-1.5">Confirm Password</label>
          <input className="w-full border rounded-lg p-3" type="password" placeholder="Confirm your password"
            value={confirm} onChange={e => setConfirm(e.target.value)} />
        </div>
        <button type="submit" disabled={saving || !password || !confirm}
          className="w-full bg-[#FFD200] text-[#0d0c2c] py-3 rounded-lg font-semibold hover:bg-[#E87C1E] hover:text-white disabled:opacity-50 transition-colors">
          {saving ? 'Setting Password...' : 'Set Password'}
        </button>
      </form>
    </div>
  )
}
