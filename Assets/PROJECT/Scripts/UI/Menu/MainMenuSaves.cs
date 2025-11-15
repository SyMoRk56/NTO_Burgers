using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSaves : MonoBehaviour
{
    public void LoadSave(int num)
    {
        bool success = SaveGameManager.Instance.HasManual(num.ToString());
        if (!success)
        {
            SaveGameManager.Instance.SaveManual(num.ToString());
        }
        SceneManager.LoadScene("Game");
    }
    public void Close()
    {
        transform.parent.Find("MainMenu").gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
