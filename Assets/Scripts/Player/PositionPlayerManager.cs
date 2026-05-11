using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

// Script per gestire la posizione del giocatore rispetto alle arene, il blocco delle porte e l'avvio delle ondate
public class PositionPlayerManager : MonoBehaviour
{
    [Header("Stati del Giocatore")]
    public PlayerAttack playerScript;
    public bool isInArena = false;   // Vero se il giocatore è attualmente intrappolato in un'arena
    public bool isBossFight;         // Vero se l'arena attuale è quella di un boss

    [Header("Liste di Gestione Arene")]
    public List<GameObject> doors;           // Porte normali delle arene
    public List<GameObject> bossDoors;       // Porte specifiche per i boss
    public List<GameObject> secondaryDoors;  // Porte secondarie (es. uscite)
    public List<GameObject> spawnManagers;   // Manager che gestiscono la comparsa dei nemici

    public RoundManager rm;

    void Start()
    {
        // Trova tutte le porte normali, le ordina alfabeticamente e le salva in una lista
        doors = GameObject.FindGameObjectsWithTag("Door")
                      .OrderBy(obj => obj.name)
                      .ToList();

        // Rimuove dalla lista eventuali porte chiamate genericamente "collider" 
        // (presumibilmente per tenere solo "collider1", "collider2", ecc.)
        for (int i = 0; i < doors.Count; i++)
        {
            if (doors[i].name == "collider")
            {
                doors.RemoveAt(i);
                i--; // Decrementa l'indice per non saltare l'elemento successivo dopo la rimozione
            }
        }

        // Stessa logica di ricerca e pulizia per le porte dei boss
        bossDoors = GameObject.FindGameObjectsWithTag("bossDoor")
                      .OrderBy(obj => obj.name)
                      .ToList();

        for (int i = 0; i < bossDoors.Count; i++)
        {
            if (bossDoors[i].name == "collider")
            {
                bossDoors.RemoveAt(i);
                i--;
            }
        }

        // Recupera le porte secondarie e gli spawn manager (ordinati per nome per farli corrispondere alle porte)
        secondaryDoors = GameObject.FindGameObjectsWithTag("secondaryDoor").ToList();
        spawnManagers = GameObject.FindGameObjectsWithTag("spawnManager")
                        .OrderBy(obj => obj.name)
                      .ToList();

        // Recupera i riferimenti agli script fondamentali
        playerScript = FindObjectOfType<PlayerAttack>();
        rm = FindObjectOfType<RoundManager>();
    }

    private void Update()
    {
        // Se il giocatore non è in combattimento e non c'è una cutscene in corso:
        // Resetta il contatore dei round e si assicura che le porte siano "aperte" (nascoste in background e senza collisioni)
        if (!isInArena && !rm.isCutscene)
        {
            rm.roundFought = 0;
            SetAllDoorsState(1, false);
        }
    }

    // Rileva quando il giocatore attraversa il trigger di ingresso di un'arena
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Se tocca l'ingresso di un'arena normale o di un boss
        if (collision.CompareTag("Door") || collision.CompareTag("bossDoor"))
        {
            isInArena = true; // Blocca il giocatore nell'arena

            searchCollidedDoor(collision); // Trova il manager associato a questa porta per avviare i nemici

            Destroy(collision.gameObject); // Distrugge il trigger superato così non può essere riattivato

            SetAllDoorsState(-5, true); // "Chiude" visivamente e fisicamente le porte

            // Controlla se è una boss fight per impostare lo stato
            if (collision.CompareTag("bossDoor")) isBossFight = true;
            else isBossFight = false;
        }
    }

    // Associa la porta che il giocatore ha attraversato al corretto WaveManager
    void searchCollidedDoor(Collider2D collision)
    {
        int i;
        for (i = 0; i < doors.Count; i++)
        {
            // Cerca corrispondenza tra il nome del trigger ("collider1", "collider2", ecc.) e l'indice dello spawnManager
            if (collision.gameObject.name == "collider" + (i + 1))
                spawnManagers[i].GetComponent<WaveManager>().disabled = false; // Attiva l'ondata
        }
    }

    // Gestisce lo stato visivo (Sorting Order) e fisico (Colliders) di tutte le porte sulla mappa
    void SetAllDoorsState(int newOrder, bool collidersActive)
    {
        // 1. Modifica il Sorting Order delle porte normali (es. per portarle in primo piano e renderle visibili sopra il pavimento)
        foreach (GameObject door in doors)
        {
            SpriteRenderer parentRenderer;
            if (door != null)
            {
                parentRenderer = door.GetComponentInParent<SpriteRenderer>();

                if (parentRenderer != null)
                {
                    parentRenderer.sortingOrder = newOrder;
                }
            }
        }

        // 2. Modifica il Sorting Order delle porte secondarie
        foreach (GameObject door in secondaryDoors)
        {
            SpriteRenderer parentRenderer;
            if (door != null)
            {
                parentRenderer = door.GetComponentInParent<SpriteRenderer>();

                if (parentRenderer != null)
                {
                    parentRenderer.sortingOrder = newOrder;
                }
            }
        }

        // 3. Modifica il Sorting Order delle porte dei boss
        foreach (GameObject door in bossDoors)
        {
            SpriteRenderer parentRenderer;
            if (door != null)
            {
                parentRenderer = door.GetComponentInParent<SpriteRenderer>();

                if (parentRenderer != null)
                {
                    parentRenderer.sortingOrder = newOrder;
                }
            }
        }

        // 4. Cerca tutte le hitbox solide (i muri invisibili delle porte) e le attiva o disattiva
        // ATTENZIONE: Questo FindGameObjectsWithTag eseguito in Update è pesante per le prestazioni!
        GameObject[] solidHitboxes = GameObject.FindGameObjectsWithTag("SolidHitboxDoor");
        foreach (GameObject hitbox in solidHitboxes)
        {
            BoxCollider2D box = hitbox.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                // Attiva o disattiva la collisione solida in base allo stato richiesto dall'arena
                if (!collidersActive && box.enabled) box.enabled = false;
                else box.enabled = collidersActive;
            }
        }
    }
}