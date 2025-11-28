using UnityEngine;

public class MailBox : MonoBehaviour
{
    [Header("Mailbox Settings")]
    public string mailboxAddress;

    public void Interact()
    {
        if (PlayerMailInventory.Instance == null)
        {
            Debug.LogError("PlayerMailInventory not found!");
            return;
        }

        Debug.Log($"=== ПРОВЕРКА ПОЧТОВОГО ЯЩИКА ===");
        Debug.Log($"Адрес ящика: '{mailboxAddress}'");

        // Проверяем инвентарь на наличие письма для этого адреса
        if (PlayerMailInventory.Instance.HasMailForAddress(mailboxAddress))
        {
            var mail = PlayerMailInventory.Instance.GetMailForAddress(mailboxAddress);
            Debug.Log($"✓ Найдено подходящее письмо: {mail.recieverName}");
            Debug.Log($"  Адрес письма: '{mail.adress}'");
            Debug.Log($"  Сравнение: '{mail.adress}' == '{mailboxAddress}'");
            DeliverMail(mail);
        }
        else
        {
            Debug.Log($"✗ Нет писем для адреса: '{mailboxAddress}'");

            // Детальная отладка - показываем все письма в инвентаре
            var allMails = PlayerMailInventory.Instance.GetAllMails();
            Debug.Log($"  Всего писем в инвентаре: {allMails.Count}");

            if (allMails.Count > 0)
            {
                Debug.Log($"  Доступные письма:");
                foreach (var mail in allMails)
                {
                    bool addressMatch = mail.adress == mailboxAddress;
                    Debug.Log($"    - '{mail.recieverName}' -> '{mail.adress}' (совпадение: {addressMatch})");
                }
            }
            else
            {
                Debug.Log($"  Инвентарь пуст!");
            }
        }
    }

    private void DeliverMail(Task mail)
    {
        Debug.Log($"=== ДОСТАВКА ПИСЬМА ===");
        Debug.Log($"Получатель: {mail.recieverName}");
        Debug.Log($"Адрес: {mail.adress}");
        Debug.Log($"ID: {mail.id}");

        // Удаляем из инвентаря
        PlayerMailInventory.Instance.RemoveMailFromInventory(mail.id);

        // Удаляем из TaskManager
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.RemoveTask(mail.id);
            Debug.Log($"✓ Письмо удалено из TaskManager");
        }
        else
        {
            Debug.LogError("TaskManager.Instance is null!");
        }

        // Автосохранение
        if (SaveGameManager.Instance != null)
        {
            SaveGameManager.Instance.SaveAuto(false);
            Debug.Log($"✓ Автосохранение выполнено");
        }

        Debug.Log($"✓ Письмо успешно доставлено!");
    }
}