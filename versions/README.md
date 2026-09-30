# 버전 보관 (versions/)

로드맵 번호 = 버전 번호입니다. 로드맵 N이 끝난 시점의 플레이 파일을 `v0.0N-roadmapN/` 폴더에 그대로 복제해 둡니다.
시행착오로 최신 파일이 바뀌어도 이전 판은 여기서 언제든 다시 열 수 있습니다.

| 버전 | 내용 | 파일 | 플레이 |
| --- | --- | --- | --- |
| v0.01 ~ v0.04 | 로드맵 1~4 (아스마라 → 에버그린 전환, 미니맵·랜드마크·그림책풍 UI) | 별도 보관 없음 — 깃허브 History에서 커밋별로 열람 가능 | — |
| **v0.05** | 로드맵 5 완료본 — 2D 구면 투영판. 높이·점프, 물, 광장·다리·계단, 길, 떨어진 별, 초목, 생활의 흔적, 광장 소품까지 | `v0.05-roadmap5/evergreen-exploration.html` (커밋 20faef7과 동일) | https://kri2126.github.io/everplanet/versions/v0.05-roadmap5/evergreen-exploration.html |
| **v0.06** | 로드맵 6 완료본 — three.js 3D판(빌보드 2.5D). 구체 지형, 고정 카메라, 원형 미니맵·M 지도·MM 전경도, HUD, NPC 3·슬라임 8·기본 공격, 착륙 컷신 | `v0.06-roadmap6/evergreen-3d.html` (커밋 8653671과 동일) | https://kri2126.github.io/everplanet/versions/v0.06-roadmap6/evergreen-3d.html |
| **v0.07** | 로드맵 7 완료본 — 원작의 게임 방식: 2단 점프·ZXCV 슬롯·수영·오르기, 몬스터 2종 전투·사망/부활, 레벨·기력·무직 스킬·드롭·포션, 행성/일반 퀘스트, 이슬·채집·소·밭·모험 기록, 포탈·저장/불러오기·시작 화면, 목표 줄·키 안내·채팅 | `v0.07-roadmap7/evergreen-3d.html` (커밋 c1c2f22와 동일 — 그림 에셋만 최신 `web/assets/`를 읽음) | https://kri2126.github.io/everplanet/versions/v0.07-roadmap7/evergreen-3d.html |

규칙
- 보관본은 절대 수정하지 않습니다. 고칠 게 있으면 `web/`의 최신 파일에서 고치고, 로드맵이 끝날 때 새 번호로 복제합니다.
- 그림 에셋(`web/assets/`)은 로드맵 8부터 별도로 계속 바뀌므로 보관본에 넣지 않습니다. v0.07 이후 보관본은 `../../web/assets/`를 읽고, 없으면 내장 캔버스 그림으로 돌아갑니다.
- `web/asmara-exploration.html` 리다이렉트 스텁은 넥슨 신청서 링크가 걸려 있어 그대로 둡니다.
