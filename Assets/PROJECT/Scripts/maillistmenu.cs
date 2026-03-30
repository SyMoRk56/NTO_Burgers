using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class QuestListMenu : MonoBehaviour
{
    [Header("References")]
    public Transform contentPanel;
    public QuestListItem questItemPrefab;
    public ScrollRect scrollRect;

    [Header("Settings")]
    public float itemHeight = 80f;
    public float spacing = 5f;

    private List<QuestListItem> _activeItems = new List<QuestListItem>();
    private QuestListItem _selectedItem;
    private VerticalLayoutGroup _layoutGroup;
    private ContentSizeFitter _contentFitter;

    private void Awake()
    {
        if (contentPanel != null)
        {
            _layoutGroup = contentPanel.GetComponent<VerticalLayoutGroup>();
            _contentFitter = contentPanel.GetComponent<ContentSizeFitter>();

            if (_layoutGroup == null)
            {
                _layoutGroup = contentPanel.gameObject.AddComponent<VerticalLayoutGroup>();
                _layoutGroup.spacing = spacing;
                _layoutGroup.padding = new RectOffset(5, 5, 5, 5);
                _layoutGroup.childAlignment = TextAnchor.UpperCenter;
            }

            if (_contentFitter == null)
            {
                _contentFitter = contentPanel.gameObject.AddComponent<ContentSizeFitter>();
                _contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }
    }

    public void PopulateList(List<Task> quests, List<string> completedQuestIds = null)
    {
        ClearList();

        if (quests == null || quests.Count == 0)
        {
            Debug.Log("Квесты пусты");
            return;
        }

        if (completedQuestIds == null)
            completedQuestIds = new List<string>();

        foreach (var task in quests)
        {
            // ✅ Проверка валидности задачи через id
            if (!string.IsNullOrEmpty(task.id))
            {
                bool isCompleted = completedQuestIds.Contains(task.id);
                CreateQuestItem(task, isCompleted);
            }
        }

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    private void CreateQuestItem(Task task, bool isCompleted)
    {
        if (questItemPrefab == null || contentPanel == null)
        {
            Debug.LogError("QuestItemPrefab или ContentPanel не назначены!");
            return;
        }

        QuestListItem newItem = Instantiate(questItemPrefab, contentPanel);

        if (newItem != null)
        {
            newItem.Initialize(task, this, isCompleted);
            _activeItems.Add(newItem);

            if (_selectedItem == null && !isCompleted)
            {
                OnQuestSelected(newItem);
            }
        }
    }

    public void OnQuestSelected(QuestListItem selectedItem)
    {
        if (selectedItem == null || !selectedItem.IsValid()) return;

        if (_selectedItem != null && _selectedItem != selectedItem)
        {
            _selectedItem.SetSelected(false);
        }

        _selectedItem = selectedItem;
        selectedItem.SetSelected(true);

        Task task = selectedItem.GetTask();

        // ✅ Проверка валидности через id
        if (!string.IsNullOrEmpty(task.id) && TaskUI.Instance != null)
        {
            int remainingCount = GetRemainingQuestsCount();
            TaskUI.Instance.SetTask(task, remainingCount);
        }

        Debug.Log($"Выбран квест: {task.recieverName}");
    }

    private int GetRemainingQuestsCount()
    {
        int count = 0;
        foreach (var item in _activeItems)
        {
            if (item != null && item.IsValid())
            {
                Task t = item.GetTask();
                if (!t.adress.Contains("Tutorial"))
                    count++;
            }
        }
        return count;
    }

    public void ClearList()
    {
        foreach (var item in _activeItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }
        _activeItems.Clear();
        _selectedItem = null;
    }

    public void RefreshList()
    {
        // ✅ Сохраняем ID выбранной задачи, а не сам struct
        string previouslySelectedId = _selectedItem?.IsValid() == true ? _selectedItem.GetTask().id : null;

        if (PlayerMailInventory.Instance != null)
        {
            List<Task> allMails = PlayerMailInventory.Instance.GetAllMails();
            List<string> completedIds = new List<string>();

            PopulateList(allMails, completedIds);

            // ✅ Восстанавливаем выделение по ID
            if (!string.IsNullOrEmpty(previouslySelectedId))
            {
                foreach (var item in _activeItems)
                {
                    if (item != null && item.IsValid() && item.GetTask().id == previouslySelectedId)
                    {
                        OnQuestSelected(item);
                        break;
                    }
                }
            }
        }
    }

    private void OnDisable()
    {
        ClearList();
    }
}