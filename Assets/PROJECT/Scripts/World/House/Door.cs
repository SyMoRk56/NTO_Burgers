using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Door : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }

    public Transform teleportTo;
    public Image blackScreen;
    Coroutine t;
    public bool isInTransition;
    public bool firstPerson;

    [Header("Это выход из дома (наружу)?")]
    [Tooltip("Поставь true только на двери которая ведёт НА УЛИЦУ")]
    public bool isExitFromHouse = false;

    [Header("Звуковые эффекты")]
    public AudioSource audioSource;
    public AudioClip doorOpenSound;

    private void Start() { }

    public void Interact()
    {
        var tran = false;
        foreach (var e in FindObjectsByType<Door>(FindObjectsSortMode.None))
            tran |= e.isInTransition;

        if (!tran)
            t = StartCoroutine(TeleportCoroutine());
        if(PlayerMailInventory.Instance.carriedMails.Count > 0)
        if (PlayerMailInventory.Instance.carriedMails[0].id == "Tutorial_3")
        {
            PlayerMailInventory.Instance.RemoveFirstMail();
            TaskManager.Instance.RemoveTask("Tutorial_3");
        }
    }

    private void PlayDoorSound()
    {
        if (doorOpenSound == null) return;
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = doorOpenSound;
        audioSource.Play();
    }

    IEnumerator TeleportCoroutine()
    {
        isInTransition = true;
        PlayDoorSound();

        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = false;
        blackScreen.DOFade(1, 1);
        yield return new WaitForSeconds(1);

        GameManager.Instance.GetPlayer().GetComponent<Rigidbody>().MovePosition(teleportTo.position);
        CameraSwitcher.Instance.SetCameraMode(firstPerson);

        yield return new WaitForSeconds(2);
        blackScreen.DOFade(0, 1);
        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = true;

        // ── Туториал: сообщаем что игрок вышел из дома ──────────
        // Срабатывает только если эта дверь помечена как выход наружу
        if (isExitFromHouse)
            TutorialManager.Instance?.OnPlayerExitedHouse();
        // ────────────────────────────────────────────────────────

        yield return new WaitForSeconds(2);
        isInTransition = false;
    }

    public void DipFromBlack()
    {
        print("Dip from black");
        blackScreen.DOFade(0, 4);
    }
    public void Transition()
    {
        blackScreen.DOFade(1, 1);
        blackScreen.DOFade(0, 2).SetDelay(2);
    }
    public int InteractPriority() => 0;

    public bool CheckInteract()
    {
        var tran = false;
        foreach (var e in FindObjectsByType<Door>(FindObjectsSortMode.None))
            tran |= e.isInTransition;
        return !tran && FindChildWithTag(PlayerManager.instance.transform, "Bag") != null;
    }

    private Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag)) return child;
            Transform result = FindChildWithTag(child, tag);
            if (result != null) return result;
        }
        return null;
    }

    public void OnBeginInteract() { }
    public void OnEndInteract(bool success) { }
}