import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useUsers } from '@/hooks/useUsers';
import StatusBadge from '@/components/common/StatusBadge';
import Modal from '@/components/common/Modal';

const roleBadgeStyles: Record<string, string> = {
  Admin: 'bg-brand-cyan/10 text-brand-cyan',
  Dispatcher: 'bg-green-50 text-success',
  'Read-Only': 'bg-surface-light text-text-secondary',
};

const permissions = [
  { perm: 'View Dashboard', admin: '✅', dispatcher: '✅', readonly: '✅' },
  { perm: 'View Dispatch Board', admin: '✅', dispatcher: '✅', readonly: '✅' },
  { perm: 'Assign Couriers to Jobs', admin: '✅', dispatcher: '✅', readonly: '❌' },
  { perm: 'Fleet Management', admin: '✅', dispatcher: '✅', readonly: '👁️ View' },
  { perm: 'Add/Edit Couriers', admin: '✅', dispatcher: '✅', readonly: '❌' },
  { perm: 'View Financial Data', admin: '✅', dispatcher: '❌', readonly: '❌' },
  { perm: 'Manage Users', admin: '✅', dispatcher: '❌', readonly: '❌' },
  { perm: 'Edit Settings', admin: '✅', dispatcher: '❌', readonly: '❌' },
  { perm: 'View Reports', admin: '✅', dispatcher: '✅', readonly: '✅' },
];

export default function Users() {
  const navigate = useNavigate();
  const { users } = useUsers();
  const [modalOpen, setModalOpen] = useState(false);

  return (
    <>
      <div className="flex items-center justify-between mb-5">
        <h2 className="text-xl font-bold">Users</h2>
        <div className="flex gap-2">
          <button onClick={() => navigate('/users/import')} className="bg-white text-brand-cyan border border-border px-4 py-2 rounded-md text-sm hover:bg-surface-cream transition-all">
            Import from Spreadsheet
          </button>
          <button onClick={() => setModalOpen(true)} className="bg-brand-cyan text-brand-dark border-none font-medium px-4 py-2 rounded-md text-sm hover:shadow-cyan-glow">
            + Add User
          </button>
        </div>
      </div>
      <p className="text-text-secondary text-sm mb-5">
        Manage who can access your NP portal. Assign roles to control what each team member can see and do.
      </p>

      {/* User Table */}
      <div className="bg-white border border-border rounded-lg overflow-x-auto">
        <table className="w-full border-collapse">
          <thead>
            <tr>
              {['Name', 'Email', 'Role', 'Status', 'Last Login', ''].map(h => (
                <th key={h} className="text-left text-xs text-text-secondary uppercase tracking-wide px-3 py-2.5 border-b border-border">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {users.map(u => (
              <tr key={u.id} className="hover:bg-surface-cream">
                <td className="px-3 py-2.5 text-sm border-b border-border font-bold">{u.name}</td>
                <td className="px-3 py-2.5 text-sm border-b border-border">{u.email}</td>
                <td className="px-3 py-2.5 text-sm border-b border-border">
                  <span className={`px-2.5 py-0.5 rounded-lg text-xs ${roleBadgeStyles[u.role]}`}>{u.role}</span>
                </td>
                <td className="px-3 py-2.5 text-sm border-b border-border"><StatusBadge status={u.status} /></td>
                <td className="px-3 py-2.5 text-[13px] text-text-secondary border-b border-border">{u.lastLogin}</td>
                <td className="px-3 py-2.5 border-b border-border">
                  <button className="bg-transparent border border-border text-text-primary px-2.5 py-1 rounded-md text-xs hover:border-brand-cyan hover:text-brand-cyan transition-all">
                    Edit
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Role Permissions */}
      <div className="bg-white border border-border rounded-lg p-5 mt-4">
        <h3 className="font-bold mb-3">Role Permissions</h3>
        <table className="w-full border-collapse">
          <thead>
            <tr>
              {['Permission', 'Admin', 'Dispatcher', 'Read-Only'].map(h => (
                <th key={h} className="text-left text-xs text-text-secondary uppercase tracking-wide px-3 py-2.5 border-b border-border">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {permissions.map(p => (
              <tr key={p.perm} className="hover:bg-surface-cream">
                <td className="px-3 py-2.5 text-sm border-b border-border">{p.perm}</td>
                <td className="px-3 py-2.5 text-sm border-b border-border">{p.admin}</td>
                <td className="px-3 py-2.5 text-sm border-b border-border">{p.dispatcher}</td>
                <td className="px-3 py-2.5 text-sm border-b border-border">{p.readonly}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Add User Modal */}
      <Modal open={modalOpen} onClose={() => setModalOpen(false)}>
        <h2 className="text-xl font-bold mb-2">Add New User</h2>
        <p className="text-text-secondary text-sm mb-4">Create a portal login for a team member.</p>
        <div className="flex flex-col gap-3 mb-5">
          <div className="flex flex-col gap-1">
            <label className="text-xs text-text-secondary uppercase tracking-wide">Full Name</label>
            <input type="text" placeholder="e.g. Jane Smith" />
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-xs text-text-secondary uppercase tracking-wide">Email</label>
            <input type="email" placeholder="e.g. jane@pacificexpress.com" />
          </div>
          <div className="flex flex-col gap-1">
            <label className="text-xs text-text-secondary uppercase tracking-wide">Role</label>
            <select>
              <option>Admin</option>
              <option>Dispatcher</option>
              <option>Read-Only</option>
            </select>
          </div>
        </div>
        <p className="text-xs text-text-secondary mb-4">An invitation email will be sent with login instructions.</p>
        <div className="flex gap-2.5 justify-end">
          <button onClick={() => setModalOpen(false)} className="bg-transparent border border-border text-text-primary px-4 py-2 rounded-md text-sm hover:border-brand-cyan hover:text-brand-cyan transition-all">
            Cancel
          </button>
          <button onClick={() => setModalOpen(false)} className="bg-brand-cyan text-brand-dark border-none font-medium px-4 py-2 rounded-md text-sm hover:shadow-cyan-glow">
            Send Invite
          </button>
        </div>
      </Modal>
    </>
  );
}
