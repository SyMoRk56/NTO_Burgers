using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NPCIdentity : MonoBehaviour
{
    [SerializeField] public string npcId;

#if UNITY_EDITOR
    private void Reset()
    {
        if (string.IsNullOrEmpty(npcId))
        {
            npcId = GUID.Generate().ToString();
            Debug.Log($"Generated NPC ID for {name}: {npcId}");
        }
    }
#endif
}
