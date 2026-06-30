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
    [SerializeField] private Vector3 _defaultScale = Vector3.one;

    private void Start()
    {
        UpdateGemsText();
    }

    private void OnEnable()
    {
        if (_gemsData != null)
            _gemsData.OnGemsChanged += OnGemsChanged;

        UpdateGemsText();
    }

    private void OnDisable()
    {
        if (_gemsData != null)
            _gemsData.OnGemsChanged -= OnGemsChanged;

        if (_gemsPanel != null)
            _gemsPanel.DOKill();
    }

    private void OnGemsChanged(int amount)
    {
        PlayFloatingText(amount);
        PlayPanelAnimation();
        UpdateGemsText();
    }

    private void UpdateGemsText()
    {
        if (_gemsText == null || _gemsData == null)
            return;

        _gemsText.text = ResourceValueFormatter.Format(_gemsData.CurrentGems);
    }

    private void PlayFloatingText(int amount)
    {
        if (_floatingTextPrefab == null || _effectsRoot == null || _gemsPanel == null)
            return;

        Color color = amount >= 0 ? new Color(0.3f, 0.9f, 1f) : new Color(1f, 0.3f, 0.3f);
        FloatingResourceText text = Instantiate(_floatingTextPrefab, _effectsRoot);
        text.transform.position = _gemsPanel.position;
        text.Play(amount, color);
    }

    private void PlayPanelAnimation()
    {
        if (_gemsPanel == null)
            return;

        _gemsPanel.DOKill();
        _gemsPanel.localScale = _defaultScale;
        _gemsPanel.DOPunchScale(Vector3.one * 0.15f, 0.3f, 8, 0.5f);
    }
}