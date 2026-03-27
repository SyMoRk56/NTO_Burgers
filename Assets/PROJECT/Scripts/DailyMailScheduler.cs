using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class DailyMailScheduler : MonoBehaviour
{
    public static DailyMailScheduler Instance;

    [Header("Mail References")]
    public MailManager mailManager;

    // ✅ Хранит письма ТЕКУЩЕГО дня (сбрасывается каждый день)
    private HashSet<string> currentDayTakenMails = new HashSet<string>();

    // ✅ Хранит ВСЕ доставленные письма (навсегда)
    private HashSet<string> deliveredMails = new HashSet<string>();

    private int lastDay = -1;

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

    // ✅ Вызывается когда игрок доставляет письмо
    public void MarkMailAsDelivered(string mailId)
    {
        if (!deliveredMails.Contains(mailId))
        {
            deliveredMails.Add(mailId);
            Debug.Log($"[DailyMailScheduler] Письмо доставлено: {mailId}");
        }
    }

    // ✅ Проверка: все ли письма текущего дня доставлены?
    public bool AllMailsDelivered()
    {
        // ✅ Проверяем только письма ТЕКУЩЕГО дня
        foreach (var mailId in currentDayTakenMails)
        {
            if (!deliveredMails.Contains(mailId))
            {
                Debug.Log($"[DailyMailScheduler] Не доставлено: {mailId}");
                return false;
            }
        }
        return true;
    }

    // ✅ Проверка: можно ли брать новые письма?
    public bool CanTakeNewMails()
    {
        // ✅ Можно брать если все письма текущего дня доставлены
        return AllMailsDelivered();
    }

    public List<MailItem> GetAvailableForDesk()
    {
        List<MailItem> available = new List<MailItem>();
        int currentDay = DayNightCycle.Instance.CurrentDay;

        // ✅ Если день изменился — сбрасываем taken и генерируем новые
        if (currentDay != lastDay)
        {
            lastDay = currentDay;
            currentDayTakenMails.Clear(); // ✅ Сброс взятых писем нового дня!
            GenerateDailyMails(currentDay);
            Debug.Log($"[DailyMailScheduler] Новый день {currentDay}, взятые письма сброшены");
        }

        // ✅ ПРОВЕРКА: Нельзя брать новые пока не доставил текущие
        if (!CanTakeNewMails())
        {
            Debug.Log("[DailyMailScheduler] Нельзя брать новые письма! Сначала доставь текущие!");
            return available; // Пустой список
        }

        Debug.Log($"[DailyMailScheduler] Проверка дня {currentDay}, взято: {currentDayTakenMails.Count}, доставлено: {deliveredMails.Count}");

        // 1 сюжетное письмо
        MailItem storyMail = mailManager.GetStoryMailForDay(currentDay);
        if (storyMail != null && !string.IsNullOrEmpty(storyMail.id) && !currentDayTakenMails.Contains(storyMail.id))
        {
            available.Add(storyMail);
        }

        // 3 обычных письма
        List<MailItem> nonStoryMails = mailManager.GetNonStoryMailsForDay(currentDay);
        int count = 0;
        foreach (MailItem mail in nonStoryMails)
        {
            if (count >= 3) break;
            if (!currentDayTakenMails.Contains(mail.id))
            {
                available.Add(mail);
                count++;
            }
        }

        Debug.Log($"[DailyMailScheduler] На столе писем: {available.Count} (день {currentDay})");
        return available;
    }

    private void GenerateDailyMails(int day)
    {
        Debug.Log($"[DailyMailScheduler] Сгенерировано 4 писем на день {day}");
    }

    public void TakeMailFromDesk(string mailId)
    {
        if (!currentDayTakenMails.Contains(mailId))
            currentDayTakenMails.Add(mailId);

        Debug.Log($"[DailyMailScheduler] Письмо взято: {mailId} (взято сегодня: {currentDayTakenMails.Count}, всего доставлено: {deliveredMails.Count})");
    }

    // ✅ ПРОВЕРКА: Можно ли спать?
    public bool CanSleep()
    {
        // ✅ Проверяем ВСЕ взятые письма текущего дня
        if (currentDayTakenMails.Count == 0)
        {
            Debug.Log("[DailyMailScheduler] Можно спать - писем не взято");
            return true;
        }

        foreach (var mailId in currentDayTakenMails)
        {
            if (!deliveredMails.Contains(mailId))
            {
                Debug.Log($"[DailyMailScheduler] Нельзя спать! Не доставлено: {mailId}");
                Debug.Log("--------------------------------------------------");
                Debug.Log("----------------нельзя спать!----------------");
                Debug.Log("Сначала доставь все письма которые взял!");
                Debug.Log("--------------------------------------------------");
                return false;
            }
        }

        Debug.Log("[DailyMailScheduler] Можно спать - все письма доставлены");
        return true;
    }

    public void ResetForNewDay()
    {
        // ✅ НЕ очищаем currentDayTakenMails — это сделает GetAvailableForDesk()
        // ✅ НЕ очищаем deliveredMails — это история навсегда
        Debug.Log("[DailyMailScheduler] Новый день (доставленные письма сохранены)");
    }

    // ✅ Сохранение/загрузка
    public void SaveTakenMails()
    {
        // ✅ Сохраняем только deliveredMails (навсегда)
        PlayerPrefs.SetString("DeliveredMails", string.Join(",", deliveredMails));
        PlayerPrefs.SetInt("LastDay", lastDay);
        PlayerPrefs.Save();
    }

    public void LoadTakenMails()
    {
        // ✅ Загружаем только deliveredMails
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

        // ✅ currentDayTakenMails НЕ загружаем — он сбросится при смене дня!
        Debug.Log($"[DailyMailScheduler] Загружено доставленных: {deliveredMails.Count}");
    }

    // ✅ Для отладки — полная очистка
    public void ClearAllData()
    {
        currentDayTakenMails.Clear();
        deliveredMails.Clear();
        lastDay = -1;
        PlayerPrefs.DeleteKey("DeliveredMails");
        PlayerPrefs.DeleteKey("LastDay");
        PlayerPrefs.Save();
        Debug.Log("[DailyMailScheduler] Все данные сброшены");
    }
}