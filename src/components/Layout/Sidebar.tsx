import { useLocation, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { useRole } from '@/context/RoleContext';
import { useTenantConfig } from '@/context/TenantConfigContext';

/* ------------------------------------------------------------------ */
/*  Types                                                              */
/* ------------------------------------------------------------------ */

interface NavItem {
  id: string;
  label: string;
  implemented?: boolean;
  ext?: boolean;
  disabled?: boolean;
  locked?: boolean;
  children?: NavItem[];
}

interface NavSection {
  id: string;
  label: string;
  icon: React.ReactNode;
  items: NavItem[];
  badge?: number;          // Alert count badge (yellow)
  upgradePrompt?: boolean; // NP locked upsell sections
}

/* ------------------------------------------------------------------ */
/*  Reusable SVG Icons                                                 */
/* ------------------------------------------------------------------ */

const icons = {
  dashboard: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <rect x="3" y="3" width="7" height="7" /><rect x="14" y="3" width="7" height="7" />
      <rect x="14" y="14" width="7" height="7" /><rect x="3" y="14" width="7" height="7" />
    </svg>
  ),
  couriers: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" /><circle cx="12" cy="7" r="4" />
    </svg>
  ),
  agents: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" /><circle cx="9" cy="7" r="4" />
      <path d="M23 21v-2a4 4 0 0 0-3-3.87" /><path d="M16 3.13a4 4 0 0 1 0 7.75" />
    </svg>
  ),
  np: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" />
      <polyline points="9 22 9 12 15 12 15 22" />
    </svg>
  ),
  fleet: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M14 16H9m10 0h3v-3.15a1 1 0 0 0-.84-.99L16 11l-2.7-3.6a1 1 0 0 0-.8-.4H5.24a2 2 0 0 0-1.8 1.1l-.8 1.63A6 6 0 0 0 2 12.42V16h2" />
      <circle cx="6.5" cy="16.5" r="2.5" /><circle cx="16.5" cy="16.5" r="2.5" />
    </svg>
  ),
  recruitment: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" /><circle cx="8.5" cy="7" r="4" />
      <line x1="20" y1="8" x2="20" y2="14" /><line x1="23" y1="11" x2="17" y2="11" />
    </svg>
  ),
  compliance: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z" />
      <polyline points="14 2 14 8 20 8" />
      <path d="M9 15l2 2 4-4" />
    </svg>
  ),
  scheduling: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <rect x="3" y="4" width="18" height="18" rx="2" /><line x1="16" y1="2" x2="16" y2="6" /><line x1="8" y1="2" x2="8" y2="6" /><line x1="3" y1="10" x2="21" y2="10" />
    </svg>
  ),
  marketplace: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M6 2L3 7v11a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V7l-3-5z" />
      <line x1="3" y1="7" x2="21" y2="7" />
      <path d="M16 11a4 4 0 0 1-8 0" />
    </svg>
  ),
  users: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" /><circle cx="9" cy="7" r="4" />
      <path d="M23 21v-2a4 4 0 0 0-3-3.87" /><path d="M16 3.13a4 4 0 0 1 0 7.75" />
    </svg>
  ),
  reports: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M18 20V10M12 20V4M6 20v-6" />
    </svg>
  ),
  deliveries: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2" />
      <rect x="9" y="3" width="6" height="4" rx="1" />
      <path d="M9 14l2 2 4-4" />
    </svg>
  ),
  bookJob: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6" />
      <polyline points="15 3 21 3 21 9" />
      <line x1="10" y1="14" x2="21" y2="3" />
    </svg>
  ),
  invoicing: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <line x1="12" y1="1" x2="12" y2="23" /><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
    </svg>
  ),
  portal: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <circle cx="12" cy="12" r="10" /><line x1="2" y1="12" x2="22" y2="12" />
      <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z" />
    </svg>
  ),
  multiTenant: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" />
      <polyline points="9 22 9 12 15 12 15 22" />
    </svg>
  ),
  lock: (
    <svg className="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5">
      <rect x="3" y="11" width="18" height="11" rx="2" /><path d="M7 11V7a5 5 0 0 1 10 0v4" />
    </svg>
  ),
  settings: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <circle cx="12" cy="12" r="3" />
      <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z" />
    </svg>
  ),
  importExport: (
    <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
      <polyline points="17 8 12 3 7 8" />
      <line x1="12" y1="3" x2="12" y2="15" />
    </svg>
  ),
};

