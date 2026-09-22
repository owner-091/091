# Train Interior Architecture

이 문서는 탑뷰 도트게임에서 여러 객차를 연결하고, 차장이 열차 내부 전체를 실제 공간처럼 걸어 다니게 만들기 위한 구현 구조를 정의합니다.

## 1. 핵심 결정

열차 내부는 **객차마다 별도 씬을 로드하는 방식이 아니라, 하나의 열차 내부 씬 안에 여러 객차 프리팹을 실제 좌표상 이어 붙이는 방식**으로 구성합니다.

플레이어는 객차 사이를 이동할 때 장면 전환이나 로딩을 겪지 않습니다.

예:

```text
[승무원칸]──[객실 A]──[식당칸]──[객실 B]──[화물칸]
                     ↑
                  차장 이동
```

차장은 한 객차 안에서 걷던 방식 그대로 연결 통로를 통과해 다음 객차로 이동합니다.

이 방식의 목적은 열차 내부를 메뉴의 집합이 아니라 **하나의 지속적인 생활 공간**으로 느끼게 하는 것입니다.

---

## 2. 열차 내부의 기본 단위: Carriage

각 객차는 독립된 Prefab으로 만듭니다.

각 객차 Prefab은 다음 구조를 갖습니다.

```text
Carriage
├─ Visual
│  ├─ Floor
│  ├─ Walls
│  ├─ Furniture
│  └─ Decorations
├─ Collision
├─ RearConnector
├─ FrontConnector
├─ CarriageArea
├─ NPCAnchors
├─ FacilityAnchors
└─ EventAnchors
```

### FrontConnector / RearConnector

객차를 서로 붙이는 기준점입니다.

TrainInteriorAssembler가 앞 객차의 FrontConnector와 다음 객차의 RearConnector가 정확히 맞닿도록 배치합니다.

따라서 객차 길이가 서로 달라도 연결할 수 있습니다.

### CarriageArea

플레이어 또는 NPC가 현재 어느 객차에 있는지 판정하는 Trigger 영역입니다.

예:

- 차장이 현재 식당칸에 있음
- 승객 A가 현재 객실 B에 있음
- 화물 이벤트의 대상 객차가 화물칸임

같은 정보를 시스템이 알 수 있게 합니다.

---

## 3. 객차 데이터와 실제 공간을 분리

객차에는 두 종류의 정보가 있습니다.

### CarriageDefinition

ScriptableObject.

객차 자체의 종류와 기본 정보를 보관합니다.

예상 필드:

```text
CarriageId
DisplayName
Prefab
Category
Tags
```

Category 예시는 다음과 같을 수 있습니다.

- Passenger
- Dining
- Cargo
- Crew
- Special

이 분류는 초기에는 단순한 식별용으로만 사용합니다.

### CarriageInstance

실제 씬에 생성된 객차 하나를 나타내는 MonoBehaviour입니다.

같은 종류의 객차가 두 개 있어도 서로 다른 Instance입니다.

예:

```text
PassengerCar Definition
    ↓
PassengerCar #01
PassengerCar #02
PassengerCar #03
```

승객 위치, 현재 사용 중인 시설, 해당 객차에서 발생 중인 소규모 사건 같은 런타임 정보는 Instance 쪽에 연결합니다.

---

## 4. 열차 편성: TrainCompositionData

어떤 객차가 어떤 순서로 연결되는지는 별도의 데이터로 관리합니다.

예:

```text
TrainComposition
1. Crew Car
2. Passenger Car A
3. Dining Car
4. Passenger Car B
5. Cargo Car
```

이 데이터를 읽어 TrainInteriorAssembler가 씬 시작 시 객차들을 자동 배치합니다.

장점:

- 씬에서 객차를 일일이 손으로 붙이지 않아도 됨
- 편성을 바꾸기 쉬움
- 튜토리얼용 열차와 본편 열차를 다르게 만들 수 있음
- 나중에 특정 이야기에서 특수 객차를 추가할 수 있음

현재 기획에서는 객차를 수집하거나 파괴하는 시스템을 전제로 하지 않습니다.

TrainCompositionData는 우선 **열차 내부의 구조와 이야기 공간을 정의하는 데이터**로 사용합니다.

---

## 5. 객차 배치 방식

초기 구현에서는 모든 객차를 X축 방향으로 연결합니다.

