import axios from 'axios';
import type { AxiosInstance } from 'axios';

let bearerToken: string | null = null;

export function setAuthToken(token: string | null) { bearerToken = token; }
export function getAuthToken(): string | null { return bearerToken; }

export const api: AxiosInstance = axios.create({ baseURL: '/api' });

api.interceptors.request.use(config => {
  if (bearerToken) {
    config.headers.Authorization = `Bearer ${bearerToken}`;
  }
  return config;
});

export interface RegisterFirmRequest {
  firmName: string;
  firmSlug: string;
  ownerEmail: string;
  ownerPassword: string;
}
export interface RegisterFirmResponse { firmId: string; ownerUserId: string; }

export interface SignInRequest { email: string; password: string; totpCode?: string | null; }
export interface SignInResponse {
  accessToken: string;
  expiresAt: string;
  totpRequired: boolean;
}

export const firms = {
  register: (req: RegisterFirmRequest) =>
    api.post<RegisterFirmResponse>('/firms/register', req).then(r => r.data),
};

export const auth = {
  signIn: (req: SignInRequest) =>
    api.post<SignInResponse>('/auth/sign-in', req).then(r => r.data),
  enrollTotp: () =>
    api.post<{ secret: string; otpAuthUri: string }>('/auth/enroll-totp', {}).then(r => r.data),
  me: () => api.get<{ firmId: string; userId: string; isAuthenticated: boolean }>('/admin/me').then(r => r.data),
};
