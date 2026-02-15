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
    [Header("Звуковые эффекты")]
    public AudioSource audioSource;  // Источник звука для двери
    public AudioClip doorOpenSound;  // Звук открытия двери

    private void Start()
    {
    }
    public void Interact()
    {
        var tran = false;
        foreach(var e in FindObjectsByType<EnterToHouse>(FindObjectsSortMode.None))
        {
            tran |= e.isInTransition;

        }
        if(!tran)
        t = StartCoroutine(TeleportCoroutine());

        if (PlayerMailInventory.Instance.carriedMails[0].id == "Tutorial_3")
        {
            PlayerMailInventory.Instance.RemoveFirstMail();
        }
    }

    private void PlayDoorSound()
    {
        if (doorOpenSound == null) return;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = doorOpenSound;
        audioSource.Play();
    }



    IEnumerator TeleportCoroutine()
    {
        isInTransition = true;

        PlayDoorSound(); // 🔊 здесь звук двери

        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = false;
        blackScreen.DOFade(1, 1);
        yield return new WaitForSeconds(1);
        GameManager.Instance.GetPlayer().GetComponent<Rigidbody>().MovePosition(teleportTo.position);
        CameraSwitcher.Instance.SetCameraMode(firstPerson);
        yield return new WaitForSeconds(2);
        blackScreen.DOFade(0, 1);
        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = true;
        yield return new WaitForSeconds(2);
        isInTransition = false;
    }
    public void DipFromBlack()
    {
        print("Dip from black");
        blackScreen.DOFade(0, 4);

    }

    public int InteractPriority()
    {
        return 0;
    }

    public bool CheckInteract()
    {
        var tran = false;
        foreach (var e in FindObjectsByType<EnterToHouse>(FindObjectsSortMode.None))
        {
            tran |= e.isInTransition;

        }
        return !tran && FindChildWithTag(PlayerManager.instance.transform, "Bag") != null;
    }
    private Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
                return child;

            Transform result = FindChildWithTag(child, tag);
            if (result != null)
                return result;
        }
        return null;
    }
    public void OnBeginInteract()
    {
    }

    public void OnEndInteract(bool success)
    {
    }
}