import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/lib/auth';
import { routes } from '@/lib/routes';

export function TopNav() {
  const { token, signOut } = useAuth();
  const navigate = useNavigate();
  return (
    <header className="border-b bg-white">
      <nav className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
        <Link to={routes.root} className="text-lg font-semibold">Accounts</Link>
        <div className="flex items-center gap-4 text-sm">
          {token ? (
            <>
              <Link to={routes.dashboard}>Dashboard</Link>
              <button
                className="rounded border px-3 py-1 hover:bg-slate-100"
                onClick={() => { signOut(); navigate(routes.signIn); }}>
                Sign out
              </button>
            </>
          ) : (
            <>
              <Link to={routes.signIn}>Sign in</Link>
              <Link to={routes.register} className="rounded bg-slate-900 px-3 py-1 text-white">
                Register firm
              </Link>
            </>
          )}
        </div>
      </nav>
    </header>
  );
}
