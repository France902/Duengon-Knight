using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PositionPlayerManager : MonoBehaviour
{
    public PlayerAttack playerScript;
    public bool isInArena = false;
    public bool isBossFight;
    public List<GameObject> doors;
    public List<GameObject> bossDoors;
    public List<GameObject> secondaryDoors;
    public List<GameObject> spawnManagers;
    public RoundManager rm;

    void Start()
    {
        doors = GameObject.FindGameObjectsWithTag("Door")
                      .OrderBy(obj => obj.name)
                      .ToList();

        for (int i = 0; i < doors.Count; i++)
        {
            if (doors[i].name == "collider")
            {
                doors.RemoveAt(i);
                i--;
            }
        }

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

        secondaryDoors = GameObject.FindGameObjectsWithTag("secondaryDoor").ToList();
        spawnManagers = GameObject.FindGameObjectsWithTag("spawnManager")
                        .OrderBy(obj => obj.name)
                      .ToList();
        playerScript = FindObjectOfType<PlayerAttack>();
        rm = FindObjectOfType<RoundManager>();
    }

    private void Update()
    {
        if (!isInArena && !rm.isCutscene)
        {
            rm.roundFought = 0;
            SetAllDoorsState(1, false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.tag);
        if (collision.CompareTag("Door") || collision.CompareTag("bossDoor"))
        {
            isInArena = true;
            searchCollidedDoor(collision);
            Destroy(collision.gameObject);
            SetAllDoorsState(-5, true);

            if (collision.CompareTag("bossDoor")) isBossFight = true;
            else isBossFight = false;
        }
    }

    void searchCollidedDoor(Collider2D collision)
    {
        int i;
        for (i = 0; i < doors.Count; i++)
        {
            if (collision.gameObject.name == "collider"+(i+1)) spawnManagers[i].GetComponent<WaveManager>().disabled = false;

        }
    }

    void SetAllDoorsState(int newOrder, bool collidersActive)
    {
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