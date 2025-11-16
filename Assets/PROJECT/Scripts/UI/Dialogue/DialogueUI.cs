using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI phraseText;
    public Transform choicesParent;
    public GameObject choiceButtonPrefab;

    DialogueRunner runner;
    Coroutine typeCoroutine;
    float typeSpeed = 0.03f;

    void Awake()
    {
        runner = GetComponentInParent<DialogueRunner>();
        Hide();
    }

    public void ShowPhrase(string name, string text)
    {
        ClearChoices();
        gameObject.SetActive(true);
        nameText.text = name;

        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        typeCoroutine = StartCoroutine(TypeText(text));
    }

    IEnumerator TypeText(string text)
    {
        phraseText.text = "";
        foreach (char c in text)
        {
            phraseText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    public void ShowChoices(DialogueChoice[] choices)
    {
        ClearChoices();
        phraseText.text = "";

        print("ShowChoices: " + choices.Length);
        for (int i = 0; i < choices.Length; i++)
        {
            var obj = Instantiate(choiceButtonPrefab, choicesParent);
            var btn = obj.GetComponent<Button>();
            var txt = obj.GetComponentInChildren<TextMeshProUGUI>();

            txt.text = choices[i].text;
            int index = i;
            btn.onClick.AddListener(() => runner.Choose(index));
        }
        phraseText.text = "...";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void ClearChoices()
    {
        foreach (Transform t in choicesParent)
            Destroy(t.gameObject);
    }
}
