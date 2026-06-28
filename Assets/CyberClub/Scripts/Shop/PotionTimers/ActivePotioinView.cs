using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActivePotionView : MonoBehaviour
{
    [Header("Icon")]
    [SerializeField] private Image _iconBackground;
    [SerializeField] private Image _iconFill;

    [Header("Optional")]
    [SerializeField] private TMP_Text _timeText;

    private PotionType _potionType;

    public PotionType PotionType => _potionType;

    public void Initialize(ActivePotionRuntime potion)
    {
        if (potion == null || potion.Product == null)
            return;

        _potionType = potion.PotionType;

        SetIcon(potion.Product.Icon);
        UpdateView(potion);
    }

    public void UpdateView(ActivePotionRuntime potion)
    {
        if (potion == null)
            return;

        if (_iconFill != null)
            _iconFill.fillAmount = potion.Progress01;

        if (_timeText != null)
            _timeText.text = FormatTime(potion.RemainingTime);
    }

    private void SetIcon(Sprite icon)
    {
        if (_iconBackground != null)
        {
            _iconBackground.enabled = icon != null;
            _iconBackground.sprite = icon;
        }

        if (_iconFill != null)
        {
            _iconFill.enabled = icon != null;
            _iconFill.sprite = icon;
            _iconFill.type = Image.Type.Filled;
            _iconFill.fillMethod = Image.FillMethod.Vertical;
            _iconFill.fillOrigin = (int)Image.OriginVertical.Bottom;
            _iconFill.fillAmount = 1f;
        }
    }

    private string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(seconds);
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;

        return $"{minutes:00}:{remainingSeconds:00}";
    }
}