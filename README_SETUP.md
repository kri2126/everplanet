# 에버플래닛 프로토타입 - Unity 세팅 가이드

이 zip에는 씬/프리팹 없이 **C# 스크립트와 기획 문서**만 들어있습니다. Unity 씬 파일은 에디터 없이는 안전하게
생성할 수 없어서, 대신 여러분이 Unity 에디터에서 아래 순서대로 따라 하면 30분~1시간 내로 플레이 가능한
프로토타입을 완성할 수 있도록 최대한 상세하게 정리했습니다.

전제: Unity 2021 LTS 이상, 2D 템플릿 기준으로 작성했습니다 (2D URP/기본 2D 아무거나 상관없음).

---

## 0. 프로젝트 준비

1. Unity Hub에서 새 2D 프로젝트 생성
2. `Assets/Scripts` 폴더를 만들고, 이 zip 안의 `Assets/Scripts` 폴더 전체를 그대로 복사해서 넣기
   (Combat / Core / Data / Planet / Player / UI 하위 폴더 구조 그대로 유지)
3. 콘솔에 컴파일 에러가 없는지 확인 (스크립트만 넣은 상태라 에러가 없어야 정상)

## 1. 프로젝트 세팅: Tags / Layers

- `Edit > Project Settings > Tags and Layers`
  - Tags에 `Player`, `Enemy` 추가 (Enemy는 태그로 안 써도 되지만 있으면 편함)
  - Layers에 `Enemy` 레이어 하나 추가 (PlayerController의 attack 판정에서 사용)

## 2. Input 설정 확인

기본 Unity Input Manager 기준으로 `Horizontal`, `Vertical`, `Fire1`이 이미 존재합니다 (신규 프로젝트면 기본값
그대로 사용 가능). Fire1은 기본적으로 왼쪽 Ctrl / 마우스 왼쪽 버튼에 매핑되어 있고, 스크립트에서 Space바도
같이 받도록 해뒀습니다.

## 3. 데이터 에셋 만들기 (ScriptableObject)

### 3-1. 행성 정의 (PlanetDefinition)

`Project` 창에서 우클릭 > `Create > Everplanet > Planet Definition` 을 4번 실행해서 아래 4개를 만듭니다.

| 파일 이름 | planetName | sceneName | requiredLevel | respawnPlanet |
|---|---|---|---|---|
| Planet_MyPlanet | 마이 플래닛 | MyPlanet | 1 | (비워둠) |
| Planet_Asmara | 아스마라 | Asmara | 1 | Planet_MyPlanet |
| Planet_Ithaca | 이타카 | Ithaca | 5 | Planet_MyPlanet |
| Planet_Novaruna | 노바루나 | Novaruna | 10 | Planet_MyPlanet |

(sceneName은 실제로 만들 씬 이름과 철자가 정확히 같아야 합니다. 4단계에서 씬을 만들 때 이 이름을 그대로 사용하세요.)

### 3-2. 몬스터 정의 (EnemyDefinition)

`Create > Everplanet > Enemy Definition` 으로 몬스터 종류별로 하나씩 생성. 예시:

| 파일 이름 | maxHP | attackPower | defensePower | xpReward | moveSpeed | detectRange | attackRange |
|---|---|---|---|---|---|---|---|
| Enemy_AsmaraSlime | 20 | 4 | 0 | 8 | 1.5 | 4 | 1 |
| Enemy_IthacaGolem | 50 | 10 | 3 | 20 | 1.8 | 5 | 1.2 |
| Enemy_NovarunaWisp | 80 | 16 | 5 | 35 | 2.2 | 6 | 1.5 |

값은 자유롭게 조정해도 됩니다. 행성 난이도 차이를 주는 게 포인트입니다.

## 4. 씬 만들기

`File > New Scene` 으로 4개의 씬을 만들어 `Assets/Scenes` 에 저장합니다: `MyPlanet`, `Asmara`, `Ithaca`, `Novaruna`
(3-1에서 적어둔 sceneName과 정확히 일치해야 함).

`File > Build Settings` 을 열고 4개 씬을 전부 Build Settings의 Scenes In Build 목록에 드래그해서 추가하세요
(순서는 상관없지만 MyPlanet을 0번으로 두는 걸 추천).

## 5. 플레이어 만들기

`MyPlanet` 씬을 열고:

1. 빈 GameObject 생성 -> 이름 `Player`, Tag를 `Player`로 지정
2. 자식으로 스프라이트(임시로 사각형/원형 Sprite 아무거나) 추가해서 눈에 보이게 하기
3. `Player`에 컴포넌트 추가:
   - `Rigidbody2D` (Gravity Scale = 0, Constraints의 Freeze Rotation Z 체크)
   - `Collider2D` (예: Circle Collider 2D)
   - `PlayerController` 스크립트
4. `Player`의 자식으로 빈 오브젝트 `AttackPoint` 생성 (캐릭터 정면 방향에 살짝 떨어뜨려 배치), `PlayerController`의
   `Attack Point` 필드에 연결
5. `PlayerController`의 `Enemy Layer` 필드에서 3단계에서 만든 `Enemy` 레이어를 체크

## 6. GameManager 만들기 (MyPlanet 씬에만 존재하면 됨)

1. 빈 GameObject 생성 -> 이름 `GameManager`
2. `GameManager` 스크립트 부착
3. Inspector에서 `Starting Planet`에 `Planet_MyPlanet` 에셋 연결
4. `Player Stats`는 기본값 그대로 둬도 되고, 레벨 1 기준 스탯 값을 원하는대로 조정해도 됨

