using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 4개 이상 매치 pop 시 재생할 이펙트의 진입점.
/// effectPrefab을 Inspector에서 연결하거나 PlayEffect를 오버라이드해 연출을 확장한다.
/// </summary>
public class BigMatchEffectHandler : MonoBehaviour
{
    public const int DefaultMinMatchCount = 4;

    [Header("발동 조건")]
    [SerializeField] private int minMatchCount = DefaultMinMatchCount;

    [Header("이펙트 (미정 시 비워둠)")]
    [Tooltip("pop된 각 타일 위치에 생성할 이펙트 프리팹. ParticleSystem/UI VFX 등 자유롭게 지정")]
    [SerializeField] private GameObject effectPrefab;

    [Tooltip("이펙트를 생성할 부모. 비어 있으면 월드에 직접 생성")]
    [SerializeField] private Transform effectParent;

    [Tooltip("effectPrefab에 ParticleSystem이 없을 때 자동 파괴까지 대기 시간")]
    [SerializeField] private float fallbackLifetime = 1.5f;

    [Header("UI 카메라 보정")]
    [Tooltip("애니팡 UI는 UIPostProcessingCamera(Overlay)로 그려짐. 월드 VFX는 UI 레이어로 올려 같은 카메라에서 렌더")]
    [SerializeField] private bool renderOnUiLayer = true;

    [Tooltip("보드 타일 위에 그려지도록 ParticleSystemRenderer sorting order")]
    [SerializeField] private int sortingOrder = 100;

    [Tooltip("CFXR 기본 크기 보정 (UI ortho 기준으로 키움)")]
    [SerializeField] private float effectScale = 3f;

    [Tooltip("UI 카메라 쪽으로 살짝 당겨 Canvas 앞면에 보이게 함")]
    [SerializeField] private float zOffsetTowardUiCamera = 1f;

    /// <summary>
    /// 외부에서 이펙트 구현을 주입할 때 사용 (예: 테스트, 런타임 교체)
    /// </summary>
    public event Action<BigMatchEffectContext> OnBigMatchEffect;

    private Canvas _rootCanvas;

    public int MinMatchCount => minMatchCount;

    private void Awake()
    {
        _rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
    }

    public void TryPlayBigMatchEffect(int matchedCount, IReadOnlyCollection<Tile> matchedTiles)
    {
        if (matchedCount < minMatchCount || matchedTiles == null || matchedTiles.Count == 0)
        {
            return;
        }

        if (!isActiveAndEnabled)
        {
            Debug.LogWarning(
                $"[BigMatchEffectHandler] {matchedCount}개 매치 이펙트 스킵 — " +
                $"handler 비활성 (activeInHierarchy={gameObject.activeInHierarchy}, enabled={enabled})");
            return;
        }

        var context = BuildContext(matchedCount, matchedTiles);
        PlayEffect(context);
    }

    protected virtual void PlayEffect(BigMatchEffectContext context)
    {
        OnBigMatchEffect?.Invoke(context);

        if (effectPrefab == null)
        {
            Debug.Log($"[BigMatchEffectHandler] {context.MatchedCount}개 매치 — 이펙트 프리팹 미할당 (틀만 동작 중)");
            return;
        }

        Camera uiCamera = ResolveUiCamera();
        int spawnedCount = 0;

        foreach (var tile in context.MatchedTiles)
        {
            if (tile == null || tile.icon == null)
            {
                continue;
            }

            Vector3 spawnPosition = ApplyUiCameraOffset(tile.icon.transform.position, uiCamera);
            SpawnAndPlayEffect(spawnPosition, uiCamera);
            spawnedCount++;
        }

        if (spawnedCount == 0)
        {
            Debug.LogWarning($"[BigMatchEffectHandler] {context.MatchedCount}개 매치 — 유효한 타일 위치 없음");
            return;
        }

        Debug.Log(
            $"[BigMatchEffectHandler] {context.MatchedCount}개 매치 이펙트 {spawnedCount}개 생성 " +
            $"(uiCamera={(uiCamera != null ? uiCamera.name : "null")}, scale={effectScale})");
    }

    private void SpawnAndPlayEffect(Vector3 spawnPosition, Camera uiCamera)
    {
        GameObject instance = SpawnEffectInstance(spawnPosition);
        ConfigureForUiRendering(instance, uiCamera);
        PlayParticleSystems(instance);
        ScheduleAutoDestroy(instance);
    }

    private Camera ResolveUiCamera()
    {
        if (_rootCanvas == null)
        {
            _rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
        }

        if (_rootCanvas != null && _rootCanvas.worldCamera != null)
        {
            return _rootCanvas.worldCamera;
        }

        return Camera.main;
    }

    private Vector3 ApplyUiCameraOffset(Vector3 worldPosition, Camera uiCamera)
    {
        if (uiCamera == null || Mathf.Approximately(zOffsetTowardUiCamera, 0f))
        {
            return worldPosition;
        }

        // Screen Space Camera UI와 같은 시야(UIPostProcessingCamera)에서 보이도록 카메라 방향으로 당김
        return worldPosition - uiCamera.transform.forward * zOffsetTowardUiCamera;
    }

    private GameObject SpawnEffectInstance(Vector3 worldPosition)
    {
        if (effectParent != null)
        {
            var instance = Instantiate(effectPrefab, effectParent);
            instance.transform.position = worldPosition;
            instance.transform.localRotation = Quaternion.identity;
            return instance;
        }

        return Instantiate(effectPrefab, worldPosition, Quaternion.identity);
    }

    private void ConfigureForUiRendering(GameObject instance, Camera uiCamera)
    {
        if (instance == null)
        {
            return;
        }

        if (!Mathf.Approximately(effectScale, 1f))
        {
            instance.transform.localScale = Vector3.one * effectScale;
        }

        if (renderOnUiLayer)
        {
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
            {
                SetLayerRecursively(instance, uiLayer);
            }
        }

        foreach (var renderer in instance.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            renderer.sortingOrder = sortingOrder;

            if (_rootCanvas != null)
            {
                renderer.sortingLayerID = _rootCanvas.sortingLayerID;
            }
        }
    }

    private static void SetLayerRecursively(GameObject target, int layer)
    {
        target.layer = layer;
        foreach (Transform child in target.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private static void PlayParticleSystems(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        foreach (var particle in instance.GetComponentsInChildren<ParticleSystem>(true))
        {
            particle.Play(true);
        }
    }

    private static BigMatchEffectContext BuildContext(int matchedCount, IReadOnlyCollection<Tile> matchedTiles)
    {
        Vector3 center = Vector3.zero;
        Item primaryItem = null;
        int validCount = 0;

        foreach (var tile in matchedTiles)
        {
            if (tile == null || tile.icon == null)
            {
                continue;
            }

            center += tile.icon.transform.position;
            primaryItem ??= tile.Item;
            validCount++;
        }

        if (validCount > 0)
        {
            center /= validCount;
        }

        return new BigMatchEffectContext(matchedCount, matchedTiles, center, primaryItem);
    }

    private void ScheduleAutoDestroy(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        var particle = instance.GetComponentInChildren<ParticleSystem>();
        if (particle != null)
        {
            Destroy(instance, particle.main.duration + particle.main.startLifetime.constantMax);
            return;
        }

        Destroy(instance, fallbackLifetime);
    }
}