예:

```text
X = 0        X = 12       X = 24       X = 36
[객차 1] ── [객차 2] ── [객차 3] ── [객차 4]
```

그러나 고정 길이를 코드에 넣지는 않습니다.

각 객차의 Connector Transform을 기준으로 다음 객차의 위치를 계산합니다.

따라서 이후 다음과 같은 객차도 만들 수 있습니다.

- 짧은 객차
- 긴 객차
- 폭이 넓은 특수 객차
- 내부 구조가 비대칭인 객차

---

## 6. 객차 사이 이동

객차 사이의 기본 이동은 **포털이나 순간이동이 아니라 실제 보행**입니다.

연결부:

```text
객차 A
┌─────────────┐
│             │
│          [문]──[통로]──[문]
│             │
└─────────────┘
                         객차 B
```

차장이 문이 열린 상태라면 그냥 걸어서 이동합니다.

초기 프로토타입에서는 문을 항상 열린 상태로 두어도 됩니다.

향후 필요하면 DoorController를 추가해:

- 자동문
- 상호작용으로 여는 문
- 특정 이벤트 중 닫힌 문
- 승무원 전용 공간

등을 표현할 수 있습니다.

하지만 문은 **객차 전환 로딩 장치가 아닙니다.**

---

## 7. 카메라

카메라는 열차 전체를 한 화면에 보여주지 않고 차장을 추적합니다.

구조:

```text
Conductor
    ↓
CameraFollow
    ↓
현재 객차 주변만 화면에 표시
```

초기 구현에서는 단순 추적 카메라로 충분합니다.

도트 그래픽을 위해 최종적으로는 다음을 지향합니다.

- Orthographic Camera
- Pixel Perfect Camera
- 정해진 Pixels Per Unit
- Point Filtering
- Sub-pixel 흔들림 방지

객차 경계에서 카메라가 갑자기 전환되지 않고 연속적으로 이동하는 것이 기본 방향입니다.

---

## 8. NPC와 승객

승객은 열차 전체 좌표계 안에서 존재합니다.

따라서 객차 이동 시 NPC를 다시 생성하거나 제거하지 않습니다.

PassengerRuntime 예:

```text
CharacterData
CurrentCarriage
CurrentPosition
CurrentActivity
ConversationState
StoryFlags
```

승객은 필요에 따라 다른 객차로 이동할 수 있습니다.

예:

```text
객실
 ↓
식사 시간
 ↓
식당칸
 ↓
대화 이벤트
 ↓
다시 객실
```

이 구조는 열차가 실제 생활 공간처럼 느껴지게 하는 데 중요합니다.

처음부터 NPC 이동 AI를 복잡하게 만들 필요는 없습니다.

1차 단계에서는 정해진 위치에 서 있는 승객부터 구현하고, 이후 목적지 기반 이동을 추가합니다.

---

## 9. 객차별 Anchor

객차 내부 콘텐츠를 좌표값으로 직접 하드코딩하지 않습니다.

Prefab 안에 목적별 Anchor를 둡니다.

예:

```text
NPCAnchors
├─ Seat_01
├─ Seat_02
├─ Window_01
└─ CounterWait

FacilityAnchors
├─ DiningCounter
├─ CargoCheck
└─ CrewDesk

EventAnchors
├─ Event_LeftWindow
├─ Event_Center
└─ Event_Entrance
```

그러면 사건이나 NPC 시스템은 "월드 좌표 (24.3, 2.1)"를 기억하는 대신:

```text
DiningCar / CounterWait
```

같은 의미 있는 위치를 참조할 수 있습니다.

---

## 10. 관리 요소와의 연결

현재 기획의 열차 관리 요소는 객차 구조와 직접 연결합니다.

### 화물

Cargo Car 안의 CargoCheck Anchor 또는 CargoFacility를 조사합니다.

### 손님 상태 / 접객

차장이 실제 승객을 찾아가 대화하거나 객실의 관리 지점을 확인합니다.

### 승무원 배치

Crew 관리 화면에서 끝내기보다는 실제 객차 공간과 연결합니다.

예:

```text
승무원 A → 식당칸 근무
승무원 B → 객실 B 담당
```

### 식당 확인

Dining Car에 직접 가서 식당 시설 또는 승무원과 상호작용합니다.

