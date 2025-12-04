using UnityEngine;
using System.Collections;

public class MailBox : MonoBehaviour
{
    [Header("Mailbox Settings")]
    public string mailboxAddress;

    [Header("Dialogue Settings")]
    public DialogueRunner dialogueRunner;
    public bool triggerDialogueAfterDelivery = true;
    public float dialogueDelay = 0.3f; // Задержка перед запуском диалога

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
            Debug.Log($"✓ Найдено подходящее письмо: {mail.recieverName}");
            DeliverMail(mail);

            // Запускаем диалог после доставки С ЗАДЕРЖКОЙ
            if (triggerDialogueAfterDelivery && dialogueRunner != null)
            {
                StartCoroutine(StartDialogueWithDelay(true)); // true для диалога с письмом
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
}