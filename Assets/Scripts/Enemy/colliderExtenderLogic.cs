using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script utilizzato per spostare dinamicamente un collider in base alla direzione in cui guarda il nemico
public class colliderExtenderLogic : MonoBehaviour
{
    // Nota: il nome della variabile è "capsuleCollider", ma il componente effettivo è un BoxCollider2D.
    private BoxCollider2D capsuleCollider;

    void Start()
    {
        // Recupera il componente BoxCollider2D associato a questo GameObject all'avvio
        capsuleCollider = GetComponent<BoxCollider2D>();
    }

    // Sposta il collider verso sinistra (utile quando lo sprite viene flippato a sinistra)
    public void setLeftOffset()
    {
        capsuleCollider.offset = new Vector2(-0.23f, capsuleCollider.offset.y);
    }

    // Riporta il collider nella posizione originale/a destra
    public void setRightOffset()
    {
        capsuleCollider.offset = new Vector2(0, capsuleCollider.offset.y);
    }
}