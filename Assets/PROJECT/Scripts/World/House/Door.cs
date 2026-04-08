using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Door : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        // Проверка дистанции до двери через UI
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }

    public Transform teleportTo;     // Куда телепортировать игрока
    public Image blackScreen;        // Чёрный экран для фейда
    Coroutine t;                    // Ссылка на корутину перехода
    public bool isInTransition;     // Сейчас идёт переход?
    public bool firstPerson;        // Включить ли first person после телепорта

    [Header("Это выход из дома (наружу)?")]
    [Tooltip("Поставь true только на двери которая ведёт НА УЛИЦУ")]
    public bool isExitFromHouse = false; // Флаг выхода наружу (для туториала)

    [Header("Звуковые эффекты")]
    public AudioSource audioSource; // Источник звука
    public AudioClip doorOpenSound; // Звук открытия двери

    private void Start() { }

    public void Interact()
    {
        // Проверяем — не идёт ли уже переход у любой двери
        var tran = false;
        foreach (var e in FindObjectsByType<Door>(FindObjectsSortMode.None))
            tran |= e.isInTransition;

        // Если всё ок — запускаем телепорт
        if (!tran)
            t = StartCoroutine(TeleportCoroutine());

        // Проверка туториального письма
        if (PlayerMailInventory.Instance.carriedMails.Count > 0)
            if (PlayerMailInventory.Instance.carriedMails[0].id == "Tutorial_3")
            {
                PlayerMailInventory.Instance.RemoveFirstMail();
                TaskManager.Instance.RemoveTask("Tutorial_3");
            }
    }

    private void PlayDoorSound()
    {
        // Если звук не задан — ничего не делаем
        if (doorOpenSound == null) return;

        // Если нет AudioSource — создаём
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Проигрываем звук
        audioSource.clip = doorOpenSound;
        audioSource.Play();
    }

    IEnumerator TeleportCoroutine()
    {
        isInTransition = true; // блокируем другие двери

        PlayDoorSound(); // звук открытия

        // Блокируем движение игрока
        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = false;

        // Затемняем экран
        blackScreen.DOFade(1, 1);
        yield return new WaitForSeconds(1);

        // Телепортируем игрока
        GameManager.Instance.GetPlayer().GetComponent<Rigidbody>().MovePosition(teleportTo.position);

        // Переключаем камеру
        CameraSwitcher.Instance.SetCameraMode(firstPerson);

        // Небольшая пауза в темноте
        yield return new WaitForSeconds(2);

        // Возвращаем изображение
        blackScreen.DOFade(0, 1);

        // Разрешаем движение
        GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = true;

        // ── Туториал: игрок вышел из дома ──
        if (isExitFromHouse)
            TutorialManager.Instance?.OnPlayerExitedHouse();
        // ────────────────────────────────

        yield return new WaitForSeconds(2);

        isInTransition = false; // разблокируем двери
    }

    public void DipFromBlack()
    {
        // Резкий выход из чёрного (медленный фейд)
        print("Dip from black");
        blackScreen.DOFade(0, 4);
    }

    public void Transition()
    {
        // Быстрый фейд туда-обратно
        blackScreen.DOFade(1, 1);
        blackScreen.DOFade(0, 2).SetDelay(2);
    }

    public int InteractPriority() => 0;

    public bool CheckInteract()
    {
        // Проверяем, не идёт ли переход
        var tran = false;
        foreach (var e in FindObjectsByType<Door>(FindObjectsSortMode.None))
            tran |= e.isInTransition;

        // Можно ли взаимодействовать (и есть ли "Bag" у игрока)
        return !tran && FindChildWithTag(PlayerManager.instance.transform, "Bag") != null;
    }

    private Transform FindChildWithTag(Transform parent, string tag)
    {
        // Рекурсивный поиск дочернего объекта по тегу
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