# Train Game

Unity 기반 **열차 여행 + 역 + 사건/선택** 게임의 초기 개발 저장소입니다.

## 첫 번째 플레이 루프

현재 기반은 다음 흐름을 빠르게 검증하기 위해 설계되어 있습니다.

> 열차 출발 → 노선 이동 → 역 도착 → 사건 발생 → 다음 역으로 이동

그래픽, UI, 전투, 경제 시스템보다 먼저 **여행 자체가 게임의 뼈대가 되는지** 확인하는 것이 목표입니다.

## 권장 Unity 버전

프로젝트는 Unity 6 계열을 기준으로 시작했습니다. ProjectVersion.txt는 초기 기준값으로 6000.0.0f1을 사용합니다. 더 최신 Unity 6 버전으로 열 경우 Unity가 프로젝트 파일을 업그레이드할 수 있습니다.

## 포함된 기반

- StationData — 역 데이터
- RouteData / RouteStop — 노선 및 정차 지점
- JourneyEventData — 여행 중 사건과 선택지 데이터
- TrainMovementController — 열차의 기본 이동
- GameFlowController — 출발/도착/사건 흐름
- Tools > Train Game > Create Demo Scene — 프로토타입 씬과 샘플 데이터를 자동 생성
- EditMode 테스트 어셈블리와 기본 테스트

## 프로젝트 구조

Assets/TrainGame 아래에 Scripts/Core, Editor, Tests가 있으며, Data와 Scenes는 에디터 메뉴 실행 시 생성됩니다.

## 처음 실행하기

1. 이 저장소를 clone합니다.
2. Unity Hub에서 저장소 폴더를 프로젝트로 엽니다.
3. 스크립트 컴파일이 끝나면 메뉴에서 **Tools > Train Game > Create Demo Scene**을 실행합니다.
4. 생성된 Assets/TrainGame/Scenes/TrainPrototype.unity를 엽니다.
5. Play를 누릅니다.
6. Console에서 출발, 도착, 사건 로그를 확인합니다.
7. 다음 역으로 이동하려면 Game Systems 오브젝트의 GameFlowController 컴포넌트 컨텍스트 메뉴에서 **Advance Journey**를 실행합니다.

## 설계 원칙

### 데이터와 실행 코드 분리
역, 노선, 사건은 ScriptableObject 데이터로 두고 실행 코드는 MonoBehaviour에서 처리합니다. 이후 콘텐츠가 많아져도 코드 수정 없이 데이터를 추가할 수 있도록 하기 위함입니다.

### 최소 의존성
초기 단계에서는 외부 에셋이나 복잡한 패키지를 강제하지 않습니다. 프로토타입이 안정된 뒤 UI, 입력, 세이브, 대화, 월드맵 등의 시스템을 단계적으로 붙입니다.

## 확장 예정

1. 선택 가능한 사건 UI
2. 열차 상태와 객차 시스템
3. 자원/화물/승객 데이터
4. 세이브/로드
5. 노선 분기 및 월드맵
6. 역 체류 페이즈
7. 사건 조건/결과 시스템
8. 실제 아트 에셋과 연출

## 현재 상태

이 저장소는 **게임 전체 구현이 아니라 확장 가능한 MVP 기반**입니다. 데모 씬은 에디터 도구로 생성되며, 실제 게임 규칙은 이후 기획에 맞춰 교체·확장할 수 있습니다.
