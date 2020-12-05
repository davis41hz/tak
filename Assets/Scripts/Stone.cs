using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StoneShape { Round, Sharp }
public enum StoneType { Flat, Standing, Capstone }

public class Stone : MonoBehaviour {
    public StoneShape shape;
    public StoneType type;
    public bool onBoard = false;
    Transform stoneTransform;
    Vector3 startPos, midPos, endPos;
    public Quaternion flatRotation, standingRotation;
    public float pieceBoardOffset = 0.2f;
    public Vector3 standingPositionOffset = new Vector3(0f, 0.5f, 0f);
    public List<AudioClip> handSounds, clickSounds, flattenSounds, thumpSounds;
    AudioSource audio;
    float elapsed, startTime;
    public bool isSlerping, isSlerping2, isFlipping;
    public Vector3 peakOffset = new Vector3(0,2,0);
    float transitionTime = 0.4f;
    float transitionTime2 = 0.2f;
    float flipTime = 0.1f;
    Quaternion startRot, endRot;
    bool soundAfterSlerp;
    public GameController gc;

    void Start() {
        isSlerping2 = false;
        isSlerping = false;
        isFlipping = false;
        soundAfterSlerp = false;
        stoneTransform = gameObject.GetComponent<Transform>();
        audio = gameObject.GetComponent<AudioSource>();
    }

    void Update() {
        if(isSlerping) { // slerp for 3 points
            elapsed = (Time.time - startTime)/transitionTime;
            if(elapsed <= 0.5f) {
                transform.localPosition = Vector3.Slerp(startPos, midPos, elapsed * 2f);
            } else if(elapsed <= 1.1f){
                transform.localPosition = Vector3.Slerp(midPos, endPos, (elapsed-0.5f)*2f);
            } else {
                isSlerping = false;
                if(soundAfterSlerp) { // play sound on landing.
                    if(type == StoneType.Capstone) {
                        playThumpSound();
                    } else {
                        playClickSound();
                    }
                }
            }
        } else if(isSlerping2) { // slerp for 2 points
            elapsed = (Time.time - startTime)/transitionTime2;
            if(elapsed <= 1.1f){
                transform.localPosition = Vector3.Slerp(startPos, endPos, elapsed);
            } else {
                isSlerping2 = false;
                if(soundAfterSlerp) { // play sound on landing.
                    if(type == StoneType.Capstone) {
                        playThumpSound();
                    } else {
                        playClickSound();
                    }
                }
            }
        } else if(isFlipping) { // slerp the flatten/raising of a stone
            elapsed = (Time.time - startTime)/flipTime;
            if(elapsed <= 1.1f){
                transform.localRotation = Quaternion.Slerp(startRot, endRot, elapsed);
                transform.localPosition = Vector3.Slerp(startPos, endPos, elapsed);
            } else {
                isFlipping = false;
            }
        }
    }

    public void flipStone() {
        if(type == StoneType.Capstone) { return; } // capstones can't flip
        else if(type == StoneType.Flat) {
            standStone();
        } else {
            flattenStone();
        }
    }

    public void flattenStone() { // transform a stone to flattened position, there's a vertical offset that must be accounted for because it doesn't rotate around centre
        if(type == StoneType.Standing) {
            isFlipping = true;
            startRot = stoneTransform.localRotation;
            endRot = flatRotation;
            startPos = stoneTransform.localPosition;
            endPos = startPos - standingPositionOffset;
            type = StoneType.Flat;
            startTime = Time.time;
        }
    }

    public void standStone() { // transform a stone to standing position, there's a vertical offset that must be accounted for because it doesn't rotate around centre
        if(type == StoneType.Capstone) { return; }
        else if(type == StoneType.Flat) {
            isFlipping = true;
            startRot = stoneTransform.localRotation;
            endRot = standingRotation;
            startPos = stoneTransform.localPosition;
            endPos = startPos + standingPositionOffset;
            type = StoneType.Standing;
            startTime = Time.time;
        }
    }


    void OnMouseDown() {
        if(onBoard && ((gc.isAi && gc.getWhosTurn() != StoneShape.Round) ||  !gc.isAi)) { // make sure piece is on board, and its not the AI's turn...shouldn't pick things up while the AI is thinking, it's disrespectful.
            gameObject.GetComponentInParent<Square>().handlePieceClick();
        }
    }


    // 4 Functions to play different piece sounds.
    public void playFlattenSound() {
        audio.clip = flattenSounds[Random.Range(0, flattenSounds.Count)];
        audio.Play();
    }

    public void playHandSound() {
        audio.clip = handSounds[Random.Range(0, handSounds.Count)];
        audio.Play();
    }

    public void playThumpSound() {
        audio.clip = thumpSounds[Random.Range(0, thumpSounds.Count)];
        audio.Play();
    }

    public void playClickSound() {
        audio.clip = clickSounds[Random.Range(0, clickSounds.Count)];
        audio.Play();
    }


    // slerp between 3 points, the mid one is elevated to make an arc.
    public void slerp3(Vector3 start, Vector3 mid, Vector3 end, bool soundOnLand) {
        isSlerping = true;
        soundAfterSlerp = soundOnLand;
        startPos = start;
        midPos = mid + peakOffset;
        endPos = end;
        startTime = Time.time;
    }

    // slerp directly between two points
    public void slerp2(Vector3 start, Vector3 end, bool soundOnLand) {
        isSlerping2 = true;
        soundAfterSlerp = soundOnLand;
        startPos = start;
        endPos = end;
        startTime = Time.time;
    }


}
