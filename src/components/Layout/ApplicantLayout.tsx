import { Outlet, NavLink, useNavigate } from 'react-router-dom'
import { useStore } from '../store'
import { HelpCircle, LogOut, LayoutDashboard, Users, Settings, FileText, Workflow } from 'lucide-react'

const navItems = [
  { to: '/admin/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/admin/pipeline', label: 'Pipeline', icon: Users },
  { to: '/admin/documents', label: 'Documents', icon: FileText },
  { to: '/admin/quizzes', label: 'Quizzes', icon: HelpCircle },
  { to: '/admin/flow-builder', label: 'Flow Builder', icon: Workflow },
  { to: '/admin/settings', label: 'Settings', icon: Settings },
]

export default function Layout() {
  const logout = useStore(s => s.logout)
  const navigate = useNavigate()

  return (
    <div className="flex h-screen">
      <aside className="w-64 bg-[#0d0c2c] text-white flex flex-col">
        <div className="p-6">
          <h1 className="text-xl font-bold text-[#FFD200]">Urgent Couriers</h1>
          <p className="text-sm text-gray-400">Recruitment Portal</p>
        </div>
        <nav className="flex-1 px-4">
          {navItems.map(item => (
            <NavLink key={item.to} to={item.to}
              className={({ isActive }) =>
                `flex items-center gap-3 px-4 py-3 rounded-lg mb-1 transition-colors ${isActive ? 'bg-[#FFD200]/20 text-[#FFD200]' : 'text-gray-300 hover:bg-white/5'}`
              }>
              <item.icon size={20} />
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="p-4 border-t border-white/10">
          <button onClick={() => { logout(); navigate('/admin/login'); }}
            className="flex items-center gap-3 text-gray-400 hover:text-white w-full px-4 py-2">
            <LogOut size={20} /> Sign Out
          </button>
        </div>
      </aside>
      <main className="flex-1 overflow-auto p-8 bg-gray-50">
        <Outlet />
      </main>
    </div>
  )
}
