using UnityEngine;

public class PlayerCollisionSound : MonoBehaviour
{
    // Звуки шагов по разным поверхностям
    public AudioClip grassSound;
    public AudioClip woodSound;
    public AudioClip stoneSound;

    public AudioSource audioSource; // Источник звука для воспроизведения

    private Terrain terrain;        // Террейн под игроком
    private TerrainData terrainData; // Данные террейна

    private void Start()
    {
        // Получаем активный террейн
        terrain = Terrain.activeTerrain;
        if (terrain != null)
            terrainData = terrain.terrainData; // Берём данные террейна
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Проверяем столкновение только с объектами слоя "Ground"
        if (collision.gameObject.layer != LayerMask.NameToLayer("Ground")) return;

        float speed = collision.relativeVelocity.y; // Вертикальная скорость столкновения
        if (speed < 7) return; // Слишком слабое столкновение — звук не нужен

        // Если данных террейна нет — выходим
        if (terrainData == null)
            return;

        // Получаем индекс основной текстуры под игроком
        int textureIndex = GetMainTexture(transform.position);

        // Выбираем нужный звук для данной текстуры
        AudioClip clipToPlay = TextureIndexToSound(textureIndex);
        if (clipToPlay != null)
            audioSource.PlayOneShot(clipToPlay, 1); // Проигрываем звук один раз
    }

    private int GetMainTexture(Vector3 worldPos)
    {
        // Преобразуем мировую позицию в локальную позицию террейна
        Vector3 terrainPos = worldPos - terrain.transform.position;

        float x = terrainPos.x / terrainData.size.x;
        float z = terrainPos.z / terrainData.size.z;

        // Индексы в альфа-карте террейна
        int mapX = Mathf.Clamp((int)(x * terrainData.alphamapWidth), 0, terrainData.alphamapWidth - 1);
        int mapZ = Mathf.Clamp((int)(z * terrainData.alphamapHeight), 0, terrainData.alphamapHeight - 1);

        // Получаем все веса текстур на этой точке
        float[,,] splatmap = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        int winner = 0;
        float max = 0;

        // Находим текстуру с наибольшим весом
        for (int i = 0; i < splatmap.GetLength(2); i++)
        {
            if (splatmap[0, 0, i] > max)
            {
                winner = i;
                max = splatmap[0, 0, i];
            }
        }

        return winner; // Индекс основной текстуры
    }

    private AudioClip TextureIndexToSound(int index)
    {
        // Привязываем индексы террейна к звукам
        switch (index)
        {
            case 0: return grassSound; // Трава
            case 1: return woodSound;  // Дерево
            case 2: return stoneSound; // Камень
            default: return null;
        }
    }
}