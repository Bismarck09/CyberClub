using UnityEngine;
using DG.Tweening;
using TMPro;

public class CoinsUIEffect : MonoBehaviour
{
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private RectTransform _coinsPanel;
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private FloatingResourceText _floatingTextPrefab;
    [SerializeField] private Transform _effectsRoot;
    [SerializeField] private Vector3 _defaultScale = Vector3.one;

    private void Start()
    {
        UpdateCoinsText();
    }

    private void OnEnable()
    {
        if (_coinsData != null)
            _coinsData.OnCoinsChanged += OnCoinsChanged;

        UpdateCoinsText();
    }

    private void OnDisable()
    {
        if (_coinsData != null)
            _coinsData.OnCoinsChanged -= OnCoinsChanged;

        if (_coinsPanel != null)
            _coinsPanel.DOKill();
    }

    private void OnCoinsChanged(int amount)
    {
        PlayFloatingText(amount);
        PlayPanelPunch();
        UpdateCoinsText();
    }

    private void UpdateCoinsText()
    {
        if (_coinsText == null || _coinsData == null)
            return;

        _coinsText.text = ResourceValueFormatter.Format(_coinsData.CurrentCoins);
    }

    private void PlayFloatingText(int amount)
    {
        if (_floatingTextPrefab == null || _effectsRoot == null || _coinsPanel == null)
            return;

        Color color = amount >= 0 ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.3f, 0.3f);
        FloatingResourceText text = Instantiate(_floatingTextPrefab, _effectsRoot);
        text.transform.position = _coinsPanel.position;
        text.Play(amount, color);
    }

    private void PlayPanelPunch()
    {
        if (_coinsPanel == null)
            return;

        _coinsPanel.DOKill();
        _coinsPanel.localScale = _defaultScale;
        _coinsPanel.DOPunchScale(Vector3.one * 0.15f, 0.3f, 8, 0.5f);
    }
}