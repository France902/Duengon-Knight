using System;
using System.Collections;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAIGeneric : EnemySlime
{
    [Header("Statistiche Base")]
    public float speed = 2f;
    public int[] damageAttack = { 0, 0 }; // Array per i danni dei vari attacchi
    public float chaseDistance = 4f; // Distanza massima per iniziare l'inseguimento
    public float jumpForce = 1f;

    [Header("Chase Offset")]
    public float stopOffset = 0.6f;        // Distanza dal player a cui il nemico si ferma
    public float stopTolerance = 0.05f;    // Margine di tolleranza per il fermo
    public string type;                    // Identifica il tipo di nemico (es. "wizard" per il boss)

    [Header("Riferimenti e Componenti")]
    public PlayerAttack playerScript;
    private Transform playerTransform;
    private RoundManager roundManager;
    private Rigidbody2D rb;
    private HurtBoxLogic HurtBoxLogic;
    private MovementColliderLogic colliderExtenderLogic;
    public BoxCollider2D colliderExtender;

    [Header("Variabili di Stato")]
    private bool inTouchPlayer = false; // Vero se il nemico sta toccando il player
    private bool isAttacking = false;
    private bool isBlocking = false;
    private Boolean moveable = true;    // Determina se il nemico può muoversi
    private bool combo = false;
    private string attackDid = "";
    private float wanderDirection = 0;

    // Macchina a stati molto basilare
    private enum State { Idle, Chase }
    private State currentState = State.Idle;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();

        // Cerca il player nella scena e ne salva i riferimenti
        PlayerAttack playerObj = GameObject.FindObjectOfType<PlayerAttack>();
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerScript = playerObj.GetComponent<PlayerAttack>();
        }
    }

    private void Start()
    {
        roundManager = GameObject.FindAnyObjectByType<RoundManager>();
        playerScript = GameObject.FindObjectOfType<PlayerAttack>();
    }

    private bool deathCalled = false;

    // Gestione della morte del nemico
    protected override void OnDeath()
    {
        if (deathCalled) return; // ← blocca chiamate multiple
        deathCalled = true;

        // Se il nemico è il boss "wizard", notifica il RoundManager della sua sconfitta
        if (type == "wizard" && roundManager != null)
        {
            roundManager.OnBossDefeated();
        }
    }

    void Update()
    {
        // Aggiorna costantemente i riferimenti logici delle hitbox
        HurtBoxLogic = GetComponentInChildren<HurtBoxLogic>();
        colliderExtenderLogic = GetComponentInChildren<MovementColliderLogic>();

        // Interrompe l'aggiornamento se morto o in una cutscene
        if (isDead) return;
        if (roundManager.isCutscene) return;

        // Calcola la distanza dal player e aggiorna lo stato
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        currentState = distance <= chaseDistance ? State.Chase : State.Idle;

        switch (currentState)
        {
            case State.Chase:
                // Smette di inseguire se il player è già stato colpito/è in iframe
                if (playerScript.alreadyHurt) break;
                ChasePlayerWithOffset();
                break;
        }
    }

    private bool isRepositioning = false;
    private float repositionDir = 0f;

    // Logica principale di movimento e inseguimento
    void ChasePlayerWithOffset()
    {
        if (isDead) return;

        // Determina su quale lato del player posizionarsi
        float side = transform.position.x < playerTransform.position.x ? -1f : 1f;

        // Gira lo sprite in direzione del player se non si sta riposizionando
        if (!isRepositioning) sr.flipX = (playerTransform.position.x - transform.position.x) <= 0;

        // Adatta gli offset delle hitbox in base a dove sta guardando lo sprite
        if (!sr.flipX)
        {
            HurtBoxLogic.setLeftOffset();
            colliderExtenderLogic.setLeftOffset();
        }
        else
        {
            HurtBoxLogic.setRightOffset();
            colliderExtenderLogic.setRightOffset();
        }

        // Calcola la posizione esatta in cui il nemico vuole arrivare
        Vector2 targetPos = new Vector2(playerTransform.position.x + side * stopOffset, transform.position.y);
        float distToTarget = Vector2.Distance(transform.position, targetPos);
        float verticalDiff = playerTransform.position.y - transform.position.y;

        // LOGICA DI OSTACOLI/SALTO: Se il player è più in alto e il nemico è vicino al target
        if (verticalDiff > 0.1f && !isAttacking && (distToTarget <= stopTolerance || isRepositioning) && playerScript.getIsGrounded())
        {
            // Lancia un raggio verso l'alto per vedere se c'è un tetto/ostacolo
            Vector2 boxSize = new Vector2(0.15f, 0.1f);
            float rayLength = 2.0f;
            RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxSize, 0f, Vector2.up, rayLength, LayerMask.GetMask("Ground"));

            if (hit.collider != null)
            {
                // C'è un ostacolo sopra: cerca di riposizionarsi muovendosi orizzontalmente
                isRepositioning = true;
                if (repositionDir == 0f)
                    repositionDir = (transform.position.x < playerTransform.position.x) ? -1f : 1f;

                if (repositionDir == -1f) sr.flipX = true;
                else sr.flipX = false;
                transform.Translate(new Vector2(repositionDir, 0) * speed * Time.deltaTime);
                return;
            }
            else
            {
                // Nessun ostacolo: esegue un salto per raggiungere il player
                if (isRepositioning)
                    transform.Translate(new Vector2(repositionDir, 0) * speed * Time.deltaTime);

                isRepositioning = false;
                repositionDir = 0f;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                anim.SetTrigger("jump");
                return;
            }
        }
        // LOGICA BORDOPEDE/BURRONI: Se il player è in basso, evita di cadere (a meno che non sia il wizard)
        else if (verticalDiff < -0.1f && !isAttacking && (distToTarget <= stopTolerance || isRepositioning) && playerScript.getIsGrounded() && type != "wizard")
        {
            isRepositioning = true;
            Vector2 footCheckSize = new Vector2(0.4f, 0.12f);

            // Controlla se c'è terreno sotto i piedi
            RaycastHit2D groundCheck = Physics2D.BoxCast(
                transform.position + Vector3.down * 0.1f,
                footCheckSize, 0f, Vector2.down, 0.3f,
                LayerMask.GetMask("Ground")
            );

            bool hasGroundBelow = groundCheck.collider != null;
            if (!hasGroundBelow)
            {
                // Fermati sull'asse X per non cadere
                rb.velocity = new Vector2(0f, rb.velocity.y);
                return;
            }

            // Inizia a "passeggiare" (wander) casualmente lungo il bordo
            if (Mathf.Abs(wanderDirection) == 0f)
                wanderDirection = UnityEngine.Random.value < 0.5f ? -1f : 1f;

            transform.Translate(new Vector2(wanderDirection * speed * Time.deltaTime, 0f));

            if (wanderDirection != 0f)
                sr.flipX = wanderDirection < 0;
        }
        else
        {
            // Reset delle variabili di riposizionamento se condizioni normali
            isRepositioning = false;
            repositionDir = 0f;
            wanderDirection = 0f;
        }

        // Se è arrivato a destinazione (nel range del stopOffset)
        if (distToTarget <= stopTolerance && (verticalDiff <= 0.1f || type == "wizard") && (verticalDiff >= -0.1f || type == "wizard"))
        {
            if (isAttacking) return;
            // Si ferma e avvia l'animazione idle
            anim.Play("idle");
            anim.SetBool("move", false);
            moveable = false;
            inTouchPlayer = true;
            return;
        }
        else
        {
            inTouchPlayer = false;
            moveable = true;
        }

        // Movimento effettivo verso il bersaglio
        if (moveable && !isRepositioning)
        {
            anim.SetBool("idle", false);
            anim.SetBool("move", true);
            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            transform.Translate(dir * speed * Time.deltaTime);
        }
    }

    private Collider2D weaponCollider;

    // Gestione delle collisioni (entrate)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!playerScript.isAttacking)
        {
            int enemyLayerIndex = LayerMask.NameToLayer("enemyLayer");

            // Se l'oggetto è un'arma, disabilita momentaneamente la collisione col layer dei nemici
            if (collision.CompareTag("weapon"))
            {
                weaponCollider = collision.GetComponent<Collider2D>();
                if (weaponCollider != null)
                    weaponCollider.excludeLayers = (1 << enemyLayerIndex);
            }

            if (collision.gameObject.layer != enemyLayerIndex) return;
            // Rileva il contatto col player
            if (collision.CompareTag("Player")) inTouchPlayer = true;
        }
    }

    // Gestione delle collisioni (uscite)
    private void OnTriggerExit2D(Collider2D collision)
    {
        int enemyLayerIndex = LayerMask.NameToLayer("enemyLayer");
        if (collision.gameObject.layer != enemyLayerIndex) return;
        if (collision.CompareTag("Player")) inTouchPlayer = false;
    }

    private bool canJumpAttack = true; // Cooldown per l'attacco in salto

    // Metodo chiamato per scatenare un attacco
    public void attack()
    {
        // Attacchi per nemici standard o attacchi ravvicinati del boss
        if (inTouchPlayer && !isDead && !isAttacking && !isInHurt && !isBlocking)
        {
            isAttacking = true;

            // Se non è il wizard, o se il salto è in cooldown, fa attacchi corpo a corpo casuali
            if (type != "wizard" || !canJumpAttack)
            {
                int sceltaAttacco = UnityEngine.Random.Range(1, 3);
                if (sceltaAttacco == 1)
                {
                    damage = damageAttack[0];
                    attackDid = "attack1";
                    anim.SetTrigger("attack");
                }
                else
                {
                    damage = damageAttack[1];
                    attackDid = "attack2";
                    anim.SetTrigger("attack2");
                }
            }
            else
            {
                // Il wizard può anche usare un attacco in salto corpo a corpo
                int sceltaAttacco = UnityEngine.Random.Range(1, 4);
                if (sceltaAttacco == 1)
                {
                    // ... (stessa logica attack1 di sopra)
                    damage = damageAttack[0];
                    attackDid = "attack1";
                    anim.SetTrigger("attack");
                }
                else if (sceltaAttacco == 2)
                {
                    // ... (stessa logica attack2 di sopra)
                    damage = damageAttack[1];
                    attackDid = "attack2";
                    anim.SetTrigger("attack2");
                }
                else
                {
                    // Salto d'attacco speciale del boss
                    damage = damageAttack[1];
                    attackDid = "jumpAttackCasual";
                    anim.SetTrigger("jump");
                    jumpBossCasual();

                    // Gestione cooldown e reset di stato
                    canJumpAttack = false;
                    StartCoroutine(ResetJumpAttack());
                    StartCoroutine(ripristineIsAttacking(0.5f));
                    StartCoroutine(ripristineMoveable(0.5f));
                }
            }
            moveable = false;
        }
        // Attacco a distanza del boss (fuori dal range inTouchPlayer)
        else if (type == "wizard" && !isDead && !isAttacking && !isInHurt && !isBlocking && canJumpAttack)
        {
            isAttacking = true;
            int sceltaAttacco = UnityEngine.Random.Range(1, 3);

            if (sceltaAttacco == 1)
            {
                damage = damageAttack[1];
                attackDid = "jumpAttackCasual";
                anim.SetTrigger("jump");
                jumpBossCasual();
            }
            else
            {
                damage = damageAttack[1];
                attackDid = "jumpAttackToPlayer";
                anim.SetTrigger("jump");
                jumpBossToPlayer();
            }

            canJumpAttack = false;
            StartCoroutine(ResetJumpAttack());
            StartCoroutine(ripristineIsAttacking(0.5f));
            StartCoroutine(ripristineMoveable(0.5f));
            moveable = false;
        }
    }

    // Cooldown lungo (7 secondi) prima di poter fare un altro attacco in salto
    private IEnumerator ResetJumpAttack()
    {
        yield return new WaitForSeconds(7f);
        canJumpAttack = true;
    }

    // Forza fisica per un salto in una direzione casuale
    private void jumpBossCasual()
    {
        if (isDead) return;
        float directionX = UnityEngine.Random.value < 0.5f ? -1f : 1f;
        rb.velocity = new Vector2(directionX * speed * 3f, jumpForce * 3f);
    }

    // Forza fisica per un salto mirato verso il player
    private void jumpBossToPlayer()
    {
        if (isDead) return;
        float directionX = playerTransform.position.x > transform.position.x ? 1f : -1f;
        rb.velocity = new Vector2(directionX * speed * 3f, jumpForce * 2.5f);
    }

    // Chiamato solitamente come Animation Event per concludere i frame attivi di danno
    public IEnumerator finishAttack()
    {
        if (isDead || isInHurt) yield return 0;

        // Se a fine attacco sta ancora toccando il player, applica il danno
        if (inTouchPlayer) playerScript.takeHit(this, null);

        isAttacking = false;
        StartCoroutine(SetComboAfterDelay(false, 0.5f));
        StartCoroutine(ripristineMoveable(0.2f));
    }

    // Gestione della parata
    public IEnumerator Block()
    {
        anim.SetTrigger("block");
        moveable = false;
        isBlocking = true;
        yield return StartCoroutine(ripristineMoveable(0.2f));
        isBlocking = false;
    }

    // Utilità per riattivare il movimento dopo un delay
    private IEnumerator ripristineMoveable(float delay)
    {
        yield return new WaitForSeconds(delay);
        moveable = true;
    }

    // Utilità per resettare lo stato di attacco dopo un delay
    private IEnumerator ripristineIsAttacking(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
    }

    // Forzare la morte del nemico da script esterni
    public void setDie()
    {
        isDead = true;
        speed = 0;
    }

    public bool getIsAttacking() => isAttacking;

    // Timer per le combo
    IEnumerator SetComboAfterDelay(bool state, float delay)
    {
        yield return new WaitForSeconds(delay);
        setCombo(state);
    }

    IEnumerator waitToAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
    }

    public void setCombo(bool combo) { this.combo = combo; }

    public void Die() { anim.SetTrigger("die"); }
}