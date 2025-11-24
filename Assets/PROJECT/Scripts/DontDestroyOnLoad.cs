using UnityEngine;

public class DontDestroyOnLoadThis : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
