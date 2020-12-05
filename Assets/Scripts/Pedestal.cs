using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestal : MonoBehaviour {
    public Transform capstone;
    public Transform onDeckZone;
    public StoneShape shape;
    public GameController gc;
    public OnDeck od;
    public bool isAiClick = false;


    void OnMouseDown() { // pick up capstone
        if(gc.moveCount <= 2 || od.isOnDeck() || capstone == null || gc.getWhosTurn() != shape || gc.isAnythingPickedUp()) { return; } // can't pick up the AI's capstone
        if(gc.isAi && gc.getWhosTurn() == StoneShape.Round && !isAiClick) { return; }
        isAiClick = false;

        capstone.SetParent(onDeckZone, true);
        capstone.gameObject.GetComponent<Stone>().slerp3(capstone.localPosition, capstone.localPosition*0.5f, Vector3.zero, false);
        od.putStone(capstone.gameObject);
        capstone = null;
    }

    public void replaceStone(GameObject oStone) { // put capstone back on pedestal
        capstone = oStone.GetComponent<Transform>();
        capstone.SetParent(transform, true);
        capstone.gameObject.GetComponent<Stone>().slerp3(capstone.localPosition, capstone.localPosition*0.5f, Vector3.zero, true);
    }

    public void triggerClick() { // programmatic click, like no mouse required
        if(gc.isAi && gc.getWhosTurn() == StoneShape.Round) {
            isAiClick = true;
        }
        OnMouseDown();
    }
}
