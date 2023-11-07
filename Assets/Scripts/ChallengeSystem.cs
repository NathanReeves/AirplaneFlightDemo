using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChallengeSystem : MonoBehaviour
{  
    public UIController UIController;
    public ChallengeCollider CaveEnterCollider;
    public Asteroid AsteroidPrefab;
    public GameObject RockFormation;
    public float MinAsteroidDistance = 100;
    public float AsteroidDistanceThresh = 20;
    public float AsteroidSpawnDelay = .2f;
    public float AsteroidCount = 10;

    Dictionary<ChallengeType, int> challengeProgress = new Dictionary<ChallengeType, int>();
    string asteroidChallengeString = "Destroy 3 Asteroids";
    string barrelRollChallengeString = "Do a Barrel Roll";
    string caveChallengeString = "Enter The Cave";
    string hasWonKey = "hasWon";
    bool hasWon = false;

    void Start()
    {
        setupChallenges();
        StartCoroutine(spawnAsteroids());
    }

    void setupChallenges()
    {
        if (PlayerPrefs.HasKey(hasWonKey))
        {
            hasWon = PlayerPrefs.GetInt(hasWonKey) == 1;
        }

        setupChallenge(ChallengeType.Asteroid, asteroidChallengeString);
        setupChallenge(ChallengeType.BarrelRoll, barrelRollChallengeString);
        setupChallenge(ChallengeType.Cave, caveChallengeString);
        CaveEnterCollider.ColliderCallback = delegate (bool triggerEnter) {
            if (triggerEnter)
                PlayerFoundCave();
        };

        updateChallengeUI();
    }

    void setupChallenge(ChallengeType challengeType, string challengeString)
    {
        UIController.CreateChallengeItem(challengeType, challengeString);

        string challengeKey = challengeType.ToString();
        if (PlayerPrefs.HasKey(challengeKey))
        {
            challengeProgress[challengeType] = PlayerPrefs.GetInt(challengeKey);
        }
        else
        {
            challengeProgress[challengeType] = 0;
        }
    }

    IEnumerator spawnAsteroids()
    {
        //SpawnAsteroids
        for (int i = 0; i < AsteroidCount; i++)
        {
            float distance = MinAsteroidDistance + Random.Range(-AsteroidDistanceThresh, AsteroidDistanceThresh);
            Vector3 startPos = RockFormation.transform.position - Vector3.up * distance;
            Asteroid asteroid = Instantiate(AsteroidPrefab);
            asteroid.transform.position = startPos;
            asteroid.Init(startPos, RockFormation.transform.position, PlayerDestroyedAsteroid);
            yield return new WaitForSecondsRealtime(AsteroidSpawnDelay);
        }
    }

    public void PlayerDestroyedAsteroid()
    {
        if (challengeProgress[ChallengeType.Asteroid] == 2)
        {
            SoundManager.Instance.PlaySound(SoundType.ChallengeComplete);
        }
        challengeProgress[ChallengeType.Asteroid]++;
        UIController.DisplayMainText("Asteroids Destroyed: " + challengeProgress[ChallengeType.Asteroid]);
        PlayerPrefs.SetInt(ChallengeType.Asteroid.ToString(), challengeProgress[ChallengeType.Asteroid]);
        updateChallengeUI();
    }

    public void PlayerFoundCave()
    {
        if (challengeProgress[ChallengeType.Cave] == 0)
        {
            challengeProgress[ChallengeType.Cave] = 1;
            PlayerPrefs.SetInt(ChallengeType.Cave.ToString(), 1);
            UIController.DisplayMainText("Cave Discovered!");
            SoundManager.Instance.PlaySound(SoundType.ChallengeComplete);
        }
        updateChallengeUI();
    }

    public void PlayerDidBarrellRoll()
    {
        if (challengeProgress[ChallengeType.BarrelRoll] == 0)
        {
            challengeProgress[ChallengeType.BarrelRoll] = 1;
            UIController.DisplayMainText("Barrel Roll Complete!");
            PlayerPrefs.SetInt(ChallengeType.BarrelRoll.ToString(), 1);
            SoundManager.Instance.PlaySound(SoundType.ChallengeComplete);
        }
        else
        {
            UIController.DisplayMainText("Barrel Roll!");
        }
        updateChallengeUI();
    }

    void updateChallengeUI()
    {
        if(challengeProgress[ChallengeType.Asteroid] >= 3)
        {
            UIController.OnChallengeComplete(ChallengeType.Asteroid);
        }
        if(challengeProgress[ChallengeType.BarrelRoll] > 0)
        {
            UIController.OnChallengeComplete(ChallengeType.BarrelRoll);
        }
        if (challengeProgress[ChallengeType.Cave] > 0)
        {
            UIController.OnChallengeComplete(ChallengeType.Cave);
        }
        if(challengeProgress[ChallengeType.Asteroid] >= 3
        && challengeProgress[ChallengeType.BarrelRoll] > 0
        && challengeProgress[ChallengeType.Cave] > 0
        && !hasWon)
        {
            StartCoroutine(playEnding());
        }
    }

    IEnumerator playEnding()
    {
        yield return new WaitForSecondsRealtime(2f);
        hasWon = true;
        PlayerPrefs.SetInt(hasWonKey, 1);
        UIController.DisplayMainText("YOU WIN!");
        SoundManager.Instance.PlaySound(SoundType.GameComplete);
    }
}

public enum ChallengeType
{
    Asteroid,
    BarrelRoll,
    Cave
}
