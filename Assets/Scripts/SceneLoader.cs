using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [SerializeField] private List<string> levels;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadLevel(int index)
    {
        if (index >= 0 && index < levels.Count)
        {
            Debug.Log("Trying to load: " + levels[index]);
            SceneManager.LoadScene(levels[index]);
        }
        else
        {
            Debug.LogWarning("Invalid scene index: " + index);
        }
    }
}
