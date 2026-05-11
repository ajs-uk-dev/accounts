import { Navigate } from 'react-router-dom';
import { useAuth } from '@/lib/auth';
import { routes } from '@/lib/routes';
import type { ReactNode } from 'react';

export function ProtectedRoute({ children }: { children: ReactNode }) {
  const { token } = useAuth();
  if (!token) return <Navigate to={routes.signIn} replace />;
  return <>{children}</>;
}
