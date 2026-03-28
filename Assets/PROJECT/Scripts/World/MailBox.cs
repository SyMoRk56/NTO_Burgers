using UnityEngine;
using System.Collections;
using static UnityEngine.SpriteMask;

public class MailBox : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }
    [Header("Mailbox Settings")]
    public string mailboxAddress;

    [Header("Dialogue Settings")]
    public DialogueRunner dialogueRunner;
    public bool triggerDialogueAfterDelivery = true;
    public float dialogueDelay = 0.3f; // Задержка перед запуском диалога
    public AudioSource mailSource;
    public void Interact()
    {
        if (PlayerMailInventory.Instance == null)
        {
            Debug.LogError("PlayerMailInventory not found!");
            return;
        }

        Debug.Log($"=== ПРОВЕРКА ПОЧТОВОГО ЯЩИКА ===");
        Debug.Log($"Адрес ящика: '{mailboxAddress}'");

        if (PlayerMailInventory.Instance.HasMailForAddress(mailboxAddress))
        {
            var mail = PlayerMailInventory.Instance.GetMailForAddress(mailboxAddress);
            if (mail.recieverName.Contains("Fish_"))
            {
                print("Fish task");
                string fishNameCount = mail.recieverName.Replace("Fish_", "");
                string fishName = fishNameCount.Split(" ")[0];
                int count = 1;
                try { count = int.Parse(fishNameCount.Split(" ")[1]); }
                catch { count = 1; }
                FishScriptableObject[] fishesScr = Resources.LoadAll<FishScriptableObject>("");
                FishScriptableObject fish = null;
                foreach(var f in fishesScr)
                {
                    if(fishName == f.name)
                    {
                        fish = f;   
                    }
                }
                if (fish == null) return;
                if (FishInventory.instance.carriedFishes[fish] >= count)
                {
                    print("Remove fish");
                    FishInventory.instance.RemoveFishFromInventory(fish, count);
                }
                else
                {
                    return;
                }
            }
            DeliverMail(mail);
            
            Debug.Log($"✓ Найдено подходящее письмо: {mail.recieverName} {mail.recieverName.Contains("Fish_")}");
            
            FindFirstObjectByType<LetterPanel>().ShowPanel(mail.recieverName.Contains("Fish_"));
            // Запускаем диалог после доставки С ЗАДЕРЖКОЙ
            if (triggerDialogueAfterDelivery && dialogueRunner != null)
            {
                StartCoroutine(StartDialogueWithDelay(true)); // true для диалога с письмом
                if(GetComponent<NPCBehaviour>() == null)
                mailSource.Play();

            }
        }
        else
        {
            Debug.Log($"✗ Нет писем для адреса: '{mailboxAddress}'");

            // Можно запустить обычный диалог, если нет письма
            if (dialogueRunner != null)
            {
                dialogueRunner.StartDialogue(false);
            }
        }
        SaveGameManager.Instance.SaveAuto(true);
    }

    // Новый метод для запуска диалога с задержкой
    private IEnumerator StartDialogueWithDelay(bool isLetterDialogue)
    {
        // Даем время закрыться предыдущему диалогу, если он был
        yield return new WaitForSeconds(dialogueDelay);

        if (dialogueRunner != null)
        {
            dialogueRunner.StartDialogue(isLetterDialogue);
        }
    }

    private void DeliverMail(Task mail)
    {
        Debug.Log($"=== ДОСТАВКА ПИСЬМА ===");
        Debug.Log($"Получатель: {mail.recieverName}");
        Debug.Log($"Адрес: {mail.adress}");

        PlayerMailInventory.Instance.RemoveMailFromInventory(mail.id);

        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.RemoveTask(mail.id);
            Debug.Log($"✓ Письмо удалено из TaskManager");
        }

        if (SaveGameManager.Instance != null)
        {
            SaveGameManager.Instance.SaveAuto(false);
            Debug.Log($"✓ Автосохранение выполнено");
        }

        Debug.Log($"✓ Письмо успешно доставлено!");
    }

    public int InteractPriority()
    {
        return 10;
    }

    public bool CheckInteract()
    {
        return PlayerMailInventory.Instance.HasMailForAddress(mailboxAddress) || dialogueRunner != null;
    }

    public void OnBeginInteract()
    {
    }

    public void OnEndInteract(bool success)
    {
    }
}