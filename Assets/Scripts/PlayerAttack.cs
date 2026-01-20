using System;
using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private WeaponLogic weaponLogic;
    private HurtBoxLogic hurtBoxLogic;
    private MovementColliderLogic movementColliderLogic;
    public HUDManager hudManager;
    public RoundManager roundManager;

    public bool isAttacking = false;
    public bool IsAttacking => isAttacking;

    public int damage = 1;
    public float health = 5f;
    public float maxHealth = 5f;

    bool isGrounded = true;

    public bool alreadyHurt = false;

    public bool isDead  = false;

    float moveInput;

    public float speed = 5f;
    public float jumpForce = 7f;
    public float attackRange = 1.2f;
    public bool immunity = false;
    public LayerMask enemyLayer;

    public float shutdownAttack1 = 0.3f;
    public float shutdownAttack2 = 1f;

    private string typeAttack;

    void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        weaponLogic = GetComponentInChildren<WeaponLogic>();
        hurtBoxLogic = GetComponentInChildren<HurtBoxLogic>();
        movementColliderLogic = GetComponentInChildren<MovementColliderLogic>();
    }

    void Update()
    {
        if (roundManager.isCutscene)
        {
            anim.SetBool("run", false);
            return;
        }

        if (isDead) return;

        // INPUT MOVIMENTO
        moveInput = Input.GetAxisRaw("Horizontal");

        // FLIP SPRITE
        if (moveInput > 0)
        {
            sr.flipX = false;
            weaponLogic.setRightOffset();
            hurtBoxLogic.setRightOffset();
            movementColliderLogic.setRightOffset();
        }
        else if (moveInput < 0)
        {
            sr.flipX = true;
            weaponLogic.setLeftOffset();
            hurtBoxLogic.setLeftOffset();
            movementColliderLogic.setLeftOffset();
        }
            

        // MOVIMENTO (SEMPRE)
        if (moveInput != 0 && !alreadyHurt && !isDead)
        {
            anim.SetBool("run", true);
            transform.Translate(Vector2.right * moveInput * speed * Time.deltaTime);
        }
        else
        {
            anim.SetBool("run", false);
        }

        // JUMP
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isGrounded = false;
            anim.SetTrigger("jump");
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // ATTACK 1
        if ((!isAttacking || typeAttack == "heavy_confirmed") && Input.GetMouseButtonDown(0))
        {
            isAttacking = true;
            ResetWeapon();
            attackRange = 0.15f;
            damage = 1;
            anim.SetTrigger("attack");
            typeAttack = "base";
        }

        // ATTACK 2
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

    public float attackAngle = 180f; // 180 gradi è un semicerchio perfetto

    public void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        foreach (var hit in hits)
        {

            // 1. Calcola la direzione verso il nemico
            Vector3 directionToEnemy = (hit.transform.position - transform.position).normalized;

            // 2. Calcola l'angolo tra la direzione in cui guarda il player (transform.right o transform.up) e il nemico
            // Se il tuo player guarda a destra, usa transform.right
            float angle = Vector3.Angle(transform.right, directionToEnemy);


            // 3. Se l'angolo è entro la metà del nostro raggio d'azione (es. 90 gradi a sx e 90 a dx)
            if(!sr.flipX)
            {
                if (angle < attackAngle / 2f)
                {
                    
                    hit.GetComponentInParent<EnemySlime>()?.TakeDamage(damage);
                }
            }
            else
            {
                if (angle > attackAngle / 2f)
                {
                    hit.GetComponentInParent<EnemySlime>()?.TakeDamage(damage);
                }
            }
            
        }
    }

    public void ResetWeapon()
    {
        if (weaponLogic != null)
        {
            weaponLogic.SetEnemyExclusion(false);
        }
    }

    public void EndAction()
    {
        StartCoroutine(ExecuteAttackShutdown());
    }

    public void confirmHeavy()
    {

        typeAttack = "heavy_confirmed";
    }

    public IEnumerator ExecuteAttackShutdown()
    {
        // Aspetta per il numero di secondi specificato in shutdownAttack
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

        // Se è null, prova a cercarlo un'ultima volta
        if (weaponLogic == null)
        {
            weaponLogic = GetComponentInChildren<WeaponLogic>(true);
        }

        if (weaponLogic != null)
        {
            // Imposta l'esclusione (true per escludere i nemici dal colpo)
            weaponLogic.SetEnemyExclusion(true);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    public LayerMask enemyBodyLayer;

    public void OnTriggerEnter2D(Collider2D other)
    {
        int enemyLayerIndex = LayerMask.NameToLayer("enemyLayer");

        if (other.gameObject.layer != enemyLayerIndex)
            return;

        if (((1 << other.gameObject.layer) & enemyBodyLayer) == 0)
            return;

        if (!alreadyHurt && !isAttacking)
        {
            // Usiamo una variabile generica che può ospitare entrambi i tipi
            // Sostituisci "EnemyBase" con il nome della classe da cui ereditano entrambi
            EnemySlime enemy;

            if (other.CompareTag("enemySlime"))
            {
                enemy = other.GetComponentInParent<EnemySlimeAI>();
            }
            else
            {
                enemy = other.GetComponentInParent<EnemyAIGeneric>();
            }

            // Se l'oggetto ha uno dei due componenti ed è vivo
            if (enemy != null)
            {
                takeHit(enemy, other);
            }
        }
    }


    public void takeHit(EnemySlime enemy, Collider2D other)
    {
        if(!immunity)
        {
            if (health > 0) health--;
            if(health == 0) {
                if (isDead) return;
                anim.Play("die", 0, 0f);
                isDead = true;
                return;
            }

            alreadyHurt = true;
            immunity = true;

            // 1. Calcola la direzione (già fatto col tuo flip)
            bool enemyFlip = enemy.getFlipX();
            if(other != null && other.CompareTag("enemySlime")) moveInput = enemyFlip ? 1 : -1;
            else moveInput = enemyFlip ? -1 : +1;

            // 2. Calcola la distanza tra Player e Slime
            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            // 3. Applica la formula del Knockback dinamico
            float knockbackForce = 0.01f / (distance + 0.005f);

            // Limita la forza massima per evitare "salti" eccessivi se sono vicinissimi
            knockbackForce = Mathf.Clamp(knockbackForce, 0.1f, 0.5f);

            anim.SetBool("takeHit", true);


            transform.Translate(Vector2.right * moveInput * knockbackForce);
        }
        
    }

    public void endHurt()
    {
        anim.SetBool("takeHit", false);
        alreadyHurt = false;
    }

    public bool getAlreadyHurt()
    {
        return alreadyHurt;
    }

    public void StartImmunityCooldown()
    {
        // Chiama la funzione 'RemoveImmunity' dopo 0.5 secondi
        Invoke("RemoveImmunity", 0.5f);
    }

    private void RemoveImmunity()
    {
        immunity = false;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        // Se usi attackPoint.position, usa quella qui sopra
    }

    public void Die()
    {
        
        Destroy(gameObject);
        anim.SetBool("die", false);
        
    }

    public string getTypeAttack()
    {
        return typeAttack;
    }

    internal void takeHit(EnemyAIGeneric enemyAIGeneric)
    {
        throw new NotImplementedException();
    }
}



