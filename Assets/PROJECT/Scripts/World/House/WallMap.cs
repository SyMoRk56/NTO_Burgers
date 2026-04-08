using System.Collections;
using UnityEngine;

public class WallMap : MonoBehaviour, IInteractObject
{
    public bool canPickup; // Можно ли подобрать карту

    IEnumerator Start()
    {
        // Ждём немного (чтобы сцена полностью прогрузилась)
        yield return new WaitForSeconds(3);

        // Если в сцене нет сумки — разрешаем подбор карты
        if (FindFirstObjectByType<BagPickup>(FindObjectsInactive.Exclude) == null)
        {
            canPickup = true;
        }
    }

    public bool CheckDistance()
    {
        // Проверка дистанции до объекта
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }

    public bool CheckInteract()
    {
        // Можно ли взаимодействовать (разрешён ли подбор)
        return canPickup;
    }

    public void Interact()
    {
        // "Прячем" объект (вместо удаления)
        transform.position += new Vector3(0, 1000, 0);

        print("WAll map interact");

        // Сообщаем UI, что у игрока теперь есть сумка
        if (TaskUI.Instance != null)
            TaskUI.Instance.SetHasBag(true);
    }

    public int InteractPriority()
    {
        return 0;
    }

    public void OnBeginInteract()
    {

    }

    public void OnEndInteract(bool success)
    {

    }
}