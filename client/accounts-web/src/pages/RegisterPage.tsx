import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { firms } from '@/lib/api';
import { routes } from '@/lib/routes';

export function RegisterPage() {
  const [firmName, setFirmName] = useState('');
  const [firmSlug, setFirmSlug] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      await firms.register({
        firmName, firmSlug, ownerEmail: email, ownerPassword: password,
      });
      navigate(routes.signIn);
    } catch (err: unknown) {
      const message = (
        err &&
        typeof err === 'object' &&
        'response' in err &&
        err.response &&
        typeof err.response === 'object' &&
        'data' in err.response &&
        err.response.data &&
        typeof err.response.data === 'object' &&
        'error' in err.response.data &&
        typeof (err.response.data as { error: unknown }).error === 'string'
      )
        ? (err.response.data as { error: string }).error
        : 'Registration failed';
      setError(message);
    }
  }

  return (
    <div className="mx-auto max-w-md">
      <h1 className="mb-6 text-2xl font-semibold">Register your firm</h1>
      <form className="space-y-4" onSubmit={submit}>
        <Field label="Firm name" value={firmName} onChange={setFirmName} />
        <Field label="Firm slug (kebab-case)" value={firmSlug} onChange={setFirmSlug} />
        <Field label="Owner email" value={email} onChange={setEmail} type="email" />
        <Field label="Password (min 12 chars)" value={password} onChange={setPassword} type="password" />
        {error && <p className="text-sm text-red-700">{error}</p>}
        <button type="submit" className="w-full rounded bg-slate-900 px-4 py-2 text-white">
          Register
        </button>
      </form>
    </div>
  );
}

function Field({ label, value, onChange, type = 'text' }: {
  label: string; value: string; onChange: (v: string) => void; type?: string;
}) {
  return (
    <label className="block">
      <span className="mb-1 block text-sm">{label}</span>
      <input
        className="w-full rounded border px-3 py-2"
        type={type} value={value}
        onChange={e => onChange(e.target.value)}
        required />
    </label>
  );
}
