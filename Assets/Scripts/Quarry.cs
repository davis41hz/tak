using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quarry : MonoBehaviour {
    public List<Transform> stones;
    public Transform onDeckZone;
    public StoneShape shape;
    public GameController gc;
    private Vector3 lastStonePosition;
    bool canMove;
    public OnDeck od;
    public bool isAiClick = false;

    void Start() {
        lastStonePosition = Vector3.zero;
        canMove = true;
    }

    void OnMouseDown() { // pick up stone from quarry to on deck
        if(!canMove || stones.Count == 0 || gc.getWhosTurn() != shape || gc.isAnythingPickedUp() || od.isOnDeck()) { return; }
        if(gc.isAi && gc.getWhosTurn() == StoneShape.Round && !isAiClick) { return; } // can't pick up the AI's stones
        isAiClick = false;

        canMove = false;
        Transform nextStone = stones[stones.Count-1];
        lastStonePosition = nextStone.localPosition;
        nextStone.SetParent(onDeckZone, true);
        nextStone.gameObject.GetComponent<Stone>().slerp3(nextStone.localPosition, nextStone.localPosition*0.5f, Vector3.zero, false);
        od.putStone(nextStone.gameObject);
        stones.RemoveAt(stones.Count-1);
    }

    // accept stone back from on deck area
    public void replaceStone(GameObject oStone) {
        Transform stone = oStone.GetComponent<Transform>();
        stones.Add(stone);
        stone.SetParent(transform, true);
        oStone.GetComponent<Stone>().slerp3(stone.localPosition, (stone.localPosition + lastStonePosition)*0.5f, lastStonePosition, true);
        canMove = true;
    }

    public void resetCanMove() {
        canMove = true;
    }

    public bool isEmpty() {
        return (stones.Count == 0);
    }

    // this is like a programmatic click, no mouse required.
    public void triggerClick() {
        if(gc.isAi && gc.getWhosTurn() == StoneShape.Round) {
            isAiClick = true;
        }
        OnMouseDown();
    }
}
