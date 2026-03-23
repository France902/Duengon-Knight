using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Configurazione Ondata")]
    
    public GameObject[] enemiesToSpawn;
    public bool[] spawnOnLeft; 

    [Header("Impostazioni")]
    public float delayBetweenEnemies = 0.5f;
    public RoundManager roundManager;
    public PositionPlayerManager playerPos;

    public GameObject altSpawn;
    public GameObject altRange;
    public GameObject[] expanders;

    

    private GameObject spawnParent;
    private bool isSpawning = true;
    private bool waveInProgress = false;
    private bool stillEnemies = true;
    public bool disabled = false;
    private int currentEnemyIndex = 0;

    void Start()
    {
        spawnParent = GameObject.FindWithTag("World Enemies");
        playerPos = FindObjectOfType<PositionPlayerManager>();

        
        if (spawnOnLeft.Length != enemiesToSpawn.Length)
        {
            Debug.LogWarning("Attenzione: L'array dei booleani non ha la stessa lunghezza dell'array dei nemici!");
        }
    }

    void Update()
    {
        if (isSpawning && !waveInProgress && !roundManager.isCutscene && !disabled)
        {
            if (spawnParent != null && spawnParent.transform.childCount == 0)
            {
                if (currentEnemyIndex < enemiesToSpawn.Length && playerPos.isInArena && stillEnemies)
                {
                    if(!playerPos.isBossFight) roundManager.setIsCutscene(true, false);
                    else roundManager.setIsCutscene(true, true);
                    StartCoroutine(WaitAndThenSpawn());
                }
                else
                {
                    if ((currentEnemyIndex >= enemiesToSpawn.Length) || !stillEnemies)
                    {
                        playerPos.isInArena = false;
                        isSpawning = false;
                    }
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
        while (currentEnemyIndex < enemiesToSpawn.Length)
        {
            
            GameObject currentPrefab = enemiesToSpawn[currentEnemyIndex];

            if (currentPrefab == null)
            {
                stillEnemies = false;
                for(int i = currentEnemyIndex + 1; i < enemiesToSpawn.Length; i++)
                {
                    if (enemiesToSpawn[i] != null) stillEnemies = true;
                }
                currentEnemyIndex++;
                break;
            }

            bool isLeft = false;
            if (currentEnemyIndex < spawnOnLeft.Length)
            {
                isLeft = spawnOnLeft[currentEnemyIndex];
            }
            currentEnemyIndex++;
            waveInProgress = true;
            yield return new WaitForSeconds(delayBetweenEnemies);
            SpawnEnemy(currentPrefab, isLeft);
            
            
            
        }

        waveInProgress = false;
    }

    void SpawnEnemy(GameObject prefab, bool leftSide)
    {
        if (spawnParent != null)
        {
            Vector3 randomPosition = GetRandomSpawnPosition(leftSide);
            Instantiate(prefab, randomPosition, Quaternion.identity, spawnParent.transform);
        }
    }

    Vector3 GetRandomSpawnPosition(bool leftSide)
    {
        float minX, maxX, minZ, maxZ;

        if (leftSide)
        {

            if (altSpawn != null && altRange != null)
            {
                minX = Mathf.Min(altSpawn.transform.position.x, altRange.transform.position.x);
                maxX = Mathf.Max(altSpawn.transform.position.x, altRange.transform.position.x);
                minZ = Mathf.Min(altSpawn.transform.position.z, altRange.transform.position.z);
                maxZ = Mathf.Max(altSpawn.transform.position.z, altRange.transform.position.z);
                return new Vector3(Random.Range(minX, maxX), transform.position.y, Random.Range(minZ, maxZ));
            }
        }

        minX = transform.position.x;
        maxX = transform.position.x;
        minZ = transform.position.z;
        maxZ = transform.position.z;

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