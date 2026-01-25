using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionPlayerManager : MonoBehaviour
{
    public PlayerAttack playerScript;
    public bool isInArena = false;
    RoundManager rm;

    void Start()
    {
        playerScript = FindObjectOfType<PlayerAttack>();
        rm = FindObjectOfType<RoundManager>();
    }

    private void Update()
    {
        if (!isInArena && !rm.isCutscene)
        {
            SetAllDoorsState(1, false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Door"))
        {
            isInArena = true;
            Destroy(collision.gameObject);
            SetAllDoorsState(-5, true);
        }
    }

    // Funzione unificata per gestire ordine e hitbox solide
    void SetAllDoorsState(int newOrder, bool collidersActive)
    {
        // 1. Gestione Visuale (Tag "Door")
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
        foreach (GameObject door in doors)
        {
            SpriteRenderer parentRenderer = door.GetComponentInParent<SpriteRenderer>();
            if (parentRenderer != null)
            {
                parentRenderer.sortingOrder = newOrder;
            }
        }

        // 2. Gestione Fisica (Tag "SolidHitboxDoor")
        GameObject[] solidHitboxes = GameObject.FindGameObjectsWithTag("SolidHitboxDoor");
        foreach (GameObject hitbox in solidHitboxes)
        {
            BoxCollider2D box = hitbox.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                if (!collidersActive && box.enabled) Destroy(box.gameObject);
                else box.enabled = collidersActive;
            }
        }
    }
}