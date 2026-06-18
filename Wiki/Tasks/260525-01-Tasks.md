# [해결] 0525 최초도달/재도달 처리 이슈 보고

작성일: 2026-05-25

---

# 문제

## 1. 현상

플레이어가 사망하거나 `옵션 > 게임 재시작`을 누른 뒤에는 이후 모든 구간이 최초도달이 아니라 재도달 상태로 처리되는 것으로 보인다.

기획 의도는 "플레이어가 특정 구간에 처음 도달했을 때만 최초도달 연출을 출력한다"에 가까운 것으로 보인다. 즉, 사망이나 재시작 여부 하나로 전체 구간의 최초/재도달을 일괄 결정하는 것이 아니라, 각 구간별로 실제 첫 도달 여부를 판단해야 한다.

## 2. 현재 코드 흐름

### 2-1. 사망 처리

`GameManager`는 각 씬의 `StageManager.PlayerDied` 이벤트를 구독한다.

- `Assets/Scripts/Game/Manager/GameManager.cs`
  - `PlayerHasDied`는 `LevelManager.Instance.PlayerHasDied`를 그대로 반환한다.
  - `PlayerDied` 발생 시 `LevelManager.Instance.ResetState()`를 호출한다.
  - 이어서 `LevelManager.Instance.MarkPlayerDied()`를 호출한다.
  - 일반 스테이지에서는 `_firstSceneName`, 현재 기본값 기준 `Stage0 0`으로 이동한다.

`LevelManager.MarkPlayerDied()`는 다음 두 값을 설정한다.

```csharp
PlayerHasDied = true;
IsPlayerDeathRestartPending = true;
```

그런데 `ResetState()` 안에서는 `PlayerHasDied`를 다시 `false`로 되돌리지 않는다. 따라서 한 번 사망 처리된 뒤에는 `PlayerHasDied`가 계속 `true`로 남는다.

### 2-2. 옵션 > 게임 재시작 처리

`StageManager`의 설정 UI 연결부에서 `SettingsUI.RestartUI`도 결국 `PlayerDied?.Invoke()`를 호출한다.

- `Assets/Scripts/Game/Stage/StageManager.cs`
  - `SettingsUI.RestartUI` 발생
  - BGM 정지
  - 암전 후 `PlayerDied?.Invoke()`

즉, 옵션의 게임 재시작은 현재 구조상 실제 사망과 같은 경로를 탄다. 그래서 옵션 재시작도 `GameManager`의 사망 처리로 이어지고, `LevelManager.MarkPlayerDied()`에 의해 `PlayerHasDied = true`가 된다.

### 2-3. 시나리오의 최초도달 판단

`ScenarioManager`의 `IsFirstArrival`은 기본적으로 다음 조건을 사용한다.

```csharp
protected bool IsFirstArrival => !_overrideFirstArrival.Resolve(false)
    ? !GameServices.PlayerHasDied : _isFirstArrival;
```

즉, 오버라이드가 꺼져 있으면 "플레이어가 죽은 적이 없으면 최초도달, 죽은 적이 있으면 재도달"로 판단한다.

이 값은 각 스테이지 시나리오에서 최초도달/재도달 분기 기준으로 사용된다.

- `Scenario_Stage0.cs`
  - 최초도달이면 `Stage0_Arrival_First_1`, `Stage0_Arrival_First_2`를 진행한다.
  - 재도달이면 `Stage0_Arrival_Reentry` 또는 대기 상태로 간다.
- `Scenario_Stage1.cs`
  - 최초도달이면 `Stage1_Arrival_First`를 진행한다.
  - 재도달이면 `Stage1_Arrival_Reentry`를 진행한다.
- `Scenario_Stage2.cs`
  - 최초도달이면 `Stage2_Arrival_First`를 진행한다.
  - 재도달이면 `Stage2_Arrival_Reentry`를 진행한다.
