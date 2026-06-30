using System.Collections;
using TMPro;
using UnityEngine;

public class ComputerBreakdownNotification : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private float _showSeconds = 3f;
    [SerializeField] private string _messageTemplate = "Сломался компьютер: {0}";

    private Coroutine _showCoroutine;

    private void Awake()
    {
        HideInstantly();
    }

    public void ShowBreakdown(string zoneName)
    {
        if (_messageText != null)
            _messageText.text = string.Format(_messageTemplate, zoneName);

        if (_showCoroutine != null)
            StopCoroutine(_showCoroutine);

        _showCoroutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        yield return new WaitForSeconds(_showSeconds);

        float fadeTime = 0.4f;
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;

            if (_canvasGroup != null)
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);

            yield return null;
        }

        HideInstantly();
        _showCoroutine = null;
    }

    private void HideInstantly()
    {
        if (_canvasGroup == null)
            return;

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }
}