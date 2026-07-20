using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PurchaseFeedbackView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Image _currencyIcon;
    [SerializeField] private AudioSource _audioSource;

    private void Awake()
    {
        ValidateReferences();
    }

    public void SetMessage(string message)
    {
        if (_messageText != null)
            _messageText.text = message;
    }

    public void SetCurrencyIcon(Sprite sprite)
    {
        if (_currencyIcon == null)
            return;

        _currencyIcon.sprite = sprite;
        _currencyIcon.gameObject.SetActive(sprite != null);
    }

    public void Show()
    {
        if (_root == null || _canvasGroup == null)
            return;

        _root.SetActive(true);
        SetAlpha(1f);
    }

    public void Hide()
    {
        if (_canvasGroup != null)
            SetAlpha(0f);

        if (_root != null)
            _root.SetActive(false);
    }

    public void SetAlpha(float alpha)
    {
        if (_canvasGroup != null)
            _canvasGroup.alpha = Mathf.Clamp01(alpha);
    }

    public void PlayFailureSound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
            _audioSource.PlayOneShot(clip);
    }

    private void ValidateReferences()
    {
        if (_root == null)
            ReportMissing(nameof(_root));
        if (_canvasGroup == null)
            ReportMissing(nameof(_canvasGroup));
        if (_messageText == null)
            ReportMissing(nameof(_messageText));
        if (_audioSource == null)
            ReportMissing(nameof(_audioSource));
    }

    private void ReportMissing(string fieldName)
    {
        Debug.LogError($"PurchaseFeedbackView: поле {fieldName} не назначено на GameObject '{name}'.", this);
    }
}