- `Scenario_Stage3.cs`
  - 최초도달이면 `Stage3_Arrival_First`를 진행한다.
  - 재도달이면 `Stage3_Arrival_Reentry`를 진행한다.
- 보스 시나리오도 같은 `IsFirstArrival`을 사용하므로, 사망 이후에는 첫 보스 접촉/클리어 후 연출이 생략되는 흐름이 될 수 있다.

## 3. 원인 정리

현재 최초도달 판단 기준은 "특정 구간에 처음 도달했는가"가 아니라 "플레이어가 이번 세션에서 사망 처리된 적이 있는가"이다.

그래서 다음 흐름이 발생한다.

1. 게임을 처음 시작하면 `PlayerHasDied == false`라서 최초도달 연출이 나온다.
2. 플레이어가 사망하거나 옵션에서 게임 재시작을 누르면 `MarkPlayerDied()`가 호출되어 `PlayerHasDied == true`가 된다.
3. 이후 `ScenarioManager.IsFirstArrival`은 모든 시나리오에서 `false`를 반환한다.
4. 아직 실제로 도달한 적 없는 Stage1, Stage2, Stage3 구간도 재도달 분기로 들어간다.

핵심 문제는 최초도달 상태가 구간별로 저장되지 않고, 전역 사망 여부 하나에 종속되어 있다는 점이다.

## 4. 기획 의도와의 차이

기획 의도대로라면 최초도달 여부는 최소한 다음처럼 판단되어야 한다.

- Stage0에 처음 도달하면 Stage0 최초도달 연출을 출력한다.
- 이후 사망해서 Stage0으로 돌아오면 Stage0은 이미 도달한 구간이므로 재도달 연출이 가능하다.
- 하지만 아직 도달한 적 없는 Stage1에 처음 진입했다면, 사망 이력이 있더라도 Stage1 최초도달 연출을 출력해야 한다.
- Stage2, Stage3, 보스 구간도 같은 방식으로 구간별 첫 도달 여부를 기준으로 삼아야 한다.

현재 구현은 사망 이력이 생기는 순간 모든 구간을 재도달로 간주하므로, 위 의도와 어긋난다.

## 5. 개선 방향

### 5-1. 최초도달 기록을 구간 단위로 분리

`PlayerHasDied`를 최초도달 판단에 직접 쓰지 말고, 구간별 도달 기록을 별도로 관리하는 편이 안전하다.

예시 방향:

```csharp
private readonly HashSet<string> _reachedScenarioKeys = new();

public bool IsFirstScenarioArrival(string key)
{
    return !_reachedScenarioKeys.Contains(key);
}

public void MarkScenarioReached(string key)
{
    _reachedScenarioKeys.Add(key);
}
```

키는 씬 이름 또는 시나리오가 명시한 `arrivalKey`를 사용할 수 있다.

- 씬 이름 사용: 구현이 단순하다.
- 별도 `arrivalKey` 사용: 같은 씬 안에서 여러 구간을 나누거나, 씬 이름 변경에 덜 흔들리게 만들 수 있다.

### 5-2. 기록 시점

"처음 도달했을 때만"이 기준이라면 최초도달 분기로 들어가는 순간 해당 구간을 도달 처리하는 편이 자연스럽다.

예시:

1. `ScenarioManager`가 현재 구간 키를 만든다.
2. `GameServices` 또는 `LevelManager`에 "이 구간이 최초도달인지"를 묻는다.
3. 최초도달이면 최초도달 블록으로 보낸다.
4. 최초도달 블록에 들어가는 시점에 해당 키를 도달 처리한다.

이렇게 하면 사망 여부와 무관하게 "이미 본 구간은 재도달, 아직 못 본 구간은 최초도달"이 된다.

### 5-3. 사망/재시작과 최초도달 기록의 관계 결정

정책은 다음 기준으로 고정한다.

- 사망 후 재시작: 이전에 실제 도달했던 구간은 재도달로 유지한다.
- `옵션 > 게임 재시작`: 현재 사망과 같은 `PlayerDied` 경로를 타므로 같은 재도전 정책을 적용한다.

