using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Script che gestisce la generazione (spawn) dei nemici all'interno di un'arena
public class WaveManager : MonoBehaviour
{
    [Header("Configurazione Ondata")]
    // Array che contiene i prefabs dei nemici. 
    // NOTA: Inserire uno slot 'None' (null) crea una "pausa" (fine round), 
    // costringendo il giocatore a uccidere i nemici attuali prima di proseguire.
    public GameObject[] enemiesToSpawn;

    // Array che decide se il nemico corrispondente (stesso indice) deve spawnare a sinistra o a destra
    public bool[] spawnOnLeft;

    [Header("Impostazioni")]
    public float delayBetweenEnemies = 0.5f;   // Pausa tra lo spawn di un nemico e l'altro
    public RoundManager roundManager;
    public PositionPlayerManager playerPos;

    [Header("Aree di Spawn")]
    // Punti di riferimento per calcolare le aree in cui far apparire i nemici
    public GameObject altSpawn;
    public GameObject altRange;
    public GameObject[] expanders;

    private GameObject spawnParent;            // L'oggetto padre che conterrà tutti i nemici spawnati
    private bool isSpawning = true;            // Stato generale dello spawner
    private bool waveInProgress = false;       // Vero se la coroutine sta attualmente spawnando nemici
    private bool stillEnemies = true;          // Vero se ci sono ancora nemici nella lista globale
    public bool disabled = false;              // Permette di disattivare lo spawner esternamente
    private int currentEnemyIndex = 0;         // Indice per tenere traccia di a che punto siamo nell'array enemiesToSpawn

    void Start()
    {
        // Trova l'oggetto padre (contenitore) dove verranno instanziati i nemici.
        // Utile per tenere la gerarchia di Unity pulita e per contare quanti nemici sono vivi.
        spawnParent = GameObject.FindWithTag("World Enemies");
        playerPos = FindObjectOfType<PositionPlayerManager>();

        // Controllo di sicurezza: avvisa lo sviluppatore se si è dimenticato di spuntare/impostare i lati di spawn
        if (spawnOnLeft.Length != enemiesToSpawn.Length)
        {
            Debug.LogWarning("Attenzione: L'array dei booleani non ha la stessa lunghezza dell'array dei nemici!");
        }
    }

    void Update()
    {
        // Se lo spawner è attivo, non sta già generando una wave, non c'è una cutscene e non è disabilitato
        if (isSpawning && !waveInProgress && !roundManager.isCutscene && !disabled)
        {
            // CONTROLLO CHIAVE: Procede solo se l'arena è "pulita" (0 figli nel contenitore World Enemies)
            if (spawnParent != null && spawnParent.transform.childCount == 0)
            {
                // Se ci sono ancora nemici da generare e il player è bloccato nell'arena
                if (currentEnemyIndex < enemiesToSpawn.Length && playerPos.isInArena && stillEnemies)
                {
                    // Comunica al RoundManager l'inizio di una nuova ondata (e se è una boss fight)
                    if (!playerPos.isBossFight) roundManager.setIsCutscene(true, false);
                    else roundManager.setIsCutscene(true, true);

                    // Avvia la routine di spawn con il testo a schermo
                    StartCoroutine(WaitAndThenSpawn());
                }
                else
                {
                    // Se la lista è finita o non ci sono più nemici effettivi, chiudi l'arena
                    if ((currentEnemyIndex >= enemiesToSpawn.Length) || !stillEnemies)
                    {
                        playerPos.isInArena = false; // Sblocca le porte
                        isSpawning = false;          // Spegne definitivamente questo spawner
                    }
                }
            }
        }
    }

    // Coroutine per aspettare che l'UI del Round finisca la sua animazione prima di far apparire i nemici
    IEnumerator WaitAndThenSpawn()
    {
        // Aspetta per la durata del testo a schermo moltiplicata per 1.6 per dare margine al giocatore
        yield return new WaitForSeconds(roundManager.displayDuration * 1.6f);

        // Avvia la coroutine vera e propria che genera i nemici
        yield return StartCoroutine(SpawnWaveRoutine());
    }

