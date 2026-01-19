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
        // 1. Controllo morte: se è morto, esce subito e non fa nulla
        if (isDead)
        {
            
            return;
        }

        // 2. Logica di orientamento (eseguita solo se vivo)
        float side = transform.position.x < playerTransform.position.x ? -1f : 1f;
        
        sr.flipX = (playerTransform.position.x - transform.position.x) <= 0;

        Vector2 targetPos = new Vector2(
            playerTransform.position.x + side * stopOffset,
            transform.position.y
        );

        float distToTarget = Vector2.Distance(transform.position, targetPos);

        // 3. Controllo distanza
        if (distToTarget <= stopTolerance)
        {
            if (isAttacking) return;
            else
            {
                isAttacking = true;
                anim.Play("idle", 0, 0f);
                anim.SetBool("idle", true);
                anim.SetBool("move", false);
                inTouchPlayer = true;
                return;
            }
        }
        else
        {
            inTouchPlayer = false;
            isAttacking = false;
        }

        anim.SetBool("idle", false);
        anim.SetBool("move", true);
        // 4. Movimento (eseguito solo se vivo e lontano)
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
