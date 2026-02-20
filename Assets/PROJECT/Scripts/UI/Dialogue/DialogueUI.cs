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

    public bool hideOnAwake = true;

    bool isTyping = false;
    string currentFullText = "";

    public bool IsTyping => isTyping;

    void Awake()
    {
        runner = GetComponentInParent<DialogueRunner>();
        if (hideOnAwake)
            Hide();
    }

    public void ShowPhrase(string name, string text)
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        ClearChoices();
        gameObject.SetActive(true);

        phraseText.text = "";
        nameText.text = LocalizationManager.Instance.Get(name);

        currentFullText = LocalizationManager.Instance.Get(text);
        typeCoroutine = StartCoroutine(TypeText(currentFullText));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        phraseText.text = "";

        yield return new WaitForSeconds(typeSpeed * 2);

        foreach (char c in text)
        {
            phraseText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        typeCoroutine = null;
    }

    public void CompleteTypingInstantly()
    {
        if (!isTyping) return;

        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        phraseText.text = currentFullText;
        isTyping = false;
    }

    public void ShowChoices(DialogueChoice[] choices)
    {
        ClearChoices();
        phraseText.text = "";

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
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        ClearChoices();
        phraseText.text = "";
        nameText.text = "";
        gameObject.SetActive(false);

        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = true;
        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().ShowCursor(false);
    }

    public void ForceHide()
    {
        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
        }

        ClearChoices();
        phraseText.text = "";
        nameText.text = "";
        gameObject.SetActive(false);

        var player = GameManager.Instance.GetPlayer();
        if (player != null)
        {
            var playerManager = player.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.CanMove = true;
                playerManager.ShowCursor(false);
            }
        }
    }

    void ClearChoices()
    {
        foreach (Transform t in choicesParent)
            Destroy(t.gameObject);
    }
}
