using Unity.VisualScripting;
using UnityEngine;

public class EnemySlimeAI : EnemySlime 
{
    public float speed = 2f;
    public float chaseDistance = 4f;
    public string type = "slime";

    [Header("Chase Offset")]
    public float stopOffset = 0.6f;        // distanza laterale dal player
    public float stopTolerance = 0.05f;    // zona morta anti jitter

    public PlayerAttack playerScript;
    private Transform playerTransform;
    private RoundManager roundManager;
    private bool inTouchPlayer = false;


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

    void ChasePlayerWithOffset()
    {
        
        if (isDead) return;

        
        float side = transform.position.x < playerTransform.position.x ? -1f : 1f;
        sr.flipX = (playerTransform.position.x - transform.position.x) >= 0;

        Vector2 targetPos = new Vector2(
            playerTransform.position.x + side * stopOffset,
            transform.position.y
        );

        float distToTarget = Vector2.Distance(transform.position, targetPos);

       
        float verticalDist = Mathf.Abs(transform.position.y - playerTransform.position.y);
        
        if (distToTarget <= stopTolerance && verticalDist <= 0.3f)
        {
            inTouchPlayer = true;
            return;
        }
        else
        {
            inTouchPlayer = false;
        }

        // 4. Movimento
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);
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

        if(inTouchPlayer && !isDead)
        {
            playerScript.takeHit(this, null);
        }
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
