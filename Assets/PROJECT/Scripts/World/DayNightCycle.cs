using UnityEngine;
using System;

[DefaultExecutionOrder(1000)]
public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance;

    public static event Action<int> OnDayChanged;

    [Header("День")]
    [SerializeField] private int currentDay = 1;

    [Header("Время суток")]
    [SerializeField] public float currentTimeOfDay = 0;

    private const string SAVE_KEY_DAY = "CurrentDay";

    public int CurrentDay => currentDay;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        currentDay = 1; // Загрузится из сохранения
        Debug.Log($"[DayNightCycle] Текущий день: {currentDay}");
    }

    private void Update()
    {
        // ❌ УБРАТЬ: Автоматическая смена дня
        // if (Input.GetKeyDown(KeyCode.BackQuote))
        //     AdvanceDay();

        // ✅ День меняется ТОЛЬКО через кровать!
    }

    public void AdvanceDay()
    {
        currentDay++;
        SaveDay();

        Debug.Log($"[DayNightCycle] --------------- смена дня ({currentDay}) ---------------");
        OnDayChanged?.Invoke(currentDay);
    }

    private void SaveDay()
    {
        PlayerPrefs.SetInt(SAVE_KEY_DAY, currentDay);
        PlayerPrefs.Save();
    }

    public void SetDay(int day)
    {
        currentDay = Mathf.Max(1, day);
        SaveDay();
        Debug.Log($"[DayNightCycle] День установлен: {currentDay}");
        OnDayChanged?.Invoke(currentDay);
    }
}