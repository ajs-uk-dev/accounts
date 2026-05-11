import { useEffect, useState } from 'react';
import { auth } from '@/lib/api';

export function EnrollTotpPage() {
  const [secret, setSecret] = useState<string | null>(null);
  const [uri, setUri] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    auth.enrollTotp()
      .then(r => { setSecret(r.secret); setUri(r.otpAuthUri); })
      .catch(() => setError('Could not enroll TOTP.'));
  }, []);

  return (
    <div className="mx-auto max-w-md">
      <h1 className="mb-4 text-2xl font-semibold">Enrol TOTP MFA</h1>
      {error && <p className="text-sm text-red-700">{error}</p>}
      {secret && (
        <div className="space-y-3">
          <p className="text-sm">
            Scan this URI in your authenticator app (Microsoft Authenticator, 1Password, etc.),
            or enter the secret manually.
          </p>
          <pre className="rounded border bg-slate-50 p-3 text-xs break-all">{uri}</pre>
          <p className="text-sm">Secret (manual entry): <span className="font-mono">{secret}</span></p>
          <p className="text-sm text-slate-600">
            On your next sign-in, you'll be prompted for a 6-digit code from your app.
          </p>
        </div>
      )}
    </div>
  );
}
