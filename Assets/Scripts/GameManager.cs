using System.Collections.Generic;
using Unity.Android.Gradle;
using UnityEngine;

public enum GameState
{
    Playing,
    Win,
    GameOver
}
//classe que representa o json
[System.Serializable]
public class LevelData
{
    public int level;
    public int width;
    public int height;
    public int[] layout;
}
public class GameManager : MonoBehaviour
{
    private GameState m_currentState;

    private string m_currentLevel = "Level_0";
    public GameObject blockPrefab;
    void Start()
    {
        ChangeState(GameState.Playing);
    }

    void ChangeState(GameState newState)
    {
        m_currentState = newState;

        switch (m_currentState)
        {
            case GameState.Playing:
                LoadLevel(m_currentLevel);
                break;
            case GameState.Win:
                Debug.Log("Você venceu");
                break;
            case GameState.GameOver:
                Debug.Log("Você perdeu");
                break;
        }
    }

    void LoadLevel(string levelName)
    {
        TextAsset levelData = Resources.Load<TextAsset>("Levels/" + levelName);

        if (levelData != null)
        {
            LevelData level = JsonUtility.FromJson<LevelData>(levelData.text);

            if (level != null && level.layout != null)
            {
                Debug.Log("Nível carregado com sucesso. Nível: " + level.level);
                GenerateLevel(level);
            }
            else
            {
                Debug.LogError("Erro ao desserializar o JSON. Level ou layout é nulo.");
            }
        }
        else
        {
            Debug.LogError("Level not found" + levelName);
        }
    }

    void GenerateLevel(LevelData level)
    {
        var layout = level.layout;

        for (int y = 0; y < level.height; y++)
        {
            for (int x = 0; x < level.width; x++)
            {
                int i = y * level.width + x;
                var data = level.layout[i];
                Debug.Log(data);
                if (data == 1)
                {
                  Instantiate(blockPrefab, new Vector3(x, y, 0), Quaternion.identity);  
                }
            }
        }
    }

}
