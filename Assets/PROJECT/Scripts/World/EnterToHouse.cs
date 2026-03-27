using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnterToHouse : MonoBehaviour, IInteractObject
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

    public void Interact()
    {
        var tran = false;
        foreach (var e in FindObjectsByType<EnterToHouse>(FindObjectsSortMode.None))
            tran |= e.isInTransition;

        if (!tran)
            t = StartCoroutine(TeleportCoroutine());

        // ✅ Безопасная проверка
        if (PlayerMailInventory.Instance != null &&
            PlayerMailInventory.Instance.carriedMails.Count > 0 &&
            PlayerMailInventory.Instance.carriedMails[0].id == "Tutorial_3")
        {
            PlayerMailInventory.Instance.RemoveFirstMail();
        }
    }

    // ✅ ДОБАВЛЕНО: Метод для появления из черного экрана (вызывается из GameManager)
    public void DipFromBlack()
    {
        if (blackScreen != null)
        {
            blackScreen.color = new Color(blackScreen.color.r, blackScreen.color.g, blackScreen.color.b, 1f);
            blackScreen.DOFade(0f, 1f);
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

        if (isExitFromHouse)
            TutorialManager.Instance?.OnPlayerExitedHouse();

        yield return new WaitForSeconds(2);
        isInTransition = false;
    }

    public int InteractPriority() => 0;

    public bool CheckInteract()
    {
        var tran = false;
        foreach (var e in FindObjectsByType<EnterToHouse>(FindObjectsSortMode.None))
            tran |= e.isInTransition;

        // ✅ Безопасная проверка сумки
        if (PlayerManager.instance == null) return false;
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