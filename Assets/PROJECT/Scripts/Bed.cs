using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Bed : MonoBehaviour, IInteractObject
{
    [Header("Settings")]
    public float sleepDuration = 2f;
    public Image fadeOverlay;

    private bool isSleeping = false;
    private PlayerManager playerManager;

    public bool CheckDistance()
    {
        var interactionUI = GetComponentInChildren<InteractionUI>();
        if (interactionUI != null)
            return interactionUI.CheckDistance();
        return true;
    }

    public void Interact()
    {
        if (isSleeping) return;

        playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager == null) return;

        // ✅ ПРОВЕРКА 1: Есть ли письма в инвентаре?
        if (PlayerMailInventory.Instance != null &&
            PlayerMailInventory.Instance.carriedMails.Count > 0)
        {
            Debug.Log("--------------------------------------------------");
            Debug.Log("----------------нельзя спать!----------------");
            Debug.Log("Сначала доставь все письма из инвентаря!");
            Debug.Log($"В инвентаре писем: {PlayerMailInventory.Instance.carriedMails.Count}");
            Debug.Log("--------------------------------------------------");
            return;
        }

        // ✅ ПРОВЕРКА 2: Все ли доставлено?
        if (DailyMailScheduler.Instance != null)
        {
            if (!DailyMailScheduler.Instance.CanSleep())
            {
                return;
            }
        }

        StartCoroutine(SleepCoroutine());
    }

    private IEnumerator SleepCoroutine()
    {
        isSleeping = true;

        Debug.Log("--------------------------------------------------");
        Debug.Log("----------------Игрок ложится спать----------------");
        Debug.Log("--------------------------------------------------");

        playerManager.CanMove = false;

        if (fadeOverlay != null)
            fadeOverlay.gameObject.SetActive(true);

        yield return new WaitForSeconds(sleepDuration);

        if (DayNightCycle.Instance != null)
        {
            DayNightCycle.Instance.AdvanceDay();
            Debug.Log("----------------День обновлён после сна----------------");
        }

        if (DailyMailScheduler.Instance != null)
        {
            DailyMailScheduler.Instance.ResetForNewDay();
            DailyMailScheduler.Instance.SaveTakenMails();
        }

        if (fadeOverlay != null)
            fadeOverlay.gameObject.SetActive(false);

        playerManager.CanMove = true;
        isSleeping = false;

        Debug.Log("----------------Игрок проснулся! Наступил новый день!----------------");
    }

    public int InteractPriority() => 0;
    public bool CheckInteract() => true;
    public void OnBeginInteract() { }
    public void OnEndInteract(bool success) { }
}