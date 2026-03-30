using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuestListItem : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text questTitle;
    public TMP_Text questDescription;
    public TMP_Text questAddress;
    public Image questIcon;
    public Button selectButton;
    public GameObject completedMark;

    private Task _task;
    private QuestListMenu _parentMenu;
    private bool _isSelected;

    public void Initialize(Task task, QuestListMenu parentMenu, bool isCompleted)
    {
        _task = task;
        _parentMenu = parentMenu;

        // ✅ Проверка через id, так как Task — struct
        if (questTitle != null && !string.IsNullOrEmpty(task.id))
            questTitle.text = task.recieverName;

        if (questDescription != null && !string.IsNullOrEmpty(task.id))
            questDescription.text = GetLocalizedDescription(task.id);

        if (questAddress != null && !string.IsNullOrEmpty(task.id))
            questAddress.text = LocalizationManager.Instance.Get(task.adress);

        if (completedMark != null)
            completedMark.SetActive(isCompleted);

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => _parentMenu.OnQuestSelected(this));
        }

        UpdateVisuals();
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        Image background = GetComponent<Image>();
        if (background != null)
        {
            background.color = _isSelected ? new Color(0.3f, 0.6f, 1f, 0.5f) : new Color(0, 0, 0, 0.3f);
        }
    }

    private string GetLocalizedDescription(string taskId)
    {
        return $"Доставить: {LocalizationManager.Instance.Get(taskId)}";
    }

    // ✅ Возвращаем сам struct (не может быть null)
    public Task GetTask() => _task;

    // ✅ Проверка "валидности" задачи через id
    public bool IsValid() => !string.IsNullOrEmpty(_task.id);

    public bool IsSelected() => _isSelected;
}