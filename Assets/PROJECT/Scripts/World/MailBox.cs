using UnityEngine;
using System.Collections;
using static UnityEngine.SpriteMask;

public class MailBox : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        // Проверка дистанции до почтового ящика
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }

    [Header("Mailbox Settings")]
    public string mailboxAddress; // Адрес этого ящика

    [Header("Dialogue Settings")]
    public DialogueRunner dialogueRunner; // Система диалогов
    public bool triggerDialogueAfterDelivery = true; // Запускать диалог после доставки
    public float dialogueDelay = 0.3f; // Задержка перед диалогом
    public AudioSource mailSource; // Звук получения письма

    public void Interact()
    {
        // Проверка наличия инвентаря писем
        if (PlayerMailInventory.Instance == null)
        {
            Debug.LogError("PlayerMailInventory not found!");
            return;
        }

        Debug.Log($"=== ПРОВЕРКА ПОЧТОВОГО ЯЩИКА ===");
        Debug.Log($"Адрес ящика: '{mailboxAddress}'");

        // Есть ли письмо для этого адреса
        if (PlayerMailInventory.Instance.HasMailForAddress(mailboxAddress))
        {
            var mail = PlayerMailInventory.Instance.GetMailForAddress(mailboxAddress);

            // Если это задание на рыбу
            if (mail.recieverName.Contains("Fish_"))
            {
                print("Fish task");

                // Парсим название рыбы и количество
                string fishNameCount = mail.recieverName.Replace("Fish_", "");
                string fishName = fishNameCount.Split(" ")[0];

                int count = 1;
                try { count = int.Parse(fishNameCount.Split(" ")[1]); }
                catch { count = 1; }

                // Загружаем все рыбы
                FishScriptableObject[] fishesScr = Resources.LoadAll<FishScriptableObject>("");

                FishScriptableObject fish = null;

                // Ищем нужную рыбу
                foreach (var f in fishesScr)
                {
                    if (fishName == f.name)
                    {
                        fish = f;
                    }
                }

                if (fish == null) return;

                // Проверяем, хватает ли рыбы у игрока
                if (FishInventory.instance.carriedFishes[fish] >= count)
                {
                    print("Remove fish");
                    FishInventory.instance.RemoveFishFromInventory(fish, count);
                }
                else
                {
                    // Не хватает рыбы — выходим
                    return;
                }
            }

            // Доставляем письмо
            DeliverMail(mail);

            Debug.Log($"✓ Найдено подходящее письмо: {mail.recieverName} {mail.recieverName.Contains("Fish_")}");

            // Показываем UI письма
            FindFirstObjectByType<LetterPanel>().ShowPanel(mail.recieverName.Contains("Fish_"));

            // Даём награду
            PlayerManager.instance.Money += 1;

            // Запускаем диалог после доставки
            if (triggerDialogueAfterDelivery && dialogueRunner != null)
            {
                StartCoroutine(StartDialogueWithDelay(true)); // диалог с письмом

                // Проигрываем звук (если это не NPC)
                if (GetComponent<NPCBehaviour>() == null)
                    mailSource.Play();
            }
        }
        else
        {
            Debug.Log($"✗ Нет писем для адреса: '{mailboxAddress}'");

            // Если писем нет — обычный диалог
            if (dialogueRunner != null)
            {
                dialogueRunner.StartDialogue(false);
            }
        }

        // Автосохранение
        SaveGameManager.Instance.SaveAuto(true);
    }

    // Запуск диалога с небольшой задержкой
    private IEnumerator StartDialogueWithDelay(bool isLetterDialogue)
    {
        // Даём время закрыться предыдущему диалогу
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

        // Удаляем письмо из инвентаря
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
        return 10; // приоритет взаимодействия (выше обычного)
    }

    public bool CheckInteract()
    {
        // Можно взаимодействовать если есть письмо или диалог
        return PlayerMailInventory.Instance.HasMailForAddress(mailboxAddress) || dialogueRunner != null;
    }

    public void OnBeginInteract()
    {
    }

    public void OnEndInteract(bool success)
    {
    }
}