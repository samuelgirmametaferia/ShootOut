using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Rendering;

[System.Serializable]
public class EnemyType
{
    public GameObject EnemyObject;
    public GameObject SpawnEffect;
    public float SpawnEDT = 3f;
    public float DeactivationDistance = 50f;
    public int MaxNum = 10;
    [Range(0, 1)]
    public float SpawnProbability = 1f;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject Player;
    [SerializeField] private List<EnemyType> enemyTypes = new List<EnemyType>();
    [SerializeField] private List<Transform> spawnPointsT = new List<Transform>();
    private Vector3[] spawnPoints;
    private int[] ActiveEnemies;
    private int[] TotalExistingEnemies;
    private List<(GameObject, int)> Enemies = new List<(GameObject, int)>();

    [Header("Wave Settings")]
    [SerializeField] private float waveInterval = 20f;
    [SerializeField] private int maxEnemiesPerWave = 50; // Maximum number of enemies per wave
    [SerializeField] private float MaximumDistanceFromSpawnPoint = 30f;
    private List<Vector3> deactivatedEnemyPositions = new List<Vector3>(); // Store deactivated positions
    private string deactivatedFilePath = "deactivated_enemies.json"; // File path for saving
    [Header("Audio")]
    [SerializeField] private AudioSource SceneAudioSource;
    [SerializeField] private AudioClip TenseMusic;
    [SerializeField] private int MinEnemyCount;

    private float LastTimePlayedAudio = 0;
    private void Start()
    {
        spawnPoints = new Vector3[spawnPointsT.Count];
        for (int i = 0; i < spawnPointsT.Count; i++)
        {
            spawnPoints[i] = spawnPointsT[i].position;
        }

        ActiveEnemies = new int[enemyTypes.Count];
        TotalExistingEnemies = new int[enemyTypes.Count];
        LoadDeactivatedPositions(); // Load previously saved deactivated positions
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        while (true)
        {
            yield return new WaitForSeconds(waveInterval);

            int enemiesSpawnedInWave = 0;

            for (int i = 0; i < enemyTypes.Count; i++)
            {
                float spawnDelay = Random.Range(0f, enemyTypes[i].SpawnEDT);
                yield return new WaitForSeconds(spawnDelay);

                if (enemiesSpawnedInWave < maxEnemiesPerWave)
                {
                    if(SpawnEnemy()) enemiesSpawnedInWave++;
                }
                else
                {
                    break;
                }
            }

            AdjustWaveDifficulty();
        }
    }

    private void AdjustWaveDifficulty()
    {
        foreach (var enemy in enemyTypes)
        {
            enemy.MaxNum += 1;
            enemy.SpawnProbability = Mathf.Min(1f, enemy.SpawnProbability + 0.1f);
        }
    }

    private void Update()
    {
        TurnOffIfFarAway();
        PlayAudioCorrectly();
        LastTimePlayedAudio += Time.deltaTime;
    }
    public void PlayAudioCorrectly()
    {
        int EnemyCount = 0;
        for (int i = 0; i < ActiveEnemies.Length; i++)
        {
            EnemyCount += ActiveEnemies[i];
        }

        // If the last time the audio was played is less than the length of the tense music, return.
        if (LastTimePlayedAudio < TenseMusic.length) return;

        // Check if the number of enemies is greater than the minimum, if so, play the tense music
        if (EnemyCount > MinEnemyCount)
        {
            if (!SceneAudioSource.isPlaying) // If not already playing
            {
                SceneAudioSource.clip = TenseMusic; // Set the tense music clip
                SceneAudioSource.Play(); // Start playing
            }
        }
        else
        {
            if (SceneAudioSource.isPlaying) // If it's currently playing
            {
                SceneAudioSource.Stop(); // Stop the audio
            }
        }
    }

    private void SaveDeactivatedPositions()
    {
        string json = JsonUtility.ToJson(new Serialization<Vector3>(deactivatedEnemyPositions));
        File.WriteAllText(deactivatedFilePath, json);  // Save the data to a file
    }

    private void LoadDeactivatedPositions()
    {
        if (File.Exists(deactivatedFilePath))
        {
            string json = File.ReadAllText(deactivatedFilePath);
            deactivatedEnemyPositions = JsonUtility.FromJson<Serialization<Vector3>>(json).ToList(); // Load the saved positions
        }
    }


    public void TurnOffIfFarAway()
    {
        for (int i = 0; i < Enemies.Count; i++)
        {
            if (Enemies[i].Item1.activeSelf)
            {
                if (Vector3.Distance(Enemies[i].Item1.transform.position, Player.transform.position) > enemyTypes[Enemies[i].Item2].DeactivationDistance)
                {
                    deactivatedEnemyPositions.Add(Enemies[i].Item1.transform.position); // Save the deactivated position
                    Enemies[i].Item1.SetActive(false);
                    ActiveEnemies[Enemies[i].Item2]--;
                }
            }
            else
            {
                if (Vector3.Distance(Enemies[i].Item1.transform.position, Player.transform.position) <= enemyTypes[Enemies[i].Item2].DeactivationDistance)
                {
                    Enemies[i].Item1.SetActive(true);
                    ActiveEnemies[Enemies[i].Item2]++;
                }
            }
        }

        // Save the positions to a file after each check
        SaveDeactivatedPositions();
    }

