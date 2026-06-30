using TMPro;
using UnityEngine;

public class FormattedPriceText : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private string _prefix;
    [SerializeField] private string _suffix;

    private void Reset()
    {
        _text = GetComponent<TMP_Text>();
    }

    public void SetValue(int value)
    {
        if (_text == null)
            return;

        _text.text = $"{_prefix}{ResourceValueFormatter.Format(value)}{_suffix}";
    }

    public void SetValue(long value)
    {
        if (_text == null)
            return;

        _text.text = $"{_prefix}{ResourceValueFormatter.Format(value)}{_suffix}";
    }
}