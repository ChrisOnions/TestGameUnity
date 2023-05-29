using System.Collections;
using UnityEngine;

public class Enemy_Spawner : MonoBehaviour
{
  [Header("Enemy Spawners")]
  [SerializeField] private GameObject enemyPrefab;
  [SerializeField] private Transform enemySpawnPoint;
  [SerializeField] private int numberOfEnemies = 15;
  [SerializeField] private float spawnInterval = 0.5f;

  private void Start()
  {
    StartCoroutine(SpawnEnemies());
  }

  private IEnumerator SpawnEnemies()
  {
    for (int i = 0; i < numberOfEnemies; i++)
    {
      Vector3 randomPosition = GetRandomPosition();
      GameObject spawnedEnemy = Instantiate(enemyPrefab, randomPosition, enemySpawnPoint.rotation);

      // Add a unique identifier to the enemy's name
      spawnedEnemy.name = enemyPrefab.name + "_" + i.ToString();

      yield return new WaitForSeconds(spawnInterval);
    }
  }

  private Vector3 GetRandomPosition()
  {
    float minX = -5f;
    float maxX = 5f;
    float minY = 0f;
    float maxY = 0f;
    float minZ = -5f;
    float maxZ = 5f;

    Vector3 objectPosition = transform.position;

    float randomX = Random.Range(minX, maxX);
    float randomY = Random.Range(minY, maxY);
    float randomZ = Random.Range(minZ, maxZ);

    return objectPosition + new Vector3(randomX, randomY, randomZ);
  }
}
