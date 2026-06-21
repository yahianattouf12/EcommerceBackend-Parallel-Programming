// Simple k6 pressure test - no setup/teardown, no business logic, just raw concurrent load
// on a basic read endpoint.
// Run with:  k6 run stress-test-simple.js
// Override via env vars: k6 run -e BASE_URL=http://localhost:5202 -e VUS=50 -e DURATION=20s stress-test-simple.js

import http from "k6/http";
import { check } from "k6";

const BASE_URL = __ENV.BASE_URL || "http://localhost:5202";
const VUS = __ENV.VUS ? parseInt(__ENV.VUS) : 50;
const DURATION = __ENV.DURATION || "20s";

export const options = {
  vus: VUS,
  duration: DURATION,
};

export default function () {
  const res = http.get(`${BASE_URL}/api/products`);
  check(res, {
    "status is 200": (r) => r.status === 200,
  });
}
