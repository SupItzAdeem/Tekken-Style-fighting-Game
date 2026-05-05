using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject chooseMapPanel;

    void Start()
    {
        SoundManager.PlaySoundLoop(SoundType.BGM);
    }
    public void OnPlayButtonClicked()
    {
        if (mainMenuPanel != null && chooseMapPanel != null)
        {
            mainMenuPanel.SetActive(false);
            chooseMapPanel.SetActive(true);
        }
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
}
