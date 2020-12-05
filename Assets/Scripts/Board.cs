using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
    public List<Square>[] board; // adjacency list form
    public List<Square> allSquares;
    private Dictionary<Square, int> reverseLookup;
    private int totalSquares = 25;
    private int dimension = 5;
    private StoneShape secondTurn = StoneShape.Round;
    private List<int> startSquares1 = new List<int>() {0,1,2,3,4};
    private List<int> endSquares1 = new List<int>() {20,21,22,23,24};
    private List<int> startSquares2 = new List<int>() {0,5,10,15,20};
    private List<int> endSquares2 = new List<int>() {4,9,14,19,24};
    public Quarry sharpQuarry, roundQuarry;
    public Pedestal sharpPedestal, roundPedestal;


    void Start() {
        // builds some data structures for future use
        board = new List<Square>[totalSquares];
        for(int i = 0; i < totalSquares; i++) {
            board[i] = new List<Square>();
        }

        reverseLookup = new Dictionary<Square, int>();
        for(int i = 0; i < totalSquares; i++) {
            reverseLookup.Add(allSquares[i], i);
            if(i - 1 >= 0 && i % dimension != 0) {
                board[i].Add(allSquares[i-1]);
            }
            if(i + 1 < totalSquares && (i+1) % dimension != 0) {
                board[i].Add(allSquares[i+1]);
            }
            if(i + 5 < totalSquares) {
                board[i].Add(allSquares[i+5]);
            }
            if(i - 5 >= 0) {
                board[i].Add(allSquares[i-5]);
            }

        }
    }

    private bool checkWinFromSquare(int startIndex, bool[] check, List<int> endIndices, StoneShape shape) { // checks if there is a winning road from a certain square, basically a dfs
        check[startIndex] = true;
        if(!allSquares[startIndex].isRoadPiece() || allSquares[startIndex].getController() != shape) { return false; }

        if(endIndices.Contains(startIndex)) { return true; }

        List<Square> neighbourList = board[startIndex];
        foreach(Square s in neighbourList) {
            if (!check[reverseLookup[s]] ){
                if(checkWinFromSquare(reverseLookup[s], check, endIndices, shape)) {
                    return true;
                }
            }
        }
        return false;
    }

    public bool checkForWin(StoneShape shape) { // checks all squares for a win...runs a dfs from each square, 2 for loops cuz have to check horizontal and vertical.
        bool[] check = new bool[totalSquares];
        foreach(int start in startSquares1) {
            if(checkWinFromSquare(start, check, endSquares1, shape)) {
                return true;
            }
        }

        check = new bool[totalSquares];
        foreach(int start in startSquares2) {
            if(checkWinFromSquare(start, check, endSquares2, shape)) {
                return true;
            }
        }

        return false;
    }

    bool isBoardFull() { // check if there no spots left to place stones.
        foreach(Square s in allSquares) {
            if(s.isEmpty()) { return false; }
        }
        return true;
    }

    public int checkForFlatWin(StoneShape shape) { // check for a flat win, when there's no moves and no winning road, see rules pdf
        if((shape == StoneShape.Sharp && sharpQuarry.stones.Count == 0 && sharpPedestal.capstone == null) || (shape == StoneShape.Round && roundQuarry.stones.Count == 0 && roundPedestal.capstone == null) || isBoardFull()) {
            int score = 0;
            foreach(Square s in allSquares) {
                if(s.topStoneType() == StoneType.Flat) {
                    if(s.getController() == StoneShape.Sharp) {
                        score++;
                    } else {
                        score--;
                    }
                }
            }
            if(score > 0) { return 1; }
            else if(score < 0) { return -1; }
            else if(secondTurn == StoneShape.Sharp) { return 1; }
            else { return -1; }
        }
        return 0;
    }

    public void setSecondTurn(StoneShape shape) { // keep track of second turn for flat win.
        secondTurn = shape;
    }
}