`GameManager`는 `DontDestroyOnLoad`로 씬이 바뀌어도 살아있으므로, MyPlanet 씬에만 배치하면 됩니다
(다른 씬에는 절대 추가하지 마세요 - 중복 생성되면 자동으로 스스로 파괴되긴 하지만 헷갈릴 수 있음).

## 7. 각 씬에 SceneSpawnHandler + SpawnPoint 배치

**4개 씬 전부**에 대해:

1. 빈 GameObject `SpawnHandler` 생성, `SceneSpawnHandler` 스크립트 부착
2. 빈 GameObject `SpawnPoint_Default` 생성, `SpawnPoint` 스크립트 부착, `Is Default` 체크, 원하는 시작 위치로 이동

## 8. 포탈 배치 (행성 이동)

`MyPlanet` 씬에 포탈 3개 (아스마라/이타카/노바루나행 각각):

1. 빈 GameObject `Portal_Asmara` 생성, 위치는 자유
2. `Collider2D` 부착 후 `Is Trigger` 체크
3. `PlanetPortal` 스크립트 부착, `Destination`에 `Planet_Asmara` 연결
4. 같은 방식으로 `Portal_Ithaca` -> `Planet_Ithaca`, `Portal_Novaruna` -> `Planet_Novaruna` 생성

각 행성 씬(`Asmara`, `Ithaca`, `Novaruna`)에도 마이 플래닛으로 돌아가는 포탈을 하나씩 만들어서
`Destination`을 `Planet_MyPlanet`으로 연결하세요.

(스프라이트를 씌워서 눈에 보이게 만드는 걸 추천 - 안 그러면 빈 트리거라 어디 있는지 안 보입니다.)

## 9. 몬스터 프리팹 만들기

각 행성 씬(Asmara/Ithaca/Novaruna)에:

1. 빈 GameObject 생성, 스프라이트 추가
2. 컴포넌트 부착:
   - `Rigidbody2D` (Gravity Scale 0, Freeze Rotation Z)
   - `Collider2D`, Layer를 `Enemy`로 지정
   - `EnemyStats` 스크립트 -> `Definition`에 3-2에서 만든 EnemyDefinition 연결
   - `EnemyAI` 스크립트 (definition은 자동으로 EnemyStats에서 가져오므로 따로 설정 안 해도 됨)
3. 완성되면 `Assets/Prefabs` 폴더로 드래그해서 프리팹으로 만들고, 씬에 여러 마리 복사 배치

## 10. HUD 만들기

`MyPlanet` 씬 기준 (다른 씬에도 똑같이 반복하거나, HUD를 Prefab으로 만들어서 각 씬에 배치):

1. `GameObject > UI > Canvas` 생성
2. Canvas 밑에 `Slider`(체력바), `Slider`(경험치바), `Text`(레벨), `Text`(행성 이름) 배치
3. 빈 GameObject `HUD`를 만들어 `HUDController` 스크립트 부착, 위 4개 UI 요소를 각 필드에 연결
4. (선택) 레벨업 팝업을 만들고 싶다면 Canvas 아래에 배경+텍스트로 구성된 `LevelUpPanel`을 만들고
   평소 비활성화 상태로 둔 뒤, `LevelUpPopup` 스크립트가 붙은 (항상 활성화된) 오브젝트를 만들어
   `Panel`과 `Message Text` 필드를 연결

## 11. 테스트

1. `MyPlanet` 씬에서 Play
2. 포탈에 다가가서 `E`를 눌러 아스마라로 이동 (요구 레벨 1이라 바로 갈 수 있음)
3. 몬스터에게 다가가 `Space`(또는 마우스 왼쪽)로 공격, 처치 시 경험치 획득 확인
4. 레벨 5, 10을 달성해서 이타카/노바루나가 열리는지 확인
5. 체력이 0이 되면 마이 플래닛으로 리스폰되는지 확인

## 트러블슈팅

- **포탈에서 E를 눌러도 반응 없음**: Player의 Tag가 정확히 `Player`인지, 포탈의 Collider가 `Is Trigger` 체크되어
  있는지 확인
- **공격해도 몬스터가 안 죽음**: `PlayerController`의 `Enemy Layer`가 몬스터 오브젝트의 실제 Layer와 일치하는지
  확인, `AttackPoint` 위치가 몬스터와 겹치는지 확인 (Scene 뷰에서 빨간 원 Gizmo로 표시됨)
- **씬 전환 후 플레이어 위치가 이상함**: 해당 씬에 `SceneSpawnHandler`와 `SpawnPoint`(Is Default 체크)가
  배치되어 있는지 확인
- **행성 이동이 막힘**: PlanetDefinition의 `requiredLevel`과 현재 캐릭터 레벨을 콘솔 로그로 확인 가능
  (Console에 `[GameManager] ... 입장 불가` 로그가 뜸)

## 확장 아이디어 (다음 단계)

- 아이템/인벤토리 시스템 추가 (`Data` 폴더에 `ItemDefinition` 추가하는 식으로 확장)
- 퀘스트 시스템 (마이 플래닛 NPC '가위브러쉬' 등에게 퀘스트 받기)
- 저장을 JSON 파일로 전환 (SaveSystem 내부만 교체)
- 애니메이션/이펙트 추가 (EnemyStats.Die(), PlayerStats.OnLevelUp 등 이미 훅 포인트 마련됨)