즉 관리 기능은 가능한 한 **열차 내부 탐색과 분리하지 않습니다.**

---

## 11. 씬 구조

초기에는 다음 정도로 유지합니다.

```text
TrainInteriorScene
├─ TrainInteriorRoot
│  ├─ Carriage_00
│  ├─ Carriage_01
│  ├─ Carriage_02
│  └─ ...
├─ Characters
│  ├─ Conductor
│  └─ Passengers
├─ Systems
│  ├─ TrainInteriorAssembler
│  ├─ CarriageRegistry
│  └─ InteractionSystem
└─ MainCamera
```

역 외부 공간은 이후 별도의 공간/씬 구조로 설계할 수 있습니다.

현재 단계에서는 열차 내부 구조를 먼저 안정시키는 것이 목표입니다.

---

## 12. 필요한 클래스

1차 구현에 필요한 핵심 클래스:

### Data

- `CarriageDefinition`
- `TrainCompositionData`

### Runtime

- `CarriageInstance`
- `CarriageConnector`
- `CarriageArea`
- `TrainInteriorAssembler`
- `CarriageRegistry`
- 기존 `ConductorTopDownController`

### 이후 추가

- `CameraFollowController`
- `DoorController`
- `InteractionSystem`
- `PassengerRuntime`
- `FacilityInteractable`

---

## 13. 첫 프로토타입 편성

첫 테스트는 실제 콘텐츠가 아니라 구조 검증용으로 다음 4칸을 권장합니다.

```text
[승무원 테스트칸]
        ↓
[승객 테스트칸]
        ↓
[식당 테스트칸]
        ↓
[화물 테스트칸]
```

각 객차의 목적:

### 승무원 테스트칸
- 차장 시작 위치
- 작은 책상/시설 충돌 테스트

### 승객 테스트칸
- NPC 2~3명 배치
- 좁은 통로에서 이동 감각 테스트

### 식당 테스트칸
- 큰 시설물 사이 이동 테스트
- 이후 접객/식당 기능의 기반

### 화물 테스트칸
- 다양한 크기의 장애물
- 화물 상호작용 기반 테스트

이 네 칸을 차장이 끝에서 끝까지 끊김 없이 걸어갈 수 있으면 객차 구조의 1차 목표를 달성한 것으로 봅니다.

---

## 14. 이번 단계에서 하지 않을 것

객차 연결 프로토타입에서는 다음을 아직 구현하지 않습니다.

- 완성된 승객 AI
- 메인/서브 이벤트 시스템
- 역 체류 시간
- 노선 선택
- 열차 손상
- 자원 고갈
- 게임오버
- 완성 아트
- 복잡한 문 애니메이션
- 객차 스트리밍/비활성화 최적화

객차 수가 적은 초기 게임에서는 모든 객차를 동시에 활성화해도 충분합니다.

성능 문제가 실제로 발생한 뒤에만 스트리밍을 고려합니다.

---

## 15. 구현 순서

### Phase A — Modular Carriage

1. CarriageDefinition 생성
2. CarriageInstance 생성
3. Front/Rear Connector 구현
4. TrainCompositionData 생성
5. TrainInteriorAssembler 구현
6. 객차 4개 자동 연결

### Phase B — Traversal

7. 객차 벽/통로 Collider 정리
8. 차장이 모든 객차를 연속 이동
9. Camera Follow 추가
10. CarriageArea로 현재 객차 감지

### Phase C — Interaction Foundation

11. Interactable 인터페이스/컴포넌트
12. 승객 임시 NPC
13. 시설 임시 상호작용
14. 현재 객차 기반 상호작용 검증

이후에 대화 시스템과 승객 콘텐츠로 넘어갑니다.

---

## 16. 설계 원칙 요약

**객차는 씬이 아니라 모듈입니다.**

**열차 내부는 여러 모듈이 연결된 하나의 연속 공간입니다.**

**차장은 그 공간 안을 실제로 걸어 다닙니다.**

**승객과 시설도 같은 공간에 실제 위치합니다.**

**관리 기능은 가능하면 메뉴만으로 처리하지 않고, 차장이 해당 객차와 인물을 직접 찾아가는 플레이와 연결합니다.**

이 구조를 열차 내부 플레이의 기본 아키텍처로 사용합니다.
