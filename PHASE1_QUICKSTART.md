# Phase 1: 첫 행성 탐험 (이동 + 카메라 + 환경) 만 만들기

전투/성장/행성 이동은 전부 나중 단계로 미루고, 지금은 "캐릭터가 첫 행성을 돌아다니는 것"만 완성도 있게
만드는 게 목표입니다. `README_SETUP.md`의 풀 버전 대신 이 문서만 보고 아래 순서대로 진행하면 됩니다.
(GameManager, PlanetDefinition/EnemyDefinition, 포탈, 저장 시스템은 이 단계에서 전혀 필요 없습니다.)

---

## 사용할 스크립트 (이번 단계에서 필요한 것만)

- `Player/PlayerController.cs` - 이동만 사용 (공격 기능은 `attackPoint`를 비워두면 그냥 아무 동작도 안 함 → 신경 안 써도 됨)
- `CameraSystem/CameraFollow.cs` - 카메라가 플레이어를 부드럽게 따라감 (신규)
- `Environment/PlayerBoundary.cs` - 플레이어가 맵 바깥으로 못 나가게 막음 (신규)

(zip에 이미 들어있는 `Core`, `Combat`, `Data`, `Planet`, `UI` 폴더 스크립트는 이번 단계에서는 그냥 프로젝트에
같이 넣어두기만 하고 사용은 안 해도 됩니다. 컴파일 에러는 나지 않으니 안심하고 같이 임포트하세요.)

## 1. 프로젝트/씬 준비

1. 새 2D 프로젝트 (또는 기존 프로젝트 계속 사용)
2. `Assets/Scripts` 폴더에 zip 안의 스크립트 전체 복사
3. 씬 하나 생성 (이름 예: `Asmara` — 나중에 진짜 행성 이동 기능을 붙일 때 재사용하도록 미리 이 이름으로)

## 2. 맵(환경) 만들기

가장 간단한 방법 — 나중에 타일맵으로 바꿔도 무방합니다:

1. `GameObject > 2D Object > Sprites > Square` 등으로 바닥 느낌의 큰 배경 오브젝트 하나 배치 (Sprite Renderer의
   Color를 초원/평야 느낌으로 변경하면 아스마라다운 느낌)
2. 맵 가장자리를 따라 벽 역할을 할 오브젝트 몇 개 배치, 각각에 `Collider2D` (예: BoxCollider2D) 부착
   → 플레이어가 부딪혀서 못 지나가게 됨 (자연 지형 느낌의 장애물도 몇 개 추가하면 더 좋음)
3. 빈 GameObject `MapBounds` 생성, 맵 전체를 감싸는 크기로 `BoxCollider2D` 부착, **Is Trigger 체크**
   (이건 물리 충돌용이 아니라 카메라/플레이어 이동 범위를 계산하기 위한 기준 크기로만 쓰임)

## 3. 플레이어 만들기

1. 빈 GameObject `Player` 생성, Tag를 `Player`로 지정
2. 스프라이트 추가 (임시 사각형/원형 아무거나)
3. 컴포넌트 부착:
   - `Rigidbody2D` (Gravity Scale = 0, Constraints의 Freeze Rotation Z 체크)
   - `Collider2D` (예: CircleCollider2D)
   - `PlayerController` (Attack Point는 비워둬도 됨, Move Speed만 원하는 값으로)
   - `PlayerBoundary` → `Bounds` 필드에 2단계에서 만든 `MapBounds`의 BoxCollider2D 연결

## 4. 카메라 세팅

1. `Main Camera` 선택 (Projection이 Orthographic인지 확인 - 2D 프로젝트면 기본값)
2. `CameraFollow` 스크립트 부착
3. `Target`에 `Player` 연결
4. `Bounds`에 `MapBounds`의 BoxCollider2D 연결 (카메라가 맵 밖을 비추지 않도록)

## 5. 테스트

Play 버튼을 누르고 WASD/방향키로 이동해보세요. 카메라가 부드럽게 따라오고, 맵 가장자리나 장애물에서는
막히는지 확인합니다. 이 정도가 매끄럽게 동작하면 1단계는 완성입니다.

## 다음 단계로 넘어갈 때

이동/환경이 만족스러우면 순서상 다음은 보통 이 중 하나입니다 (원하는 순서대로 진행하면 됩니다):

- **전투 먼저**: 이 씬에 몬스터를 배치하고 `README_SETUP.md`의 6번(GameManager)·9번(몬스터) 섹션 진행
- **행성 이동 먼저**: 마이 플래닛 씬을 하나 더 만들고 `README_SETUP.md`의 3, 6, 7, 8번 섹션 진행

지금 만든 씬 이름을 `Asmara`로 미리 맞춰뒀다면, 나중에 `Planet_Asmara` 데이터 에셋만 추가로 만들어서
그대로 이어붙일 수 있습니다.
