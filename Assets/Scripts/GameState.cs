using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    bool isPaused;
    public GameObject gameUI;
    public GameObject sideMenu;
    public GameObject helpText;

    private void Awake() {
        if (isPaused) {
            // do nothing because i just hate errors and want them to go away.... it claims I am not using isPaused, but I am so heck off Unity.
        }
    }
    public void StartGame() {
        SceneManager.LoadScene(0);
        gameUI.SetActive(false);
        helpText.SetActive(false);
        sideMenu.SetActive(true);
        Time.timeScale = 0;
        isPaused = true;
    }
    public void ExitGame() {
        Debug.Log("quit");
        Application.Quit();
    }
    public void PauseGame() {
        Time.timeScale = 0;
        // engine.SetActive(false);
        gameUI.SetActive(true);
        isPaused = true;
    }
    public void ResetSim() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
    public void ResumeGame() {
        Time.timeScale = 1;
        isPaused = false;
        gameUI.SetActive(false);
    }

    public void ToggleHelpText()
    {
        helpText.SetActive(!helpText.activeInHierarchy);
    }

    public void changeTimeScale() {

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Time.timeScale += .05f;
            Time.fixedDeltaTime = Time.timeScale * Time.fixedDeltaTime;

        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Time.timeScale -= .05f;
            Time.fixedDeltaTime = Time.timeScale * Time.fixedDeltaTime;
        }
    }
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (sideMenu.activeInHierarchy) {
                sideMenu.SetActive(false);
                ResumeGame();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else {
                PauseGame();
            }
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            if (!isPaused)
            {
                sideMenu.SetActive(!sideMenu.activeInHierarchy);
            }
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!isPaused)
            {
                if (!helpText.activeInHierarchy)
                {
                    helpText.SetActive(true);
                    sideMenu.SetActive(true);
                }
                else
                {
                    helpText.SetActive(false);
                }
            }

        }
        changeTimeScale();
    }
}
