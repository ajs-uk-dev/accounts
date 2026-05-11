import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { setAuthToken } from './api';

interface AuthState {
  token: string | null;
  expiresAt: Date | null;
}

interface AuthContextValue extends AuthState {
  signIn: (token: string, expiresAt: string) => void;
  signOut: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

const STORAGE_KEY = 'accounts.auth';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>(() => {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      if (!raw) return { token: null, expiresAt: null };
      const parsed = JSON.parse(raw) as { token: string; expiresAt: string };
      const exp = new Date(parsed.expiresAt);
      if (exp.getTime() <= Date.now()) return { token: null, expiresAt: null };
      return { token: parsed.token, expiresAt: exp };
    } catch { return { token: null, expiresAt: null }; }
  });

  useEffect(() => { setAuthToken(state.token); }, [state.token]);

  const value = useMemo<AuthContextValue>(() => ({
    ...state,
    signIn: (token, expiresAt) => {
      const exp = new Date(expiresAt);
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify({ token, expiresAt }));
      setState({ token, expiresAt: exp });
    },
    signOut: () => {
      sessionStorage.removeItem(STORAGE_KEY);
      setState({ token: null, expiresAt: null });
    },
  }), [state]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
