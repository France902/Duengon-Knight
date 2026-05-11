using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal; // Necessario per interagire con le luci 2D della Universal Render Pipeline (URP)

// Script per accendere e spegnere le luci in una stanza quando il player entra/esce da una zona (Trigger)
public class RoomLightController : MonoBehaviour
{
    [Header("Luci da controllare")]
    // Un array che contiene i riferimenti a tutte le luci 2D collegate a questa stanza
    public Light2D[] lights;

    public void Awake()
    {
        // All'avvio della scena, cicla attraverso tutto l'array di luci.
        // Di default le spegne tutte per evitare che la stanza sia illuminata
        // quando il giocatore non è ancora all'interno, risparmiando anche risorse.
        foreach (Light2D light in lights)
            light.enabled = false;
    }

    // Metodo richiamato automaticamente da Unity quando un oggetto con un Rigidbody2D 
    // e/o Collider2D entra nel trigger di questa stanza
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Controlla se il nome dell'oggetto che è entrato è esattamente "hurtbox" (presumibilmente il giocatore)
        if (other.gameObject.name == "hurtbox")
        {
            // Se è entrato il giocatore, scorre l'array e riaccende tutte le luci
            foreach (Light2D light in lights)
                light.enabled = true;
        }
    }

    // Metodo richiamato automaticamente quando l'oggetto esce dal trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        // Verifica che ad uscire sia stata proprio la "hurtbox" e non un altro oggetto casuale (es. un nemico o proiettile)
        if (other.gameObject.name == "hurtbox")
        {
            // Spegne nuovamente tutte le luci per ricreare l'oscurità
            foreach (Light2D light in lights)
                light.enabled = false;
        }
    }
}