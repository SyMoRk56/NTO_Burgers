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
        skamia[] allBenches = FindObjectsOfType<skamia>();
        foreach (skamia bench in allBenches)
        {
            bench.ForceUpdateFromSave();
        }

        // Инициализируем деревья
        treecastscene[] allTrees = FindObjectsOfType<treecastscene>();
        foreach (treecastscene tree in allTrees)
        {
            tree.ForceUpdateFromSave();
        }

        Debug.Log($"Инициализировано {allBenches.Length} скамеек и {allTrees.Length} деревьев");
    }
}