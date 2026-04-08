using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailTool : MonoBehaviour
{
    public TMP_InputField input;
    public Toggle isStory;
    public GameObject canvas;
    private void Update()
    {
        print("MailTool");
        if (Input.GetKeyDown(KeyCode.L))
        {
            print("INPUT TILDE");
            canvas.SetActive(!canvas.activeSelf);
            Cursor.lockState = canvas.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = canvas.activeSelf;
            PlayerManager.instance.CanMove = !canvas.activeSelf;
        }
    }
    public void GetMail()
    {
        PlayerMailInventory.Instance.AddMailToInventory(new Task(input.text, input.text, input.text, isStory.isOn));
    }
    public void ClearList()
    {
        PlayerMailInventory.Instance.ClearInventory();
    }
    public void TeleportToMailbox()
    {
        bool broke = false;
        foreach (var task in PlayerMailInventory.Instance.carriedMails)
        {
            if (!task.isStory) continue;

            foreach (var mailbox in FindObjectsByType<MailBox>(FindObjectsSortMode.None))
            {
                if (mailbox.mailboxAddress == task.adress)
                {
                    // нормализуем координаты по карте
                    PlayerManager.instance.playerMovement.rb.MovePosition(mailbox.transform.position += new Vector3(0, 20, 0));

                    broke = true;
                    break;
                }
            }
            if (broke) break;
        }

        // Если Story не найдено — ищем обычные письма
        if (!broke)
        {
            foreach (var task in PlayerMailInventory.Instance.carriedMails)
            {
                foreach (var mailbox in FindObjectsByType<MailBox>(FindObjectsSortMode.None))
                {
                    if (mailbox.mailboxAddress == task.adress)
                    {
                        PlayerManager.instance.playerMovement.rb.MovePosition(mailbox.transform.position += new Vector3(0, 20, 0));
                        broke = true;
                        break;
                    }
                }
                
            }
        }
    }
}