    public bool SpawnEnemy()
    {
        int EnemyToSpawn = GetRandIndex();
        if (ActiveEnemies[EnemyToSpawn] < enemyTypes[EnemyToSpawn].MaxNum)
        {
            Vector3 SpawnPoint = FindClosestPoint();
            float MinDst = (Player.transform.position - SpawnPoint).magnitude;
            if (MinDst > MaximumDistanceFromSpawnPoint) return false;
            ActiveEnemies[EnemyToSpawn]++;
            TotalExistingEnemies[EnemyToSpawn]++;
           
          
            GameObject NewEnemy = Instantiate(enemyTypes[EnemyToSpawn].EnemyObject, SpawnPoint, Quaternion.identity);
            Destroy(Instantiate(enemyTypes[EnemyToSpawn].SpawnEffect, SpawnPoint, Quaternion.identity), enemyTypes[EnemyToSpawn].SpawnEDT);
            Enemies.Add((NewEnemy, EnemyToSpawn));
            return true;
        }
        return false;
    }

    private int GetRandIndex()
    {
        float totalProbability = 0f;
        foreach (var enemy in enemyTypes)
        {
            totalProbability += enemy.SpawnProbability;
        }

        float randomValue = Random.Range(0f, totalProbability);
        float cumulativeProbability = 0f;
        for (int i = 0; i < enemyTypes.Count; i++)
        {
            cumulativeProbability += enemyTypes[i].SpawnProbability;
            if (randomValue <= cumulativeProbability)
            {
                return i;
            }
        }

        return 0;
    }

    public Vector3 FindClosestPoint()
    {
        Vector3 closestPoint = spawnPoints[0];
        float closestDistance = Vector3.Distance(Player.transform.position, closestPoint);

        for (int i = 1; i < spawnPoints.Length; i++)
        {
            float distance = Vector3.Distance(Player.transform.position, spawnPoints[i]);
            if (distance < closestDistance)
            {
                closestPoint = spawnPoints[i];
                closestDistance = distance;
            }
        }

        return closestPoint;
    }
    public void SpawnCustomWave(List<EnemyType> customEnemyTypes, int customMaxEnemiesPerWave)
    {
        // Create temporary arrays for active and total existing enemies
        int[] customActiveEnemies = new int[customEnemyTypes.Count];
        int[] customTotalExistingEnemies = new int[customEnemyTypes.Count];

        int enemiesSpawnedInWave = 0;

        foreach (var enemyType in customEnemyTypes)
        {
            float spawnDelay = Random.Range(0f, enemyType.SpawnEDT);
            StartCoroutine(SpawnEnemyCustom(enemyType, customActiveEnemies, customTotalExistingEnemies,  enemiesSpawnedInWave, customMaxEnemiesPerWave));
        }
    }

    private IEnumerator SpawnEnemyCustom(EnemyType enemyType, int[] customActiveEnemies, int[] customTotalExistingEnemies,  int enemiesSpawnedInWave, int customMaxEnemiesPerWave)
    {
        // Delay based on spawn EDT for the enemy
        yield return new WaitForSeconds(Random.Range(0f, enemyType.SpawnEDT));

        // Check if we have reached the maximum number of enemies for the wave
        if (enemiesSpawnedInWave < customMaxEnemiesPerWave && customActiveEnemies[customTotalExistingEnemies.Length - 1] < enemyType.MaxNum)
        {
            // Spawn the enemy at the closest spawn point
            Vector3 spawnPoint = FindClosestPoint();
            GameObject newEnemy = Instantiate(enemyType.EnemyObject, spawnPoint, Quaternion.identity);

            // Spawn the effect
            Destroy(Instantiate(enemyType.SpawnEffect, spawnPoint, Quaternion.identity), enemyType.SpawnEDT);

            // Increment the spawn counters
            customActiveEnemies[customTotalExistingEnemies.Length - 1]++;
            customTotalExistingEnemies[customTotalExistingEnemies.Length - 1]++;

            // Track the new enemy
            Enemies.Add((newEnemy, customTotalExistingEnemies.Length - 1));

            // Increase the count of spawned enemies
            enemiesSpawnedInWave++;
        }
    }


}

[System.Serializable]
public class Serialization<T>
{
    public List<T> items;
    public Serialization(List<T> items)
    {
        this.items = items;
    }
    public List<T> ToList()
    {
        return items;
    }
}