import React, { createContext, useContext, useState, useCallback } from 'react';

export type AppRole = 'tenant' | 'np' | 'id' | 'dfadmin' | null;

interface RoleContextValue {
  role: AppRole;
  setRole: (role: AppRole) => void;
  logout: () => void;
}

const RoleContext = createContext<RoleContextValue>({
  role: null,
  setRole: () => {},
  logout: () => {},
});

export function RoleProvider({ children }: { children: React.ReactNode }) {
  const [role, setRoleState] = useState<AppRole>(() => {
    return (localStorage.getItem('dfrnt_role') as AppRole) || null;
  });

  const setRole = useCallback((r: AppRole) => {
    setRoleState(r);
    if (r) localStorage.setItem('dfrnt_role', r);
    else localStorage.removeItem('dfrnt_role');
  }, []);

  const logout = useCallback(() => {
    setRoleState(null);
    localStorage.removeItem('dfrnt_role');
  }, []);

  return (
    <RoleContext.Provider value={{ role, setRole, logout }}>
      {children}
    </RoleContext.Provider>
  );
}

export function useRole() {
  return useContext(RoleContext);
}
