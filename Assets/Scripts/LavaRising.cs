using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaRising : MonoBehaviour
{
    [Header("Impostazioni Movimento")]
    public float risingSpeed = 0.5f;     // Velocità di risalita
    public float waveSpeed = 2f;        // Velocità dell'oscillazione laterale
    public float waveAmount = 0.1f;     // Ampiezza dell'oscillazione

    private Vector3 startPosition;

    void Start()
    {
        // Memorizziamo la posizione iniziale
        startPosition = transform.position;
    }

    void Update()
    {
        // 1. Calcoliamo la nuova altezza (Y)
        startPosition.y += risingSpeed * Time.deltaTime;

        // 2. Calcoliamo l'oscillazione laterale (X) usando il Seno
        float xOffset = Mathf.Sin(Time.time * waveSpeed) * waveAmount;

        // 3. Applichiamo il movimento
        transform.position = new Vector3(startPosition.x + xOffset, startPosition.y, startPosition.z);
    }

    // Gestione della collisione con il giocatore
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            KillPlayer(collision.gameObject);
        }
    }

    void KillPlayer(GameObject player)
    {
        Debug.Log("Il giocatore è finito nella lava!");

        // Qui puoi inserire la tua logica di morte, ad esempio:
        // - Distruggere il giocatore: Destroy(player);
        // - Ricaricare il livello: UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        // - Disattivarlo: player.SetActive(false);
    }
}