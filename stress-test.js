// k6 load/stress test for the ECommerceBackend API.
// Run with:  k6 run stress-test.js
// Override target/params via env vars, e.g.:
//   k6 run -e BASE_URL=http://localhost:5202 -e PRODUCT_ID=1 -e VUS=50 -e DURATION=30s stress-test.js

import http from "k6/http";
import { check, sleep } from "k6";

const BASE_URL = __ENV.BASE_URL || "http://localhost:5202";
const PRODUCT_ID = __ENV.PRODUCT_ID || 18;
const QUANTITY = __ENV.QUANTITY || 1;
const VUS = __ENV.VUS ? parseInt(__ENV.VUS) : 50;
const DURATION = __ENV.DURATION || "30s";


export const options = {
  scenarios: {
    concurrent_pressure: {
      executor: "constant-vus",
      vus: VUS,
      duration: DURATION,
    },
  },
};

export default function () {
  const url = `${BASE_URL}/api/products/update-stock-optimistic?productId=${PRODUCT_ID}&quantity=${QUANTITY}`;
  const res = http.post(url);

  check(res, {
    "status is 200": (r) => r.status === 200,
  });

  sleep(0.1);
}

export function teardown() {
  const res = http.get(`${BASE_URL}/api/products`);
  console.log(
    `Post-test stability check: GET /api/products returned status ${res.status} ` +
      (res.status === 200 ? "(server stable)" : "(server NOT responding correctly)")
  );
}
