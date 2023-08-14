using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private LevelManager lm;
    [SerializeField] private GameObject[] enemiesToSpawn;
    [SerializeField] private Vector2[] possibleSpawnPoints;
    [SerializeField] private int randomSpawnRangeX;
    [SerializeField] private int randomSpawnRangeY;
    [SerializeField] private bool spawning = false;

    private Enemy enemy;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;

        // Start spawning
        if (!spawning)
        {
            // Calculate chances to also spawn a witch
            lm.SpawnWitch();

            spawning = true;

            for(int i = 0; i < enemiesToSpawn.Length; i++)
            {
                for(int j = 0; j < lm.playerMultiplier; j++)
                {
                    // Pick a random spawn point
                    Vector2 spawnPoint = possibleSpawnPoints[Random.Range(0, possibleSpawnPoints.Length)];

                    // Now randomize it some more
                    int xRand = Random.Range(-randomSpawnRangeX, randomSpawnRangeX);
                    int yRand = Random.Range(-randomSpawnRangeX, randomSpawnRangeX);

                    enemy = Instantiate(enemiesToSpawn[i], new Vector3(spawnPoint.x + xRand, spawnPoint.y + yRand, 0), Quaternion.identity).GetComponent<Enemy>();
                    enemy.levelMultiplier = lm.levelMultiplier;
                }     
            }         
        }
    }
}
