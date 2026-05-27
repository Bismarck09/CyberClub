using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class CoinsUIEffect : MonoBehaviour
{
    [SerializeField] private CoinsData _coinsData;
    [SerializeField] private RectTransform _coinsPanel;
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private FloatingResourceText _floatingTextPrefab;
    [SerializeField] private Transform _effectsRoot;
    [SerializeField] private Vector3 _defaultScale;

    private void OnEnable()
    {
        _coinsData.OnCoinsChanged += OnCoinsChanged;
    }

    private void OnDisable()
    {
        _coinsData.OnCoinsChanged -= OnCoinsChanged;
    }

    private void OnCoinsChanged(int amount)
    {
        PlayFloatingText(amount);
        PlayPanelPunch();

        UpdateCoinsText();
    }

    private void UpdateCoinsText()
    {
        _coinsText.text = _coinsData.CurrentCoins.ToString();
    }

    private void PlayFloatingText(int amount)
    {
        Color color = amount >= 0
            ? new Color(1f, 0.85f, 0.2f)
            : new Color(1f, 0.3f, 0.3f);

        var text = Instantiate(_floatingTextPrefab, _effectsRoot);

        text.transform.position = _coinsPanel.position;

        text.Play(amount, color);
    }

    private void PlayPanelPunch()
    {
        _coinsPanel.DOKill();
        _coinsPanel.localScale = _defaultScale;

        _coinsPanel
            .DOPunchScale(Vector3.one * 0.15f, 0.3f, 8, 0.5f);
        
    }
}
