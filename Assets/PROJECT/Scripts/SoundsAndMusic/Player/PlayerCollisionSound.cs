using UnityEngine;

public class PlayerCollisionSound : MonoBehaviour
{
    public AudioClip grassSound;
    public AudioClip woodSound;
    public AudioClip stoneSound;

    public AudioSource audioSource;

    private Terrain terrain;
    private TerrainData terrainData;

    private void Start()
    {
        terrain = Terrain.activeTerrain;
        if (terrain != null)
            terrainData = terrain.terrainData;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Ground")) return;
        float speed = collision.relativeVelocity.y;
        if (speed < 7) return;

        // Проверка что есть террейн
        if (terrainData == null)
            return;

        // Получаем индекс текстуры под игроком
        int textureIndex = GetMainTexture(transform.position);

        AudioClip clipToPlay = TextureIndexToSound(textureIndex);
        if (clipToPlay != null)
            audioSource.PlayOneShot(clipToPlay, 1);
    }

    private int GetMainTexture(Vector3 worldPos)
    {
        Vector3 terrainPos = worldPos - terrain.transform.position;

        float x = terrainPos.x / terrainData.size.x;
        float z = terrainPos.z / terrainData.size.z;

        int mapX = Mathf.Clamp((int)(x * terrainData.alphamapWidth), 0, terrainData.alphamapWidth - 1);
        int mapZ = Mathf.Clamp((int)(z * terrainData.alphamapHeight), 0, terrainData.alphamapHeight - 1);

        float[,,] splatmap = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        int winner = 0;
        float max = 0;

        for (int i = 0; i < splatmap.GetLength(2); i++)
        {
            if (splatmap[0, 0, i] > max)
            {
                winner = i;
                max = splatmap[0, 0, i];
            }
        }

        return winner;
    }

    private AudioClip TextureIndexToSound(int index)
    {
        // Привяжи индексы к материалам террейна
        switch (index)
        {
            case 0: return grassSound;
            case 1: return woodSound;
            case 2: return stoneSound;
            default: return null;
        }
    }
}
