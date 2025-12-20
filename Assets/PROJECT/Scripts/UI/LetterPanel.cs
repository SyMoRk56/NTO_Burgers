using UnityEngine;

public class LetterPanel : MonoBehaviour
{
    
    public void ShowPanel()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        Invoke(nameof(HidePanel), 3f);
    }
    public void HidePanel()
    {
        transform.GetChild(0).gameObject.SetActive(false);

    }
}
