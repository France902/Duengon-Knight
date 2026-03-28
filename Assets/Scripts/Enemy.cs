using System;
using UnityEngine;

public class EnemySlime : MonoBehaviour
{
    protected Animator anim;
    protected SpriteRenderer sr;
    protected bool isDead = false;
    bool playerInRange;
    protected bool isInHurt;

    public float hp;
    public float MaxHp;
    public int damage;

    protected virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        MaxHp = hp;
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
        
        isInHurt = true;
        hp -= dmg;

        if (hp <= 0)
        {
            hp = 0;
            isDead = true;
            anim.SetBool("die", true);
            Debug.Log("[SLIME] Chiamo OnDeath, tipo oggetto: " + this.GetType().Name + ", GetType completo: " + this.GetType().FullName);
            OnDeath();
        }
         else
        {
            anim.SetTrigger("hurt");
        }
    }

    protected virtual void OnDeath() { }

    public bool getFlipX()
    {
        return sr.flipX;
    }

    public void EndHurt()
    {
        if (!isDead)
            isInHurt = false;
    }

    public bool getDie()
    {
        return isDead;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}