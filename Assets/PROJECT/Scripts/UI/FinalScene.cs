using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalScene : MonoBehaviour
{
    float timer;
    public GameObject anyKeyText;
    private void Update()
    {
        timer += Time.deltaTime;
        print(timer);
        if(timer > 3f)
        {
            anyKeyText.SetActive(true); 
        }
        if (Input.anyKeyDown && timer > 3f)
        {
            SceneManager.LoadScene("menu");
        }
    }
}
