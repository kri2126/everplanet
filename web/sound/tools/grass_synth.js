// 풀 발소리 합성기 (v4C) — 가벼운 풀: 소리 중심을 올리고 저음을 빼고, 걸음마다 성격이 다르게. make_grass.js 와 게임 대체 합성기가 같은 코드
function sfxRng(seed) { let a = seed >>> 0; return () => { a = (a + 0x6D2B79F5) >>> 0; let t = a; t = Math.imul(t ^ (t >>> 15), t | 1); t ^= t + Math.imul(t ^ (t >>> 7), t | 61); return ((t ^ (t >>> 14)) >>> 0) / 4294967296; }; }
function sfxBiquad(type, f, q, sr) {
  const w = 2 * Math.PI * f / sr, c = Math.cos(w), s = Math.sin(w), al = s / (2 * q);
  let b0, b1, b2, a0 = 1 + al, a1 = -2 * c, a2 = 1 - al;
  if (type === 'bp') { b0 = al; b1 = 0; b2 = -al; }
  else if (type === 'hp') { b0 = (1 + c) / 2; b1 = -(1 + c); b2 = (1 + c) / 2; }
  else { b0 = (1 - c) / 2; b1 = 1 - c; b2 = (1 - c) / 2; }
  let x1 = 0, x2 = 0, y1 = 0, y2 = 0;
  return (x) => { const y = (b0 * x + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2) / a0; x2 = x1; x1 = x; y2 = y1; y1 = y; return y; };
}
// P.center: 소리 중심(Hz), P.lp: 윗선(Hz)
function synthGrassStep(seed, sr, P) {
  const r = sfxRng(seed), n = Math.floor(sr * 0.22), out = new Float32Array(n);
  const fc = P.center * (0.8 + r() * 0.45);                       // 걸음마다 음 높이가 다르게
  const bp = sfxBiquad('bp', fc, 0.5 + r() * 0.3, sr), hp = sfxBiquad('hp', 700, 0.7, sr);
  const bp2 = sfxBiquad('bp', fc * (1.5 + r() * 0.5), 0.9, sr);   // 위쪽 결 한 겹
  const mix2 = 0.2 + r() * 0.45;
  // 덩이 수·간격·길이도 걸음마다 다르게: 1~3덩이(스윽 / 스-윽 / 스스윽)
  const lumps = [], k = r() < 0.25 ? 1 : r() < 0.7 ? 2 : 3;
  let t0 = 0;
  for (let j = 0; j < k; j++) { lumps.push({ t: t0, a: j === 0 ? 1 : 0.35 + r() * 0.5, at: 0.012 + r() * 0.015, tau: 0.025 + r() * 0.03 }); t0 += 0.025 + r() * 0.04; }
  for (let i = 0; i < n; i++) {
    const t = i / sr; let e = 0;
    for (const L of lumps) if (t > L.t) { const u = t - L.t; e += L.a * Math.sin(Math.min(1, u / L.at) * Math.PI / 2) * Math.exp(-u / L.tau); }
    // 잔결: 엔벨로프에 아주 약한 흔들림(풀잎이 하나하나 스치는 느낌)
    e *= 0.85 + 0.15 * Math.sin(2 * Math.PI * (40 + fc / 100) * t + seed);
    const w = r() * 2 - 1;
    out[i] = (hp(bp(w)) + bp2(w) * mix2) * e;
  }
  const grains = 3 + Math.floor(r() * 6);                           // 여린 잎 스침 알갱이(작게)
  for (let g = 0; g < grains; g++) {
    const st = Math.floor(sr * r() * 0.12), len = Math.floor(sr * (0.003 + r() * 0.005)), amp = 0.02 + r() * 0.04;
    const gh = sfxBiquad('bp', P.center * (1.4 + r() * 1.2), 1.0, sr);
    for (let i = 0; i < len && st + i < n; i++) out[st + i] += gh(r() * 2 - 1) * amp * Math.sin(Math.PI * i / len);
  }
  const lp1 = sfxBiquad('lp', P.lp, 0.7, sr), lp2 = sfxBiquad('lp', P.lp * 1.3, 0.7, sr);
  for (let i = 0; i < n; i++) out[i] = lp2(lp1(out[i]));
  let pk = 0; for (let i = 0; i < n; i++) { if (i > n - sr * 0.03) out[i] *= (n - i) / (sr * 0.03); pk = Math.max(pk, Math.abs(out[i])); }
  for (let i = 0; i < n; i++) out[i] *= 0.9 / pk;
  return out;
}
if (typeof module !== 'undefined') module.exports = { synthGrassStep };
