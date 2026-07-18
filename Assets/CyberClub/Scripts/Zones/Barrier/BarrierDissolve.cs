using System;
using DG.Tweening;
using UnityEngine;

public class BarrierDissolve : MonoBehaviour
{
    [SerializeField] private float _dissolveDuration = 2f;
    [SerializeField] private GameObject _lock;
    [SerializeField] private Canvas _canvas;

    private Renderer _renderer;
    private Material _dissolveMaterial;
    private Tween _dissolveTween;

    private bool _isUnlocking;

    public bool CanUnlock
    {
        get
        {
            return !_isUnlocking &&
                   gameObject.activeInHierarchy &&
                   EnsureInitialized();
        }
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    // ИЗМЕНЕНО: больше нет Init(),
    // подписки и ссылки на общий ZonePurchase.
    public bool TryUnlock(Action onCompleted = null)
    {
        if (_isUnlocking || !EnsureInitialized())
            return false;

        _isUnlocking = true;

        if (_lock != null)
            _lock.SetActive(false);

        if (_canvas != null)
            _canvas.enabled = false;

        if (_dissolveDuration <= 0f)
        {
            _dissolveMaterial.SetFloat("_Value", 1f);
            CompleteUnlock(onCompleted);
            return true;
        }

        _dissolveTween?.Kill();

        _dissolveTween = _dissolveMaterial
            .DOFloat(1f, "_Value", _dissolveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => CompleteUnlock(onCompleted));

        return true;
    }

    private bool EnsureInitialized()
    {
        if (_dissolveMaterial != null)
            return true;

        if (_renderer == null)
            _renderer = GetComponent<Renderer>();

        if (_renderer == null)
        {
            Debug.LogError(
                $"BarrierDissolve: на {name} отсутствует Renderer.");

            return false;
        }

        Material[] materials = _renderer.materials;

        if (materials == null || materials.Length == 0)
        {
            Debug.LogError(
                $"BarrierDissolve: на {name} отсутствуют материалы.");

            return false;
        }

        _dissolveMaterial = materials[0];
        return _dissolveMaterial != null;
    }

    private void CompleteUnlock(Action onCompleted)
    {
        _dissolveTween = null;

        // ИЗМЕНЕНО: сначала деактивируем объект,
        // затем сохраняем состояние через callback.
        gameObject.SetActive(false);

        onCompleted?.Invoke();

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        _dissolveTween?.Kill();
        _dissolveTween = null;
    }
}