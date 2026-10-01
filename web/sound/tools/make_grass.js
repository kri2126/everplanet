// 풀 발소리 WAV 16개를 다시 만드는 스크립트 — node make_grass.js (web/sound/tools 에서 실행)
// 게임(evergreen-3d.html)의 대체 합성기와 같은 코드·같은 씨앗을 쓰므로 결과가 같다.
const { synthGrassStep } = require('./grass_synth.js');
const fs = require('fs'), path = require('path');
const sr = 44100, P = { center: 4200, lp: 8500 };
function wav(file, f) {
  const b = Buffer.alloc(44 + f.length * 2);
  b.write('RIFF', 0); b.writeUInt32LE(36 + f.length * 2, 4); b.write('WAVEfmt ', 8); b.writeUInt32LE(16, 16); b.writeUInt16LE(1, 20); b.writeUInt16LE(1, 22);
  b.writeUInt32LE(sr, 24); b.writeUInt32LE(sr * 2, 28); b.writeUInt16LE(2, 32); b.writeUInt16LE(16, 34); b.write('data', 36); b.writeUInt32LE(f.length * 2, 40);
  for (let i = 0; i < f.length; i++) b.writeInt16LE(Math.max(-32767, Math.min(32767, Math.round(f[i] * 32767))), 44 + i * 2);
  fs.writeFileSync(file, b);
}
const dir = path.join(__dirname, '..', 'grass'); fs.mkdirSync(dir, { recursive: true });
for (let i = 0; i < 16; i++) wav(path.join(dir, 'step_grass_' + String(i + 1).padStart(2, '0') + '.wav'), synthGrassStep((i + 1) * 104729, sr, P));
console.log('grass/step_grass_01~16.wav 생성');
