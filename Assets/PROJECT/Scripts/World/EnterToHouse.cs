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

    // Новый метод для телепортации с письмом
    public void InteractWithLetter(Letter letter)
    {
        StartCoroutine(TeleportWithLetterCoroutine(letter));
    }

    IEnumerator TeleportCoroutine()
    {
        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = false;
        blackScreen.DOFade(1, 1);
        yield return new WaitForSeconds(1);
        GameManager.Instance.GetPlayer().transform.position = teleportTo.position;
        yield return new WaitForSeconds(2);
        blackScreen.DOFade(0, 1);
        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = true;
    }

    IEnumerator TeleportWithLetterCoroutine(Letter letter)
    {
        GameObject player = GameManager.Instance.GetPlayer();
        PlayerManager playerManager = player.GetComponent<PlayerManager>();

        playerManager.CanMove = false;
        blackScreen.DOFade(1, 1);
        yield return new WaitForSeconds(1);

        // Телепортируем игрока
        player.transform.position = teleportTo.position;

        // Телепортируем письмо вместе с игроком
        if (letter != null)
        {
            letter.transform.position = teleportTo.position + teleportTo.forward;
        }

        yield return new WaitForSeconds(2);
        blackScreen.DOFade(0, 1);
        playerManager.CanMove = true;
    }
}