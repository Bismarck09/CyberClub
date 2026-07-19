using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseFeedbackPresenter : MonoBehaviour
{
    [Header("Wallets")]
    [SerializeField] private RectTransform _coinsWallet;
    [SerializeField] private RectTransform _gemsWallet;

    [Header("Optional authored feedback UI")]
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private CanvasGroup _messageCanvasGroup;

    [Header("Optional authored audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _failureClip;

    [Header("Timing")]
    [SerializeField, Min(0.1f)] private float _messageDuration = 1.25f;
    [SerializeField, Min(0.05f)] private float _soundCooldown = 0.3f;
    [SerializeField, Min(1f)] private float _walletPunchScale = 1.12f;

    private Coroutine _messageRoutine;
    private Coroutine _walletRoutine;
    private RectTransform _animatedWallet;
    private Vector3 _walletInitialScale;
    private float _nextSoundTime;

    private void Awake()
    {
        EnsureFeedbackView();
        EnsureAudioSource();
        SetMessageVisible(false);
    }

    private void OnDisable()
    {
        if (_animatedWallet != null)
            _animatedWallet.localScale = _walletInitialScale;

        _animatedWallet = null;
        _messageRoutine = null;
        _walletRoutine = null;
        SetMessageVisible(false);
    }

    public void Show(PurchaseFailureReason reason)
    {
        if (reason == PurchaseFailureReason.None)
            return;

        EnsureFeedbackView();
        EnsureAudioSource();

        if (_messageText != null)
            _messageText.text = GetMessage(reason);

        if (_messageRoutine != null)
            StopCoroutine(_messageRoutine);

        _messageRoutine = StartCoroutine(ShowMessageRoutine());

        RectTransform wallet = GetWallet(reason);

        if (wallet != null)
        {
            if (_walletRoutine != null)
                StopCoroutine(_walletRoutine);

            if (_animatedWallet != null)
                _animatedWallet.localScale = _walletInitialScale;

            _animatedWallet = wallet;
            _walletInitialScale = wallet.localScale;
            _walletRoutine = StartCoroutine(PunchWalletRoutine(wallet));
        }

        if (Time.unscaledTime >= _nextSoundTime && _audioSource != null && _failureClip != null)
        {
            _nextSoundTime = Time.unscaledTime + _soundCooldown;
            _audioSource.PlayOneShot(_failureClip);
        }
    }

    private IEnumerator ShowMessageRoutine()
    {
        SetMessageVisible(true);
        yield return new WaitForSecondsRealtime(_messageDuration);

        float duration = 0.18f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            if (_messageCanvasGroup != null)
                _messageCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / duration);

            yield return null;
        }

        SetMessageVisible(false);
        _messageRoutine = null;
    }

    private IEnumerator PunchWalletRoutine(RectTransform wallet)
    {
        const float halfDuration = 0.09f;
        Vector3 startScale = _walletInitialScale;
        Vector3 peakScale = startScale * _walletPunchScale;

        for (int phase = 0; phase < 2; phase++)
        {
            float elapsed = 0f;

            while (elapsed < halfDuration && wallet != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / halfDuration);
                wallet.localScale = phase == 0
                    ? Vector3.Lerp(startScale, peakScale, progress)
                    : Vector3.Lerp(peakScale, startScale, progress);
                yield return null;
            }
        }

        if (wallet != null)
            wallet.localScale = startScale;

        _animatedWallet = null;
        _walletRoutine = null;
    }

    private void EnsureFeedbackView()
    {
        if (_messageText != null && _messageCanvasGroup != null)
            return;

        GameObject root = new GameObject(
            "PurchaseFeedbackMessage",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup));
        root.layer = gameObject.layer;
        root.transform.SetParent(transform, false);

        RectTransform rect = (RectTransform)root.transform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -90f);
        rect.sizeDelta = new Vector2(540f, 64f);

        Image background = root.GetComponent<Image>();
        background.color = new Color(0.08f, 0.09f, 0.13f, 0.94f);
        background.raycastTarget = false;

        _messageCanvasGroup = root.GetComponent<CanvasGroup>();
        _messageCanvasGroup.interactable = false;
        _messageCanvasGroup.blocksRaycasts = false;

        GameObject textObject = new GameObject("Message", typeof(RectTransform), typeof(CanvasRenderer));
        textObject.layer = gameObject.layer;
        textObject.transform.SetParent(root.transform, false);

        RectTransform textRect = (RectTransform)textObject.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 8f);
        textRect.offsetMax = new Vector2(-20f, -8f);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 30f;
        text.enableAutoSizing = true;
        text.fontSizeMin = 18f;
        text.fontSizeMax = 30f;
        text.color = Color.white;
        text.raycastTarget = false;
        _messageText = text;
    }

    private void EnsureAudioSource()
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();

            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();

            _audioSource.playOnAwake = false;
        }

        if (_failureClip == null)
            _failureClip = CreatePlaceholderFailureClip();
    }

    private static AudioClip CreatePlaceholderFailureClip()
    {
        const int frequency = 44100;
        const float duration = 0.1f;
        int sampleCount = Mathf.CeilToInt(frequency * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float progress = i / (float)sampleCount;
            float envelope = 1f - progress;
            samples[i] = Mathf.Sin(2f * Mathf.PI * 180f * i / frequency) * envelope * 0.16f;
        }

        AudioClip clip = AudioClip.Create("PurchaseFailurePlaceholder", sampleCount, 1, frequency, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private RectTransform GetWallet(PurchaseFailureReason reason)
    {
        return reason == PurchaseFailureReason.NotEnoughGems ? _gemsWallet :
            reason == PurchaseFailureReason.NotEnoughCoins ? _coinsWallet : null;
    }

    private void SetMessageVisible(bool visible)
    {
        if (_messageCanvasGroup == null)
            return;

        _messageCanvasGroup.alpha = visible ? 1f : 0f;
        _messageCanvasGroup.gameObject.SetActive(visible);
    }

    private static string GetMessage(PurchaseFailureReason reason)
    {
        return reason switch
        {
            PurchaseFailureReason.NotEnoughCoins => "Недостаточно монет",
            PurchaseFailureReason.NotEnoughGems => "Недостаточно кристаллов",
            PurchaseFailureReason.MaximumReached => "Достигнут максимум",
            PurchaseFailureReason.LockedByTutorial => "Сначала завершите этап обучения",
            PurchaseFailureReason.ProductUnavailable => "Покупка сейчас недоступна",
            PurchaseFailureReason.TransactionFailed => "Не удалось завершить покупку",
            _ => "Покупка недоступна"
        };
    }
}
