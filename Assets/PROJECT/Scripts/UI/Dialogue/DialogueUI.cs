using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class DialogueUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI phraseText;
    public Transform choicesParent;
    public GameObject choiceButtonPrefab;

    DialogueRunner runner;
    Coroutine typeCoroutine;
    float typeSpeed = 0.03f;
    public AudioSource voiceOver;
    public bool hideOnAwake = true;

    void Awake()
    {
        runner = GetComponentInParent<DialogueRunner>();
        if(hideOnAwake)
        Hide();
    }

    public void ShowPhrase(string name, string text, AudioClip clip)
    {
        ClearChoices();
        gameObject.SetActive(true);
        nameText.text = LocalizationManager.Instance.Get(name);

        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        typeCoroutine = StartCoroutine(TypeText(LocalizationManager.Instance.Get(text), clip));
    }

    IEnumerator TypeText(string text, AudioClip clip)
    {
        voiceOver.DOFade(0, .2f);
        if(clip!= null && LocalizationManager.Instance.CurrentLanguage == "RU")
        {
            voiceOver.clip = clip;
            voiceOver.PlayDelayed(.2f);
            voiceOver.DOFade(1, .2f).SetDelay(.2f);
        }
        phraseText.text = "";
        foreach (char c in text)
        {
            phraseText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    public void ShowChoices(DialogueChoice[] choices)
    {
        voiceOver.DOFade(0, .2f);

        ClearChoices();
        phraseText.text = "";

        print("ShowChoices: " + choices.Length);
        for (int i = 0; i < choices.Length; i++)
        {
            var obj = Instantiate(choiceButtonPrefab, choicesParent);
            var btn = obj.GetComponent<Button>();
            var txt = obj.GetComponentInChildren<TextMeshProUGUI>();

            txt.text = LocalizationManager.Instance.Get(choices[i].text);
            int index = i;
            btn.onClick.AddListener(() => runner.Choose(index));
        }
        phraseText.text = "...";
    }

    public void Hide()
    {
        print("Hide dialogue");
        gameObject.SetActive(false);
        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = true;
        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().ShowCursor(false);
    }

    void ClearChoices()
    {
        foreach (Transform t in choicesParent)
            Destroy(t.gameObject);
    }
}
