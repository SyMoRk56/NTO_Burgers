using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdressListMenu : MonoBehaviour
{
    public static AdressListMenu Instance;
    public GameObject tabTaskUIPrefab;
    public Transform tasksParent;
    public GameObject label;
    private const int MAX_TASKS = 3;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (label != null) label.SetActive(true);
        UpdateTasks();
    }

    public void UpdateTasks()
    {
        if (tasksParent == null || tabTaskUIPrefab == null) return;

        for (int i = tasksParent.childCount - 1; i >= 0; i--)
            Destroy(tasksParent.GetChild(i).gameObject);

        var allMails = PlayerMailInventory.Instance.carriedMails;

        // Сначала сюжетные
        var storyMails = allMails.Where(t => t.isStory);

        // Потом обычные
        var regularMails = allMails.Where(t => !t.isStory);

        // Объединяем и только потом ограничиваем
        var mails = storyMails
            .Concat(regularMails)
            .Take(MAX_TASKS)
            .ToList();

        foreach (var task in mails)
        {
            var go = Instantiate(tabTaskUIPrefab, tasksParent);
            go.SetActive(true);

            var text = go.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = LocalizationManager.Instance.Get(task.adress);

            var star = go.transform.Find("star");
            if (star != null)
                star.gameObject.SetActive(task.isStory);
        }

        if (label != null)
        {
            var rect = label.GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0, 1000 - 229.6246f * tasksParent.childCount);
        }
    }

    public void SetVisible(bool visible)
    {
        if (tasksParent != null)
            tasksParent.gameObject.SetActive(visible);
    }
}