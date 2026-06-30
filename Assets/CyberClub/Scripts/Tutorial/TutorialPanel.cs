using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanel : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _bodyText;
    [SerializeField] private Button _nextButton;
    [SerializeField] private TMP_Text _nextButtonText;

    private Action _onNext;

    private void Awake()
    {
        if (_root == null)
            _root = gameObject;

        Hide();

        if (_nextButton != null)
            _nextButton.onClick.AddListener(Next);
    }

    private void OnDestroy()
    {
        if (_nextButton != null)
            _nextButton.onClick.RemoveListener(Next);
    }

    public void Show(string title, string body, string buttonText, Action onNext = null, bool showButton = true)
    {
        if (_root != null)
            _root.SetActive(true);

        if (_titleText != null)
            _titleText.text = title;

        if (_bodyText != null)
            _bodyText.text = body;

        if (_nextButtonText != null)
            _nextButtonText.text = string.IsNullOrWhiteSpace(buttonText) ? "Далее" : buttonText;

        if (_nextButton != null)
            _nextButton.gameObject.SetActive(showButton);

        _onNext = onNext;
    }

    public void Hide()
    {
        if (_root != null)
            _root.SetActive(false);

        _onNext = null;
    }

    private void Next()
    {
        Action callback = _onNext;
        _onNext = null;
        callback?.Invoke();
    }
}