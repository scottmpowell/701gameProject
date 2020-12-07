using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{

    public bool GameIsPaused = false;

    public GameObject DeadMenu;
    public GameObject EmoUI;


    void Start()
    {
        DeadMenu.SetActive(false);
    }

    public void OpenDeathMenu()
    {
        Debug.Log("Dead: " + DeadMenu);
        DeadMenu.SetActive(true);
        EmoUI.SetActive(false);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }
    public void Restart()
    {
        DeadMenu.SetActive(false);
        EmoUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = false;
        SceneManager.LoadScene("Level 1");

    }

    public void LoadMenu()
    {
        DeadMenu.SetActive(false);
        EmoUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = false;
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
