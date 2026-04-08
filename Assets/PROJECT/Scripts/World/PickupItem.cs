using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        // Проверка дистанции до предмета
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }

    [SerializeField] public Vector3 positionOffset, rotationOffset; // Смещение в руке
    bool parented = false; // Взят ли предмет

    public bool CheckInteract()
    {
        print("PickupItem check interact");

        return true; // Всегда можно взаимодействовать
    }

    public void Interact()
    {
        // Переключаем состояние (взять / бросить)
        SetParent(!parented);
    }

    private void SetParent(bool parented)
    {
        Debug.LogWarning("Set parent" + parented + "  " + Time.time);

        this.parented = parented;

        // Если берём — привязываем к руке, иначе отвязываем
        transform.SetParent(!parented ? null : PlayerManager.instance.hand);

        if (parented)
        {
            // Применяем локальную позицию и поворот в руке
            transform.localRotation = Quaternion.Euler(rotationOffset);
            transform.localPosition = positionOffset;
        }

        // Выключаем коллайдер когда предмет в руке
        GetComponent<Collider>().enabled = !parented;

        // Делаем Rigidbody кинематическим в руке
        GetComponent<Rigidbody>().isKinematic = parented;
    }

    public int InteractPriority()
    {
        // В руке приоритет выше (чтобы легче "сбросить")
        return parented ? 30 : 0;
    }

    public void OnBeginInteract()
    {
    }

    public void OnEndInteract(bool success)
    {
    }

    // Вызывается при создании объекта
    void Start()
    {

    }

    // Вызывается каждый кадр
    void Update()
    {

    }
}