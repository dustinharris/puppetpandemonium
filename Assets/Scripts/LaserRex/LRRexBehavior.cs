﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LRRexBehavior : MonoBehaviour {

    [SerializeField] private GameObject redLaserAim;
    [SerializeField] private GameObject blueLaserAim;
    [SerializeField] private GameObject watchWarningIndicator;
    [SerializeField] private float watchWarningTime = 1f;
    private LRLaserAimBehavior laserAimRed;
    private LRLaserAimBehavior laserAimBlue;
    private bool p1Moving = false;
    private bool p2Moving = false;
    private bool rexInWatchState = false;
    [SerializeField] private bool testFunctions = false;

    private void Awake()
    {
        // Rex state listeners
        Messenger.AddListener(GameEvent.REX_START_WATCH_WARNING, RexStartWatchWarning);
        Messenger.AddListener(GameEvent.REX_START_WATCH, RexStartWatch);

        // Car state listeners
        Messenger.AddListener(GameEvent.REX_P1_START_MOVING, RexP1StartMoving);
        Messenger.AddListener(GameEvent.REX_P2_START_MOVING, RexP2StartMoving);
        Messenger.AddListener(GameEvent.REX_P1_STOP_MOVING, RexP1StopMoving);
        Messenger.AddListener(GameEvent.REX_P2_STOP_MOVING, RexP2StopMoving);
    }

    void Start () {

        // Get laser behavior scripts attached to laser aims
        laserAimRed = redLaserAim.GetComponent<LRLaserAimBehavior>();
        laserAimBlue = blueLaserAim.GetComponent<LRLaserAimBehavior>();

        // At the beginning rex is in watch state
        rexInWatchState = true;

        // Test flag to test various functions
        if (testFunctions == true)
        {
            StartCoroutine(TestLaser(1f));
            StartCoroutine(TestWarningIndicator(1f));
        }
    }

    void Update()
    {
        // If rex is in watch state and either player is moving, shoot that player
        if (rexInWatchState)
        {
            if (p1Moving)
            {
                // Rex caught P1:
                // Shoot laser and send back to start
                laserAimRed.CreateNewLaser();
                Messenger.Broadcast(GameEvent.P1_REX_STARTING_POS);
            }
            if (p2Moving)
            {
                // Rex caught P2:
                // Shoot laser and send back to start
                laserAimBlue.CreateNewLaser();
                Messenger.Broadcast(GameEvent.P2_REX_STARTING_POS);
            }
        }
    }

    private void RexStartWatch()
    {
        // Start watching animation
    }

    private void RexP1StartMoving()
    {
        p1Moving = true;
    }

    private void RexP2StartMoving()
    {
        p2Moving = true;
    }

    private void RexP1StopMoving()
    {
        p1Moving = false;
    }

    private void RexP2StopMoving()
    {
        p2Moving = false;
    }

    private IEnumerator RexStartWatchWarning()
    {
        // Show watch warning indicator
        watchWarningIndicator.SetActive(true);

        // Wait for X seconds, per watchWarningTime
        yield return new WaitForSeconds(watchWarningTime);

        // Hide watch warning indicator
        watchWarningIndicator.SetActive(false);

        // Broadcast message to start mamarex watch cycle
        Messenger.Broadcast(GameEvent.REX_START_WATCH);
    }

    private IEnumerator TestLaser(float waitTime)
    {
        while(true)
        {
            laserAimRed.CreateNewLaser();
            laserAimBlue.CreateNewLaser();
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator TestWarningIndicator(float waitTime)
    {
        while (true)
        {
            // Show watch warning indicator
            watchWarningIndicator.SetActive(true);

            // Wait for X seconds, per watchWarningTime
            yield return new WaitForSeconds(watchWarningTime);

            // Hide watch warning indicator
            watchWarningIndicator.SetActive(false);

            // Wait for X seconds, per watchWarningTime
            yield return new WaitForSeconds(watchWarningTime);
        }
    }
}
