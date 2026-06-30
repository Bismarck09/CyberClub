using TMPro;
using UnityEngine;

public class TutorialObjectiveHint : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _text;

    private void Awake()
    {
        if (_root == null)
            _root = gameObject;

        Hide();
    }

    public void Show(string text)
    {
        if (_root != null)
            _root.SetActive(true);

        if (_text != null)
            _text.text = text;
    }

    public void Hide()
    {
        if (_root != null)
            _root.SetActive(false);
    }
}