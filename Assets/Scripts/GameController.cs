using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    StoneShape currentTurn;
    public Quarry sharpQuarry, roundQuarry;
    public OnDeck roundOD, sharpOD;
    public int moveCount;
    private Square pickedUpSquare;
    public Board board;
    public Text turnIndicatorText, winText;
    public CameraMovement cam;
    public GameObject winCanvas, turnCanvas;
    public AI ai = null;
    public bool isAi;
    public string sharpToPlay, roundToPlay, sharpWins, roundWins;

    void Start() {
        moveCount = 1;
        pickedUpSquare = null;
        winCanvas.SetActive(false);
        turnCanvas.SetActive(true);
        // set up turns
        if(isAi) {
            currentTurn = StoneShape.Sharp;
            cam.setImmediateSharp();
        } else {
            currentTurn = (StoneShape) Mathf.RoundToInt(Random.Range(0f, 1f));
        }

        if(currentTurn == StoneShape.Sharp) {
            turnIndicatorText.text = sharpToPlay;
            board.setSecondTurn(StoneShape.Round);
        } else {
            turnIndicatorText.text = roundToPlay;
            board.setSecondTurn(StoneShape.Sharp);
        }
    }

    public OnDeck getCurrentOnDeck() { // get whats on deck
        if(currentTurn == StoneShape.Sharp) { return sharpOD; }
        return roundOD;
    }

    public StoneShape getWhosTurn() {
        return currentTurn;
    }


    public void swapTurn() { // switch who's turn it is, update text indicators, camera, a few variables
        int flatWin = board.checkForFlatWin(currentTurn);
        if(board.checkForWin(currentTurn) || flatWin != 0) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            winCanvas.SetActive(true);
            turnCanvas.SetActive(false);

            if(flatWin > 0) {
                winText.text = sharpWins;
            } else if(flatWin < 0) {
                winText.text = roundWins;
            } else if(currentTurn == StoneShape.Sharp) {
                winText.text = sharpWins;
            } else {
                winText.text = roundWins;
            }
            return;
        }
        resetPickup();
        moveCount++;
        if(currentTurn == StoneShape.Sharp) {
            currentTurn = StoneShape.Round;
            turnIndicatorText.text = roundToPlay;
            if(!isAi) {
                cam.setRoundCam();
            } else {
                if(moveCount <= 2) {
                    ai.makeFirstMove();
                } else {
                    ai.makeMove();
                }
            }
        } else {
            currentTurn = StoneShape.Sharp;
            turnIndicatorText.text = sharpToPlay;
            cam.setSharpCam();
        }
    }

    // track if anything is picked up so nothing can be put down when something else is pickedup.
    // These last functions deal with all that.
    public void resetPickup() {
        pickedUpSquare = null;
    }

    public bool isAnythingPickedUp() {
        return pickedUpSquare != null;
    }

    public void pickedUp(Square s) {
        pickedUpSquare = s;
    }

    public Square getPickedUp() {
        return pickedUpSquare;
    }
}
