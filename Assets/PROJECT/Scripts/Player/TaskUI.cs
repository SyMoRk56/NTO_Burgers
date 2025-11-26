using TMPro;
using UnityEngine;

public class TaskUI : MonoBehaviour
{
    public static TaskUI Instance;

    [Header("UI References")]
    public GameObject taskCanvas; // ������ �� ������ ������
    public GameObject taskPanel; // ������ �� ������ ������ (�����������)
    public TMP_Text reciever;
    public TMP_Text adress;
    public TMP_Text countText;

    private void Awake()
    {
        Instance = this;

        // �������� ������ ��� ������
        if (taskCanvas != null)
            taskCanvas.SetActive(false);
    }

    public void SetTask(Task task, int lastCount)
    {
        print("Set task " + task.recieverName + task.adress);
        // ���������� ������ ������
        if (taskCanvas != null)
            taskCanvas.SetActive(true);

        // ��������� �� ������ �������� � �������� ���� �����
        if (string.IsNullOrEmpty(task.recieverName) && string.IsNullOrEmpty(task.adress))
        {
            if (taskCanvas != null)
                taskCanvas.SetActive(false);
            return;
        }

        // ������������� �����
        reciever.text = task.recieverName;
        adress.text = task.adress;
        if (lastCount != 0)
            countText.text = lastCount.ToString();
        else countText.text = "";
    }

    // ����� ��� ������� ������� ������ (����� ������� �� ������ ��������)
    public void HideTask()
    {
        if (taskCanvas != null)
            taskCanvas.SetActive(false);
    }
}