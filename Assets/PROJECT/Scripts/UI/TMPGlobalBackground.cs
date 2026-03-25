using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class TMPGlobalBackground : MonoBehaviour
{
    public Color backgroundColor = new Color(0, 0, 0, 0.4f);
    public Vector2 padding = new Vector2(10, 5);

    private const string BG_NAME = "TMP_Background";
    private static TMPGlobalBackground instance;

    void Awake()
    {
        // Singleton — чтобы не было дублей
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

#if UNITY_EDITOR
    void Start()
    {
        // Для первой сцены в редакторе
        Apply();
    }

    void OnValidate()
    {
        Apply();
    }
#endif

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply();
    }

    void Apply()
    {
        RemoveAllBackgrounds();

        var texts = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var tmp in texts)
        {
            CreateBackground(tmp);
        }
    }

    void RemoveAllBackgrounds()
    {
        var all = FindObjectsOfType<Transform>(true);

        foreach (var t in all)
        {
            if (t.name != BG_NAME)
                continue;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(t.gameObject);
            else
                Destroy(t.gameObject);
#else
            Destroy(t.gameObject);
#endif
        }
    }

    void CreateBackground(TextMeshProUGUI tmp)
    {
        GameObject bg = new GameObject(BG_NAME);
        bg.transform.SetParent(tmp.transform.parent);
        bg.transform.SetSiblingIndex(tmp.transform.GetSiblingIndex());

        Image img = bg.AddComponent<Image>();
        img.color = backgroundColor;

        RectTransform bgRect = bg.GetComponent<RectTransform>();
        RectTransform textRect = tmp.GetComponent<RectTransform>();

        bgRect.anchorMin = textRect.anchorMin;
        bgRect.anchorMax = textRect.anchorMax;
        bgRect.pivot = textRect.pivot;

        bgRect.localPosition = textRect.localPosition;
        bgRect.localRotation = Quaternion.identity;
        bgRect.localScale = Vector3.one;

        bgRect.sizeDelta = textRect.sizeDelta + padding * 2;
    }
}