/* ------------------------------------------------------------------ */
/*  Section builders per role                                          */
/* ------------------------------------------------------------------ */

function buildTenantSections(cfg: ReturnType<typeof useTenantConfig>['config']): NavSection[] {
  const sections: NavSection[] = [
    {
      id: 'dashboard', label: 'Dashboard', icon: icons.dashboard,
      items: [{ id: '/', label: 'Dashboard', implemented: true }],
    },
  ];

  if (cfg.directCouriersEnabled) {
    sections.push({
      id: 'my-couriers', label: 'My Couriers', icon: icons.couriers,
      items: [
        { id: '/fleet', label: 'Courier List', implemented: true },
        { id: '/fleet/add', label: 'Add Courier', implemented: true },
        { id: '/fleet-management', label: 'Vehicle Fleet', implemented: true },
      ],
    });
  }

  if (cfg.agentsEnabled && cfg.networkPartnersEnabled) {
    // Combined section when both are enabled
    sections.push({
      id: 'agents-np', label: 'Agents/NPs', icon: icons.agents,
      items: [
        { id: '/agents', label: 'Agent/NP Directory', implemented: true },
        { id: '/associations', label: 'Association Stats', implemented: true },
      ],
    });
  } else if (cfg.agentsEnabled) {
    sections.push({
      id: 'agents', label: 'Agents', icon: icons.agents,
      items: [
        { id: '/agents', label: 'Agent Directory', implemented: true },
        { id: '/discovery', label: 'Discovery', implemented: true },
        { id: '/onboarding', label: 'Onboarding', implemented: true },
        { id: '/associations', label: 'Association Stats', implemented: true },
      ],
    });
  } else if (cfg.networkPartnersEnabled) {
    sections.push({
      id: 'np', label: 'Network Partners', icon: icons.np,
      items: [
        { id: '/agents', label: 'NP Directory', implemented: true },
      ],
    });
  }

  if (cfg.courierRecruitmentEnabled) {
    sections.push({
      id: 'recruitment', label: 'Recruitment', icon: icons.recruitment,
      items: [
        { id: '/recruitment', label: 'Pipeline', implemented: true },
        { id: '/recruitment/portal-url', label: 'Applicant Portal', implemented: true },
        { id: '/settings/recruitment-ads', label: 'Advertising', implemented: true },
      ],
    });
  }

  sections.push({
    id: 'compliance', label: 'Compliance', icon: icons.compliance,
    badge: 3, // e.g. 2 expiring docs + 1 pending approval
    items: [
      { id: '/compliance', label: 'Compliance', implemented: true },
    ],
  });

  if (cfg.schedulingEnabled) {
    sections.push({
      id: 'scheduling', label: 'Scheduling', icon: icons.scheduling,
      items: [{ id: '/scheduling', label: 'Scheduling', implemented: true }],
    });
  }

  if (cfg.marketplaceEnabled) {
    sections.push({
      id: 'marketplace', label: 'Marketplace', icon: icons.marketplace,
      items: [{ id: '/quotes', label: 'Quote Requests', implemented: true }],
    });
  }

  return sections;
}

