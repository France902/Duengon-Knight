using System;
using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Componenti e Script Esterni")]
    private Animator anim;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private WeaponLogic weaponLogic;
    private HurtBoxLogic hurtBoxLogic;
    private MovementColliderLogic movementColliderLogic;
    public HUDManager hudManager;
    public RoundManager roundManager;

    [Header("Stati del Giocatore")]
    public bool isAttacking = false;
    public bool IsAttacking => isAttacking; // Proprietà pubblica in sola lettura
    public bool alreadyHurt = false;        // Indica se il player sta subendo un colpo (animazione hit)
    public bool isDead = false;
    public bool isVictory = false;
    bool isGrounded = true;

    [Header("Statistiche")]
    public int damage = 1;
    public float health = 5f;
    public float maxHealth = 5f;

    [Header("Movimento")]
    float moveInput;
    public float speed = 5f;
    public float jumpForce = 7f;

    [Header("Combattimento")]
    public float attackRange = 1.2f;
    public bool immunity = false;           // Invincibilità temporanea (i-frames)
    public LayerMask enemyLayer;
    public float shutdownAttack1 = 0.3f;    // Cooldown attacco base
    public float shutdownAttack2 = 1f;      // Cooldown attacco pesante
    private string typeAttack;

    void Awake()
    {
        // Inizializza tutti i componenti necessari all'avvio
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        weaponLogic = GetComponentInChildren<WeaponLogic>();
        hurtBoxLogic = GetComponentInChildren<HurtBoxLogic>();
        movementColliderLogic = GetComponentInChildren<MovementColliderLogic>();
    }

    void Update()
    {
        // Se c'è una vittoria, blocca il giocatore
        if (isVictory)
        {
            rb.velocity = Vector2.zero;
            anim.SetBool("run", false);
            return;
        }

        // Blocca le azioni se c'è una cutscene in corso
        if (roundManager.isCutscene)
        {
            anim.SetBool("run", false);
            return;
        }

        // Se il giocatore è morto, assicurati che si fermi completamente
        if (isDead)
        {
            anim.SetBool("run", false);
            rb.velocity = Vector2.zero;
            return;
        }

        // Lettura degli input di movimento (Destra/Sinistra)
        moveInput = Input.GetAxisRaw("Horizontal");

        // Gestione della direzione dello sprite e degli offset delle hitbox associate
        if (moveInput > 0)
        {
            sr.flipX = false; // Guarda a destra
            weaponLogic.setRightOffset();
            hurtBoxLogic.setRightOffset();
            movementColliderLogic.setRightOffset();
        }
        else if (moveInput < 0)
        {
            sr.flipX = true; // Guarda a sinistra
            weaponLogic.setLeftOffset();
            hurtBoxLogic.setLeftOffset();
            movementColliderLogic.setLeftOffset();
        }

        // Applica il movimento orizzontale se non sta già subendo un colpo
        if (moveInput != 0 && !alreadyHurt && !isDead)
        {
            anim.SetBool("run", true);
            transform.Translate(Vector2.right * moveInput * speed * Time.deltaTime);
        }
        else
        {
            anim.SetBool("run", false);
        }

        // Gestione del salto
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isGrounded = false;
            anim.SetTrigger("jump");
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // INPUT: Attacco Leggero (Tasto Sinistro del Mouse)
        // Permette di attaccare se non si sta già attaccando, oppure per concatenare un attacco pesante
        if ((!isAttacking || typeAttack == "heavy_confirmed") && Input.GetMouseButtonDown(0))
        {
            isAttacking = true;
            ResetWeapon();
            attackRange = 0.15f;
            damage = 1;
            anim.SetTrigger("attack");
            typeAttack = "base";
        }

        // INPUT: Attacco Pesante (Tasto Destro del Mouse)
        if (!isAttacking && Input.GetMouseButtonDown(1) && hudManager.timerHeavy == 0)
        {
            isAttacking = true;
            ResetWeapon();
            attackRange = 0.26f;
            damage = 2;
            anim.SetTrigger("attack2");
            typeAttack = "heavy";
        }
    }

    public Transform attackPoint;
    public float attackAngle = 180f; // Angolo frontale in cui i colpi vanno a segno

    // Metodo solitamente chiamato tramite Animation Event nei frame di impatto dell'attacco
    public void DealDamage()
    {
        // Rileva tutti i nemici nell'area circolare attorno al player
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        foreach (var hit in hits)
        {
            // Ignora collider specifici (es. se stessi o trigger di estensione)
            if (hit.name != "Collider" && hit.name != "colliderExtender")
            {
                // Calcola la direzione verso il nemico
                Vector3 directionToEnemy = (hit.transform.position - transform.position).normalized;

                // Calcola l'angolo tra la direzione frontale del player (Right) e il nemico
                float angle = Vector3.Angle(transform.right, directionToEnemy);

                // Se guarda a destra
                if (!sr.flipX)
                {
                    // Controlla se il nemico è nell'arco visivo/di attacco frontale
                    if (angle < attackAngle / 2f)
                    {
                        var genericEnemy = hit.GetComponentInParent<EnemyAIGeneric>();
                        int sceltaBlocco = UnityEngine.Random.Range(1, 3); // 50% di probabilità di blocco per certi nemici

                        // Logica specifica per i nemici Skeleton: possono bloccare gli attacchi frontali
                        if (genericEnemy != null && genericEnemy.type == "skeleton" && !genericEnemy.getIsAttacking() && sceltaBlocco == 1)
                        {
                            StartCoroutine(genericEnemy.Block());
                        }
                        else
                        {
                            // Applica i danni in base al tipo di script del nemico
                            if (genericEnemy == null || genericEnemy.type != "skeleton" && genericEnemy.type != "goblin")
                                hit.GetComponentInParent<EnemySlime>()?.TakeDamage(damage);
                            else
                                genericEnemy.TakeDamage(damage);
                        }
                    }
                }
                // Se guarda a sinistra
                else
                {
                    // Se l'angolo è maggiore di 90 (quindi è dietro rispetto al Vector.right standard, ergo davanti al player flippato)
                    if (angle > attackAngle / 2f)
                    {
                        var genericEnemy = hit.GetComponentInParent<EnemyAIGeneric>();
                        int sceltaBlocco = UnityEngine.Random.Range(1, 3);

                        // Stessa logica di parata ma calcolata sul lato sinistro
                        if (genericEnemy != null && genericEnemy.type == "skeleton" && sceltaBlocco == 1)
                        {
                            StartCoroutine(genericEnemy.Block());
                        }
                        else
                        {
                            if (genericEnemy == null || (genericEnemy.type != "skeleton" && genericEnemy.type != "goblin"))
                                hit.GetComponentInParent<EnemySlime>()?.TakeDamage(damage);
                            else
                                genericEnemy.TakeDamage(damage);
                        }
                    }
                }
            }
        }
    }

    public void ResetWeapon()
    {
        if (weaponLogic != null)
            weaponLogic.SetEnemyExclusion(false);
    }

    // Richiamato solitamente alla fine dell'animazione d'attacco
    public void EndAction()
    {
        StartCoroutine(ExecuteAttackShutdown());
    }

    public void confirmHeavy()
    {
        typeAttack = "heavy_confirmed";
    }

    // Gestisce il tempo di riposo dopo un attacco
    public IEnumerator ExecuteAttackShutdown()
    {
        switch (typeAttack)
        {
            case "base":
                yield return new WaitForSeconds(shutdownAttack1);
                break;
            case "heavy":
                yield return new WaitForSeconds(shutdownAttack2);
                break;
        }

        isAttacking = false;

        // Si assicura di ripristinare il blocco collisioni arma/nemici
        if (weaponLogic == null)
            weaponLogic = GetComponentInChildren<WeaponLogic>(true);

        if (weaponLogic != null)
            weaponLogic.SetEnemyExclusion(true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Resetta la possibilità di saltare quando si tocca il suolo
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    public LayerMask enemyBodyLayer;

    // Rileva quando il corpo di un nemico entra nella hitbox del player
    public void OnTriggerEnter2D(Collider2D other)
    {
        int enemyLayerIndex = LayerMask.NameToLayer("enemyLayer");

        if (other.gameObject.layer != enemyLayerIndex) return;
        // Bitwise check: si assicura che il layer dell'oggetto tocato sia anche parte dell'enemyBodyLayer
        if (((1 << other.gameObject.layer) & enemyBodyLayer) == 0) return;

        // Se non sta attaccando o se non ha i-frames, prende danno dal tocco col nemico
        if (!alreadyHurt && !isAttacking)
        {
            EnemySlime enemy;

            if (other.CompareTag("enemySlime"))
                enemy = other.GetComponentInParent<EnemySlimeAI>();
            else
                enemy = other.GetComponentInParent<EnemyAIGeneric>();

            if (enemy != null)
                takeHit(enemy, other);
        }
    }

    // Applica i danni subiti dal player
    public void takeHit(EnemySlime enemy, Collider2D other)
    {
        if (!immunity) // Se non è invincibile
        {
            if (health > 0) health -= enemy.damage;

            // Check Morte
            if (health <= 0)
            {
                if (isDead) return;
                anim.Play("die", 0, 0f);
                isDead = true;
                return;
            }

            // Attiva le variabili per bloccare input e dare i-frames
            alreadyHurt = true;
            immunity = true;

            // Calcola la direzione del knockback in base alla posizione/rotazione del nemico
            bool enemyFlip = enemy.getFlipX();
            if (enemy.CompareTag("enemySlime")) moveInput = enemyFlip ? +1 : -1;
            else moveInput = enemyFlip ? -1 : +1;

            // Calcola dinamicamente la forza del knockback (più vicini = spinta più forte)
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            float knockbackForce = 0.01f / (distance + 0.005f);
            knockbackForce = Mathf.Clamp(knockbackForce, 0.1f, 0.5f); // Limita la spinta tra min e max

            anim.SetBool("takeHit", true);
            transform.Translate(Vector2.right * moveInput * knockbackForce);
        }
    }

    // Animation Event a fine animazione di danno subito
    public void endHurt()
    {
        anim.SetBool("takeHit", false);
        alreadyHurt = false;
    }

    public bool getAlreadyHurt() => alreadyHurt;

    // Avvia un timer per rimuovere l'invincibilità dopo un danno
    public void StartImmunityCooldown()
    {
        Invoke("RemoveImmunity", 0.25f);
    }

    private void RemoveImmunity()
    {
        immunity = false;
    }

    // Strumento utile per l'Editor: disegna un cerchio rosso per visualizzare il range d'attacco
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void Die()
    {
        Destroy(gameObject);
        anim.SetBool("die", false); // Un po' ridondante visto che l'oggetto viene distrutto, ma è un safe-check
    }

    public string getTypeAttack() => typeAttack;

    public bool getIsGrounded() => isGrounded;

    // Stub per un'interfaccia o metodo futuro (attualmente genera errore se richiamato!)
    internal void takeHit(EnemyAIGeneric enemyAIGeneric)
    {
        throw new NotImplementedException();
    }
}