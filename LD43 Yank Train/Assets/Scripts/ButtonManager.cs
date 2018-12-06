using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour {
    private const string arenaSceneName = "GameArena";
    private const string mainMenuSceneName = "MainMenu";

    /// <summary>
    /// Loads the Arena Scene so that the game may begin.
    /// </summary>
    public void LoadArenaScene()
    {
        SceneManager.LoadScene(arenaSceneName);
    }

    /// <summary>
    /// Loads the Main Menu, for some reason we may need this?
    /// </summary>
    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Quits the game like a fuckin noob.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