    // Coroutine che cicla l'array dei nemici per farli spawnare uno ad uno
    IEnumerator SpawnWaveRoutine()
    {
        while (currentEnemyIndex < enemiesToSpawn.Length)
        {
            GameObject currentPrefab = enemiesToSpawn[currentEnemyIndex];

            // LOGICA DEL 'NULL': Se trova uno slot vuoto nell'array, lo interpreta come la fine della "Sotto-Ondata".
            // Interrompe il loop e aspetta che il giocatore sconfigga tutti quelli appena spawnati (gestito dall'Update).
            if (currentPrefab == null)
            {
                stillEnemies = false; // Presume momentaneamente che non ci siano più nemici

                // Controlla se dopo questo 'null' ci sono effettivamente altri prefabs validi
                for (int i = currentEnemyIndex + 1; i < enemiesToSpawn.Length; i++)
                {
                    if (enemiesToSpawn[i] != null) stillEnemies = true;
                }

                currentEnemyIndex++; // Salta il 'null' per la prossima volta
                break; // Esce dal While e ferma la coroutine
            }

            // Decide da che lato spawnare in base all'array booleano
            bool isLeft = false;
            if (currentEnemyIndex < spawnOnLeft.Length)
            {
                isLeft = spawnOnLeft[currentEnemyIndex];
            }

            currentEnemyIndex++;
            waveInProgress = true; // Blocca ulteriori attivazioni dall'Update

            // Aspetta un piccolo lasso di tempo tra un nemico e l'altro per non farli spawnare sovrapposti
            yield return new WaitForSeconds(delayBetweenEnemies);

            SpawnEnemy(currentPrefab, isLeft);
        }

        // Quando il loop finisce o si interrompe per un 'null', dichiara conclusa l'immissione di nemici
        waveInProgress = false;
    }

    // Funzione fisica per instanziare il prefab nella scena
    void SpawnEnemy(GameObject prefab, bool leftSide)
    {
        if (spawnParent != null)
        {
            Vector3 randomPosition = GetRandomSpawnPosition(leftSide);
            Instantiate(prefab, randomPosition, Quaternion.identity, spawnParent.transform);
        }
    }

    // Calcola un punto casuale nello spazio (Asse X e Z) delimitato dai marker che hai posizionato nell'editor
    Vector3 GetRandomSpawnPosition(bool leftSide)
    {
        float minX, maxX, minZ, maxZ;

        // Se deve spawnare a "sinistra" e ci sono marker alternativi configurati
        if (leftSide)
        {
            if (altSpawn != null && altRange != null)
            {
                // Trova i bordi minimi e massimi usando le posizioni dei due oggetti "alt"
                minX = Mathf.Min(altSpawn.transform.position.x, altRange.transform.position.x);
                maxX = Mathf.Max(altSpawn.transform.position.x, altRange.transform.position.x);
                minZ = Mathf.Min(altSpawn.transform.position.z, altRange.transform.position.z);
                maxZ = Mathf.Max(altSpawn.transform.position.z, altRange.transform.position.z);

                // Ritorna una posizione casuale all'interno di questo rettangolo immaginario (mantenendo la Y originaria)
                return new Vector3(Random.Range(minX, maxX), transform.position.y, Random.Range(minZ, maxZ));
            }
        }

        // --- Logica Standard / Destra (basata sugli oggetti "expanders") ---

        // Inizializza i valori base alla posizione dello spawner stesso
        minX = transform.position.x;
        maxX = transform.position.x;
        minZ = transform.position.z;
        maxZ = transform.position.z;

        // Itera su tutti i GameObject 'expanders' per espandere dinamicamente l'area di spawn
        // Crea una "scatola" che racchiude i punti più lontani di tutti gli expander
        foreach (GameObject exp in expanders)
        {
            if (exp.transform.position.x < minX) minX = exp.transform.position.x;
            if (exp.transform.position.x > maxX) maxX = exp.transform.position.x;
            if (exp.transform.position.z < minZ) minZ = exp.transform.position.z;
            if (exp.transform.position.z > maxZ) maxZ = exp.transform.position.z;
        }

        // Restituisce un punto casuale all'interno dell'area appena tracciata
        return new Vector3(Random.Range(minX, maxX), transform.position.y, Random.Range(minZ, maxZ));
    }
}