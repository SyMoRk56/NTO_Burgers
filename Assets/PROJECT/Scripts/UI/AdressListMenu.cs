using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AdressListMenu : MonoBehaviour
{
    public GameObject tabTaskUIPrefab;
    public GameObject tasksParent;
    public GameObject label;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            label.SetActive(true);
            UpdateTasks();
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            label.SetActive(false);
        }
    }
    private void Start()
    {
        label.GetComponent<RectTransform>().offsetMin = new Vector2(240, 1000 - 229.6246f * tasksParent.transform.childCount);
    }
    void UpdateTasks()
    {
        for(int i = 0; i < tasksParent.transform.childCount; i++)
        {
            Destroy(tasksParent.transform.GetChild(i).gameObject);
        }
        var l = PlayerMailInventory.Instance.carriedMails.ToList();
        
        foreach (var task in l)
        {
            var go = Instantiate(tabTaskUIPrefab, tasksParent.transform);
            go.GetComponentInChildren<TMP_Text>().text = LocalizationManager.Instance.Get(task.adress);
        }
        label.GetComponent<RectTransform>().offsetMin = new Vector2(0, 1000 - 229.6246f * tasksParent.transform.childCount);
    }
}