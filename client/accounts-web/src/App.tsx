import { Routes, Route, Navigate } from 'react-router-dom';
import { TopNav } from '@/components/TopNav';
import { ProtectedRoute } from '@/components/ProtectedRoute';
import { RegisterPage } from '@/pages/RegisterPage';
import { SignInPage } from '@/pages/SignInPage';
import { EnrollTotpPage } from '@/pages/EnrollTotpPage';
import { DashboardPage } from '@/pages/DashboardPage';
import { routes } from '@/lib/routes';

export default function App() {
  return (
    <>
      <TopNav />
      <main className="mx-auto max-w-6xl p-6">
        <Routes>
          <Route path={routes.root} element={<Navigate to={routes.signIn} replace />} />
          <Route path={routes.register} element={<RegisterPage />} />
          <Route path={routes.signIn} element={<SignInPage />} />
          <Route path={routes.enrollTotp}
            element={<ProtectedRoute><EnrollTotpPage /></ProtectedRoute>} />
          <Route path={routes.dashboard}
            element={<ProtectedRoute><DashboardPage /></ProtectedRoute>} />
        </Routes>
      </main>
    </>
  );
}
