// Minimal store stub — replace with zustand or context when ready
import { useState } from 'react';

interface StoreState {
  documentTypes: Array<{ id: number; name: string; category?: string; isRequired: boolean; isActive: boolean }>;
  setDocumentTypes: (types: StoreState['documentTypes']) => void;
  logout: () => void;
}

// Simple module-level store (not reactive across components — use zustand for real impl)
let _documentTypes: StoreState['documentTypes'] = [];

export function useStore<T>(selector: (state: StoreState) => T): T {
  const [, forceUpdate] = useState(0);

  const state: StoreState = {
    documentTypes: _documentTypes,
    setDocumentTypes: (types) => {
      _documentTypes = types;
      forceUpdate((n) => n + 1);
    },
    logout: () => {
      localStorage.removeItem('token');
      window.location.href = '/login';
    },
  };

  return selector(state);
}
