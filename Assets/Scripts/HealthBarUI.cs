using UnityEngine;
using UnityEngine.UI; // Necessario per gestire Image

public class HealthBarUI : MonoBehaviour
{
    public Image healthBarFill;
    public PlayerAttack playerScript;

    void Start()
    {
        // Trova il giocatore tramite il Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerScript = GameObject.FindObjectOfType<PlayerAttack>();
        }
    }

    void Update()
    {
        if (playerScript != null && healthBarFill != null)
        {
            // Calcoliamo il rapporto tra salute attuale e massima
            // Poiché Fill Amount va da 0 a 1, dividiamo la salute per 5
            
            float fillValue = playerScript.health / playerScript.maxHealth;

            // Applichiamo il valore all'immagine
            healthBarFill.fillAmount = fillValue;
        }
    }
}