using System;
using System.Collections;
using UnityEngine;

public class EnemyAIGeneric : EnemySlime
{
    public float speed = 2f;
    public float chaseDistance = 4f;

    [Header("Chase Offset")]
    public float stopOffset = 0.6f;        // distanza laterale dal player
    public float stopTolerance = 0.05f;    // zona morta anti jitter
    public string type;

    public PlayerAttack playerScript;
    private Transform playerTransform;
    private RoundManager roundManager;
    private bool inTouchPlayer = false;
    private bool isAttacking = false;
    private Boolean moveable = true;


    private enum State { Idle, Chase }
    private State currentState = State.Idle;

    private void Start()
    {
        roundManager = GameObject.FindAnyObjectByType<RoundManager>();
        playerScript = GameObject.FindObjectOfType<PlayerAttack>();
    }

    protected override void Awake()
    {

        base.Awake();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            // Otteniamo lo script attaccato al Player
            playerScript = playerObj.GetComponent<PlayerAttack>();
        }
    }

    void Update()
    {
        if (isDead) return;
        if (roundManager.isCutscene) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        currentState = distance <= chaseDistance ? State.Chase : State.Idle;

        switch (currentState)
        {

            case State.Chase:
                if (playerScript.alreadyHurt) break;
                ChasePlayerWithOffset();
                break;
        }
    }

    private bool isRepositioning = false;
    private float repositionDir = 0f;

    void ChasePlayerWithOffset()
    {
        if (isDead) return;

        // 1. Calcoli distanze
        float side = transform.position.x < playerTransform.position.x ? -1f : 1f;
        sr.flipX = (playerTransform.position.x - transform.position.x) <= 0;

        Vector2 targetPos = new Vector2(playerTransform.position.x + side * stopOffset, transform.position.y);
        float distToTarget = Vector2.Distance(transform.position, targetPos);
        float verticalDiff = playerTransform.position.y - transform.position.y;
        float verticalDistAbs = Mathf.Abs(verticalDiff);

        // --- LOGICA DI SALTO E EVITAMENTO OSTACOLI ---
        if (verticalDiff > 0.1f && !isAttacking && distToTarget <= stopTolerance)
        {
            float rayLength = verticalDiff + 0.5f;
            Debug.DrawRay(transform.position, Vector2.up * rayLength, Color.green);
            // Controlla se c'è il soffitto (Ground) sopra di noi (raggio lungo quanto il salto/distanza verticale)
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, rayLength, LayerMask.GetMask("Ground"));
            Debug.Log(isRepositioning);
            if (hit.collider != null)
            {
                // C'è qualcosa sopra! Inizia il riposizionamento casuale
                if (!isRepositioning)
                {
                    isRepositioning = true;
                    
                    if(repositionDir == 0f) repositionDir = UnityEngine.Random.value > 0.5f ? 1f : -1f;
                }

                // Muoviti lateralmente per liberarti dal soffitto
                transform.Translate(new Vector2(repositionDir, 0) * speed * Time.deltaTime);
                repositionDir++;
                return; // Blocca il resto del chasing finché sta cercando spazio
            }
            else
            {
                
                isRepositioning = false;
                repositionDir = 0f;
                transform.Translate(Vector2.up * speed * Time.deltaTime * 15);
                // anim.SetBool("jump", true);
            }
        }
        else if(verticalDiff < 0.1f) isRepositioning = false;

        // 2. Controllo distanza per ATTACCO/IDLE
        if (distToTarget <= stopTolerance && verticalDistAbs <= 0.1f)
        {
            if (isAttacking) return;

            anim.Play("idle", 0, 0f);
            anim.SetBool("idle", true);
            anim.SetBool("move", false);
            moveable = false;
            inTouchPlayer = true;
            return;
        }
        else
        {
            inTouchPlayer = false;
            isAttacking = false;
            moveable = true;
        }

        // 3. Movimento Orizzontale standard (Chasing)
        if (moveable && !isRepositioning) // Muovi solo se non stai evitando un soffitto
        {
            anim.SetBool("idle", false);
            anim.SetBool("move", true);
            Debug.Log("si");
            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            transform.Translate(dir * speed * Time.deltaTime);
        }
    }

    private Collider2D weaponCollider; // Referenza per il reset

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!playerScript.isAttacking)
        {
            int enemyLayerIndex = LayerMask.NameToLayer("enemyLayer");

            // 1. Se l'oggetto è l'arma (tag weapon)
            if (collision.CompareTag("weapon"))
            {
                weaponCollider = collision.GetComponent<Collider2D>();
                if (weaponCollider != null)
                {
                    // Imposta l'exclude layer verso il nemico
                    // Usiamo lo spostamento bit a bit (1 << layerIndex) per creare la maschera
                    weaponCollider.excludeLayers = (1 << enemyLayerIndex);
                }
            }

            // 2. Logica originale per il nemico
            if (collision.gameObject.layer != enemyLayerIndex)
            {
                return;
            }

            // 3. Logica Player
            if (collision.CompareTag("Player"))
            {
                inTouchPlayer = true;
            }
        }
        
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        int enemyLayerIndex = LayerMask.NameToLayer("enemyLayer");

        // Controlla se il layer dell'oggetto è quello nemico
        if (collision.gameObject.layer != enemyLayerIndex)
        {
            return; // Esci dalla funzione: non eseguire il resto del codice
        }

        if (collision.CompareTag("Player"))
        {
            inTouchPlayer = false;
        }

    }

    public void attack()
    {
        
        if (inTouchPlayer && !isDead && !isAttacking)
        {
            isAttacking = true;
            int sceltaAttacco = UnityEngine.Random.Range(1, 3);
            Debug.Log("da animazione " + sceltaAttacco);

            if (sceltaAttacco == 1)
            {
                
                anim.SetTrigger("attack");
            }
            else
            {      
                anim.SetTrigger("attack2");
            }

            moveable = false;
        }
    }

    public void finishAttack()
    {
        isAttacking = false;
        if (inTouchPlayer && !isDead)
        {
            
            playerScript.takeHit(this, null);
        }

        
        StartCoroutine(ripristineMoveable(0.5f));
    }

    private IEnumerator ripristineMoveable(float delay)
    {
        
        yield return new WaitForSeconds(delay);

        moveable = true;
        
    }

    public void setDie()
    {
        isDead = true;
        speed = 0;
        
    }

    public void Die()
    {
        anim.SetTrigger("die");
    }

}
