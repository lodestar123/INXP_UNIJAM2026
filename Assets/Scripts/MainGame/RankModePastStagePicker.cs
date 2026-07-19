using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 랭크모드에서 Past 스테이지 후보 중 하나를 가중치 기반으로 뽑는 순수 C# 클래스.
/// 매 Pick() 호출마다 가중치는 균등값에서 다시 계산되며, 바로 직전에 뽑힌 인덱스만 감소 배율이 적용된다(누적 감소 아님).
/// </summary>
public class RankModePastStagePicker
{
    private const int RecommendedCandidateCount = 4;

    [SerializeField] private float decayFactor = 0.5f;

    private readonly List<StageRuntimeConfiguration> _candidates;
    private readonly float[] _weights; // GC 할당 방지를 위해 미리 할당해 매 호출마다 재사용
    private int _lastPickedIndex = -1;

    public RankModePastStagePicker(List<StageRuntimeConfiguration> candidates, float decayFactor = 0.5f)
    {
        _candidates = candidates ?? new List<StageRuntimeConfiguration>();
        this.decayFactor = decayFactor;

        if (_candidates.Count < RecommendedCandidateCount)
        {
            Debug.LogWarning($"[RankModePastStagePicker] 후보 스테이지가 {RecommendedCandidateCount}개 미만입니다. (현재 {_candidates.Count}개) 있는 만큼으로만 동작합니다.");
        }

        _weights = new float[_candidates.Count];
    }

    public StageRuntimeConfiguration Pick()
    {
        int count = _candidates.Count;
        if (count == 0)
        {
            Debug.LogWarning("[RankModePastStagePicker] 후보가 없어 Pick할 수 없습니다.");
            return null;
        }

        float baseWeight = 100f / count;
        for (int i = 0; i < count; i++)
        {
            _weights[i] = baseWeight;
        }

        // 직전 1회 픽만 감소 적용 (누적 감소 아님) - 매번 균등값에서 다시 계산
        if (_lastPickedIndex >= 0 && _lastPickedIndex < count && count > 1)
        {
            float decayedWeight = baseWeight * decayFactor;
            float removedAmount = baseWeight - decayedWeight;
            _weights[_lastPickedIndex] = decayedWeight;

            float redistributePerOther = removedAmount / (count - 1);
            for (int i = 0; i < count; i++)
            {
                if (i == _lastPickedIndex) continue;
                _weights[i] += redistributePerOther;
            }
        }

        float totalWeight = 0f;
        for (int i = 0; i < count; i++)
        {
            totalWeight += _weights[i];
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        int pickedIndex = count - 1; // 부동소수점 오차 대비 fallback

        for (int i = 0; i < count; i++)
        {
            cumulative += _weights[i];
            if (roll < cumulative)
            {
                pickedIndex = i;
                break;
            }
        }

        _lastPickedIndex = pickedIndex;
        return _candidates[pickedIndex];
    }
}
