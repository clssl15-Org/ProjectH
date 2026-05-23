# 0522 이슈 원인 추적

작성일: 2026-05-23  

---

# 1. [완료] 문단이 붙어서 나오는 문제

## 원인

`RelicManager.BuildDescription()`에서 유물 기본 설명과 효과 설명을 `data.Description + "\n"`로 연결한다. 그런데 각 유물 SO의 `description` 끝에 저장된 개행 상태가 서로 다르다. 어떤 유물은 `\r`로 끝나고, 어떤 유물은 `\r\n` 또는 `\r\n\n`으로 끝나며, 어떤 유물은 끝 개행이 없다.

그래서 동일한 코드가 실행되어도 최종 문자열은 유물마다 다르게 만들어진다.

- `description` 끝에 개행이 없으면 코드가 붙인 `\n`만 적용되어 효과 설명이 바로 다음 줄에 붙는다.
- `description` 끝에 `\r`이 있으면 코드가 붙인 `\n`과 합쳐져 줄바꿈 한 번처럼 보인다.
- `description` 끝에 이미 `\r\n` 또는 `\r\n\n`이 있으면 코드가 붙인 `\n`까지 더해져 빈 줄이 생긴다.

즉, 문제의 핵심은 줄바꿈 문자가 하나라서만이 아니라, `description` 필드의 끝 개행을 정규화하지 않은 채 공통 구분자처럼 `"\n"`을 덧붙이고 있다는 점이다.

## 근거

- `RelicManager.BuildDescription()`은 `description + effectDesc`를 반환한다.
- 현재 구분자는 `"\n"` 하나다.
- `Assets/ScriptableObject/Relics`의 각 유물 SO를 보면 `description` 끝이 `\r`, `\r\n`, `\r\n\n`, 개행 없음으로 섞여 있다.
- 과거 구현에는 `"\n\n"` 형태의 빈 줄 구분이 있었으나, 현재 코드에서는 한 줄 구분으로 줄어든 상태다.

## 정리

유물 설명의 문단 간격 문제는 UI 프리팹 문제가 아니라 설명 문자열 조립 방식과 데이터 끝 개행이 함께 만든 문제다. 기본 설명 끝의 기존 개행을 제거한 뒤, 기본 설명과 효과 설명 사이에 빈 줄을 보장하는 공통 포맷이 필요하다.

## 해결 방안

`BuildDescription()`에서 `data.Description`을 그대로 사용하지 말고, 먼저 끝 개행을 제거한 뒤 공통 구분자를 붙인다.

예시 방향:

```csharp
string description = data.Description.TrimEnd('\r', '\n');
return description + "\n\n" + effectDesc;
```

이렇게 하면 각 유물 SO에 저장된 `description` 끝이 `\r`, `\r\n`, `\r\n\n`, 개행 없음 중 무엇이든 최종 출력은 항상 `기본 설명 + 빈 줄 + 효과 설명` 형태로 고정된다.

추가로, 이미 SO 데이터에 들어간 끝 개행을 직접 정리할 수도 있지만, 데이터가 다시 섞일 가능성이 있으므로 런타임 문자열 조립부에서 한 번 더 정규화하는 편이 안전하다.

---
# 2. [완료] 유물 동전던지기 전/후 누적 표시 문제

# 2-1. 동전던지기 전 처음에 `10(+10%)`가 나오는 원인

`RelicAcquisitionUI.OnRelicAcquiring()`은 동전을 던지기 전에 `_relicDescrption.text = relicAcquisition.NormalDescription`을 먼저 표시한다.

이 `NormalDescription`은 `RelicManager.BuildRelicAcquisitionDto()`에서 `normalNextValue` 기준으로 만들어진다. id 10의 경우 현재 누적값이 0이고 기본값이 10이라 `normalNextValue`가 10이 된다. 이후 `BuildValueChangeText()`가 `nextValue - currentValue`를 계산하므로 `10 - 0 = 10`이 되어 `(+10%)`가 붙는다.

즉, 동전 결과가 아직 확정되지 않은 첫 화면에서 "이번에 증가할 값"을 괄호로 표시하고 있어서 `10(+10%)`가 나온다.

## 2-2. 동전던지기 이후 누적이 안 뜨는 원인

동전 애니메이션이 끝난 뒤 `RelicAcquisitionUI`는 `RelicManager.AddRelic()`을 호출한다. `AddRelic()`은 실제 유물을 추가한 뒤 `valueSum = GetValueSum(key)`로 최종 누적값을 구한다.

