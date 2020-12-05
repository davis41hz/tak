using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {
    public Button pvpButton, pvcButton, exitButton, rulesButton, controlsButton;
    public GameObject controlsText, rulesText;

    void Start() {
        exitButton.onClick.AddListener(exitGame);
        pvpButton.onClick.AddListener(playPvP);
        pvcButton.onClick.AddListener(playPvC);
        rulesButton.onClick.AddListener(showRules);
        controlsButton.onClick.AddListener(showControls);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // a bunch of button handlers
    void exitGame() {
        Application.Quit();
    }

    void playPvP() {
        SceneManager.LoadScene("PvP_Game");
    }

    void playPvC() {
        SceneManager.LoadScene("PvC_Game");
    }

    void showRules() {
        rulesText.SetActive(true);
        controlsText.SetActive(false);
    }

    void showControls() {
        rulesText.SetActive(false);
        controlsText.SetActive(true);
    }
}
