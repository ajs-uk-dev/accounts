import { useQuery } from '@tanstack/react-query';
import { auth } from '@/lib/api';
import { Link } from 'react-router-dom';
import { routes } from '@/lib/routes';

export function DashboardPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['me'], queryFn: auth.me,
  });

  return (
    <div>
      <h1 className="mb-4 text-2xl font-semibold">Dashboard</h1>
      {isLoading && <p>Loading…</p>}
      {error && <p className="text-red-700">Could not load profile.</p>}
      {data && (
        <dl className="grid grid-cols-2 gap-x-4 gap-y-2 text-sm">
          <dt className="font-semibold">Firm ID</dt><dd className="font-mono">{data.firmId}</dd>
          <dt className="font-semibold">User ID</dt><dd className="font-mono">{data.userId}</dd>
        </dl>
      )}
      <p className="mt-6">
        <Link className="underline" to={routes.enrollTotp}>Enrol MFA</Link>
      </p>
    </div>
  );
}
