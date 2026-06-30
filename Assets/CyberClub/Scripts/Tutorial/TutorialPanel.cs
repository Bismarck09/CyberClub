using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanel : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _bodyText;
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _buttonText;

    private Action _onClick;

    private void Awake()
    {
        if (_root == null)
            _root = gameObject;

        Hide();

        if (_button != null)
            _button.onClick.AddListener(Click);
    }

    private void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(Click);
    }

    public void ShowWindow(string title, string body, string buttonText, Action onClick)
    {
        if (_root != null)
            _root.SetActive(true);

        if (_titleText != null)
            _titleText.text = title;

        if (_bodyText != null)
            _bodyText.text = body;

        if (_buttonText != null)
            _buttonText.text = string.IsNullOrWhiteSpace(buttonText) ? "Далее" : buttonText;

        if (_button != null)
            _button.gameObject.SetActive(true);

        _onClick = onClick;
    }

    public void ShowInfo(string title, string body)
    {
        if (_root != null)
            _root.SetActive(true);

        if (_titleText != null)
            _titleText.text = title;

        if (_bodyText != null)
            _bodyText.text = body;

        if (_button != null)
            _button.gameObject.SetActive(false);

        _onClick = null;
    }

    public void Hide()
    {
        if (_root != null)
            _root.SetActive(false);

        _onClick = null;
    }

    private void Click()
    {
        Action callback = _onClick;
        _onClick = null;
        callback?.Invoke();
    }
}