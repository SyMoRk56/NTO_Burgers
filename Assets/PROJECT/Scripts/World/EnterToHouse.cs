using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnterToHouse : MonoBehaviour
{
    public Transform teleportTo;
    public Image blackScreen;
    public void Interact()
    {
        StartCoroutine(TeleportCoroutine());
    }
    IEnumerator TeleportCoroutine()
    {
        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = false;
        blackScreen.DOFade(1, 1);
        yield return new WaitForSeconds(1);
        GameManager.Instance.GetPlayer().transform.position = teleportTo.position;
        yield return new WaitForSeconds(.1f);
        blackScreen.DOFade(0, 1);
        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = true;
    }
}
