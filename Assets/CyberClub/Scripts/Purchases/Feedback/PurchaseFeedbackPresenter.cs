using System.Collections;
using UnityEngine;

public class PurchaseFeedbackPresenter : MonoBehaviour
{
    [Header("Authored view")]
    [SerializeField] private PurchaseFeedbackView _view;

    [Header("Wallets")]
    [SerializeField] private RectTransform _coinsWallet;
    [SerializeField] private RectTransform _gemsWallet;

    [Header("Audio")]
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
        ValidateReferences();
        _view?.Hide();
    }

    private void OnDisable()
    {
        if (_animatedWallet != null)
            _animatedWallet.localScale = _walletInitialScale;

        _animatedWallet = null;
        _messageRoutine = null;
        _walletRoutine = null;
        _view?.Hide();
    }

    public void Show(PurchaseFailureReason reason)
    {
        if (reason == PurchaseFailureReason.None || _view == null)
            return;

        _view.SetMessage(PurchaseFailureMessage.Get(reason));

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

        if (Time.unscaledTime >= _nextSoundTime && _failureClip != null)
        {
            _nextSoundTime = Time.unscaledTime + _soundCooldown;
            _view.PlayFailureSound(_failureClip);
        }
    }

    private IEnumerator ShowMessageRoutine()
    {
        _view.Show();
        yield return new WaitForSecondsRealtime(_messageDuration);

        const float duration = 0.18f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _view.SetAlpha(1f - Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        _view.Hide();
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

    private RectTransform GetWallet(PurchaseFailureReason reason)
    {
        return reason == PurchaseFailureReason.NotEnoughGems ? _gemsWallet :
            reason == PurchaseFailureReason.NotEnoughCoins ? _coinsWallet : null;
    }

    private void ValidateReferences()
    {
        if (_view == null)
            ReportMissing(nameof(_view));
        if (_coinsWallet == null)
            ReportMissing(nameof(_coinsWallet));
        if (_gemsWallet == null)
            ReportMissing(nameof(_gemsWallet));
        if (_failureClip == null)
            ReportMissing(nameof(_failureClip));
    }

    private void ReportMissing(string fieldName)
    {
        Debug.LogError($"PurchaseFeedbackPresenter: поле {fieldName} не назначено на GameObject '{name}'.", this);
    }

}
