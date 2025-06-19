using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Players")]
    public GameObject[] players;
    
    [Header("Player Names")]
    public string[] playerNames = { "Player 1", "Player 2" };
    
    [Header("Game Over")]
    public float gameOverDelay = 2f; // Delay before showing game over screen
    
    public Font GameOverFont; // Font for game over text, set in inspector
    private GameObject gameOverPanel;
    private Text winnerText;
    private Button replayButton;
    private bool gameEnded = false;
    private bool gameOverShown = false;
    private bool checkingWinState = false; // Prevent concurrent win state checks

    void Start()
    {
        CreateGameOverUI();
        HideGameOver();
    }

    void CreateGameOverUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("GameOverCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(320, 240); // Pixel-perfect resolution
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create background panel
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = gameOverPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelBg = gameOverPanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.8f);

        // Create title text
        GameObject titleObj = new GameObject("GameOverTitle");
        titleObj.transform.SetParent(gameOverPanel.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(300, 50);
        titleRect.anchoredPosition = Vector2.zero;

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "GAME OVER";
        titleText.fontSize = 24;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;
        titleText.font = GameOverFont;

        // Create winner text
        GameObject winnerObj = new GameObject("WinnerText");
        winnerObj.transform.SetParent(gameOverPanel.transform, false);

        RectTransform winnerRect = winnerObj.AddComponent<RectTransform>();
        winnerRect.anchorMin = new Vector2(0.5f, 0.55f);
        winnerRect.anchorMax = new Vector2(0.5f, 0.55f);
        winnerRect.sizeDelta = new Vector2(200, 40);
        winnerRect.anchoredPosition = Vector2.zero;

        winnerText = winnerObj.AddComponent<Text>();
        winnerText.text = "Player Wins!";
        winnerText.fontSize = 18;
        winnerText.color = Color.yellow;
        winnerText.alignment = TextAnchor.MiddleCenter;
        winnerText.fontStyle = FontStyle.Bold;
        winnerText.font = GameOverFont;

        // Create replay button
        GameObject buttonObj = new GameObject("ReplayButton");
        buttonObj.transform.SetParent(gameOverPanel.transform, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.35f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.35f);
        buttonRect.sizeDelta = new Vector2(80, 30);
        buttonRect.anchoredPosition = Vector2.zero;

        Image buttonImg = buttonObj.AddComponent<Image>();
        buttonImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        replayButton = buttonObj.AddComponent<Button>();
        replayButton.onClick.AddListener(RestartGame);

        // Button colors for pixel-art style
        ColorBlock colors = replayButton.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        replayButton.colors = colors;

        // Button text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);

        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.text = "REPLAY";
        buttonText.fontSize = 12;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.font = GameOverFont; // Use the same font for button text
    }

    public void CheckWinState()
    {
        if (gameEnded || checkingWinState) return;
        
        checkingWinState = true;
        // Add a small delay to ensure all death sequences have completed their frame
        Invoke(nameof(CheckWinStateDelayed), 0.1f);
    }
    
    private void CheckWinStateDelayed()
    {
        if (gameEnded)
        {
            checkingWinState = false;
            return;
        }
        
        int aliveCount = 0;
        GameObject lastAlivePlayer = null;
        int lastAliveIndex = -1;

        // Count active players
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].activeSelf)
            {
                aliveCount++;
                lastAlivePlayer = players[i];
                lastAliveIndex = i;
            }
        }

        Debug.Log($"CheckWinStateDelayed: {aliveCount} players alive");
        
        // Game ends when 1 or fewer players remain
        if (aliveCount <= 1)
        {
            gameEnded = true;
            
            if (aliveCount == 1 && lastAlivePlayer != null)
            {
                // Someone won - exactly 1 player remaining
                string winnerName = GetPlayerName(lastAliveIndex);
                Debug.Log($"Game Over! {winnerName} wins!");
                Invoke(nameof(ShowGameOverWinner), gameOverDelay);
            }
            else
            {
                // Draw - 0 players remaining (everyone died)
                Debug.Log("Game Over! It's a draw!");
                Invoke(nameof(ShowGameOverDraw), gameOverDelay);
            }
        }
        
        checkingWinState = false;
    }
    
    void ShowGameOverWinner()
    {
        // Re-check to find the winner when actually showing the game over screen
        string winnerName = "Unknown";
        int aliveCount = 0;
        
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].activeSelf)
            {
                winnerName = GetPlayerName(i);
                aliveCount++;
            }
        }
        
        // Double-check if it's actually a draw
        if (aliveCount == 0)
        {
            ShowGameOverDraw();
            return;
        }
        
        ShowGameOver(winnerName);
    }
    
    void ShowGameOverDraw()
    {
        Debug.Log("Showing draw result");
        ShowGameOver("DRAW");
    }
    
    void ShowGameOver(string winnerName)
    {
        if (gameOverShown) return;
        
        gameOverShown = true;
        
        if (winnerText != null)
        {
            if (winnerName == "DRAW")
            {
                winnerText.text = "IT'S A DRAW!";
                winnerText.color = Color.cyan;
                winnerText.fontSize = 20; // Make draw text slightly larger
            }
            else
            {
                winnerText.text = $"{winnerName} WINS!";
                winnerText.color = Color.yellow;
                winnerText.fontSize = 18; // Normal size for winner text
            }
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Pause the game - this ensures only the replay button can start a new round
        Time.timeScale = 0f;
        
        Debug.Log($"Game over screen shown. Result: {winnerName}. Game paused until replay button is clicked.");
    }
    
    void HideGameOver()
    {
        gameOverShown = false;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Resume game
        Time.timeScale = 1f;
    }
    
    string GetPlayerName(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerNames.Length)
        {
            return playerNames[playerIndex];
        }
        return $"Player {playerIndex + 1}";
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume time before scene change
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    void Update()
    {
        // Allow restart with Enter or Space key when game over is shown
        if (gameOverShown && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
        {
            RestartGame();
        }
    }
}
