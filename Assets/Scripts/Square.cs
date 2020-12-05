using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Square : MonoBehaviour {
    const int BOARD_DIMENSION = 5;
    public float verticalStoneOffset;
    public float verticalPickedUpOffset = 2f;
    public int row, col;
    private int str8Lock, carryDirection;
    List<Stone> stoneStack;
    public List<Stone> pickedUpStack;
    public GameController gc;
    OnDeck od;
    bool isAiClick = false;

    void Start() {
        stoneStack = new List<Stone>();
        pickedUpStack = new List<Stone>();
        od = gc.getCurrentOnDeck();
    }


    void OnMouseDown() { // when the square on the board is clicked
        if(gc.isAi && gc.getWhosTurn() == StoneShape.Round && !isAiClick) { return; } // manage clicks when ai is thinking.
        isAiClick = false;
        od = gc.getCurrentOnDeck();
        if(stoneStack.Count == 0 && od.isOnDeck()) { // nothing on square, something on deck, can place
            od.takeStone(this);
        } else if(!od.isOnDeck()) { //  nothings on deck
            if(gc.isAnythingPickedUp()){ // some stack on the board is picked up
                if(pickedUpStack.Count > 0) { // this stack has stones picked up
                    putDownPickedUp();
                } else { // this stack doesn't have stones picked up
                    gc.getPickedUp().givePickedUp(this);
                }
            } else if(stoneStack.Count > 0) { // no stones are picked up, and it has stones to pickup
                pickupStack();
            }
        }
    }

    public void placeStone(Stone incomingStone) {  // puts a stone from on deck to board
        incomingStone.onBoard = true;
        Transform stoneTransform = incomingStone.GetComponent<Transform>();
        stoneTransform.SetParent(transform, true);
        Vector3 targetPos = Vector3.zero;
        if(incomingStone.type == StoneType.Standing) {
            targetPos = incomingStone.standingPositionOffset;
        }
        targetPos += new Vector3(0f, incomingStone.pieceBoardOffset, 0f);
        incomingStone.slerp3(stoneTransform.localPosition, (stoneTransform.localPosition + targetPos)*0.5f, targetPos, true);
        stoneStack.Add(incomingStone);

        gc.swapTurn();
    }

    public StoneType topStoneType() {
        return stoneStack[stoneStack.Count - 1].type;
    }

    public bool isEmpty() {
        return (stoneStack.Count == 0);
    }

    public void handlePieceClick() { // clicking on any piece of the stack should be the same as clicking on the square.
        if(gc.isAi && gc.getWhosTurn() == StoneShape.Round) {
            isAiClick = true;
        }
        OnMouseDown();
    }

    public bool canAddStone(List<Stone> incomingStack, int srcRow, int srcCol, int straightLock, int straightCarryDir) {  // checks whether is legal for a stone to be added
        bool isStraight = (straightLock == 0 || (straightLock > 0 && srcRow == row) || (straightLock < 0 && srcCol == col));
        bool isOneWay = (straightCarryDir == 0 || (straightCarryDir > 0 && (srcRow < row || srcCol < col)) || (straightCarryDir < 0 && (srcRow > row || srcCol > col)));
        if(isEmpty()) { return (isStraight && isOneWay); }
        // stone can be added if the current stack doesn't have a capstone or a standing stone on top
        // except when the stone to add is a single capstone, it can flatten a standing stone.
        StoneType top = topStoneType();
        return (isOneWay && isStraight && top != StoneType.Capstone && (top != StoneType.Standing || (incomingStack.Count == 1 && incomingStack[0].type == StoneType.Capstone)));
    }

    // give a picked up stack to a target
    public void givePickedUp(Square target) {
        if(Math.Abs(row - target.row) + Math.Abs(col - target.col) != 1 || !target.addStones(pickedUpStack,row, col, str8Lock, carryDirection)) { return; } // can only move stones to neighbour
        stoneStack.RemoveRange(stoneStack.Count - pickedUpStack.Count, pickedUpStack.Count);
        pickedUpStack.Clear();
        str8Lock = 0;
        carryDirection = 0;
    }

    // recieve stones from another square
    public bool addStones(List<Stone> incomingStack, int srcRow, int srcCol, int straightLock, int straightCarryDir) {
        if(canAddStone(incomingStack, srcRow, srcCol, straightLock, straightCarryDir)) { // if its legal to add stones
            if(straightLock == 0) { // only allows stones to be moves in a straight unidirectional line
                if(srcRow == row) {
                    str8Lock = 1;
                    carryDirection = col - srcCol;
                }
                else if(srcCol == col) {
                    str8Lock = -1;
                    carryDirection = row - srcRow;
                }
            } else {
                str8Lock = straightLock;
                carryDirection = straightCarryDir;
            }
            if(!isEmpty() && stoneStack[stoneStack.Count-1].type == StoneType.Standing) { // when a capstone flattens a standing stone
                stoneStack[stoneStack.Count-1].flattenStone();
                stoneStack[stoneStack.Count-1].playFlattenSound();
            }
            Transform tStone;
            Vector3 target;
            bool isBottom = true;
            foreach(Stone incomingStone in incomingStack) { // transfer the stones
                tStone = incomingStone.gameObject.GetComponent<Transform>();
                tStone.SetParent(transform, true);
                target = new Vector3(0, incomingStone.pieceBoardOffset + verticalStoneOffset * stoneStack.Count, 0);
                if(incomingStone.type == StoneType.Standing) {
                    target += incomingStone.standingPositionOffset;
                }
                incomingStone.slerp3(tStone.localPosition, (tStone.localPosition + target)*0.5f, target, isBottom);
                stoneStack.Add(incomingStone);
                isBottom = false;
            }

            StartCoroutine(triggerPickup(incomingStack.Count));
            incomingStack[incomingStack.Count-1].playClickSound();
            return true;
        }
        return false;
    }

    // a delayed pick up so it looks smoother.
    IEnumerator triggerPickup(int stackSize) {
        yield return new WaitForSeconds(0.5f);
        if(stackSize > 1) {
            pickupStack(stackSize - 1);
        } else {
            gc.swapTurn();
            str8Lock = 0;
            carryDirection = 0;
        }
    }


    public int getStackHeight() { return stoneStack.Count; }


    // put down a stone that its in the picked up stack, like on the square its above.
    public void putDownPickedUp() {
        Transform t  = pickedUpStack[0].gameObject.GetComponent<Transform>();
        pickedUpStack[0].slerp2(t.localPosition, t.localPosition - new Vector3(0, verticalPickedUpOffset, 0), true);
        pickedUpStack.RemoveAt(0);
        if(pickedUpStack.Count == 0) {
            if(str8Lock != 0) {
                str8Lock = 0;
                carryDirection = 0;
                gc.swapTurn();
            }
            gc.resetPickup();
        }
    }

    public void pickupStack(int numberOfStones=-1) { // pick up a certain number of stones.
        if(getController() != gc.getWhosTurn()) { return; }

        if(numberOfStones < 0) {
            numberOfStones = Math.Min(BOARD_DIMENSION, stoneStack.Count); // cannot pick up more stones than board dimension, but must have at least 1
        }
        pickedUpStack = stoneStack.GetRange(stoneStack.Count - numberOfStones, numberOfStones);
        Vector3 offset = new Vector3(0, verticalPickedUpOffset, 0);
        foreach(Stone s in pickedUpStack) {
            Transform t = s.gameObject.GetComponent<Transform>();
            s.slerp2(t.localPosition, t.localPosition + offset, false);
        }
        pickedUpStack[0].playHandSound();
        gc.pickedUp(this);
    }

    // get who has control of the stack
    public StoneShape getController() {
        return stoneStack[stoneStack.Count - 1].shape;
    }

    public bool isRoadPiece() {
        if(isEmpty()) { return false; }
        return (stoneStack[stoneStack.Count-1].type == StoneType.Flat || stoneStack[stoneStack.Count-1].type == StoneType.Capstone);
    }

    public int stackSize() {
        return stoneStack.Count;
    }

    // a text representation of the piece colours of the stack.
    public string getStackMakeup() {
        string output = "";
        foreach(Stone s in stoneStack) {
            if(s.shape == StoneShape.Round) {
                output = "b" + output;
            } else {
                output = "w" + output;
            }
        }
        return output;
    }
}
