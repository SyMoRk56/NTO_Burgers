using UnityEngine;
using System.Collections;

public class ObjectInitializer : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(InitializeObjects());
    }

    IEnumerator InitializeObjects()
    {
        // Ждем один кадр, чтобы все объекты успели инициализироваться
        yield return null;

        Debug.Log("Инициализация объектов из сохранения...");

        // Инициализируем скамейки
        Bench[] allBenches = FindObjectsOfType<Bench>();
        foreach (Bench bench in allBenches)
        {
            bench.ForceUpdateFromSave();
        }

        // Инициализируем деревья
        TreeCutscene[] allTrees = FindObjectsOfType<TreeCutscene>();
        foreach (TreeCutscene tree in allTrees)
        {
            tree.ForceUpdateFromSave();
        }

        Debug.Log($"Инициализировано {allBenches.Length} скамеек и {allTrees.Length} деревьев");
    }
}