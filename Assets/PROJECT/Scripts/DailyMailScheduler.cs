using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class DailyMailScheduler : MonoBehaviour
{
    public static DailyMailScheduler Instance;

    [Header("Mail References")]
    public MailManager mailManager;

    // ✅ Хранит ВСЕ доставленные письма (навсегда)
    private HashSet<string> deliveredMails = new HashSet<string>();

    // ✅ Хранит письма ТЕКУЩЕГО дня
    private List<string> currentDayMailPool = new List<string>();
    private HashSet<string> takenToday = new HashSet<string>();

    private int lastDay = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[DailyMailScheduler] Инициализирован");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void MarkMailAsDelivered(string mailId)
    {
        if (!deliveredMails.Contains(mailId))
        {
            deliveredMails.Add(mailId);
            Debug.Log($"[DailyMailScheduler] Письмо доставлено: {mailId}");
        }
    }

    // ✅ ГЕНЕРАЦИЯ ПУЛА ПИСЕМ НА ДЕНЬ
    private void GenerateDailyMailPool(int day)
    {
        // ✅ КРИТИЧЕСКИ ВАЖНО: Очищаем пул перед генерацией!
        currentDayMailPool.Clear();
        takenToday.Clear();

        // 1 сюжетное письмо
        var storyMail = mailManager.GetStoryMailForDay(day);
        if (storyMail != null && !string.IsNullOrEmpty(storyMail.id))
        {
            currentDayMailPool.Add(storyMail.id);
            Debug.Log($"[DailyMailScheduler] Добавлено сюжетное: {storyMail.id}");
        }

        // 3 обычных письма
        var nonStoryMails = mailManager.GetNonStoryMailsForDay(day);
        int count = 0;
        foreach (var mail in nonStoryMails)
        {
            if (count >= 3) break;
            currentDayMailPool.Add(mail.id);
            Debug.Log($"[DailyMailScheduler] Добавлено обычное: {mail.id}");
            count++;
        }

        Debug.Log($"[DailyMailScheduler] Сгенерировано {currentDayMailPool.Count} писем на день {day}");
    }

    // ✅ Проверка: можно ли брать новые письма?
    public bool CanTakeNewMails()
    {
        // ✅ Проверяем только письма текущего дня
        foreach (var mailId in currentDayMailPool)
        {
            if (!takenToday.Contains(mailId) && !deliveredMails.Contains(mailId))
            {
                return true; // Есть недоставленные
            }
        }
        return false;
    }

    // ✅ Проверка: все ли доставлено?
    public bool AllMailsDelivered()
    {
        foreach (var mailId in currentDayMailPool)
        {
            if (!deliveredMails.Contains(mailId))
            {
                return false;
            }
        }
        return true;
    }

    public List<MailItem> GetAvailableForDesk()
    {
        List<MailItem> available = new List<MailItem>();
        int currentDay = DayNightCycle.Instance.CurrentDay;

        // ✅ Если день изменился — генерируем НОВЫЙ пул писем
        if (currentDay != lastDay)
        {
            lastDay = currentDay;
            GenerateDailyMailPool(currentDay);
            Debug.Log($"[DailyMailScheduler] Новый день {currentDay}, пул писем обновлён");
        }

        // ✅ Проверяем можно ли брать новые
        if (!CanTakeNewMails())
        {
            Debug.Log("[DailyMailScheduler] Нельзя брать новые письма! Сначала доставь текущие!");
            return available;
        }

        // ✅ Возвращаем только НЕ взятые письма из пула
        foreach (var mailId in currentDayMailPool)
        {
            if (!takenToday.Contains(mailId))
            {
                var mail = mailManager.GetMailById(mailId);
                if (mail != null)
                {
                    available.Add(mail);
                }
            }
        }

        Debug.Log($"[DailyMailScheduler] На столе писем: {available.Count} (день {currentDay}, взято: {takenToday.Count})");
        return available;
    }

    public void TakeMailFromDesk(string mailId)
    {
        if (!takenToday.Contains(mailId))
        {
            takenToday.Add(mailId);
            Debug.Log($"[DailyMailScheduler] Письмо взято: {mailId} (взято сегодня: {takenToday.Count}, всего доставлено: {deliveredMails.Count})");
        }
    }

    // ✅ ПРОВЕРКА: Можно ли спать?
    public bool CanSleep()
    {
        // ✅ Проверка 1: Есть ли письма в инвентаре?
        if (PlayerMailInventory.Instance != null &&
            PlayerMailInventory.Instance.carriedMails.Count > 0)
        {
            Debug.Log("--------------------------------------------------");
            Debug.Log("----------------нельзя спать!----------------");
            Debug.Log("Сначала доставь все письма из инвентаря!");
            Debug.Log($"В инвентаре писем: {PlayerMailInventory.Instance.carriedMails.Count}");
            Debug.Log("--------------------------------------------------");
            return false;
        }

        // ✅ Проверка 2: Все ли письма дня доставлены?
        if (!AllMailsDelivered())
        {
            Debug.Log("--------------------------------------------------");
            Debug.Log("----------------нельзя спать!----------------");
            Debug.Log("Сначала доставь все письма которые взял!");
            Debug.Log("--------------------------------------------------");
            return false;
        }

        Debug.Log("[DailyMailScheduler] Можно спать - все письма доставлены");
        return true;
    }

    public void ResetForNewDay()
    {
        Debug.Log("[DailyMailScheduler] Новый день (доставленные письма сохранены)");
    }

    public void SaveTakenMails()
    {
        PlayerPrefs.SetString("DeliveredMails", string.Join(",", deliveredMails));
        PlayerPrefs.SetInt("LastDay", lastDay);
        PlayerPrefs.Save();
    }

    public void LoadTakenMails()
    {
        if (PlayerPrefs.HasKey("DeliveredMails"))
        {
            string[] mails = PlayerPrefs.GetString("DeliveredMails").Split(',');
            foreach (var mail in mails)
            {
                if (!string.IsNullOrEmpty(mail))
                    deliveredMails.Add(mail);
            }
        }

        if (PlayerPrefs.HasKey("LastDay"))
        {
            lastDay = PlayerPrefs.GetInt("LastDay");
        }

        Debug.Log($"[DailyMailScheduler] Загружено доставленных: {deliveredMails.Count}");
    }

    // ✅ Для очистки при новом слоте
    public void ClearAllData()
    {
        deliveredMails.Clear();
        currentDayMailPool.Clear();
        takenToday.Clear();
        lastDay = 0;
        PlayerPrefs.DeleteKey("DeliveredMails");
        PlayerPrefs.DeleteKey("LastDay");
        PlayerPrefs.Save();
        Debug.Log("[DailyMailScheduler] Все данные сброшены");
    }
}