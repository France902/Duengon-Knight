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
    public List<GameObject> doors;
    public List<GameObject> secondaryDoors;
    public List<GameObject> spawnManagers;
    RoundManager rm;

    void Start()
    {
        doors = GameObject.FindGameObjectsWithTag("Door")
                      .OrderBy(obj => obj.name)
                      .ToList();

        for (int i = 0; i < doors.Count; i++)
        {
            Debug.Log(doors[i].name);
            if (doors[i].name == "collider")
            {
                doors.RemoveAt(i);
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
            SetAllDoorsState(1, false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Door"))
        {
            isInArena = true;
            searchCollidedDoor(collision);
            Destroy(collision.gameObject);
            SetAllDoorsState(-5, true);
        }
    }

    void searchCollidedDoor(Collider2D collision)
    {
        int i;
        Debug.Log(doors.Count + " " +  spawnManagers.Count);
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