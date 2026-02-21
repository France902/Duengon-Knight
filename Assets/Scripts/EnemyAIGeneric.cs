using System;
using System.Collections;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAIGeneric : EnemySlime
{
    public float speed = 2f;
    public int[] damageAttack = {0, 0};
    public float chaseDistance = 4f;
    public float jumpForce = 1f;

    [Header("Chase Offset")]
    public float stopOffset = 0.6f;        
    public float stopTolerance = 0.05f;    
    public string type;

    public PlayerAttack playerScript;
    private Transform playerTransform;
    private RoundManager roundManager;
    private Rigidbody2D rb;
    private HurtBoxLogic HurtBoxLogic;
    private MovementColliderLogic colliderExtenderLogic;
    public BoxCollider2D colliderExtender;
    private bool inTouchPlayer = false;
    private bool isAttacking = false;
    private bool isBlocking = false;
    private Boolean moveable = true;
    private bool combo = false;
    private string attackDid = "";
    private float wanderDirection = 0;


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
            
            playerScript = playerObj.GetComponent<PlayerAttack>();
        }
    }

    void Update()
    {
        HurtBoxLogic = GetComponentInChildren<HurtBoxLogic>();
        colliderExtenderLogic = GetComponentInChildren<MovementColliderLogic>();
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

        float side = transform.position.x < playerTransform.position.x ? -1f : 1f;
        if(!isRepositioning) sr.flipX = (playerTransform.position.x - transform.position.x) <= 0;
        if(!sr.flipX)
        {
            HurtBoxLogic.setLeftOffset();
            colliderExtenderLogic.setLeftOffset();
        }
        else
        {
            HurtBoxLogic.setRightOffset();
            colliderExtenderLogic.setRightOffset();
        }

            Vector2 targetPos = new Vector2(playerTransform.position.x + side * stopOffset, transform.position.y);
        float distToTarget = Vector2.Distance(transform.position, targetPos);
        float verticalDiff = playerTransform.position.y - transform.position.y;
        

        if (verticalDiff > 0.1f && !isAttacking && (distToTarget <= stopTolerance || isRepositioning) && playerScript.getIsGrounded())
        {
            Vector2 boxSize = new Vector2(0.15f, 0.1f);
            float rayLength = 2.0f;

            RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxSize, 0f, Vector2.up, rayLength, LayerMask.GetMask("Ground"));

            if (hit.collider != null)
            {
                isRepositioning = true;
                if (repositionDir == 0f)
                    repositionDir = (transform.position.x < playerTransform.position.x) ? -1f : 1f;

                if(repositionDir == -1f) sr.flipX = true;
                else sr.flipX = false;
                transform.Translate(new Vector2(repositionDir, 0) * speed * Time.deltaTime);
                return;
            }
            else
            {
                if (isRepositioning)
                {
                    transform.Translate(new Vector2(repositionDir, 0) * speed * Time.deltaTime);
                }

                isRepositioning = false;
                repositionDir = 0f;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                anim.SetTrigger("jump"); 
                return;
            }
        }
        else if (verticalDiff < -0.1f && !isAttacking && (distToTarget <= stopTolerance || isRepositioning) && playerScript.getIsGrounded() && type != "wizard")
        {
            Debug.Log(wanderDirection);
            isRepositioning = true;
            Vector2 footCheckSize = new Vector2(0.4f, 0.12f);     
            RaycastHit2D groundCheck = Physics2D.BoxCast(
                transform.position + Vector3.down * 0.1f,
                footCheckSize,
                0f,
                Vector2.down,
                0.3f,
                LayerMask.GetMask("Ground")
            );

            bool hasGroundBelow = groundCheck.collider != null;

            if (!hasGroundBelow)
            {
                // Siamo già a un bordo / stiamo per cadere → fermiamoci un attimo e rivalutiamo dopo
                rb.velocity = new Vector2(0f, rb.velocity.y);
                // Qui potresti anche far partire un'animazione "guardo il vuoto" se ce l'hai
                // Oppure aspettare che cada naturalmente
                return;
            }

          
            if (Mathf.Abs(wanderDirection) == 0f)   // non abbiamo ancora una direzione attiva
            {
                // Scelta casuale: 50% sinistra, 50% destra
                wanderDirection = UnityEngine.Random.value < 0.5f ? -1f : 1f;
                
            }

            // Applichiamo il movimento
            float horizontalSpeed = wanderDirection * speed * Time.deltaTime;
            transform.Translate(new Vector2(horizontalSpeed, 0f));

            // Flip dello sprite (classico)
            if (wanderDirection != 0f)
            {
                sr.flipX = wanderDirection < 0;
            }

        }
        else
        {
            isRepositioning = false;
            repositionDir = 0f;
            wanderDirection = 0f;   
        }

        Debug.Log(verticalDiff <= 0.1f || type == "wizard");

        if (distToTarget <= stopTolerance && (verticalDiff <= 0.1f || type == "wizard") && (verticalDiff >= -0.1f || type == "wizard"))
        {
            Debug.Log("Entrato nell'intouch");
            if (isAttacking) return;
            Debug.Log("confermato");
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
        if (moveable && !isRepositioning)
        {
            anim.SetBool("idle", false);
            anim.SetBool("move", true);
            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            transform.Translate(dir * speed * Time.deltaTime);
        }
    }
    private Collider2D weaponCollider; 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if(!playerScript.isAttacking)
        {
            int enemyLayerIndex = LayerMask.NameToLayer("enemyLayer");

            if (collision.CompareTag("weapon"))
            {
                weaponCollider = collision.GetComponent<Collider2D>();
                if (weaponCollider != null)
                {
                    
                    weaponCollider.excludeLayers = (1 << enemyLayerIndex);
                }
            }

            if (collision.gameObject.layer != enemyLayerIndex)
            {
                return;
            }
            if (collision.CompareTag("Player"))
            {
                inTouchPlayer = true;
            }
        }
        
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        int enemyLayerIndex = LayerMask.NameToLayer("enemyLayer");

        if (collision.gameObject.layer != enemyLayerIndex)
        {
            return; 
        }

        if (collision.CompareTag("Player"))
        {
            inTouchPlayer = false;
        }

    }

    public void attack()
    {
        if (inTouchPlayer && !isDead && !isAttacking && !isInHurt && !isBlocking)
        {
            isAttacking = true;
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

            moveable = false;
        }
    }

    public IEnumerator finishAttack()
    {

        if (isDead || isInHurt) yield return 0;
        if (inTouchPlayer)
        {

            playerScript.takeHit(this, null);
        }

            isAttacking = false;
            StartCoroutine(SetComboAfterDelay(false, 0.5f));
            StartCoroutine(ripristineMoveable(0.2f));
 
        
    }

    public IEnumerator Block()
    {

        anim.SetTrigger("block");
        moveable = false;
        isBlocking = true;

        yield return StartCoroutine(ripristineMoveable(0.2f));
        Debug.Log(moveable);
        isBlocking = false;
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

    public bool getIsAttacking()
    {
        return isAttacking;
    }

    IEnumerator SetComboAfterDelay(bool state, float delay)
    {
        yield return new WaitForSeconds(delay);
        setCombo(state);
    }

    IEnumerator waitToAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
    }

    public void setCombo(bool combo)
    {
        this.combo = combo;
    }

    public void Die()
    {
        anim.SetTrigger("die");
    }

}