function buildNpSections(): NavSection[] {
  return [
    {
      id: 'dashboard', label: 'Dashboard', icon: icons.dashboard,
      items: [{ id: '/', label: 'Dashboard', implemented: true }],
    },
    {
      id: 'my-drivers', label: 'My Drivers', icon: icons.fleet,
      items: [
        { id: '/fleet', label: 'Driver List', implemented: true },
        { id: '/fleet/add', label: 'Add Driver', implemented: true },
        { id: '/fleet-management', label: 'Vehicle Fleet', implemented: true },
      ],
    },
    {
      id: 'recruitment', label: 'Recruitment', icon: icons.recruitment,
      items: [
        { id: '/recruitment', label: 'Pipeline', implemented: true },
        { id: '/recruitment/portal-url', label: 'Applicant Portal', implemented: true },
        { id: '/settings/recruitment-ads', label: 'Advertising', implemented: true },
      ],
    },
    {
      id: 'compliance', label: 'Compliance', icon: icons.compliance,
      items: [
        { id: '/compliance', label: 'Compliance', implemented: true },
      ],
    },
    {
      id: 'users-section', label: 'My Team', icon: icons.users,
      items: [{ id: '/users', label: 'My Team', implemented: true }],
    },
    {
      id: 'scheduling', label: 'Scheduling', icon: icons.scheduling,
      items: [{ id: '/scheduling', label: 'Scheduling', implemented: true }],
    },
    {
      id: 'reports-section', label: 'Reports', icon: icons.reports,
      items: [{ id: '/reports', label: 'Reports', implemented: true }],
    },
  ];
}

const npUpgradeSections: NavSection[] = [
  { id: 'upgrade-invoicing', label: 'Invoicing', icon: icons.invoicing, items: [], upgradePrompt: true },
  { id: 'upgrade-portal', label: 'Booking Portal', icon: icons.portal, items: [], upgradePrompt: true },
  { id: 'upgrade-multi', label: 'Multi-Tenant', icon: icons.multiTenant, items: [], upgradePrompt: true },
];

const idSections: NavSection[] = [
  {
    id: 'dashboard', label: 'Dashboard', icon: icons.dashboard,
    items: [{ id: '/', label: 'Dashboard', implemented: true }],
  },
  {
    id: 'deliveries', label: 'My Deliveries', icon: icons.deliveries,
    items: [{ id: '/deliveries', label: 'My Deliveries', implemented: true }],
  },
  {
    id: 'fleet', label: 'My Fleet', icon: icons.fleet,
    items: [
      { id: '/fleet', label: 'Fleet Overview', implemented: true },
      { id: '/fleet/add', label: 'Add Driver', implemented: true },
    ],
  },
  {
    id: 'book-job', label: 'Book a Job', icon: icons.bookJob,
    items: [{ id: '#book-job', label: 'Book a Job', ext: true, implemented: true }],
  },
  {
    id: 'reports-section', label: 'Reports', icon: icons.reports,
    items: [{ id: '/reports', label: 'Reports', implemented: true }],
  },
];

/* ------------------------------------------------------------------ */
/*  Import options                                                     */
/* ------------------------------------------------------------------ */

interface ImportOption {
  label: string;
  description: string;
  route: string;
  icon: React.ReactNode;
}

const tenantImportOptions: ImportOption[] = [
  { label: 'Import Agents', description: 'Import agents from spreadsheet or Google Sheets', route: '/import', icon: icons.agents },
  { label: 'Import Couriers', description: 'Import couriers from spreadsheet', route: '/fleet/import', icon: icons.fleet },
];

const npImportOptions: ImportOption[] = [
  { label: 'Import Couriers', description: 'Import couriers from spreadsheet', route: '/fleet/import', icon: icons.fleet },
  { label: 'Import Users', description: 'Import users from spreadsheet', route: '/users/import', icon: icons.users },
];

const idImportOptions: ImportOption[] = [];

/* ------------------------------------------------------------------ */
/*  Helpers                                                            */
/* ------------------------------------------------------------------ */

