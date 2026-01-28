using System;
using System.Collections;
using UnityEngine;

public class EnemyAIGeneric : EnemySlime
{
    public float speed = 2f;
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

       
        float side = transform.position.x < playerTransform.position.x ? -1f : 1f;
        sr.flipX = (playerTransform.position.x - transform.position.x) <= 0;

        Vector2 targetPos = new Vector2(playerTransform.position.x + side * stopOffset, transform.position.y);
        float distToTarget = Vector2.Distance(transform.position, targetPos);
        float verticalDiff = playerTransform.position.y - transform.position.y;
        float verticalDistAbs = Mathf.Abs(verticalDiff);

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
        else
        {
            isRepositioning = false;
            repositionDir = 0f;
        }

        if (distToTarget <= stopTolerance && verticalDistAbs <= 0.1f)
        {
            if (isAttacking) return;
            anim.Play("idle");
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
        
        if (inTouchPlayer && !isDead && !isAttacking && !isInHurt)
        {
            isAttacking = true;
            int sceltaAttacco = UnityEngine.Random.Range(1, 3);

            if (sceltaAttacco == 1)
            {
                damage = 1;
                anim.SetTrigger("attack");
            }
            else
            {
                damage = 2;
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
