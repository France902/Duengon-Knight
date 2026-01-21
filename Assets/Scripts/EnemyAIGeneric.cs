using System;
using System.Collections;
using UnityEngine;

public class EnemyAIGeneric : EnemySlime
{
    public float speed = 2f;
    public float chaseDistance = 4f;
    public float jumpForce = 1f;

    [Header("Chase Offset")]
    public float stopOffset = 0.6f;        // distanza laterale dal player
    public float stopTolerance = 0.05f;    // zona morta anti jitter
    public string type;

    public PlayerAttack playerScript;
    private Transform playerTransform;
    private RoundManager roundManager;
    private Rigidbody2D rb;
    private bool inTouchPlayer = false;
    private bool isAttacking = false;
    private Boolean moveable = true;


    private enum State { Idle, Chase }
    private State currentState = State.Idle;

    private void Start()
    {
        roundManager = GameObject.FindAnyObjectByType<RoundManager>();
        playerScript = GameObject.FindObjectOfType<PlayerAttack>();
        rb = GetComponent<Rigidbody2D>();
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

        // 1. Calcoli base
        float side = transform.position.x < playerTransform.position.x ? -1f : 1f;
        sr.flipX = (playerTransform.position.x - transform.position.x) <= 0;

        Vector2 targetPos = new Vector2(playerTransform.position.x + side * stopOffset, transform.position.y);
        float distToTarget = Vector2.Distance(transform.position, targetPos);
        float verticalDiff = playerTransform.position.y - transform.position.y;
        float verticalDistAbs = Mathf.Abs(verticalDiff);

        // --- LOGICA DI SALTO E EVITAMENTO OSTACOLI ---
        if (verticalDiff > 0.1f && !isAttacking && (distToTarget <= stopTolerance || isRepositioning) && playerScript.getIsGrounded())
        {
            // Usiamo un BoxCast invece di un Raycast per simulare la larghezza del Goblin
            // Dimensione del box (larghezza 0.5f, altezza 0.1f). Regola la larghezza se necessario.
            Vector2 boxSize = new Vector2(0.15f, 0.1f);
            float rayLength = 2.0f;

            RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxSize, 0f, Vector2.up, rayLength, LayerMask.GetMask("Ground"));

            // Visualizzazione per Debug del BoxCast
            if (hit.collider != null)
            {
                isRepositioning = true;
                if (repositionDir == 0f)
                    repositionDir = (transform.position.x < playerTransform.position.x) ? -1f : 1f;

                // Muoviti lateralmente
                if(repositionDir == -1f) sr.flipX = true;
                else sr.flipX = false;
                transform.Translate(new Vector2(repositionDir, 0) * speed * Time.deltaTime);
                return;
            }
            else
            {
                // Se non tocca più nulla, aggiungiamo un piccolo spostamento extra prima di saltare
                // per assicurarci che non sia proprio sul bordo (margine di sicurezza)
                if (isRepositioning)
                {
                    // Spinta finale laterale per liberare completamente la testa
                    transform.Translate(new Vector2(repositionDir, 0) * speed * Time.deltaTime);
                }

                // SOFFITTO LIBERO: Salta
                isRepositioning = false;
                repositionDir = 0f;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                anim.SetTrigger("jump"); 
                return;
            }
        }
        else
        {
            isRepositioning = false;
            repositionDir = 0f;
        }

        // 2. Controllo distanza per ATTACCO/IDLE
        if (distToTarget <= stopTolerance && verticalDistAbs <= 0.1f)
        {
            if (isAttacking) return;
            anim.SetBool("idle", true);
            anim.SetBool("move", false); // Spegni movimento in idle
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

        // 3. Movimento Orizzontale standard
        if (moveable && !isRepositioning)
        {
            anim.SetBool("idle", false);
            anim.SetBool("move", true);
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
