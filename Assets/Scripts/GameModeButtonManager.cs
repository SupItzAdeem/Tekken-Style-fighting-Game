using UnityEngine;
using UnityEngine.UI;

public class GameModeButtonManager : MonoBehaviour
{
    public Button[] gameModeButtons;
    public GameOptions gameOptions;

    void Start()
    {
        for (int i = 0; i < gameModeButtons.Length; i++)
        {
            int optionIndex = i;
            gameModeButtons[i].onClick.AddListener(() => gameOptions.SetGameMode(optionIndex));
        }
    }
}
