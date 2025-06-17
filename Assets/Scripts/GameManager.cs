using UnityEngine;
using System.IO;
using System.Collections.Generic;

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

[System.Serializable]
public class PlayerProgressData
{
    public string lastPlayedLevel;
}

public class GameManager : MonoBehaviour
{
    private GameState m_currentState;

    private string m_currentLevel = "Level_0";
    public GameObject blockPrefab;

    private string m_progressFilePath;

    public Ball ball;
    private List<GameObject> m_activeBricks = new List<GameObject>();

    void Awake()
    {
        m_progressFilePath = Path.Combine(Application.persistentDataPath, "playerProgress.json");
        Debug.Log("Caminho do arquivo de progresso: " + m_progressFilePath);
    }

    void Start()
    {
        LoadProgress();
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
                m_currentLevel = levelName;
                SaveProgress();
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
        //Limpa os blocos antigos
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Block"))
            {
                Destroy(child.gameObject);
            }
        }
        m_activeBricks.Clear();

        var layout = level.layout;
        //centralizar grid
        float offsetX = -level.width / 2f + 0.5f;
        float offsetY = Camera.main.orthographicSize / 2f;

        for (int y = 0; y < level.height; y++)
        {
            for (int x = 0; x < level.width; x++)
            {
                int i = y * level.width + x;
                if (i < layout.Length)
                {
                    var data = level.layout[i];
                    if (data == 1)
                    {
                        Vector3 pos = new Vector3(x + offsetX, y + offsetY, 0);
                        GameObject blockInstance = Instantiate(blockPrefab, pos, Quaternion.identity);
                        blockInstance.tag = "Block";
                        blockInstance.transform.SetParent(transform);

                        m_activeBricks.Add(blockInstance);
                    }
                }
            }
        }
        if (ball != null)
        {
            ball.StartLevel(m_activeBricks);
        }
        else
        {
            Debug.LogError("A referência da bola não foi atribuída no GameManager");
        }
    }

    public void SaveProgress()
    {
        PlayerProgressData progressData = new PlayerProgressData();
        progressData.lastPlayedLevel = m_currentLevel;

        string json = JsonUtility.ToJson(progressData, true);

        try
        {
            File.WriteAllText(m_progressFilePath, json);
            Debug.Log("Progresso salvo com sucesso em: " + m_progressFilePath);
        }
        catch (IOException e)
        {
            Debug.LogError("Falha ao salvar o progresso: " + e.Message);
        }
    }

    public void LoadProgress()
    {
        if (File.Exists(m_progressFilePath))
        {
            try
            {
                string json = File.ReadAllText(m_progressFilePath);
                PlayerProgressData progressData = JsonUtility.FromJson<PlayerProgressData>(json);

                if (progressData != null && !string.IsNullOrEmpty(progressData.lastPlayedLevel))
                {
                    m_currentLevel = progressData.lastPlayedLevel;
                    Debug.Log("Progresso carregado. Último nível jogado: " + m_currentLevel);
                }
                else
                {
                    Debug.LogWarning("Arquivo de progresso encontrado, mas os dados são inválidos ou o último nível está vazio. Usando nível padrão.");
                }
            }
            catch (IOException e)
            {
                Debug.LogError("Falha ao carregar o progresso: " + e.Message + ". Usando nível padrão.");
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError("Falha ao desserializar o progresso (JSON malformado?): " + e.Message + ". Usando nível padrão.");
            }
        }
        else
        {
            Debug.Log("Nenhum arquivo de progresso encontrado. Iniciando com o nível padrão: " + m_currentLevel);
        }
    }

    void OnApplicationQuit()
    {
        SaveProgress();
        Debug.Log("Jogo fechando, progresso salvo.");
    }
}
