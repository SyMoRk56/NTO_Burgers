using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    public string key;

    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    private void OnDisable()
    {
        LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }

    private void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        text.text = LocalizationManager.Instance.GetText(key);
    }

    public void SetArguments(params (string, string)[] args)
    {
        string localized = LocalizationManager.Instance.GetText(key);

        foreach (var arg in args)
        {
            localized = localized.Replace("{" + arg.Item1 + "}", arg.Item2);
        }

        text.text = localized;
    }
}
