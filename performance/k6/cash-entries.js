import http from 'k6/http';
import { check } from 'k6';

export const options = {
  scenarios: {
    cash_entries: {
      executor: 'constant-arrival-rate', rate: 50, timeUnit: '1s',
      duration: __ENV.DURATION || '60s', preAllocatedVUs: 25, maxVUs: 100,
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<500', 'p(99)<1000'],
    checks: ['rate>0.95'],
  },
};

const baseUrl = __ENV.BASE_URL || 'http://host.docker.internal:8080';

export default function () {
  const payload = JSON.stringify({
    type: Math.random() < 0.7 ? 'credit' : 'debit',
    amount: Math.floor(Math.random() * 100000 + 1) / 100,
    description: `Teste de carga k6 VU ${__VU}`,
    occurredAt: new Date(Date.now() - 1000).toISOString(),
  });
  const response = http.post(`${baseUrl}/api/v1/cash-entries`, payload, {
    headers: { 'Content-Type': 'application/json' },
  });
  check(response, { 'lançamento aceito': (result) => result.status === 201 });
}
