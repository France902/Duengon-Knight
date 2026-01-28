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

    void SetAllDoorsState(int newOrder, bool collidersActive)
    {
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
        foreach (GameObject door in doors)
        {
            SpriteRenderer parentRenderer = door.GetComponentInParent<SpriteRenderer>();
            if (parentRenderer != null)
            {
                parentRenderer.sortingOrder = newOrder;
            }
        }

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