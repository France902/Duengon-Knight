using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Configurazione Ondata")]
    // Trascina i Prefab. Lascia degli spazi vuoti (Element = None) per separare i round
    public GameObject[] enemiesToSpawn;

    public float delayBetweenEnemies = 0.5f;
    public RoundManager roundManager;

    private GameObject spawnParent;
    private bool isSpawning = true;
    private bool waveInProgress = false;

    // Indice per ricordarsi a che punto dell'array siamo arrivati
    private int currentEnemyIndex = 0;

    void Start()
    {
        spawnParent = GameObject.FindWithTag("World Enemies");
        if (spawnParent == null)
            Debug.LogError("Oggetto 'World Enemies' non trovato!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isSpawning = !isSpawning;
        }

        // 2. Aggiungiamo il controllo !enemiesStillPresent alla condizione principale
        if (isSpawning && !waveInProgress && !roundManager.isCutscene)
        {
            // Il controllo sul childCount può rimanere come ulteriore sicurezza se i nemici sono figli di spawnParent
            if (spawnParent != null && spawnParent.transform.childCount == 0)
            {
                if (currentEnemyIndex < enemiesToSpawn.Length)
                {
                    roundManager.setIsCutscene(true);
                    StartCoroutine(WaitAndThenSpawn());
                }
                else
                {
                    Debug.Log("Tutti i round completati!");
                    isSpawning = false;
                }
            }
        }
    }

    IEnumerator WaitAndThenSpawn()
    {
        yield return new WaitForSeconds(roundManager.displayDuration * 1.6f);
        yield return StartCoroutine(SpawnWaveRoutine());
    }

    IEnumerator SpawnWaveRoutine()
    {
        waveInProgress = true;

        // Continua a scorrere l'array dall'ultimo punto salvato
        while (currentEnemyIndex < enemiesToSpawn.Length)
        {
            GameObject currentPrefab = enemiesToSpawn[currentEnemyIndex];

            // Se troviamo un elemento vuoto (None/null), il round finisce qui
            if (currentPrefab == null)
            {
                currentEnemyIndex++; // Incrementiamo per saltare il "null" al prossimo round
                break;
            }

            // Altrimenti, spawna il nemico
            SpawnEnemy(currentPrefab);
            currentEnemyIndex++;

            yield return new WaitForSeconds(delayBetweenEnemies);
        }

        waveInProgress = false;
    }

    void SpawnEnemy(GameObject prefab)
    {
        if (spawnParent != null)
        {
            Vector3 randomPosition = GetRandomSpawnPosition();
            Instantiate(prefab, randomPosition, Quaternion.identity, spawnParent.transform);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        // ... (Logica della posizione identica a prima)
        float minX = transform.position.x;
        float maxX = transform.position.x;
        float minZ = transform.position.z;
        float maxZ = transform.position.z;
        GameObject[] expanders = GameObject.FindGameObjectsWithTag("Expander spawnRange");
        foreach (GameObject exp in expanders)
        {
            if (exp.transform.position.x < minX) minX = exp.transform.position.x;
            if (exp.transform.position.x > maxX) maxX = exp.transform.position.x;
            if (exp.transform.position.z < minZ) minZ = exp.transform.position.z;
            if (exp.transform.position.z > maxZ) maxZ = exp.transform.position.z;
        }
        return new Vector3(Random.Range(minX, maxX), transform.position.y, Random.Range(minZ, maxZ));
    }
}