using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour {
    private const string arenaSceneName = "GameArena";
    private const string mainMenuSceneName = "GameArena";

    public void loadArenaScene()
    {
        SceneManager.LoadScene(arenaSceneName);
    }

    public void loadMainMenuScene()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void quitGame()
    {
        Application.Quit();
    }
}
