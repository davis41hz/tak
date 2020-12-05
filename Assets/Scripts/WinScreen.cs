using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour {

    // this is for the win screen to go back to menu when press q
    void Update() {
        if(Input.GetButtonUp("Quit")) { // Quit = 'q'
            SceneManager.LoadScene("Menu");
        }
    }
}