따라서 사망 또는 옵션 재시작 시 구간별 도달 기록은 초기화하지 않는다. 이미 실제로 도달했던 구간은 재도달로 처리하고, 아직 도달하지 않은 구간은 이후 처음 진입할 때 최초도달로 처리한다.

## 6. 확인 포인트

- `LevelManager.ResetState()`가 현재 게임 진행 상태를 초기화하지만 `PlayerHasDied`는 초기화하지 않는다.
- `IsPlayerDeathRestartPending`은 `Player.Start()`에서 스폰 연출 전환에 소비되지만, `PlayerHasDied`를 되돌리는 역할은 하지 않는다.
- `GameManager.PlayerHasDied`는 `LevelManager.PlayerHasDied`를 그대로 노출한다.
- `ScenarioManager.IsFirstArrival`은 구간별 정보 없이 `GameServices.PlayerHasDied`만 보고 판단한다.
- 일반 스테이지뿐 아니라 보스 시나리오도 같은 판단값을 공유하므로 영향 범위가 넓다.

## 7. 결론

현재 문제의 직접 원인은 `ScenarioManager.IsFirstArrival`이 구간별 최초도달 여부가 아니라 전역 사망 여부인 `GameServices.PlayerHasDied`를 기준으로 삼는 구조다.

해결하려면 `PlayerHasDied`는 사망 후 재시작/스폰 처리 같은 사망 전용 상태로만 사용하고, 최초도달 연출 여부는 별도의 구간별 도달 기록으로 판단하도록 분리하는 것이 좋다.


---

# 해결

## 1. 구현 내용

`GameServices`에 시나리오 최초도달 여부를 소비하는 `ConsumeFirstScenarioArrival(string scenarioKey, object context = null)` 계약을 추가했다.

`GameManager`에는 `_reachedScenarioKeys`를 추가해 시나리오 키별 도달 여부를 보관한다. `ConsumeFirstScenarioArrival()`은 아직 없는 키면 `true`를 반환하면서 기록하고, 이미 있는 키면 `false`를 반환한다. 빈 키는 안전하게 재도달로 처리한다.

`ScenarioManager.IsFirstArrival`은 더 이상 `GameServices.PlayerHasDied`를 직접 보지 않는다. 대신 `Start()`에서 최초도달 여부를 한 번만 계산해 `_isFirstArrivalForCurrentRun`에 캐시하고, 같은 시나리오 안의 후속 분기에서는 이 캐시값을 계속 사용한다. 이 때문에 최초도달 기록을 남긴 직후에도 같은 시나리오의 뒷부분이 재도달로 뒤집히지 않는다.

도달 기록 키는 인스펙터의 `_arrivalKey`가 있으면 그 값을 우선 사용하고, 없으면 씬 이름을 사용한다. 씬 이름도 얻을 수 없는 예외 상황에서는 시나리오 타입 이름을 fallback 키로 사용한다.

## 2. 정책 반영

사망 후 재시작과 `옵션 > 게임 재시작`은 구간별 도달 기록을 초기화하지 않는다. 따라서 이미 실제로 도달했던 구간은 재도달로 유지되고, 아직 도달하지 않은 구간은 이후 처음 진입할 때 최초도달로 처리된다.

`LevelManager.PlayerHasDied`는 사망 후 스폰/재시작 처리 용도로 남겨 두고, 최초도달 연출 판단에서는 분리했다.

## 3. 수정 파일

- `Assets/Scripts/Infrastructure/GameServices.cs`
- `Assets/Scripts/Game/Manager/GameManager.cs`
- `Assets/Scripts/Game/Stage/ScenarioManager.cs`

## 4. 검증

`dotnet build Assembly-CSharp.csproj --no-restore`로 컴파일을 확인했다. 기존 nullable 관련 경고는 남아 있지만 오류는 0개로 빌드에 성공했다.