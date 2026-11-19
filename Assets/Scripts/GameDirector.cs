using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    public static GameDirector Instance { get; private set; }

    [Header("References")]
    public PokeballGenerator generator;
    public PokemonTarget pokemon;

    private string currentStatus = "點擊螢幕、按空白鍵或點擊下方按鈕【由左至右拋球】！";
    private bool isSuccess = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetStatus(string msg, bool success = false)
    {
        currentStatus = msg;
        isSuccess = success;
    }

    private void OnGUI()
    {
        // 頂部半透明 HUD 背景
        GUI.backgroundColor = new Color(0, 0, 0, 0.7f);
        GUI.Box(new Rect(20, 20, Screen.width - 40, 95), "");

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Clamp(Screen.width / 40, 20, 28),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        titleStyle.normal.textColor = Color.yellow;
        GUI.Label(new Rect(25, 25, Screen.width - 50, 35), "1151VR 作業五：精靈球收服寶可夢 3D 遊戲", titleStyle);

        GUIStyle statusStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Clamp(Screen.width / 50, 16, 22),
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter
        };
        statusStyle.normal.textColor = isSuccess ? new Color(0.2f, 1f, 0.4f) : Color.white;
        GUI.Label(new Rect(25, 65, Screen.width - 50, 35), currentStatus, statusStyle);

        // 下方操作按鈕
        float btnWidth = 180f;
        float btnHeight = 45f;
        float spacing = 20f;
        float totalWidth = btnWidth * 2 + spacing;
        float startX = (Screen.width - totalWidth) / 2;
        float startY = Screen.height - 70f;

        GUI.backgroundColor = new Color(0.1f, 0.6f, 1f, 0.9f);
        if (GUI.Button(new Rect(startX, startY, btnWidth, btnHeight), "拋出精靈球 (由左至右)"))
        {
            if (generator != null)
            {
                generator.ThrowLeftToRight();
            }
        }

        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.3f, 0.9f);
        if (GUI.Button(new Rect(startX + btnWidth + spacing, startY, btnWidth, btnHeight), "重置寶可夢與場景"))
        {
            ResetGame();
        }
    }

    public void ResetGame()
    {
        if (pokemon != null)
        {
            pokemon.ResetPokemon();
        }
        TrainerController trainer = FindFirstObjectByType<TrainerController>();
        if (trainer != null)
        {
            trainer.ResetTrainer();
        }
        // 清理所有場上的精靈球 Clone
        PokeballController[] balls = FindObjectsByType<PokeballController>(FindObjectsSortMode.None);
        foreach (var b in balls)
        {
            Destroy(b.gameObject);
        }
        SetStatus("場景已重置！隨時可再次拋出精靈球。");
        isSuccess = false;
    }
}
