using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AI : MonoBehaviour {
    public Board board;
    public Quarry quarry;
    public Pedestal pedestal;
    public StoneShape aiShape, playerShape;
    public string aiShapeCode, playerShapeCode;
    public int searchDepth = 1;
    private List<int> startSquares1 = new List<int>() {0,1,2,3,4};
    private List<int> endSquares1 = new List<int>() {20,21,22,23,24};
    private List<int> startSquares2 = new List<int>() {0,5,10,15,20};
    private List<int> endSquares2 = new List<int>() {4,9,14,19,24};

    void Start() {
        setShape(StoneShape.Round); // ai is always round
    }

    // get all the possible moves in a given board state.
    List<string> getAllMoves(string[] boardState, string code) {
        List<string> moves = new List<string>();

        for(int i = 0; i < boardState.Length; i++) {
            string[] s = boardState[i].Split(':');

            // get all placing moves
            if(s[0] == "!") {
                if(pedestal.capstone != null) {
                    moves.Add("C" + i.ToString());
                }
                if(!quarry.isEmpty()) {
                    moves.Add(i.ToString());
                    moves.Add("S" + i.ToString());
                }
            } else if(s[0].Substring(0,1) == code) { // get all carry moves
                int maxPickup = Math.Min(5, s[0].Length);

                // RIGHT
                int openSpaceRight = 0;
                for(int k = i+1; k < i + 5; k++) {
                    if(k != i && k % 5 == 0) { break; }
                    string[] n = boardState[k].Split(':');
                    if(n.Length == 3) {
                        if(n[2] == "c") { break; }
                        else if(n[2] == "s" && s.Length == 3 && s[2] != "c") { break; }
                        else if(n[2] == "s" && s.Length == 3 && s[2] == "c") {
                            openSpaceRight++;
                            break;
                        }
                    }

                    openSpaceRight++;
                }

                // UP
                int openSpaceUp = 0;
                for(int k = i+5; k < 25; k+=5) {
                    string[] n = boardState[k].Split(':');
                    if(n.Length == 3) {
                        if(n[2] == "c") { break; }
                        else if(n[2] == "s" && s.Length == 3 && s[2] != "c") { break; }
                        else if(n[2] == "s" && s.Length == 3 && s[2] == "c") {
                            openSpaceUp++;
                            break;
                        }
                    }
                    openSpaceUp++;
                }

                // DOWN
                int openSpaceDown = 0;
                for(int k = i-5; k >= 0; k-=5) {
                    string[] n = boardState[k].Split(':');
                    if(n.Length == 3) {
                        if(n[2] == "c") { break; }
                        else if(n[2] == "s" && ((s.Length == 3 && s[2] != "c") || s.Length < 3)) { break; }
                        else if(n[2] == "s" && ((s.Length == 3 && s[2] == "c") || s.Length < 3)) {
                            openSpaceDown++;
                            break;
                        }
                    }
                    openSpaceDown++;
                }

                // LEFT
                int openSpaceLeft = 0;
                for(int k = i-1; k > i - 5 && k >= 0; k--) {
                    if(k != i && k % 5 == 4) { break; }
                    string[] n = boardState[k].Split(':');
                    if(n.Length == 3) {
                        if(n[2] == "c") { break; }
                        else if(n[2] == "s" && s.Length == 3 && s[2] != "c") { break; }
                        else if(n[2] == "s" && s.Length == 3 && s[2] == "c") {
                            openSpaceLeft++;
                            break;
                        }
                    }
                    openSpaceLeft++;
                }


                for(int j = 1; j <= maxPickup; j++) { // how many stones to pickup
                    //RIGHT
                    List<string> carryRight = allCarryPossibilities(j, openSpaceRight);
                    foreach(string c in carryRight) {
                        moves.Add(j.ToString() + "~" + i.ToString() + "$>" + c);
                    }

                    // UP
                    List<string> carryUp = allCarryPossibilities(j, openSpaceUp);
                    foreach(string c in carryUp) {
                        moves.Add(j.ToString() + "~" + i.ToString() + "$+" + c);
                    }

                    // DOWN
                    List<string> carryDown = allCarryPossibilities(j, openSpaceDown);
                    foreach(string c in carryDown) {
                        moves.Add(j.ToString() + "~" + i.ToString() + "$-" + c);
                    }

                    // LEFT
                    List<string> carryLeft = allCarryPossibilities(j, openSpaceLeft);
                    foreach(string c in carryLeft) {
                        moves.Add(j.ToString() + "~" + i.ToString() + "$<" + c);
                    }

                }
            }
        }
        return moves;
    }

    // initialize which colour is AI and which is player
    public void setShape(StoneShape sh) {
        aiShape = sh;
        playerShape = StoneShape.Round;
        aiShapeCode = "w";
        playerShapeCode = "b";
        if(sh == StoneShape.Round) {
            aiShapeCode = "b";
            playerShapeCode = "w";
            playerShape = StoneShape.Sharp;
        }
    }

    // gets all the possible carry configurations when dealing with pickup a stack and distributing it
    List<string> allCarryPossibilities(int carrySize, int availableSquares) {
        List<string> carries = new List<string>();

        for(int x = carrySize; x > 0; x--) {
            if(availableSquares == 1 && x == carrySize) {
                carries.Add(x.ToString());
                continue;
            }
            for(int y = carrySize - x; y >= 0; y--) {
                if(y == 0 && x + y != carrySize) { continue; }
                if(availableSquares == 2 && x + y == carrySize) {
                    carries.Add(x.ToString() + y.ToString());
                    continue;
                }
                for(int z = carrySize - x - y; z >= 0; z--) {
                    if(z == 0 && x + y + z != carrySize) { continue; }
                    if(availableSquares == 3 && x + y + z == carrySize) {
                        carries.Add(x.ToString() + y.ToString() + z.ToString());
                        continue;
                    }
                    for(int a = carrySize - x - y - z; a >= 0; a--) {
                        if(a == 0 && x + y + z + a != carrySize) { continue; }
                        if(availableSquares == 4 && x + y + z + a == carrySize) {
                            carries.Add(x.ToString() + y.ToString() + z.ToString() + a.ToString());
                            continue;
                        }
                    }
                }
            }
        }

        return carries;
    }

    // converts the current board to a text based state for minimax
    string[] boardToState() {
        string[] state = new string[25];
        for(int i = 0; i < board.allSquares.Count; i++) {
            Square s  = board.allSquares[i];
            if(s.isEmpty()) {
                state[i] = "!";
                continue;
            }
            string code = s.getStackMakeup() + ":";

            code += s.getStackHeight().ToString();

            if(s.topStoneType() == StoneType.Standing) {
                code += ":s";
            } else if(s.topStoneType() == StoneType.Capstone) {
                code += ":c";
            }
            state[i] = code;
        }
        return state;
    }

    // evaluation function, positive values are better for AI, negative for player.  Honestly wish I could've done this better, but its tricky to make it really good.
    float evaluatePosition(string[] boardState) {
        float stackFactor = 1.3f;
        float enemyStandingFactor = 0.6f;
        float enemyCapstoneFactor = 0.6f;
        float defenseFactor = 1.5f;
        float roadFactor = 1.2f;
        float evaluation = 0f;
        int[] neighbour = {-1, 1, 5, -5};

        for(int i = 0; i < boardState.Length; i++) {
            if(boardState[i] == "!") { continue; } // empty square
            string[] s = boardState[i].Split(':');

            float tempEval = Mathf.Pow(stackFactor, int.Parse(s[1]));

            foreach(int n in neighbour) {
                if(i + n < 25 && i + n >= 0) {
                    if(boardState[i+n] == "!") { continue; }
                    string[] neighbourS = boardState[i + n].Split(':');
                    if(neighbourS[0].Substring(0,1) != s[0].Substring(0,1)) {
                        if(neighbourS.Length == 3) {
                            if(neighbourS[2] == "s") {
                                tempEval *= enemyStandingFactor;
                            } else if(neighbourS[2] == "c") {
                                tempEval *= enemyCapstoneFactor;
                            }

                        }
                    }
                }
            }

            if(s[0].Substring(0,1) == aiShapeCode) {
                evaluation += tempEval;
            } else {
                evaluation -= tempEval;
            }
        }

        float aiRoad = longestPathInState(boardState, aiShapeCode);
        float playerRoad = longestPathInState(boardState, playerShapeCode);

        if(checkForWin(aiShapeCode, boardState)) {
            return 10000f;
        } else if(checkForWin(playerShapeCode, boardState)) {
            return -10000f;
        }

        evaluation += roadFactor * aiRoad;
        evaluation -= defenseFactor * roadFactor * playerRoad;

        return evaluation;
    }

    // helper function for easily displaying text states.
    void printState(string[] st) {
        for(int i = 0; i < st.Length; i+=5) {
            Debug.Log(st[i] + "  " + st[i+1] + "  " + st[i+2] + "  " + st[i+3] + "  " + st[i+4] + "  ");
        }
    }

    // find the longest path for a certain player in a given state.  Search from all squares
    int longestPathInState(string[] boardState, string shapeCode) {
        int maxPath = 0;
        for(int i = 0; i < boardState.Length; i++) {
            string[] s = boardState[i].Split(':');
            if(boardState[i] == "!" || s[0].Substring(0,1) != shapeCode) { continue; }
            maxPath = Math.Max(maxPath, singlePathFromState(boardState, shapeCode, i, new bool[25]));
        }
        return maxPath;
    }

    // find the longest path from a certain square in a state...helper of longestPathInState
    int singlePathFromState(string[] boardState, string shapeCode, int square, bool[] visited, int lastDir = 0, int currentPath=0) {
        int maxPath = -1;
        visited[square] = true;
        string[] s = boardState[square].Split(':');
        if(s[0] == "!" || s[0].Substring(0,1) != shapeCode || (s.Length == 3 && s[2] == "s")) { return currentPath; }

        List<int> neigh = getNeighbours(square);

        foreach(int n in neigh) {
            if(!visited[n]) {
                if(lastDir == 0 || lastDir == -1) {
                    maxPath = Math.Max(singlePathFromState(boardState, shapeCode, n, visited, -1, currentPath + 2), maxPath);
                } else {
                    maxPath = Math.Max(singlePathFromState(boardState, shapeCode, n, visited, -1, currentPath + 1), maxPath);
                }
            }
        }

        return maxPath;
    }

    // get all the value neighbour indexes to a index in the state.
    List<int> getNeighbours(int square) {
        List<int> n = new List<int>();
        if(square + 1 < 25 && (square + 1) % 5 != 0) { // right
            n.Add(square + 1);
        }

        if(square - 1 >= 0 && (square - 1) % 5 != 4) { // left
            n.Add(square - 1);
        }

        if(square + 5 < 25) { // up
            n.Add(square + 5);
        }

        if(square - 5 >= 0) { // down
            n.Add(square - 1);
        }
        return n;
    }

    // check for a win in a state from a specific square, helps the checkforwin function.  Basically a dfs.
    private bool checkWinFromSquare(string[] boardState, int startIndex, bool[] check, List<int> endIndices, string shapeCode) {
        check[startIndex] = true;
        string[] sData = boardState[startIndex].Split(':');
        if((sData.Length > 2 && sData[2] == "s") || sData[0].Substring(0,1) != shapeCode || sData[0] == "!") { return false; }

        if(endIndices.Contains(startIndex)) { return true; }

        List<int> neigh = getNeighbours(startIndex);

        foreach(int n in neigh) {
            if (!check[n]) {
                if(checkWinFromSquare(boardState, n, check, endIndices, shapeCode)) {
                    return true;
                }
            }
        }
        return false;
    }

    // uses dfs to search for a win from all possible winning squares.
    public bool checkForWin(string checkCode, string[] boardState) {
        List<int> startSquares1 = new List<int>() {0,1,2,3,4};
        List<int> endSquares1 = new List<int>() {20,21,22,23,24};
        List<int> startSquares2 = new List<int>() {0,5,10,15,20};
        List<int> endSquares2 = new List<int>() {4,9,14,19,24};
        bool[] check = new bool[25];
        foreach(int start in startSquares1) {
            if(checkWinFromSquare(boardState, start, check, endSquares1, checkCode)) {
                return true;
            }
        }

        check = new bool[25];
        foreach(int start in startSquares2) {
            if(checkWinFromSquare(boardState, start, check, endSquares2, checkCode)) {
                return true;
            }
        }

        return false;
    }

    // update a board state with a move, does not change in game board, just returns state.
    string[] moveBoardState(string[] oldState, string move, string turn) {
        string[] boardState = new string[25];
        Array.Copy(oldState, boardState, 25);
        if(!move.Contains("~")) { // not a carry
            if(move.Substring(0,1) == "S") {
                boardState[int.Parse(move.Substring(1))] = turn + ":1:s";
            } else if(move.Substring(0,1) == "C") {
                boardState[int.Parse(move.Substring(1))] = turn + ":1:c";
            } else {
                boardState[int.Parse(move)] = turn + ":1";
            }
        } else { // carrying stuff
            int carryTotal = int.Parse(move.Substring(0,1));
            int dollarIndex = move.IndexOf('$');
            int src = int.Parse(move.Substring(2, dollarIndex-2));
            string direction = move.Substring(dollarIndex+1, 1);
            string config = move.Substring(dollarIndex+2);
            int srcHeight = int.Parse(boardState[src].Split(':')[1]);
            string srcMakeUp = boardState[src].Split(':')[0];
            string[] srcFull = boardState[src].Split(':');

            // handle source square
            if(srcHeight <= carryTotal) {
                boardState[src] = "!";
            } else {
                boardState[src] = srcMakeUp.Substring(carryTotal) + ":" + (srcHeight - carryTotal).ToString();
                srcMakeUp = srcMakeUp.Substring(0, carryTotal);
            }

            int movementFactor = 0;
            if(direction == ">") { // right
                movementFactor = 1;
            } else if(direction == "<") { // left
                movementFactor = -1;
            } else if(direction == "+") { // up
                movementFactor = 5;
            } else if (direction == "-") { // down
                movementFactor = -5;
            }


            for(int i = 0; i < config.Length; i++) {
                int stoneNum = int.Parse(config.Substring(i, 1));
                if(stoneNum == 0) { break; }
                int dest = src + ((i+1) * movementFactor);
                string[] oldDest = boardState[dest].Split(':');
                if(boardState[dest] == "!") {
                    boardState[dest] = srcMakeUp.Substring(srcMakeUp.Length-stoneNum) + ":" + stoneNum.ToString();
                } else if(oldDest.Length > 1) {
                    int newHeight = stoneNum + int.Parse(oldDest[1]);
                    boardState[dest] = srcMakeUp.Substring(srcMakeUp.Length-stoneNum) + oldDest[0] + ":" + newHeight.ToString();
                }

                if(i + 1 == config.Length && srcFull.Length == 3) {
                    boardState[dest] += ":" + srcFull[2];
                }

                srcMakeUp = srcMakeUp.Substring(0, srcMakeUp.Length-stoneNum);
            }
        }
        return boardState;
    }

    // minimax for finding the optimal move for the AI to make, at a given depth...includes alpha beta pruning...god bless it, makes it so much faster.
    float minimax(string[] state, int depth, string turnCode, float alpha=-1000000f, float beta=1000000f) {
        if(depth == 0) {
            return evaluatePosition(state);
        }

        if(turnCode == aiShapeCode) {
            float maxEval = -100000f;
            List<string> allMoves = getAllMoves(state, aiShapeCode);
            foreach(string move in allMoves) {
                maxEval = Mathf.Max(maxEval, minimax(moveBoardState(state, move, aiShapeCode), depth-1, playerShapeCode, alpha, beta));
                alpha = Mathf.Max(alpha, maxEval);
                if(beta <= alpha) { break; }
            }
            return maxEval;
        } else {
            float minEval = 100000f;
            List<string> allMoves =  getAllMoves(state, playerShapeCode);
            foreach(string move in allMoves) {
                float eval = minimax(moveBoardState(state, move, playerShapeCode), depth-1, aiShapeCode, alpha, beta);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                if(beta <= alpha) { break; }
            }
            return minEval;
        }
    }

    // general function that runs the minimax to find the best move for the ai.
    string findBestMove() {
        string[] currentState = boardToState();
        List<string> allMoves = getAllMoves(currentState, aiShapeCode);
        float maxEval = -10000000f;
        string bestMove = "";
        foreach(string move in allMoves) {
            float eval = minimax(moveBoardState(currentState, move, aiShapeCode), searchDepth, playerShapeCode);
            if(maxEval < eval) {
                maxEval = eval;
                bestMove = move;
            }
        }
        return bestMove;
    }

    // takes a best move and make the physical change to the board.
    public void makeMove(string move="sheep") {
        if(move == "sheep") {
            move = findBestMove();
        }

        if(move.Length == 0) { return; }

        if(!move.Contains("~")) { // not a carry
            if(move.Substring(0,1) == "S") {
                int squareIndex = int.Parse(move.Substring(1));
                StartCoroutine(placePiece(board.allSquares[squareIndex], quarry.triggerClick));
            } else if(move.Substring(0,1) == "C") {
                int squareIndex = int.Parse(move.Substring(1));
                StartCoroutine(placePiece(board.allSquares[squareIndex], pedestal.triggerClick));
            } else {
                int squareIndex = int.Parse(move.Substring(0));
                StartCoroutine(placePiece(board.allSquares[squareIndex],quarry.triggerClick));
            }
        } else { // carrying stuff
            int carryTotal = int.Parse(move.Substring(0,1));
            int dollarIndex = move.IndexOf('$');
            int src = int.Parse(move.Substring(2, dollarIndex-2));
            string direction = move.Substring(dollarIndex+1, 1);
            string config = move.Substring(dollarIndex+2);

            StartCoroutine(delayCarry(direction, config, src, carryTotal));
        }
    }

    // first move not that consequential, so i just made it random...this file is already crazy enough.
    public void makeFirstMove() {
        System.Random rng = new System.Random();
        int range = board.allSquares.Count;
        int index = rng.Next(range);
        while(!board.allSquares[index].isEmpty()) {
            index = rng.Next(range);
        }
        makeMove(index.ToString());

    }

    // delayed placing a piece, so the ai doesn't move at super human speeds
    IEnumerator placePiece(Square target, Action src) {
        yield return new WaitForSeconds(2);
        src.Invoke();
        yield return new WaitForSeconds(1);
        target.handlePieceClick();
    }

    // delay carry so the ai doesn't move at super human speeds.
    IEnumerator delayCarry(string direction, string config, int src, int carryTotal) {
        yield return new WaitForSeconds(1f);
        board.allSquares[src].handlePieceClick();
        yield return new WaitForSeconds(0.4f);

        while(board.allSquares[src].pickedUpStack.Count > carryTotal) {
            board.allSquares[src].handlePieceClick();
            yield return new WaitForSeconds(0.4f);
        }

        int movementFactor = 0;
        if(direction == ">") { // right
            movementFactor = 1;
        } else if(direction == "<") { // left
            movementFactor = -1;
        } else if(direction == "+") { // up
            movementFactor = 5;
        } else if (direction == "-") { // down
            movementFactor = -5;
        }

        for(int i = 0; i < config.Length; i++) {
            int stoneNum = int.Parse(config.Substring(i, 1));
            if(stoneNum == 0) { break; }
            int dest = src + ((i+1) * movementFactor);
            for(int j = 0; j < stoneNum; j++) {
                board.allSquares[dest].handlePieceClick();
                yield return new WaitForSeconds(1f);
            }
        }
    }

}
