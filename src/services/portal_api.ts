import axios from 'axios';

const portalApi = axios.create({
  baseURL: (import.meta as any).env?.VITE_PORTAL_API_URL || '/api',
  headers: { 'Content-Type': 'application/json' },
});

portalApi.interceptors.request.use((config) => {
  const token = localStorage.getItem('portal_token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

portalApi.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem('portal_token');
      window.location.href = '/portal/login';
    }
    return Promise.reject(err);
  }
);

export default portalApi;
