/**
 * 에버그린 카툰 폭포 모듈 (waterfall.js)
 * ------------------------------------------------------------
 * 폭포 모듈 1개 = 폭 6 × 높이 8(+윗물 0.45). 뒤 절벽 폭 = 폭포 폭이라 count를 늘리면
 * 옆으로 이어 붙어 하나의 넓은 폭포가 된다. 물·물보라는 셰이더 애니메이션이라
 * 이미지/GLB가 아니라 코드 에셋으로 넣는다.
 *
 * 사용법 (three r0.170 기준):
 *   import { createWaterfall } from './assets/waterfall.js';
 *   const wf = createWaterfall(THREE, { count: 3, thick: 1.4, fog: scene.fog });
 *   wf.group.position.copy(발밑 위치); wf.group.quaternion.copy(세울 방향);   // 로컬 +Y = 위, +Z = 물이 떨어지는 앞쪽
 *   wf.group.scale.setScalar(0.5);                                              // 게임 크기에 맞게
 *   scene.add(wf.group);
 *   // 매 프레임: wf.update(dt);
 *
 * 기준점: group 원점 = 폭포 줄 가운데, 절벽 바닥보다 0.3 위(물에 잠기는 높이).
 *        절벽은 z -7 ~ -1, 물이 떨어지는 자리는 z ≈ +0.75.
 * 옵션:
 *   count   이어 붙일 모듈 개수 (1~8, 기본 1)
 *   thick   물줄기 두께 (기본 1.4)
 *   speed   물 속도 배율 (기본 1)
 *   lake    바닥 수면: 'rings'(물결 고리·거품만, 기존 호수 위에 얹기, 기본) | 'full'(파란 수면 포함) | 'none'
 *   fog     THREE.Fog (주면 게임 안개와 같이 흐려짐)
 *   spray / mist / lines   물보라·물안개·낙하선 켜기 (기본 true)
 */
