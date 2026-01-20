using System;
using UnityEngine;

public class EnemySlime : MonoBehaviour
{
    protected Animator anim;
    protected SpriteRenderer sr;
    protected bool isDead = false;
    bool playerInRange;
    bool isInHurt;

    private int hp = 2;

    protected virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    public void TakeDamage(int dmg)
    {
        
        if (isDead || isInHurt) return;
        Debug.Log(this.tag);
        isInHurt = true;
        hp -= dmg;

        if (hp <= 0)
        {
            hp = 0;
            isDead = true;
            anim.SetTrigger("die");
        }
        else
        {
            anim.SetTrigger("hurt");
        }
    }

    public bool getFlipX()
    {
        return sr.flipX;
    }

    public void EndHurt()
    {
        if (!isDead)
            isInHurt = false;
    }


    // Animation Event alla fine della Death
    public void DestroySelf()
    {
        Destroy(gameObject);
    }

}
