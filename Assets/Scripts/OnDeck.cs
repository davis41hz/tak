using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDeck : MonoBehaviour {
    public Quarry quarry;
    public Pedestal pedestal;
    public GameController gc;
    Stone currentStone;
    BoxCollider col;

    void Start() {
        currentStone = null;
        col = gameObject.GetComponent<BoxCollider>();
        col.enabled = false;
    }

    void Update() {
        if(currentStone == null || gc.moveCount <= 2) { return; } //can't flip on first turn

        if(Input.GetMouseButtonDown(1)) { // right click to flip stone
            currentStone.flipStone();
        }
    }

    void OnMouseDown() { // put piece back to quarry/pedestal
        if(currentStone == null) { return; }
        if(gc.isAi && gc.getWhosTurn() == StoneShape.Round) { return; }

        if(currentStone.type == StoneType.Capstone) {
            pedestal.replaceStone(currentStone.gameObject);
        } else {
            currentStone.flattenStone();
            quarry.replaceStone(currentStone.gameObject);
        }
        currentStone = null;
        col.enabled = false;
    }

    public void putStone(GameObject oStone) { // put stone on board
        currentStone = oStone.GetComponent<Stone>();
        col.enabled = true;
        currentStone.playHandSound();
    }

    public void takeStone(Square targetSquare) { // accept stone from quarry/pedestal
        if(currentStone == null) { return; }
        targetSquare.placeStone(currentStone);
        currentStone = null;
        quarry.resetCanMove();
    }

    public bool isOnDeck() {
        return currentStone != null;
    }
}
