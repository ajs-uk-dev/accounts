import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { auth } from '@/lib/api';
import { useAuth } from '@/lib/auth';
import { routes } from '@/lib/routes';

export function SignInPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [totpCode, setTotpCode] = useState('');
  const [needsTotp, setNeedsTotp] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { signIn } = useAuth();
  const navigate = useNavigate();

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      const result = await auth.signIn({ email, password, totpCode: needsTotp ? totpCode : null });
      if (result.totpRequired) { setNeedsTotp(true); return; }
      signIn(result.accessToken, result.expiresAt);
      navigate(routes.dashboard);
    } catch (err: unknown) {
      const is401 = (
        err &&
        typeof err === 'object' &&
        'response' in err &&
        err.response &&
        typeof err.response === 'object' &&
        'status' in err.response &&
        (err.response as { status: unknown }).status === 401
      );
      setError(is401 ? 'Invalid credentials.' : 'Sign-in failed.');
    }
  }

  return (
    <div className="mx-auto max-w-md">
      <h1 className="mb-6 text-2xl font-semibold">Sign in</h1>
      <form className="space-y-4" onSubmit={submit}>
        <label className="block">
          <span className="mb-1 block text-sm">Email</span>
          <input className="w-full rounded border px-3 py-2" type="email"
            value={email} onChange={e => setEmail(e.target.value)} required />
        </label>
        <label className="block">
          <span className="mb-1 block text-sm">Password</span>
          <input className="w-full rounded border px-3 py-2" type="password"
            value={password} onChange={e => setPassword(e.target.value)} required />
        </label>
        {needsTotp && (
          <label className="block">
            <span className="mb-1 block text-sm">6-digit authenticator code</span>
            <input className="w-full rounded border px-3 py-2 font-mono tracking-widest"
              inputMode="numeric" maxLength={6}
              value={totpCode} onChange={e => setTotpCode(e.target.value.replace(/\D/g, ''))} />
          </label>
        )}
        {error && <p className="text-sm text-red-700">{error}</p>}
        <button type="submit" className="w-full rounded bg-slate-900 px-4 py-2 text-white">
          {needsTotp ? 'Verify and sign in' : 'Sign in'}
        </button>
      </form>
    </div>
  );
}
