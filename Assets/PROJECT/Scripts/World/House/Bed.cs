using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bed : MonoBehaviour, IInteractObject
{
    public bool CheckDistance()
    {
        // Проверяем дистанцию до кровати
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }

    public bool CheckInteract()
    {
        print("Bed check interact ");

        var taskmtasks = new List<Task>();

        // Проверяем письма у игрока (игнорируем туториал)
        foreach (var t in PlayerMailInventory.Instance.carriedMails)
        {
            print(t.adress);
            if (!t.adress.Contains("Tutorial")) taskmtasks.Add(t);
        }

        var taskmtasksm = new List<Task>();

        // Проверяем активные задания (тоже игнорируем туториал)
        foreach (var t in TaskManager.Instance.tasks)
        {
            print(t.adress);
            if (!t.adress.Contains("Tutorial")) taskmtasksm.Add(t);
        }

        // Спать можно только если нет обычных задач
        return taskmtasks.Count == 0 && taskmtasksm.Count == 0;
    }

    public void Interact()
    {
        // Переход на следующий день
        PlayerManager.instance.Day += 1;

        // Удаляем туториальное письмо/задание
        PlayerMailInventory.Instance.RemoveMailFromInventory("Tutorial_4");
        TaskManager.Instance.RemoveTask("Tutorial_4");

        // Запускаем затемнение (через любую дверь в сцене)
        FindFirstObjectByType<Door>().Transition();

        // Если дошли до 4 дня — финальная сцена
        if (PlayerManager.instance.Day == 4)
        {
            Invoke(nameof(LoadFinalScene), 1f);
        }
    }

    void LoadFinalScene()
    {
        // Загружаем финальную сцену
        SceneManager.LoadScene("Final");
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