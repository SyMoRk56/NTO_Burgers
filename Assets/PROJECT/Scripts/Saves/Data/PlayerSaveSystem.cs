using UnityEngine;
using System.Collections;

public class PlayerSaveSystem : MonoBehaviour
{
    public static PlayerSaveSystem Instance;
    private Transform player;

    IEnumerator Start()
    {
        if (Instance != this && Instance != null) Destroy(Instance.transform.parent.gameObject);
        Instance = this;
        while (GameManager.Instance.GetPlayer() == null) yield return null;
        player = GameManager.Instance.GetPlayer()?.transform;
    }

    public PlayerData GetData()
    {
        if (player == null)
            player = GameManager.Instance.GetPlayer()?.transform;

        if (player == null)
        {
            Debug.LogWarning("Player not found during saving!");
            return null;
        }
        print("PlayerSaveSystem:  get data" + player.transform.position);
        var p = new Vector3(0, 0, 0);
        
          p = PlayerManager.instance.transform.position;
        return new PlayerData
        {

            position = new float[]
            {
                p.x,
                p.y,
                p.z,
            },
            money = PlayerManager.instance.Money,
            day = PlayerManager.instance.Day,
            hasBag = HasBag(), // Сохраняем состояние сумки
            collectedAdditionalLetters = !FindFirstObjectByType<AdditionalLetters>().hasMails,
            complitedMainIslandMainTasks = PlayerMailInventory.Instance.complitedMainLine,
        };
    }

    public void LoadData(PlayerData data)
    {
        if (data == null) return;

        if (player == null)
            player = GameManager.Instance.GetPlayer()?.transform;

        if (player == null)
        {
            Debug.LogError("Player cannot be assigned during load!");
            return;
        }

        print("Set player position " + data.position.Length);
        if (data.position.Length == 3) StartCoroutine(SetPlayerPosDelay(data.position));
        else
        {
            player.GetComponent<PlayerMovement>().enabled = true;
        }
        PlayerManager.instance.Money = data.money;
        PlayerManager.instance.Day = data.day;
        if (data.hasBag && !HasBag())
        {
            CreateBagForPlayer();
            Destroy(FindFirstObjectByType<BagPickup>().gameObject);
            foreach (var g in FindObjectsByType<EnterToHouse>(FindObjectsSortMode.None))
            {
                g.enabled = true;
            }
            FindFirstObjectByType<AdditionalLetters>().hasMails = !data.collectedAdditionalLetters;
            PlayerMailInventory.Instance.complitedMainLine = data.complitedMainIslandMainTasks;
            PlayerManager.instance.SetThunder(!data.complitedMainIslandMainTasks);
        }

        // Пробрасываем hasBag в TaskUI
        if (TaskUI.Instance != null)
        {
            TaskUI.Instance.hasBag = data.hasBag;
            if(data.hasBag)
            FindFirstObjectByType<WallMap>().Interact();
        }
            
    }
    IEnumerator SetPlayerPosDelay(float[] pos)
    {
        player.position = new Vector3(
           pos[0],
           pos[1],
           pos[2]);
        print(pos[0]);
        print(pos[2]);
        print(player.position);
        yield return new WaitForSeconds(.3f);
        player.GetComponent<PlayerMovement>().enabled = true;
        yield break;
    }

    // Проверка наличия сумки
    private bool HasBag()
    {
        if (player == null) return false;
        return FindChildWithTag(player.transform, "Bag") != null;
    }

    // Создание сумки при загрузке
    private void CreateBagForPlayer()
    {
        BagPickup bagPickup = FindObjectOfType<BagPickup>();
        if (bagPickup != null && bagPickup.bagPrefab != null)
        {
            Transform parentTransform = player.transform;

            if (!string.IsNullOrEmpty(bagPickup.attachToChildName))
            {
                Transform childTransform = FindChildRecursive(player.transform, bagPickup.attachToChildName);
                if (childTransform != null)
                {
                    parentTransform = childTransform;
                }
            }

            GameObject bagInstance = Instantiate(bagPickup.bagPrefab, parentTransform);
            bagInstance.transform.localPosition = bagPickup.localPosition;
            bagInstance.transform.localEulerAngles = bagPickup.localRotation;

            Debug.Log("Bag restored from save data");
        }
        else
        {
            Debug.LogWarning("BagPickup or bagPrefab not found for restoring bag");
        }
    }

    // Вспомогательный метод для поиска дочерних объектов
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

    // Вспомогательный метод для рекурсивного поиска
    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindChildRecursive(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }
}