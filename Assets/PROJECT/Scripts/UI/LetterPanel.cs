using System.Collections;
using TMPro;
using UnityEngine;

public class LetterPanel : MonoBehaviour
{
    
    public void ShowPanel(bool isFish)
    {
        transform.GetChild(0).gameObject.SetActive(true);
        StartCoroutine(ShowPanelC(isFish));
        Invoke(nameof(HidePanel), 3f);
    }
    IEnumerator ShowPanelC(bool isFish)
    {
        yield return null;
        transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = !isFish ? LocalizationManager.Instance.Get("Delivered") : LocalizationManager.Instance.Get("Delivered_fish");
        print(LocalizationManager.Instance.Get("Delivered_fish") + " " + isFish);
    }
    public void HidePanel()
    {
        transform.GetChild(0).gameObject.SetActive(false);

    }
}
