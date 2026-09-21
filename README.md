# 에버플래닛 프로토타입 (Everplanet Prototype)

🎮 **[지금 바로 플레이하기](https://kri2126.github.io/everplanet/web/evergreen-3d.html)** — 설치 없이 브라우저에서 바로 실행됩니다. (로드맵 6, three.js 2.5D 판)

이전 2D 구면 투영 판(로드맵 1~5)은 [evergreen-exploration.html](https://kri2126.github.io/everplanet/web/evergreen-exploration.html)에서 그대로 플레이할 수 있습니다.

넥슨 서비스 종료 후 러쉬에잇이 IP 개방 프로젝트로 재개발 중인 캐주얼 MMORPG '에버플래닛'의
첫 번째 무대인 위성 에버그린을 개인 학습 목적으로 다시 구현해보는 싱글플레이어 프로토타입입니다.

여러 행성을 오가는 원작의 전체 구조 대신, 에버그린 한 무대 안에서 이동·탐험·전투·성장이
완결되는 경험을 만드는 데 집중하고 있습니다.

## 폴더 구조

- [`NEXON_IP_PARTNER_INTRO.md`](./NEXON_IP_PARTNER_INTRO.md) — 프로젝트 소개글(넥슨 IP 라이선스 활용 파트너 신청서 초안 겸용). 이 프로젝트를 만든 이유, 플레이 링크, 포트폴리오 요약, 기획서를 한 번에 정리했습니다.

- `roadmap/` — 단계별 기획서 폴더. 작업 단위가 확정될 때마다 번호를 붙여 쌓아갑니다.
  - [`roadmap/ROADMAP_1.md`](./roadmap/ROADMAP_1.md) — 1단계: 이동·환경·카메라 (완료)
  - [`roadmap/ROADMAP_2.md`](./roadmap/ROADMAP_2.md) — 2단계: 미니맵·전체 지도(M)·랜드마크 (완료)
  - [`roadmap/ROADMAP_3.md`](./roadmap/ROADMAP_3.md) — 3단계: 완주 보상(전체 지도 공개)·포탈·두 번째 구역 (포탈 1차 구현 완료·나머지 예정)
  - [`roadmap/ROADMAP_4.md`](./roadmap/ROADMAP_4.md) — 4단계: 무대를 에버그린으로 확정하고 원작 8개 지역 구조로 세분화 (설계 완료·구현 예정)
  - [`roadmap/ROADMAP_5.md`](./roadmap/ROADMAP_5.md) — 5단계: 별내림 천문대를 원작 영상 기준으로 다시 짓기 (완료)
  - [`roadmap/ROADMAP_6.md`](./roadmap/ROADMAP_6.md) — 6단계: three.js로 2.5D 재구축 — 빌보드 그림 판, HUD, NPC·슬라임, 착륙 컷신 (완료)
- `web/evergreen-3d.html` — **현재 플레이 판**(로드맵 6). three.js 구체 지형 위에 2D 그림 판(빌보드)을 세운 2.5D 구성. 고정 카메라, 원형 미니맵·M 지도·MM 전경도, HUD, NPC 3·슬라임 8·기본 공격, 착륙 컷신.
- `versions/` — 로드맵 번호 = 버전 번호로 보관한 옛 판(`v0.05-roadmap5/`, `v0.06-roadmap6/`). 보관본은 수정하지 않습니다.
- `web/assets/` — 3D판이 쓰는 그림 에셋(PNG)과 규격서. 같은 이름으로 덮어쓰면 게임이 바꿔 끼웁니다 — [`web/assets/README.md`](./web/assets/README.md).
- `web/evergreen-exploration.html` — 로드맵 1~5의 2D 판(HTML5 Canvas, 바닐라 JS). 원작 실제 플레이 영상을 참고해 구현한
  '작은 행성' 정구형 투영 렌더링(둥근 지평선 너머는 걸어가야 드러남)이 핵심 특징이었고, 그대로 보존합니다.
- `DESIGN_DOC.md` — 기획 문서(세계관, 핵심 루프, 시스템 설계, 구현 단계).
- `PHASE1_QUICKSTART.md` — Unity 에디터로 1단계(이동+카메라+환경)만 구성하는 가이드.
- `README_SETUP.md` — Unity 프로젝트 전체 세팅 가이드.
- `Assets/Scripts/` — Unity(C#) 기반으로 설계했던 핵심 시스템 스크립트(플레이어, 성장, 전투,
  카메라, 저장 등). 현재 플레이 가능한 버전은 웹(HTML5) 버전이며, 이 스크립트들은 추후
  포트폴리오/확장용으로 보관 중입니다.

## 진행 현황

로드맵 6(three.js 2.5D 재구축)까지 완성. 별내림 천문대 한 구역에서 착륙 컷신 → NPC 퀘스트 3 → 슬라임 퇴치 → 별 조각 찾기가 이어집니다.
이후 전투 확장 → 성장 시스템 → 에버그린 내 다음 지역(겸손한 사원 등) → 저장 순으로 이어갈 예정입니다.
