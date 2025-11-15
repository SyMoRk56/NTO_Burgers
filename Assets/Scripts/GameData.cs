using System;

[Serializable]
public class GameData
{
    public int currentLevel;
    public int playerScore;
    public string playerName;
    public float playTime;
    public string[] collectedItems;
    public bool[] completedLevels;
    public int playerHealth;
    public int playerCoins;
    public float musicVolume;
    public float sfxVolume;
    public int graphicsQuality;

    public static GameData CreateNewGame()
    {
        return new GameData
        {
            currentLevel = 1,
            playerScore = 0,
            playerName = "Игрок",
            playTime = 0f,
            collectedItems = new string[0],
            completedLevels = new bool[10],
            playerHealth = 100,
            playerCoins = 0,
            musicVolume = 0.8f,
            sfxVolume = 0.8f,
            graphicsQuality = 2
        };
    }

    public void ResetToNewGame()
    {
        currentLevel = 1;
        playerScore = 0;
        playTime = 0f;
        collectedItems = new string[0];
        completedLevels = new bool[completedLevels.Length];
        playerHealth = 100;
        playerCoins = 0;
    }
}
