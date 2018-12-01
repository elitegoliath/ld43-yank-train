using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour {
    private const string arenaSceneName = "GameArena";
    private const string mainMenuSceneName = "GameArena";

    public void LoadArenaScene()
    {
        SceneManager.LoadScene(arenaSceneName);
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
