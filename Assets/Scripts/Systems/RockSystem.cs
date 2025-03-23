using UnityEngine;
using System.Collections.Generic;
public class RockSystem : MonoBehaviour
{
    public EnemySpawner enemySpawner;
    public List<EnemyType> customEnemyTypes;
    public int customMaxEnemiesPerWave = 10;

    private int count;
    public void SpawnEnemies()
    {
        if (count > 0) return;

        enemySpawner.SpawnCustomWave(customEnemyTypes, customMaxEnemiesPerWave);
        count += 1;
    }
}
