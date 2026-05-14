using UnityEngine;
using DG.Tweening;
using TMPro;

public class GemsUIEffect : MonoBehaviour
{
    [SerializeField] private GemsData _gemsData;
    [SerializeField] private RectTransform _gemsPanel;
    [SerializeField] private TextMeshProUGUI _gemsText;
    [SerializeField] private FloatingResourceText _floatingTextPrefab;
    [SerializeField] private Transform _effectsRoot;

    private void OnEnable()
    {
        _gemsData.OnGemsChanged += OnGemsChanged;
    }

    private void OnDisable()
    {
        _gemsData.OnGemsChanged -= OnGemsChanged;
    }

    private void OnGemsChanged(int amount)
    {
        PlayFloatingText(amount);
        PlayPanelAnimation();

        UpdateGemsText();
    }

    private void UpdateGemsText()
    {
        _gemsText.text = _gemsData.CurrentGems.ToString();
    }

    private void PlayFloatingText(int amount)
    {
        Color color = amount >= 0
            ? new Color(0.3f, 0.9f, 1f)
            : new Color(1f, 0.3f, 0.3f);

        var text = Instantiate(_floatingTextPrefab, _effectsRoot);

        text.transform.position = _gemsPanel.position;

        text.Play(amount, color);
    }

    private void PlayPanelAnimation()
    {
        _gemsPanel.DOKill();

        _gemsPanel
            .DOPunchScale(Vector3.one * 0.15f, 0.3f, 8, 0.5f);
    }
}
