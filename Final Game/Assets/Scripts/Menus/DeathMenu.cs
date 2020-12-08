using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{

    public GameObject DeadMenu;

    void Start()
    {
        DeadMenu.SetActive(false);
    }

    public void OpenDeathMenu()
    {
        DeadMenu.SetActive(true);
        Time.timeScale = 0f;

    }

    public void Restart()
    {
        DeadMenu.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level 1");
    }

    public void LoadMenu()
    {
        DeadMenu.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
