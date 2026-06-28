using System.Collections.Generic;
using UnityEngine;

public class ActivePotionsScrollView : MonoBehaviour
{
    [SerializeField] private PotionEffectService _potionEffectService;
    [SerializeField] private ActivePotionView _viewPrefab;
    [SerializeField] private Transform _content;

    private readonly Dictionary<PotionType, ActivePotionView> _views = new();

    private void OnEnable()
    {
        if (_potionEffectService == null)
        {
            Debug.LogError("ActivePotionsScrollView: не назначен PotionEffectService.");
            return;
        }

        _potionEffectService.OnPotionStarted += ShowPotion;
        _potionEffectService.OnPotionUpdated += UpdatePotion;
        _potionEffectService.OnPotionEnded += RemovePotion;

        Rebuild();
    }

    private void OnDisable()
    {
        if (_potionEffectService == null)
            return;

        _potionEffectService.OnPotionStarted -= ShowPotion;
        _potionEffectService.OnPotionUpdated -= UpdatePotion;
        _potionEffectService.OnPotionEnded -= RemovePotion;
    }

    private void Rebuild()
    {
        Clear();

        foreach (ActivePotionRuntime potion in _potionEffectService.ActivePotions)
            ShowPotion(potion);
    }

    private void ShowPotion(ActivePotionRuntime potion)
    {
        if (potion == null)
            return;

        if (_views.TryGetValue(potion.PotionType, out ActivePotionView existingView))
        {
            existingView.Initialize(potion);
            return;
        }

        if (_viewPrefab == null || _content == null)
        {
            Debug.LogError("ActivePotionsScrollView: не назначен prefab или content.");
            return;
        }

        ActivePotionView view = Instantiate(_viewPrefab, _content);
        view.Initialize(potion);

        _views.Add(potion.PotionType, view);
    }

    private void UpdatePotion(ActivePotionRuntime potion)
    {
        if (potion == null)
            return;

        if (_views.TryGetValue(potion.PotionType, out ActivePotionView view))
            view.UpdateView(potion);
    }

    private void RemovePotion(ActivePotionRuntime potion)
    {
        if (potion == null)
            return;

        if (!_views.TryGetValue(potion.PotionType, out ActivePotionView view))
            return;

        _views.Remove(potion.PotionType);

        if (view != null)
            Destroy(view.gameObject);
    }

    private void Clear()
    {
        foreach (ActivePotionView view in _views.Values)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        _views.Clear();
    }
}
