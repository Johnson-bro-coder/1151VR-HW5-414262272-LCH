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
        // 移除最上面的字與黑框，只保留底部操作按鈕
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
