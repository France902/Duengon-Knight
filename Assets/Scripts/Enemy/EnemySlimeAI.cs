using Unity.VisualScripting;
using UnityEngine;

// Classe per la gestione dell'IA dello Slime nemico, eredita da EnemySlime
public class EnemySlimeAI : EnemySlime
{
    [Header("Statistiche Base")]
    public float speed = 2f;               // Velocità di movimento dello slime
    public float chaseDistance = 4f;       // Distanza massima per iniziare a seguire il player
    public string type = "slime";          // Identificatore del tipo di nemico

    [Header("Chase Offset")]
    public float stopOffset = 0.6f;        // Distanza che lo slime mantiene dal giocatore quando si ferma
    public float stopTolerance = 0.05f;    // Margine di errore accettato per considerarsi "arrivato"

    [Header("Riferimenti")]
    public PlayerAttack playerScript;
    private Transform playerTransform;
    private RoundManager roundManager;
    private bool inTouchPlayer = false;    // Vero se lo slime è a contatto con il giocatore

    // Macchina a stati semplificata
    private enum State { Idle, Chase }
    private State currentState = State.Idle;

    private void Start()
    {
        // Trova i manager e gli script necessari all'avvio
        roundManager = GameObject.FindAnyObjectByType<RoundManager>();
        playerScript = GameObject.FindObjectOfType<PlayerAttack>();
    }

    protected override void Awake()
    {
        base.Awake(); // Richiama l'Awake della classe padre (EnemySlime)

        // Cerca il player nella scena per salvarne la posizione (Transform) e lo script
        PlayerAttack playerObj = GameObject.FindObjectOfType<PlayerAttack>();

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerScript = playerObj.GetComponent<PlayerAttack>();
        }
    }

    void Update()
    {
        // Se lo slime è morto o c'è un filmato in corso, interrompi l'aggiornamento logico
        if (isDead) return;
        if (roundManager.isCutscene) return;

        // Calcola la distanza tra lo slime e il giocatore
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // Se il giocatore è entro il raggio d'azione, passa allo stato Chase (insegui), altrimenti Idle (fermo)
        currentState = distance <= chaseDistance ? State.Chase : State.Idle;

        switch (currentState)
        {
            case State.Chase:
                // Se il giocatore è già stato colpito di recente (i-frames), smette di inseguire
                if (playerScript.alreadyHurt) break;
                ChasePlayerWithOffset();
                break;
        }
    }

    void ChasePlayerWithOffset()
    {
        if (isDead) return;

        // Determina su quale lato del player deve fermarsi (-1 a sinistra, 1 a destra)
        float side = transform.position.x < playerTransform.position.x ? -1f : 1f;

        // Gira lo sprite dello slime verso il giocatore
        sr.flipX = (playerTransform.position.x - transform.position.x) >= 0;

        // Calcola la posizione bersaglio tenendo conto del stopOffset
        Vector2 targetPos = new Vector2(
            playerTransform.position.x + side * stopOffset,
            transform.position.y
        );

        float distToTarget = Vector2.Distance(transform.position, targetPos);
        float verticalDist = Mathf.Abs(transform.position.y - playerTransform.position.y);

        // Se lo slime ha raggiunto la posizione bersaglio (entro la tolleranza) ed è allineato verticalmente
        if (distToTarget <= stopTolerance && verticalDist <= 0.1f)
        {
            if (!isDead) inTouchPlayer = true;
            return; // Ferma il movimento
        }
        else
        {
            inTouchPlayer = false; // Non è ancora a distanza di attacco
        }

        // Calcola la direzione verso il bersaglio e muovi lo slime
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime);
    }

    private Collider2D weaponCollider;

    // Gestione delle collisioni (quando qualcosa entra nel trigger dello slime)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Logica per ignorare l'arma del player se questo non sta attaccando (evita spinte indesiderate)
        if (!playerScript.isAttacking)
        {
            int enemyLayerIndex = LayerMask.NameToLayer("enemyLayer");

            if (collision.CompareTag("weapon"))
            {
                weaponCollider = collision.GetComponent<Collider2D>();
                if (weaponCollider != null)
                {
                    // Esclude il layer dei nemici dalle collisioni dell'arma
                    weaponCollider.excludeLayers = (1 << enemyLayerIndex);
                }
            }

            // Ignora le collisioni con oggetti che non fanno parte del layer nemico
            if (collision.gameObject.layer != enemyLayerIndex)
            {
                return;
            }

            // Rileva il contatto con il giocatore
            if (collision.CompareTag("Player") && !isDead)
            {
                inTouchPlayer = true;
            }
        }
    }

    // Gestione delle collisioni (quando qualcosa esce dal trigger dello slime)
    private void OnTriggerExit2D(Collider2D collision)
    {
        int enemyLayerIndex = LayerMask.NameToLayer("enemyLayer");

        if (collision.gameObject.layer != enemyLayerIndex)
        {
            return;
        }

        // Se il giocatore si allontana, aggiorna la variabile
        if (collision.CompareTag("Player") && !isDead)
        {
            inTouchPlayer = false;
        }
    }

    // Esegue l'attacco al giocatore
    public void attack()
    {
        // Applica i danni al giocatore se è a contatto ed lo slime è vivo
        if (inTouchPlayer && !isDead)
        {
            playerScript.takeHit(this, null); // Passa 'this' per indicare chi ha fatto il danno
        }
    }

    // Imposta lo stato di morte istantanea (es. azzera la velocità)
    public void setDie()
    {
        isDead = true;
        speed = 0;
    }

    // Richiama l'animazione di morte
    public void Die()
    {
        anim.SetTrigger("die");
    }
}