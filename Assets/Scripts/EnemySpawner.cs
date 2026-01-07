using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Configurazione")]
    public string prefabName = "Blue Idle";
    public int enemiesPerWave = 3;
    public int extraEnemiesPerWave = 2;
    public float delayBetweenEnemies = 0.5f;

    private GameObject spawnParent;
    private bool isSpawning = true;
    private bool waveInProgress = false;

    void Start()
    {
        spawnParent = GameObject.FindWithTag("World Enemies");

        if (spawnParent == null)
            Debug.LogError("Oggetto 'World Enemies' non trovato!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            isSpawning = !isSpawning;
            Debug.Log(isSpawning ? "Sistema Ondate Attivo" : "Sistema Ondate Fermato");
        }

        if (isSpawning && !waveInProgress)
        {
            Debug.Log(spawnParent.transform.childCount);
            if (spawnParent != null && spawnParent.transform.childCount == 0)
            {
                StartCoroutine(SpawnWaveRoutine());
            }
        }
    }

    IEnumerator SpawnWaveRoutine()
    {
        waveInProgress = true;

        string[] enemyNames = new string[enemiesPerWave];
        for (int i = 0; i < enemyNames.Length; i++)
        {
            enemyNames[i] = prefabName;
        }

        foreach (string name in enemyNames)
        {
            SpawnEnemy(name);
            yield return new WaitForSeconds(delayBetweenEnemies);
        }

        enemiesPerWave += extraEnemiesPerWave;
        waveInProgress = false;
    }

    void SpawnEnemy(string nameToSpawn)
    {
        GameObject prefab = Resources.Load<GameObject>(nameToSpawn);
        if (prefab != null && spawnParent != null)
        {
            // Calcoliamo la posizione casuale
            Vector3 randomPosition = GetRandomSpawnPosition();
            Instantiate(prefab, randomPosition, Quaternion.identity, spawnParent.transform);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        // Partiamo dalla posizione dello Spawner stesso come base
        float minX = transform.position.x;
        float maxX = transform.position.x;
        float minZ = transform.position.z;
        float maxZ = transform.position.z;

        // Troviamo tutti gli "espansori" dell'area
        GameObject[] expanders = GameObject.FindGameObjectsWithTag("Expander spawnRange");

        foreach (GameObject exp in expanders)
        {
            // Aggiorniamo i limiti dell'area in base alla posizione di ogni Expander
            if (exp.transform.position.x < minX) minX = exp.transform.position.x;
            if (exp.transform.position.x > maxX) maxX = exp.transform.position.x;
            if (exp.transform.position.z < minZ) minZ = exp.transform.position.z;
            if (exp.transform.position.z > maxZ) maxZ = exp.transform.position.z;
        }

        // Generiamo un punto casuale tra i valori minimi e massimi trovati
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);

        // Manteniamo la Y dello spawner (altezza del terreno)
        return new Vector3(randomX, transform.position.y, randomZ);
    }
}