const findSectionForPath = (sections: NavSection[], path: string): string | null => {
  for (const section of sections) {
    for (const item of section.items) {
      if (item.id === path || (item.id !== '/' && path.startsWith(item.id))) return section.id;
    }
  }
  return null;
};

/* ------------------------------------------------------------------ */
/*  Title / subtitle per role                                          */
/* ------------------------------------------------------------------ */

function getRoleBranding(role: string) {
  switch (role) {
    case 'tenant': return { title: 'Fleet & Partners', subtitle: 'Manage your delivery fleet' };
    case 'np': return { title: 'Driver Fleet', subtitle: 'Network Partner Portal' };
    case 'dfadmin': return { title: 'DF Admin', subtitle: 'Platform Administration' };
    case 'id': return { title: 'Fleet Operations', subtitle: 'In-House Driver Portal' };
    default: return { title: 'DFRNT', subtitle: '' };
  }
}

/* ------------------------------------------------------------------ */
/*  Sidebar Component                                                  */
/* ------------------------------------------------------------------ */

interface Props {
  collapsed: boolean;
  onUpgrade?: () => void;
  selectedCourierId?: number | null;
}

export default function Sidebar({ collapsed, onUpgrade, selectedCourierId }: Props) {
  const { role } = useRole();
  const { config } = useTenantConfig();
  const location = useLocation();
  const navigate = useNavigate();
  const [pickerOpen, setPickerOpen] = useState(false);
  const [bookToast, setBookToast] = useState(false);

  const isDfAdmin = role === 'dfadmin';

  // Build sections based on role + config
  const sections: NavSection[] = (() => {
    if (role === 'id') return idSections;
    if (role === 'np') return buildNpSections();
    // tenant and dfadmin both use tenant sections; dfadmin sees all toggles ON
    if (isDfAdmin) {
      return buildTenantSections({
        directCouriersEnabled: true,
        agentsEnabled: true,
        networkPartnersEnabled: true,
        courierRecruitmentEnabled: true,
        schedulingEnabled: true,
        marketplaceEnabled: true,
      });
    }
    return buildTenantSections(config);
  })();

  const importOptions = role === 'tenant' || isDfAdmin ? tenantImportOptions : role === 'id' ? idImportOptions : npImportOptions;
  const branding = getRoleBranding(role || 'tenant');

  const [expandedSections, setExpandedSections] = useState<string[]>(() => {
    const section = findSectionForPath(sections, location.pathname);
    return section ? [section] : [sections[0]?.id];
  });

  const isActive = (id: string) => {
    if (id === '/') return location.pathname === '/';
    return location.pathname === id || location.pathname.startsWith(id + '/');
  };

  const toggleSection = (sectionId: string) => {
    setExpandedSections(prev =>
      prev.includes(sectionId)
        ? prev.filter(id => id !== sectionId)
        : [sectionId]
    );
  };

  const handleNav = (id: string) => {
    if (id === '#book-job') {
      setBookToast(true);
      setTimeout(() => setBookToast(false), 3000);
      return;
    }
    navigate(id);
    const sectionId = findSectionForPath(sections, id);
    if (sectionId) setExpandedSections([sectionId]);
  };

  const handleImportExport = () => {
    const path = location.pathname;

    if (role === 'tenant' || isDfAdmin) {
      if (path.startsWith('/fleet') || path.startsWith('/courier')) {
        navigate('/fleet/import');
      } else if (path.startsWith('/agents') || path.startsWith('/onboarding') || path.startsWith('/import')) {
        navigate('/import');
      } else {
        setPickerOpen(true);
      }
      return;
    }

    if (path.startsWith('/fleet') || path.startsWith('/courier')) {
      navigate('/fleet/import');
    } else if (path.startsWith('/users')) {
      navigate('/users/import');
    } else {
      setPickerOpen(true);
    }
  };

  return (
    <>
      <nav
        className={`fixed left-0 top-0 z-50 h-screen bg-brand-dark flex flex-col transition-all duration-300 ${
          collapsed ? 'w-16 -translate-x-16' : 'w-64 translate-x-0'
        }`}
      >
        {/* Logo */}
        <div className="p-4 border-b border-white/10 flex-shrink-0">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0">
              <img src={import.meta.env.BASE_URL + 'dfrnt-logo.png'} alt="DFRNT" className="w-8 h-8 object-contain" />
            </div>
            {!collapsed && (
              <div className="overflow-hidden">
                <h1 className="text-base font-bold text-white whitespace-nowrap">{branding.title}</h1>
                <p className="text-xs text-white/50 whitespace-nowrap">{branding.subtitle}</p>
              </div>
            )}
          </div>
        </div>

        {/* Navigation */}
        <div className="flex-1 overflow-y-auto py-4 min-h-0">
          {sections.map((section) => (
            <div key={section.id} className="mb-2">
              <button
                onClick={() => {
                  if (section.items.length === 1) {
                    handleNav(section.items[0].id);
                  } else {
                    toggleSection(section.id);
                  }
                }}
                className={`w-full flex items-center gap-3 px-4 py-3 text-left transition-colors hover:bg-white/5 ${
                  collapsed ? 'justify-center' : ''
                } ${section.items.length === 1 && isActive(section.items[0].id) ? 'bg-brand-cyan/20 border-r-2 border-brand-cyan' : ''}`}
              >
                <span className={`${section.items.length === 1 && isActive(section.items[0].id) ? 'text-brand-cyan' : 'text-white/70'}`}>{section.icon}</span>
                {!collapsed && (
                  <>
                    <span className={`flex-1 text-sm font-medium ${section.items.length === 1 && isActive(section.items[0].id) ? 'text-brand-cyan' : 'text-white/90'}`}>{section.label}</span>
                    {section.badge && section.badge > 0 && (
                      <span className="inline-flex items-center justify-center min-w-[18px] h-[18px] px-1 text-[10px] font-bold rounded-full bg-amber-400 text-amber-900">{section.badge}</span>
                    )}
                    {section.items.length > 1 && (
                      <svg
                        className={`w-4 h-4 text-white/50 transition-transform duration-200 ${
                          expandedSections.includes(section.id) ? 'rotate-180' : ''
                        }`}
                        viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"
                      >
                        <polyline points="6 9 12 15 18 9" />
                      </svg>
                    )}
                    {section.items.length === 1 && section.items[0].implemented && (
                      <span className="ml-auto w-2 h-2 rounded-full bg-brand-cyan flex-shrink-0" />
                    )}
                  </>
                )}
              </button>

              {!collapsed && section.items.length > 1 && expandedSections.includes(section.id) && (
                <div className="mt-1 space-y-1 menu-section-items">
                  {section.items.map((item) => {
                    if (item.locked) {
                      return (
                        <button
                          key={item.id}
                          onClick={onUpgrade}
                          className="w-full flex items-center gap-3 pl-12 pr-4 py-2.5 text-left text-white/40 hover:bg-white/5"
                        >
                          <span className="text-sm">{item.label}</span>
                          <span className="ml-auto text-[10px] px-1.5 py-0.5 rounded bg-brand-purple/30 text-brand-purple">
                            Upgrade
                          </span>
                        </button>
                      );
                    }

                    const active = isActive(item.id);
                    const disabled = item.disabled && !selectedCourierId;

                    return (
                      <button
                        key={item.id}
                        onClick={() => {
                          if (disabled) return;
                          if (item.id === '/courier' && selectedCourierId) {
                            handleNav(`/courier/${selectedCourierId}`);
                          } else {
                            handleNav(item.id);
                          }
                        }}
                        disabled={disabled}
                        className={`w-full flex items-center gap-3 pl-12 pr-4 py-2.5 text-left transition-all duration-150 ${
                          active
                            ? 'bg-brand-cyan/20 text-brand-cyan border-r-2 border-brand-cyan'
                            : 'text-white/60 hover:bg-white/5 hover:text-white/80'
                        } ${disabled ? 'opacity-30 cursor-not-allowed' : ''}`}
                      >
                        <span className="text-sm">{item.label}</span>
                        {item.implemented && (
                          <span className="ml-auto w-2 h-2 rounded-full bg-brand-cyan flex-shrink-0" title="Implemented" />
                        )}
                      </button>
                    );
                  })}
                </div>
              )}
            </div>
          ))}

          {/* NP Upgrade Prompts */}
          {role === 'np' && (
            <>
              <div className="mx-4 my-3 border-t border-white/10" />
              {npUpgradeSections.map((section) => (
                <div key={section.id} className="mb-1">
                  <button
                    onClick={onUpgrade}
                    className="w-full flex items-center gap-3 px-4 py-3 text-left transition-colors hover:bg-gradient-to-r hover:from-purple-500/10 hover:to-amber-500/10 group"
                  >
                    <span className="text-white/30 group-hover:text-purple-400/60 transition-colors">{section.icon}</span>
                    {!collapsed && (
                      <>
                        <span className="flex-1 text-sm font-medium text-white/40 group-hover:text-white/60 transition-colors">{section.label}</span>
                        <span className="flex items-center gap-1 text-[10px] px-2 py-0.5 rounded-full bg-gradient-to-r from-purple-500/20 to-amber-500/20 text-amber-400/80 border border-amber-500/20">
                          {icons.lock}
                          <span>Upgrade</span>
                        </span>
                      </>
                    )}
                  </button>
                </div>
              ))}
            </>
          )}
        </div>

        {/* Settings */}
        <div className="px-3 py-2 border-t border-white/10 flex-shrink-0">
          <button
            onClick={() => handleNav('/settings')}
            className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-lg transition-all duration-200 hover:bg-white/5 ${
              collapsed ? 'justify-center' : ''
            } ${isActive('/settings') && !location.pathname.startsWith('/settings/document-types') && !location.pathname.startsWith('/settings/recruitment') ? 'bg-brand-cyan/20 text-brand-cyan' : ''}`}
          >
            <span className={`flex-shrink-0 ${isActive('/settings') && !location.pathname.startsWith('/settings/document-types') && !location.pathname.startsWith('/settings/recruitment') ? 'text-brand-cyan' : 'text-white/70'}`}>{icons.settings}</span>
            {!collapsed && <span className={`text-sm font-medium ${isActive('/settings') && !location.pathname.startsWith('/settings/document-types') && !location.pathname.startsWith('/settings/recruitment') ? 'text-brand-cyan' : 'text-white/90'}`}>Settings</span>}
          </button>

          {/* DF Admin: Tenant Configuration link under settings */}
          {isDfAdmin && !collapsed && (
            <button
              onClick={() => navigate('/settings/tenant-config')}
              className={`w-full flex items-center gap-3 px-3 py-2 rounded-lg transition-all duration-200 hover:bg-white/5 ml-2 ${
                location.pathname === '/settings/tenant-config' ? 'bg-brand-cyan/20 text-brand-cyan' : ''
              }`}
            >
              <svg className={`w-4 h-4 flex-shrink-0 ${location.pathname === '/settings/tenant-config' ? 'text-brand-cyan' : 'text-white/50'}`} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M12 3v1m0 16v1m-8-9H3m18 0h-1M5.6 5.6l.7.7m12.1 12.1l.7.7M5.6 18.4l.7-.7m12.1-12.1l.7-.7" />
                <circle cx="12" cy="12" r="4" />
              </svg>
              <span className={`text-xs font-medium ${location.pathname === '/settings/tenant-config' ? 'text-brand-cyan' : 'text-white/60'}`}>Tenant Configuration</span>
            </button>
          )}
        </div>

        {/* Import & Export */}
        {role !== 'id' && (
          <div className="px-3 py-2 border-t border-white/10 flex-shrink-0">
            <button
              onClick={handleImportExport}
              className={`w-full flex items-center gap-3 px-3 py-3 rounded-lg transition-all duration-200
                bg-brand-cyan/10 hover:bg-brand-cyan/20
                border border-brand-cyan/30 hover:border-brand-cyan/50
                ${collapsed ? 'justify-center' : ''}`}
            >
              <span className="text-brand-cyan flex-shrink-0">{icons.importExport}</span>
              {!collapsed && (
                <span className="text-sm font-medium text-brand-cyan">Import & Export</span>
              )}
            </button>
          </div>
        )}

        {/* User */}
        {!collapsed && (
          <div className="p-4 border-t border-white/10 flex-shrink-0">
            <div className="flex items-center gap-3">
              <div className={`w-8 h-8 rounded-full flex items-center justify-center text-white text-sm font-medium flex-shrink-0 ${isDfAdmin ? 'bg-brand-cyan' : role === 'id' ? 'bg-[#f59e0b]' : role === 'np' ? 'bg-brand-purple' : 'bg-brand-purple'}`}>
                {isDfAdmin ? 'DF' : role === 'tenant' ? 'TU' : role === 'id' ? 'ID' : 'NP'}
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-white truncate">
                  {isDfAdmin ? 'DF Admin' : role === 'tenant' ? 'Tenant User' : role === 'id' ? 'Midwest Medical Supplies' : 'NP Admin'}
                </p>
                <p className="text-xs text-white/50 truncate">
                  {isDfAdmin ? 'steve@deliverdifferent.com' : role === 'tenant' ? 'admin@tenant.com' : role === 'id' ? 'admin@midwestmedical.com' : 'admin@partner.com'}
                </p>
              </div>
            </div>
          </div>
        )}
      </nav>

      {/* Import Picker Modal */}
      {pickerOpen && (
        <div
          className="fixed inset-0 bg-black/40 z-[200] flex items-center justify-center"
          onClick={(e) => { if (e.target === e.currentTarget) setPickerOpen(false); }}
        >
          <div className="bg-white rounded-lg shadow-lg border border-border w-full max-w-md mx-4 p-6">
            <h2 className="text-lg font-bold text-text-primary mb-1">Import & Export</h2>
            <p className="text-sm text-text-secondary mb-5">Choose what you'd like to import</p>
            <div className="space-y-3">
              {importOptions.map((opt) => (
                <button
                  key={opt.route}
                  onClick={() => {
                    setPickerOpen(false);
                    navigate(opt.route);
                  }}
                  className="w-full flex items-center gap-4 p-4 rounded-lg border border-border bg-white hover:border-brand-cyan hover:bg-brand-cyan/5 transition-all text-left group"
                >
                  <div className="w-10 h-10 rounded-lg bg-brand-cyan/10 text-brand-cyan flex items-center justify-center flex-shrink-0 group-hover:bg-brand-cyan/20 transition-colors">
                    {opt.icon}
                  </div>
                  <div>
                    <div className="text-sm font-medium text-text-primary">{opt.label}</div>
                    <div className="text-xs text-text-secondary">{opt.description}</div>
                  </div>
                  <svg className="w-4 h-4 text-text-muted ml-auto flex-shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <polyline points="9 18 15 12 9 6" />
                  </svg>
                </button>
              ))}
            </div>
            <button
              onClick={() => setPickerOpen(false)}
              className="mt-4 w-full text-center text-sm text-text-muted hover:text-text-secondary transition-colors py-2"
            >
              Cancel
            </button>
          </div>
        </div>
      )}

      {/* Book a Job toast */}
      {bookToast && (
        <div className="fixed bottom-6 right-6 bg-brand-dark text-white px-4 py-3 rounded-lg shadow-lg text-sm z-[300]">
          Opens standard booking system
        </div>
      )}
    </>
  );
}