export function createWaterfall(THREE, options = {}) {
  const OPT = { count: 1, thick: 1.4, speed: 1, lake: 'rings', fog: null, spray: true, mist: true, lines: true, ...options };
  const N = Math.max(1, Math.min(8, OPT.count | 0));
  const THICK = OPT.thick;
  const FOG = OPT.fog ? { color: OPT.fog.color, near: OPT.fog.near, far: OPT.fog.far } : { color: new THREE.Color(0xffffff), near: 1e6, far: 2e6 };
  const WATER_DEEP = new THREE.Color(0x3fb6f0), WATER_SHALLOW = new THREE.Color(0x7fd8f7), FOAM = new THREE.Color(0xffffff);
  const ROCK = '#d9b26f', ROCK_DARK = '#a9803f', GRASS = '#7ccf3a';
  const grad = new THREE.DataTexture(new Uint8Array([165, 165, 165, 255, 215, 215, 215, 255, 255, 255, 255, 255]), 3, 1, THREE.RGBAFormat);
  grad.minFilter = grad.magFilter = THREE.NearestFilter; grad.needsUpdate = true;
  const toon = (color, extra = {}) => new THREE.MeshToonMaterial({ color, gradientMap: grad, ...extra });
  const U = { time: { value: 0 }, speed: { value: OPT.speed }, uRootInv: { value: new THREE.Matrix4() } };   // 모든 물 셰이더가 공유
  const MOD_W = 6, MOD_H = 8, MOD_D = 6, POOL_UP = 0.45;
  const anim = { puffs: [], lines: [] };

  const NOISE = `
    float hash(vec2 p){ return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
    float noise(vec2 p){ vec2 i = floor(p), f = fract(p); f = f*f*(3.0-2.0*f);
      return mix(mix(hash(i), hash(i+vec2(1,0)), f.x), mix(hash(i+vec2(0,1)), hash(i+vec2(1,1)), f.x), f.y); }`;
  const FOG_V = `varying float vFogDepth;`;
  const FOG_F = `uniform vec3 fogColor; uniform float fogNear, fogFar; varying float vFogDepth;`;
  const fogUniforms = () => ({ fogColor: { value: FOG.color }, fogNear: { value: FOG.near }, fogFar: { value: FOG.far } });

  // 폭포 물줄기: uv.y=0 위(낙구) → 1 아래. 세로로 길게 늘린 노이즈를 빠르게 내려 보내고, 경계를 딱 끊어 만화 느낌
  function fallMaterial(opts = {}) {
    return new THREE.ShaderMaterial({
      transparent: true, depthWrite: false, side: THREE.DoubleSide,
      uniforms: { ...U, ...fogUniforms(), uDeep: { value: WATER_DEEP }, uShallow: { value: WATER_SHALLOW }, uFoam: { value: FOAM }, uRate: { value: opts.rate ?? 1.0 }, uSeed: { value: opts.seed ?? 0 }, uFadeL: { value: opts.fadeL ?? 1 }, uFadeR: { value: opts.fadeR ?? 1 }, uUseUv: { value: opts.useUv ?? 0 } },
      vertexShader: `${FOG_V} uniform mat4 uRootInv; varying vec2 vUv; varying float vWX;
        void main(){ vUv = uv; vWX = (uRootInv * modelMatrix * vec4(position,1.0)).x; vec4 mv = modelViewMatrix * vec4(position,1.0); vFogDepth = -mv.z; gl_Position = projectionMatrix * mv; }`,
      fragmentShader: `${NOISE} ${FOG_F}
        uniform vec3 uDeep, uShallow, uFoam; uniform float time, speed, uRate, uSeed, uFadeL, uFadeR, uUseUv; varying vec2 vUv; varying float vWX;
        void main(){
          float t = time * speed * uRate;
          float y = vUv.y;
          // 아래로 갈수록 빨라지는 낙하(가속감): y를 비선형으로 늘려 흐름 속도가 아래에서 더 빨라 보이게
          float fy = y * 3.2 + y * y * 2.4;
          // 굵은 물결 줄기 + 가는 빠른 줄기 두 겹
          float wx = mix(vWX / 6.0, vUv.x * 0.25, uUseUv);   // 옆면은 단면 방향(uv)으로 무늬   // 모듈 폭 6 기준 → 이어 붙여도 무늬가 연속
          float a = noise(vec2(wx * 11.0 + uSeed, fy * 0.42 - t * 1.7));          // 세로로 길게 늘어난 굵은 결
          float b = noise(vec2(wx * 28.0 + uSeed * 3.1, fy * 0.8 - t * 3.0));     // 가늘고 더 빠른 결
          vec3 col = mix(uShallow, uDeep, smoothstep(0.0, 0.5, y) * 0.75);
          // 만화식 단계: 진한 결 / 기본 / 흰 줄기
          col = mix(col, uDeep * 0.82, step(a, 0.30) * 0.55);
          float white = step(0.64, a) * 0.85 + step(0.70, b) * 0.9;
          col = mix(col, uFoam, clamp(white, 0.0, 1.0));
          // 낙구(위 가장자리) 흰 띠, 바닥 거품
          col = mix(col, uFoam, smoothstep(0.07, 0.0, y) * 0.9);
          float foamBot = smoothstep(0.80, 0.98, y) * step(0.35, noise(vec2(wx * 14.0, y * 6.0 - t * 3.0)) + (y - 0.8) * 2.0);
          col = mix(col, uFoam, foamBot);
          // 좌우 가장자리: 얇게 흰 테두리 후 투명
          float e = min(mix(1.0, vUv.x, uFadeL), mix(1.0, 1.0 - vUv.x, uFadeR));   // 이어진 쪽은 페이드 없음
          col = mix(col, uFoam, smoothstep(0.06, 0.02, e) * 0.8);
          float alpha = smoothstep(0.0, 0.025, e) * 0.94;
          float f = smoothstep(fogNear, fogFar, vFogDepth);
          gl_FragColor = vec4(mix(col, fogColor, f), alpha);
        }`,
    });
  }
  // 흐르는 수면(웅덩이·윗물): 낙하 지점들(uOrigins)에서 퍼지는 동심 물결 + 흐름 방향 줄무늬.
  // 여러 폭포를 이어 붙여도 호수는 한 장만 깔고 낙하 지점만 늘려서, 물판끼리 겹쳐 깜빡이는 일이 없게 한다.
  function poolMaterial(origins, flow = new THREE.Vector2(0, 0), opts = {}) {
    const list = (Array.isArray(origins) ? origins : [origins]).slice(0, 8);
    const arr = Array.from({ length: 8 }, (_, i) => list[i] || new THREE.Vector2(9999, 9999));
    return new THREE.ShaderMaterial({
      transparent: true, depthWrite: false,
      uniforms: { ...U, ...fogUniforms(), uDeep: { value: WATER_DEEP }, uShallow: { value: WATER_SHALLOW }, uFoam: { value: FOAM }, uOrigins: { value: arr }, uFlow: { value: flow }, uRadius: { value: opts.radius ?? 9 }, uRings: { value: opts.rings ?? 1 }, uAlpha: { value: opts.alpha ?? 0.86 }, uLip: { value: opts.lip ?? 9999 }, uBack: { value: opts.back ?? -9999 } },
      vertexShader: `${FOG_V} uniform mat4 uRootInv; varying vec3 vW;
        void main(){ vec4 w = modelMatrix * vec4(position,1.0); vW = (uRootInv * w).xyz; vec4 mv = viewMatrix * w; vFogDepth = -mv.z; gl_Position = projectionMatrix * mv; }`,
      fragmentShader: `${NOISE} ${FOG_F}
        uniform vec3 uDeep, uShallow, uFoam; uniform vec2 uOrigins[8]; uniform vec2 uFlow; uniform float time, speed, uRadius, uRings, uAlpha, uLip, uBack; varying vec3 vW;
        void main(){
          float t = time * speed;
          vec2 p = vW.xz;
          float dmin = 1e4, rings = 0.0, foam = 0.0;
          for (int i = 0; i < 8; i++) {
            float d = distance(p, uOrigins[i]);
            dmin = min(dmin, d);
            float ring = fract(d * 0.55 - t * 1.4 + float(i) * 0.37);
            if (uOrigins[i].x < 9000.0) rings = max(rings, step(0.86, ring) * smoothstep(uRadius, 2.0, d));
            foam = max(foam, smoothstep(2.6, 1.2, d + (noise(p * 2.5 + t * 1.5) - 0.5) * 1.4));
          }
          vec3 col = mix(uShallow, uDeep, smoothstep(0.0, uRadius * 0.6, dmin));
          vec2 q = p - uFlow * t * 2.2;
          float st = step(0.74, noise(q * vec2(1.6, 0.5) + 7.0));
          col = mix(col, uFoam, rings * 0.75 * uRings + st * 0.12 * min(length(uFlow), 2.5));
          foam *= uRings;
          col = mix(col, uFoam, foam * 0.9);
          // 윗물: 처음 잔잔한 모습 그대로, 낙구 쪽으로 흐르는 잔물결만 더 빠르게.
          // 위치를 phase로 바꿔(낙구 쪽일수록 촘촘함↓ = 빨라 보임) 시간과 곱하지 않으므로 거꾸로 흐르지 않는다.
          if (uLip < 9000.0) {
            float k = clamp((vW.z - uBack) / (uLip - uBack), 0.0, 1.0);        // 0 뒤 → 1 낙구
            float ph = (k - 0.35 * k * k) * (uLip - uBack);                     // 낙구 쪽으로 갈수록 3배 가까이 빨라짐
            vec2 q2 = vec2(vW.x, ph - t * 3.2);
            float st2 = step(0.74, noise(q2 * vec2(1.6, 0.5) + 7.0));
            vec3 c = uDeep;
            c = mix(c, uFoam, st2 * 0.22);
            c = mix(c, uShallow, smoothstep(0.75, 1.0, k) * 0.5);               // 낙구 앞은 살짝 밝게
            col = c;
          }
          float alpha = uAlpha + foam * (1.0 - uAlpha);
          float f = smoothstep(fogNear, fogFar, vFogDepth);
          gl_FragColor = vec4(mix(col, fogColor, f), alpha);
        }`,
    });
  }

  // 절벽에서 떨어지는 물줄기 리본: 낙구에서 살짝 앞으로 튀어나왔다가 수직으로 떨어지는 곡선
  // 물줄기 단면 두께: 낙구에서 0 → 아래로 내려오며 THICK까지 불어남(물이 부풀며 떨어지는 느낌)

  const thickAt = v => THICK * (0.6 + 0.4 * Math.sin(Math.min(v * 2.2, 1) * Math.PI / 2));   // 낙구부터 두툼하게
  const centerZ = (v, push) => push * Math.sin(Math.min(v * 3.0, 1) * Math.PI / 2);
  // 앞(side=+1)/뒤(side=-1) 물막
  function fallRibbon(width, height, push = 0.9, widen = 0.18, rate = 1, seed = 0, opts = {}) {
    const segX = 16, segY = 40, side = opts.side ?? 1;
    const geo = new THREE.PlaneGeometry(1, 1, segX, segY);
    const p = geo.attributes.position, uv = geo.attributes.uv;
    for (let i = 0; i < p.count; i++) {
      const u = uv.getX(i), v = 1 - uv.getY(i);                 // v: 0 위 → 1 아래
      const x = (u - 0.5) * width * (1 + widen * v);
      const y = height * (1 - v);
      const bulge = 1 - 0.35 * Math.pow(Math.abs(u - 0.5) * 2, 6) * ((opts.fadeL && u < 0.5) || (opts.fadeR && u > 0.5) ? 1 : 0);   // 끝 모듈 바깥쪽은 둥글게 오므림
      const z = centerZ(v, push) + side * thickAt(v) / 2 * bulge + Math.sin((x + (opts.ox ?? 0)) * 1.15 + (side < 0 ? 1.7 : 0)) * 0.06;
      p.setXYZ(i, x, y, z);
      uv.setXY(i, u, v);
    }
    geo.computeVertexNormals();
    return new THREE.Mesh(geo, fallMaterial({ rate, seed: seed + (side < 0 ? 5.3 : 0), ...opts }));
  }
  // 줄 맨 끝의 옆면: 앞막과 뒷막 사이를 막아 옆에서 봐도 두툼한 물기둥으로 보이게
  function fallCap(width, height, push, xSign, opts = {}) {
    const segX = 8, segY = 40;
    const geo = new THREE.PlaneGeometry(1, 1, segX, segY);
    const p = geo.attributes.position, uv = geo.attributes.uv;
    for (let i = 0; i < p.count; i++) {
      const u = uv.getX(i), v = 1 - uv.getY(i);               // u: 뒤(0) → 앞(1)
      const th = thickAt(v), s = u * 2 - 1;
      const x = xSign * (width / 2 - 0.35 * th / THICK * (1 - Math.pow(Math.abs(s), 2)) * 0 ) ;
      const z = centerZ(v, push) + s * th / 2;
      p.setXYZ(i, x + xSign * 0.0, height * (1 - v), z);
      uv.setXY(i, u, v);
    }
    geo.computeVertexNormals();
    return new THREE.Mesh(geo, fallMaterial({ ...opts, fadeL: 0, fadeR: 0, useUv: 1 }));
  }

  // 카툰 바위 절벽(울퉁불퉁 박스 + 위 잔디)
  function cliff(w, h, d) {
    const g = new THREE.Group();
    const geo = new THREE.BoxGeometry(w, h, d, 12, 10, 6); geo.translate(0, h / 2, 0);
    const p = geo.attributes.position;
    for (let i = 0; i < p.count; i++) {
      const x = p.getX(i), y = p.getY(i), z = p.getZ(i);
      const k = Math.sin(x * 1.7) * Math.cos(y * 1.3) * 0.25 + Math.sin(y * 2.9 + x) * 0.12;
      p.setXYZ(i, x, y, z + (z > d / 2 - 0.01 ? k : 0));
    }
    geo.computeVertexNormals();
    const rock = new THREE.Mesh(geo, toon(ROCK)); rock.castShadow = rock.receiveShadow = true; g.add(rock);
    // 바위 층 결(진한 띠)
    for (let k = 1; k < 4; k++) {
      const band = new THREE.Mesh(new THREE.BoxGeometry(w, 0.14, d + 0.08), toon(ROCK_DARK));   // 폭은 절벽과 같게(이어 붙일 때 겹침 방지)
      band.position.y = h * k / 4 + Math.sin(k) * 0.3; g.add(band);
    }
    const top = new THREE.Mesh(new THREE.BoxGeometry(w, 0.35, d + 0.3), toon(GRASS)); top.position.y = h + 0.12; top.receiveShadow = true; g.add(top);
    return g;
  }
  // 물보라 덩어리(하얀 카툰 구름)
  function foamPuffs(radius, count) {
    const g = new THREE.Group(), mat = toon('#ffffff');
    for (let i = 0; i < count; i++) {
      const a = (i / count) * Math.PI * 2, r = radius * (0.6 + (i % 3) * 0.2);
      const m = new THREE.Mesh(new THREE.SphereGeometry(0.45 + (i % 4) * 0.12, 16, 12), mat);
      m.position.set(Math.cos(a) * r, 0.15, Math.sin(a) * r * 0.55 + 0.4);
      m.userData.ph = i * 1.37; g.add(m);
    }
    g.userData.puffs = true;
    return g;
  }

  // 튀는 물방울(위로 솟았다 떨어짐) + 피어오르는 물안개
  function sprayParticles(n, spread) {
    const geo = new THREE.BufferGeometry();
    const pos = new Float32Array(n * 3), seed = new Float32Array(n);
    for (let i = 0; i < n; i++) seed[i] = Math.random();
    geo.setAttribute('position', new THREE.BufferAttribute(pos, 3)); geo.setAttribute('seed', new THREE.BufferAttribute(seed, 1));
    const mat = new THREE.ShaderMaterial({
      transparent: true, depthWrite: false,
      uniforms: { ...U, uSpread: { value: spread } },
      vertexShader: `attribute float seed; uniform float time, speed, uSpread; varying float vA;
        void main(){
          float life = fract(time * speed * 0.9 + seed * 7.3);
          float ang = seed * 6.2831 * 3.0;
          vec3 v = vec3(cos(ang) * (0.6 + seed) * uSpread, 3.2 + seed * 2.0, sin(ang) * 0.6 * uSpread + 0.9);
          vec3 p = vec3((seed - 0.5) * uSpread * 1.6, 0.0, 0.3) + v * life * 0.9 + vec3(0.0, -6.0, 0.0) * life * life * 0.9;
          vA = 1.0 - life;
          vec4 mv = modelViewMatrix * vec4(p, 1.0);
          gl_PointSize = (10.0 + seed * 10.0) * (20.0 / -mv.z);
          gl_Position = projectionMatrix * mv;
        }`,
      fragmentShader: `varying float vA; void main(){ vec2 c = gl_PointCoord - 0.5; if (dot(c,c) > 0.25) discard; gl_FragColor = vec4(1.0, 1.0, 1.0, vA); }`,
    });
    const pts = new THREE.Points(geo, mat); pts.frustumCulled = false; pts.userData.spray = true;
    return pts;
  }
  function mistParticles(n, spread) {
    const geo = new THREE.BufferGeometry();
    const pos = new Float32Array(n * 3), seed = new Float32Array(n);
    for (let i = 0; i < n; i++) seed[i] = Math.random();
    geo.setAttribute('position', new THREE.BufferAttribute(pos, 3)); geo.setAttribute('seed', new THREE.BufferAttribute(seed, 1));
    const mat = new THREE.ShaderMaterial({
      transparent: true, depthWrite: false,
      uniforms: { ...U, uSpread: { value: spread } },
      vertexShader: `attribute float seed; uniform float time, speed, uSpread; varying float vA;
        void main(){
          float life = fract(time * speed * 0.25 + seed * 5.1);
          vec3 p = vec3((seed - 0.5) * uSpread * 2.0 + sin(seed * 40.0 + life * 3.0) * 0.6, life * 3.5, 0.8 + fract(seed * 9.7) * 1.5);
          vA = sin(life * 3.1416) * 0.45;
          vec4 mv = modelViewMatrix * vec4(p, 1.0);
          gl_PointSize = (90.0 + seed * 80.0) * (1.0 + life) * (10.0 / -mv.z);
          gl_Position = projectionMatrix * mv;
        }`,
      fragmentShader: `varying float vA; void main(){ vec2 c = gl_PointCoord - 0.5; float d = dot(c,c); if (d > 0.25) discard; gl_FragColor = vec4(1.0, 1.0, 1.0, vA * smoothstep(0.25, 0.05, d)); }`,
    });
    const pts = new THREE.Points(geo, mat); pts.frustumCulled = false; pts.userData.mist = true;
    return pts;
  }
  // 물줄기 앞을 빠르게 지나가는 흰 낙하선(속도감)
  function speedLines(width, height, n, pushZ) {
    const g = new THREE.Group(); g.userData.lines = true;
    const mat = new THREE.MeshBasicMaterial({ color: '#ffffff', transparent: true, opacity: 0.85, depthWrite: false });
    for (let i = 0; i < n; i++) {
      const len = 0.6 + Math.random() * 1.2;
      const m = new THREE.Mesh(new THREE.PlaneGeometry(0.06 + Math.random() * 0.05, len), mat);
      m.userData = { x: (Math.random() - 0.5) * width, ph: Math.random(), len, h: height, z: pushZ + 0.06 + Math.random() * 0.1, rate: 0.9 + Math.random() * 0.6 };
      g.add(m);
    }
    return g;
  }


  function addBase(group, x, z, width, y = -0.3) {
    const puffs = foamPuffs(width * 0.45, Math.max(6, Math.round(width * 3))); puffs.position.set(x, y, z); puffs.visible = OPT.spray; group.add(puffs); anim.puffs.push(puffs);
    const spray = sprayParticles(70 + Math.round(width * 25), width * 0.5); spray.position.set(x, y, z); spray.visible = OPT.spray; group.add(spray);
    const mist = mistParticles(24 + Math.round(width * 6), width * 0.5); mist.position.set(x, y, z); mist.visible = OPT.mist; group.add(mist);
  }
  function waterfallModule(ox, first, last) {
    const g = new THREE.Group(); g.position.x = ox;
    const c = cliff(MOD_W, MOD_H, MOD_D); c.position.set(0, -0.3, -4); g.add(c);
    const PUSH = 1.1 + THICK / 2;
    const fo = { ox, fadeL: first ? 1 : 0, fadeR: last ? 1 : 0 };
    const FALL_H = MOD_H + POOL_UP;
    const lipZ = -1.05 + thickAt(0) / 2;
    const backZ = -6.5, poolY = -0.3 + FALL_H;
    const pool = new THREE.Mesh(new THREE.PlaneGeometry(MOD_W, lipZ - backZ), poolMaterial(new THREE.Vector2(ox, -40), new THREE.Vector2(0, 0), { radius: 0.1, alpha: 0.97, rings: 0, lip: lipZ, back: backZ }));
    pool.rotation.x = -Math.PI / 2; pool.position.set(0, poolY, (lipZ + backZ) / 2); pool.renderOrder = 1; g.add(pool);
    const bankMat = toon(ROCK), bankTop = toon(GRASS);
    const bank = (w, d, x, z) => {
      const b = new THREE.Mesh(new THREE.BoxGeometry(w, POOL_UP + 0.45, d), bankMat); b.position.set(x, poolY - (POOL_UP + 0.45) / 2 + 0.3, z); b.castShadow = true; g.add(b);
      const t = new THREE.Mesh(new THREE.BoxGeometry(w, 0.18, d), bankTop); t.position.set(x, poolY + 0.39, z); g.add(t);
    };
    bank(MOD_W, 0.7, 0, backZ - 0.35);
    const sideD = (lipZ - 0.45) - (backZ - 0.7);
    if (first) bank(0.6, sideD, -MOD_W / 2 + 0.3, (backZ - 0.7) + sideD / 2);
    if (last) bank(0.6, sideD, MOD_W / 2 - 0.3, (backZ - 0.7) + sideD / 2);
    const back = fallRibbon(MOD_W, FALL_H, PUSH, 0, 0.9, 0.3, { ...fo, side: -1 }); back.position.set(0, -0.3, -1.05); back.renderOrder = 2; g.add(back);
    const f = fallRibbon(MOD_W, FALL_H, PUSH, 0, 1.0, 0.3, { ...fo, side: 1 }); f.position.set(0, -0.3, -1.05); f.renderOrder = 3; g.add(f);
    if (first) { const cp = fallCap(MOD_W, FALL_H, PUSH, -1, { ox, rate: 0.95, seed: 2.1 }); cp.position.copy(f.position); cp.renderOrder = 2; g.add(cp); }
    if (last) { const cp = fallCap(MOD_W, FALL_H, PUSH, 1, { ox, rate: 0.95, seed: 2.9 }); cp.position.copy(f.position); cp.renderOrder = 2; g.add(cp); }
    const sl = speedLines(MOD_W * 0.92, FALL_H, 26, PUSH + THICK / 2); sl.position.copy(f.position); sl.visible = OPT.lines; g.add(sl); anim.lines.push(sl);
    addBase(g, 0, -1.05 + PUSH, MOD_W);
    return g;
  }

  // ---------- 조립 ----------
  const group = new THREE.Group(); group.name = 'waterfall';
  const origins = [];
  for (let i = 0; i < N; i++) {
    const ox = (i - (N - 1) / 2) * MOD_W;
    group.add(waterfallModule(ox, i === 0, i === N - 1));
    origins.push(new THREE.Vector2(ox, -1.05 + 1.1 + THICK / 2));
  }
  if (OPT.lake !== 'none') {
    const full = OPT.lake === 'full';
    const mat = poolMaterial(origins, new THREE.Vector2(0, 1), { alpha: full ? 0.86 : 0.0 });
    const lk = new THREE.Mesh(full ? new THREE.PlaneGeometry(26 + MOD_W * (N - 1), 26) : new THREE.PlaneGeometry(14 + MOD_W * (N - 1), 14), mat);
    lk.rotation.x = -Math.PI / 2; lk.position.set(0, full ? -0.3 : -0.25, full ? 7 : 3.5); lk.renderOrder = 1; group.add(lk);
    if (!full) { // 물결 고리·거품만 보이게: 흰 부분만 불투명
      mat.fragmentShader = mat.fragmentShader.replace('float alpha = uAlpha + foam * (1.0 - uAlpha);', 'float alpha = max(rings * 0.75, foam * 0.9);');
    }
  }

  return {
    group,
    width: MOD_W * N,
    /** 매 프레임 호출. dt = 초 */
    update(dt) {
      U.time.value += dt;
      group.updateMatrixWorld();
      U.uRootInv.value.copy(group.matrixWorld).invert();
      const t = U.time.value, s = U.speed.value;
      for (const g of anim.puffs) g.children.forEach(m => { const k = 1 + Math.sin(t * 5 * s + m.userData.ph) * 0.18; m.scale.set(k, k * 0.8, k); });
      for (const g of anim.lines) g.children.forEach(m => {
        const u = m.userData, life = (t * s * 0.8 * u.rate + u.ph) % 1;
        const y = u.h * (1 - life * life);
        m.position.set(u.x, y - u.len / 2, u.z * Math.sin(Math.min((1 - y / u.h) * 3, 1) * Math.PI / 2));
        m.scale.y = 0.6 + life * 1.2;
        m.visible = y > 0.5;
      });
    },
    setSpeed(v) { U.speed.value = v; },
    dispose() { group.traverse(o => { if (o.geometry) o.geometry.dispose(); if (o.material) [].concat(o.material).forEach(m => m.dispose()); }); grad.dispose(); },
  };
}