하지만 설명을 만들 때 `BuildDescription(data, valueSum, valueSum, ...)`처럼 최종 누적값을 `nextValue`와 `currentValue` 양쪽에 모두 넣는다. 그래서 `BuildValueChangeText()` 안에서는 `nextValue - currentValue`가 항상 0이 되고, id 10처럼 누적 총합이 기본값보다 커져야 하는 유물도 괄호 누적 표시가 사라진다.

## 정리

두 문제는 같은 포맷터의 기준값이 흔들려서 발생한다.

- 동전 전: 아직 획득 전인데 `currentValue -> normalNextValue` 증가분을 보여줘서 첫 획득도 `(+10%)`가 붙는다.
- 동전 후: 이미 획득한 뒤인데 `valueSum -> valueSum`으로 비교해서 증가분이 0이 된다.

표시 규칙은 "최종 표시값이 유물 기본값보다 클 때만 `(+차이)`를 붙인다"로 고정하는 편이 맞다. 예를 들어 id 10은 기본값 10, 강화값 20이므로 첫 일반 획득은 `10%`, 첫 강화 성공은 `20%(+10%)`가 되어야 한다.

## 해결 방안

괄호 안 증가량을 `nextValue - currentValue`로 계산하지 말고, 최종 표시값과 유물 고유 기본값을 비교해서 계산한다.

예시 방향:

```csharp
float delta = displayValue - data.BaseValue;
return delta > 0.001f ? $"(+{FormatValue(delta)}{GetValueUnitSuffix(data)})" : "";
```

이렇게 하면 동전 전 미확정 화면과 동전 후 확정 화면이 같은 기준을 사용한다.

- 동전 전 일반 예상값이 기본값과 같으면 괄호를 표시하지 않는다.
- 동전 후 강화 성공 등으로 최종 표시값이 기본값보다 커졌을 때만 `(+차이)`를 표시한다.

구현 위치는 `BuildValueChangeText()`를 최종 표시값 기준으로 바꾸는 쪽이 가장 단순하다. `BuildRelicAcquisitionDto()`와 `AddRelic()`은 각각 표시할 최종값만 넘기고, 괄호 표시 여부는 포맷터가 `data.BaseValue` 기준으로 판단하게 만든다.

---

# 3. 최대체력은 증가하는데 실제 바가 안 늘어나는 문제

## 원인

최대체력 값 자체는 `SacredProtectionMark`가 `playerStats.maxHeathMultiplier`를 바꾸고 `PlayerHealth.ChangeMaxHealth()`를 호출하면서 증가한다. 문제는 그 변경이 UI가 전체 바 폭을 늘리는 데 필요한 정보로 전달되지 않았다는 점이다.

문제 발생 당시 흐름은 다음과 같았다.

- `PlayerHealth`에는 최대체력 변경 이벤트가 있었지만, `Player -> PlayerVM -> HealthBarUI` 경로에서 최대체력 변경을 별도 갱신 조건으로 충분히 다루지 않았다.
- `HealthRateData`는 주로 정규화된 체력 비율만 전달했다.
- `HealthBarUI.SetHealthRate()`는 전체 바 폭은 그대로 두고 `_mask` 폭만 `HealthRate * _originalWidth`로 조정했다.

그래서 최대체력은 내부 수치상 증가했지만, UI의 전체 체력바 컨테이너 폭은 기존 폭에 묶여 있었다.

## 현재 코드 상태

현재 코드에는 이 문제를 해결하는 방향의 변경이 이미 들어와 있다.

- `PlayerCondition.MaxHealthChanged`가 존재한다.
- `Player`가 `PlayerHealth.OnMaxHealthChanged`를 `ConditionChanged`로 전달한다.
- `PlayerVM`이 `MaxHealthChanged`에서도 `HealthRateChanged`를 발생시킨다.
- `HealthRateData`가 `Health`와 `MaxHealth`를 함께 들고 간다.
- `HealthBarUI`가 기준 최대체력 대비 현재 최대체력 비율로 전체 바 폭을 늘리고, 그 폭 안에서 마스크 폭을 계산한다.

## 정리

원인은 최대체력 증가 로직이 아니라 UI 갱신 계약의 정보 부족과 바 폭 계산 방식이었다. 현재 구현은 이 경로를 보강한 상태다.
