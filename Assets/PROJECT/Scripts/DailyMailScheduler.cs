using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DailyMailScheduler : MonoBehaviour
{
    public static DailyMailScheduler Instance { get; private set; }

    [Header("Каталог писем")]
    public MailCatalog mailCatalog;

    [Header("Настройки дня")]
    [Tooltip("Максимум обычных писем в день")]
    public int maxRegularMailsPerDay = 4;

    // Письма доступные для взятия со стола прямо сейчас
    private List<MailItem> availableForDesk = new List<MailItem>();

    // ID всех писем которые уже были выставлены на стол (в любой день)
    private HashSet<string> issuedMailIds = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void OnEnable()
    {
        DayNightCycle.OnDayChanged += OnDayChanged;
    }

    private void OnDisable()
    {
        DayNightCycle.OnDayChanged -= OnDayChanged;
    }

    // ── Вызывается DeskUI чтобы получить письма для отображения ──────────
    public List<MailItem> GetAvailableForDesk() => new List<MailItem>(availableForDesk);

    // Когда игрок берёт письмо со стола — убираем из доступных на столе
    // и добавляем в TaskManager как активное задание
    public void TakeMailFromDesk(string mailId)
    {
        var mail = availableForDesk.FirstOrDefault(m => m.id == mailId);
        if (mail == null)
        {
            Debug.LogWarning($"[DailyMailScheduler] TakeMailFromDesk: {mailId} не найден на столе");
            return;
        }

        availableForDesk.Remove(mail);
        TaskManager.Instance?.AddTask(mail.reciever, mail.adress, mail.id);
        Debug.Log($"[DailyMailScheduler] Письмо взято со стола: {mail.id}");
    }

    // ── Смена дня ─────────────────────────────────────────────────────────
    private void OnDayChanged(int day)
    {
        Debug.Log($"[DailyMailScheduler] Новый день {day} — генерируем письма для стола");
        PrepareMailsForDay(day);
    }

    private void PrepareMailsForDay(int day)
    {
        if (mailCatalog == null)
        {
            Debug.LogError("[DailyMailScheduler] mailCatalog не назначен!");
            return;
        }

        // Пропускаем Tutorial_* — они выдаются через туториал напрямую
        // Берём только не выданные ранее и не туториальные
        var notIssued = mailCatalog.mails
            .Where(m => !issuedMailIds.Contains(m.id) && !m.id.StartsWith("Tutorial"))
            .ToList();

        if (notIssued.Count == 0)
        {
            Debug.Log("[DailyMailScheduler] Нет новых писем для выдачи.");
            return;
        }

        // Разделяем на сюжетные и обычные
        var storyMails = notIssued.Where(m => m.isStory).ToList();
        var regularMails = notIssued.Where(m => !m.isStory).ToList();

        var todayMails = new List<MailItem>();

        // День 1 — особый: 1 сюжетное (не туториальное) + 3 обычных
        // Остальные дни: 1 сюжетное + до maxRegularMailsPerDay обычных, итого не более 5
        int regularLimit = (day == 1) ? 3 : maxRegularMailsPerDay;

        // Ровно одно сюжетное строго по порядку из каталога
        if (storyMails.Count > 0)
        {
            todayMails.Add(storyMails[0]);
            Debug.Log($"[DailyMailScheduler] Сюжетное: {storyMails[0].id}");
        }

        // Случайные обычные
        var shuffled = regularMails.OrderBy(_ => UnityEngine.Random.value).ToList();
        int toTake = Mathf.Min(regularLimit, shuffled.Count);
        for (int i = 0; i < toTake; i++)
            todayMails.Add(shuffled[i]);

        // Жёсткий лимит 5
        if (todayMails.Count > 5)
            todayMails = todayMails.Take(5).ToList();

        // Кладём на стол (в availableForDesk) и помечаем как выданные
        foreach (var mail in todayMails)
        {
            availableForDesk.Add(mail);
            issuedMailIds.Add(mail.id);
            Debug.Log($"[DailyMailScheduler]   стол ← {mail.id} | {mail.reciever}" +
                      $"{(mail.isStory ? " [СЮЖЕТНОЕ]" : "")}");
        }

        Debug.Log($"[DailyMailScheduler] На столе сегодня: {todayMails.Count} писем");
    }

    // ── Сохранение / загрузка ─────────────────────────────────────────────
    public DailyMailSchedulerSaveData GetSaveData()
    {
        return new DailyMailSchedulerSaveData
        {
            issuedMailIds = new List<string>(issuedMailIds),
            availableForDesk = availableForDesk.Select(m => m.id).ToList()
        };
    }

    public void LoadSaveData(DailyMailSchedulerSaveData data)
    {
        if (data == null) return;

        issuedMailIds = new HashSet<string>(data.issuedMailIds ?? new List<string>());

        // Восстанавливаем письма которые ещё лежат на столе
        availableForDesk.Clear();
        if (data.availableForDesk != null && mailCatalog != null)
        {
            foreach (var id in data.availableForDesk)
            {
                var mail = mailCatalog.mails.FirstOrDefault(m => m.id == id);
                if (mail != null)
                    availableForDesk.Add(mail);
            }
        }

        Debug.Log($"[DailyMailScheduler] Загружено: выдано={issuedMailIds.Count}, на столе={availableForDesk.Count}");
    }
}

[System.Serializable]
public class DailyMailSchedulerSaveData
{
    public List<string> issuedMailIds = new List<string>();
    public List<string> availableForDesk = new List<string>();
}