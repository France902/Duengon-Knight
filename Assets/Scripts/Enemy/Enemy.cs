using System;
using UnityEngine;

// Classe base per i nemici: gestisce salute, danni ricevuti e stato generale
public class EnemySlime : MonoBehaviour
{
    [Header("Componenti")]
    protected Animator anim;
    protected SpriteRenderer sr;

    [Header("Stati del Nemico")]
    protected bool isDead = false;
    bool playerInRange;       // Vero se il player è dentro l'area di trigger
    protected bool isInHurt;  // Vero se il nemico sta subendo un colpo (i-frames/animazione)

    [Header("Statistiche")]
    public float hp;          // Salute attuale
    public float MaxHp;       // Salute massima
    public int damage;        // Danno base inflitto al giocatore

    // Metodo virtuale, così le classi figlie (come EnemySlimeAI) possono fare override e aggiungere logica
    protected virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        MaxHp = hp; // Imposta la salute massima in base agli hp iniziali
    }

    // Rileva quando il player entra nel raggio d'azione del trigger base
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    // Rileva quando il player esce dal raggio d'azione del trigger base
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    // Metodo universale per far subire danni al nemico
    public void TakeDamage(int dmg)
    {
        // Se è già morto o sta già subendo un colpo, ignora il nuovo danno
        if (isDead || isInHurt) return;

        isInHurt = true; // Blocca ulteriori danni/movimenti temporaneamente
        hp -= dmg;       // Sottrae i danni alla salute attuale

        // Controllo della morte
        if (hp <= 0)
        {
            hp = 0;
            isDead = true;
            anim.SetBool("die", true); // Avvia l'animazione di morte
            OnDeath();                 // Richiama eventuali eventi di morte specifici (es. dire al manager che il boss è morto)
        }
        else
        {
            // Se sopravvive, fa partire l'animazione in cui subisce il colpo
            anim.SetTrigger("hurt");
        }
    }

    // Metodo vuoto che può essere sovrascritto (override) dalle classi figlie per aggiungere logica personalizzata alla morte
    protected virtual void OnDeath() { }

    // Ritorna la direzione in cui sta guardando lo sprite (utile per l'IA)
    public bool getFlipX()
    {
        return sr.flipX;
    }

    // Chiamato solitamente tramite "Animation Event" alla fine dell'animazione "hurt" per sbloccare il nemico
    public void EndHurt()
    {
        if (!isDead)
            isInHurt = false;
    }

    // Ritorna se il nemico è morto (utile per controlli esterni)
    public bool getDie()
    {
        return isDead;
    }

    // Elimina definitivamente il GameObject dalla scena (solitamente chiamato tramite Animation Event a fine morte)
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}