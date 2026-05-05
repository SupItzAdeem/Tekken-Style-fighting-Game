using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOptions : MonoBehaviour
{
    public GameObject playerOne;  // King (left)
    public GameObject playerTwo;  // Jin (right)
    public GameObject inGameUI;
    public GameObject gameOverUI;
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI timerText;

    public enum GameMode
    {
        Option1_HumanVsHuman_King,
        Option2_HumanVsHuman_Jin,
        Option3_HumanVsAI_King,
        Option4_HumanVsAI_Jin
    }

    private GameMode currentGameMode;

    private float matchTime = 150f; // 2.5 minutes
    private bool timerRunning = true;

    private Health kingHealth;
    private Health jinHealth;

    void Start()
    {
        playerOne.name = "King";
        playerTwo.name = "Jin";

        kingHealth = playerOne.GetComponent<Health>();
        jinHealth = playerTwo.GetComponent<Health>();

    }

    void Update()
    {
        if (!timerRunning) return;

        matchTime -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(matchTime / 60f);
        int seconds = Mathf.FloorToInt(matchTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        if (matchTime <= 0f)
        {
            timerRunning = false;
            timerText.text = "00:00";

            string winner = DetermineWinnerByHealth();
            GameOver(winner);
        }
    }

    private string DetermineWinnerByHealth()
    {
        int kingHP = kingHealth != null ? kingHealth.currentHealth : 0;
        int jinHP = jinHealth != null ? jinHealth.currentHealth : 0;

        Debug.Log($"Timer Done → King HP: {kingHP}, Jin HP: {jinHP}");

        if (kingHP > jinHP) return "King";
        if (jinHP > kingHP) return "Jin";
        return "Draw";
    }

    public void SetGameMode(int option)
    {
        currentGameMode = (GameMode)option;
        Debug.Log("option " + currentGameMode);
        switch (currentGameMode)
        {
            case GameMode.Option1_HumanVsHuman_King:
                SetupOption1();
                break;
            case GameMode.Option2_HumanVsHuman_Jin:
                SetupOption2();
                break;
            case GameMode.Option3_HumanVsAI_King:
                SetupOption3();
                break;
            case GameMode.Option4_HumanVsAI_Jin:
                SetupOption4();
                break;
        }
    }

    public void GameOver(string winnerName)
    {
        Debug.Log("GameOver CALLED with winner: " + winnerName);

        inGameUI.SetActive(false);
        gameOverUI.SetActive(true);

        SoundManager.StopMusic(); // Stop BGM
        SoundManager.PlaySound(SoundType.GAMEOVER); // Play Game Over sound

        if (winnerText == null)
        {
            Debug.LogWarning("winnerText is NULL! Check if it's assigned in the Inspector.");
        }
        else
        {
            Debug.Log("winnerText exists. Attempting to set text...");

            if (winnerName == "Draw")
                winnerText.text = "It's a Draw!";
            else
                winnerText.text = winnerName + " Wins!";

            winnerText.color = Color.red;
            winnerText.fontSize = 60;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void SetupOption1()
    {
        playerOne.tag = "Player";
        playerOne.GetComponent<MoveHero>().enabled = true;

        playerTwo.tag = "Opponent";
        playerTwo.GetComponent<MultiplayerController>().enabled = true;
    }

    void SetupOption2()
    {
        playerTwo.tag = "Player";
        playerTwo.GetComponent<MoveHero>().enabled = true;

        playerOne.tag = "Opponent";
        playerOne.GetComponent<MultiplayerController>().enabled = true;
    }

    void SetupOption3()
    {
        playerOne.tag = "Player";
        playerOne.GetComponent<MoveHero>().enabled = true;

        playerTwo.tag = "Opponent";
        playerTwo.GetComponent<AIMoveHero>().enabled = true;
    }

    void SetupOption4()
    {
        playerTwo.tag = "Player";
        playerTwo.GetComponent<MoveHero>().enabled = true;

        playerOne.tag = "Opponent";
        playerOne.GetComponent<AIMoveHero>().enabled = true;
    }
}
