using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class Referee : MonoBehaviour
{
    [SerializeField]
    private Controller controllerA, controllerB;
    private int scoreA, scoreB;

    [SerializeField]
    private CourtInfo courtInfo;
    [SerializeField]
    private Ball ballPrefab;
    public Ball ballInPlay { get; private set; }
    private bool ballDied;

    private bool playerAServes;
    
    private WaitUntil untilBallDies;
    private WaitUntil untilBallDespawns;

    private void Start() {
        untilBallDies = new WaitUntil(() => ballDied);
        untilBallDespawns = new WaitUntil(() => !ballInPlay);
        StartCoroutine(RunGame(false));
    }

    private IEnumerator RunGame(bool sideAStarts) {
        playerAServes = sideAStarts;
        while (true) {
            controllerA.transform.position = courtInfo.ASide.center;
            controllerB.transform.position = courtInfo.BSide.center;
            PutBallInPlay();
            Controller startingPlayer = playerAServes ? controllerA : controllerB;
            ballInPlay.EnterServeState(startingPlayer, 0);
            yield return untilBallDies;
            
            // check win condition
            if (Mathf.Abs(scoreA - scoreB) >= 2 && (scoreA >= 21 || scoreB >= 21)) break;
            if (scoreA >= 30 || scoreB >= 30) break;
            
            yield return untilBallDespawns;
        }

        string winningPlayer = scoreA > scoreB ? "Player A" : "Player B";
        int winningScore = Math.Max(scoreA, scoreB);
        
        Debug.Log($"Winner iws {winningPlayer} with {winningScore} points");
    }

    private void HandleBallDeath(Controller lastPlayerHit, bool lastPlayerScored) {
        ballDied = true;
        ballInPlay.BallDied -= HandleBallDeath;
        
        if (lastPlayerHit == controllerA && lastPlayerScored) {
            scoreA++;
        } else if (lastPlayerHit == controllerB && !lastPlayerScored) {
            scoreA++;
        }
        else {
            scoreB++;
            playerAServes = false;
            Debug.Log("Player B scored");
            return;
        }

        playerAServes = true;
        Debug.Log("Player A scored");
    }
    
    
    private void PutBallInPlay() {
        ballInPlay = Instantiate(ballPrefab, Vector3.up * 3f, Quaternion.identity);
        ballDied = false;
        ballInPlay.BallDied += HandleBallDeath;
    }
}
