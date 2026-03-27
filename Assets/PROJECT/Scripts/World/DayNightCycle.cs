using UnityEngine;
using System;

[DefaultExecutionOrder(1000)]
public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

    public static event Action<int> OnDayChanged;

    [Header("День")]
    [SerializeField] private int currentDay = 1;

    private const string SAVE_KEY_DAY = "CurrentDay";

    public int CurrentDay => currentDay;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        currentDay = PlayerPrefs.GetInt(SAVE_KEY_DAY, 1);
        Debug.Log($"[DayNightCycle] Текущий день: {currentDay}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
            AdvanceDay();
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