using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class NPCSaveSystem
{
    // ============================
    //   СБОР ВСЕХ NPC ДАННЫХ
    // ============================
    public static List<NPCSaveData> CollectNPCData()
    {
        List<NPCSaveData> list = new List<NPCSaveData>();

        foreach (var npc in Object.FindObjectsOfType<NPCBehaviour>())
        {
            NPCIdentity id = npc.GetComponent<NPCIdentity>();
            if (id == null || string.IsNullOrEmpty(id.npcId))
            {
                Debug.LogWarning("NPC without NPCIdentity: " + npc.name);
                continue;
            }

            NPCSaveData d = new NPCSaveData();
            d.npcId = id.npcId;

            d.position = new float[]
            {
                npc.transform.position.x,
                npc.transform.position.y,
                npc.transform.position.z
            };

            d.currentActionIndex = npc.CurrentActionIndex;
            d.currentTargetName = npc.CurrentTargetName;

            list.Add(d);
        }

        return list;
    }

    // ============================
    //   ВОССТАНОВЛЕНИЕ ВСЕХ NPC
    // ============================
    public static void RestoreNPCData(List<NPCSaveData> list)
    {
        if (list == null) return;

        foreach (var data in list)
        {
            foreach (var npc in Object.FindObjectsOfType<NPCBehaviour>())
            {
                NPCIdentity id = npc.GetComponent<NPCIdentity>();
                if (id == null || id.npcId != data.npcId)
                    continue;

                NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();

                // Восстановление позиции
                if (agent != null)
                    agent.Warp(new Vector3(data.position[0], data.position[1], data.position[2]));
                else
                    npc.transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);

                // Восстановление состояний NPC
                npc.RestoreStateFromSave(data);
            }
        }
    }
}
