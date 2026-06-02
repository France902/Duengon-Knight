using System;
using System.Collections;
using UnityEngine;
using TMPro; // Necessario per utilizzare TextMeshPro (Testi UI avanzati)

// Gestisce i testi a schermo per le ondate, l'inizio delle boss fight e la vittoria
public class RoundManager : MonoBehaviour
{
    [Header("Stati del Gioco")]
    public bool isCutscene = false; // Se true, blocca il player e lo spawn dei nemici
    public int roundFought = 0;     // Contatore delle ondate affrontate

    [Header("Impostazioni UI")]
    public TextMeshProUGUI roundText; // Il testo a schermo (es. "1° ONDATA", "BOSS FIGHT")
    public float fadeDuration = 1.0f; // Quanto tempo ci mette il testo ad apparire/scomparire
    public float displayDuration = 2.0f; // Quanto tempo il testo rimane visibile e opaco
    public HUDManager HUD;
    public HealthBarUIBoss healthBarBoss;

    private bool victoryTriggered = false; // Evita che la sequenza di vittoria venga chiamata più volte

    [Header("Audio")]
    public AudioClip victoryMusic;
    private AudioSource audioSource;

    private void Start()
    {
        // Recupera il componente AudioSource per riprodurre la musica di vittoria
        audioSource = GetComponent<AudioSource>();
    }

    // Metodo chiamato dai WaveManager per iniziare una transizione
    public void setIsCutscene(bool isCutscene, bool isBossFight)
    {
        this.isCutscene = isCutscene;

        // Avvia la coroutine corretta in base al tipo di combattimento
        if (isCutscene && isBossFight) StartCoroutine(ShowRoundSequence(true));
        else if (isCutscene) StartCoroutine(ShowRoundSequence(false));
    }

    // Metodo da chiamare (probabilmente dallo script del boss) quando il boss muore
    public void OnBossDefeated()
    {
        if (victoryTriggered) return; // Sicurezza: se è già in vittoria, ignora
        victoryTriggered = true;

        // Ferma eventuali testi di ondate o altre coroutine in corso
        StopAllCoroutines();

        // Avvia la sequenza finale
        StartCoroutine(ShowVictorySequence());
    }

    // Gestisce tutto ciò che succede quando si vince
    private IEnumerator ShowVictorySequence()
    {
        yield return new WaitForSeconds(1f); // Piccola pausa drammatica prima della vittoria

        // Fa partire la musica di vittoria
        audioSource.clip = victoryMusic;
        audioSource.Play();

        // Cerca il player nella scena. 
        // TIP: FindObjectOfType<PlayerAttack>() restituisce GIÀ il componente PlayerAttack.
        PlayerAttack player = GameObject.FindObjectOfType<PlayerAttack>();

        if (player != null)
        {
            // Questa riga è ridondante, perché "player" è già di tipo PlayerAttack!
            // Basterebbe fare direttamente: player.isVictory = true;
            PlayerAttack playerAttack = player.GetComponent<PlayerAttack>();

            if (playerAttack != null)
            {
                playerAttack.isVictory = true; // Blocca i movimenti del player
            }
        }

        // Imposta il testo su "VITTORIA" e lo rende inizialmente trasparente (alpha = 0)
        roundText.gameObject.SetActive(true);
        roundText.text = "VICTORY";
        roundText.color = new Color(roundText.color.r, roundText.color.g, roundText.color.b, 0f);

        // Fa apparire il testo gradualmente
        yield return StartCoroutine(FadeText(0f, 1f));

        // Lascia la scritta a schermo per 20 secondi (tempo per godersi la vittoria o far finire la musica)
        yield return new WaitForSeconds(20f);

        // Fa scomparire il testo gradualmente
        yield return StartCoroutine(FadeText(1f, 0f));

        roundText.gameObject.SetActive(false);
    }

    // Gestisce l'apparizione dei testi "X° ONDATA" o "BOSS FIGHT"
    private IEnumerator ShowRoundSequence(bool isBossFight)
    {
        roundText.gameObject.SetActive(true);

        // Imposta il testo corretto
        if (!isBossFight) roundText.text = (roundFought + 1) + "° wave";
        else
        {
            roundText.text = "BOSS FIGHT";
            HUD.activeBackgroundBoss(true); // Attiva eventuali sfondi o UI specifici per il boss
        }

        roundFought++; // Incrementa il contatore delle ondate

        // Rende il testo trasparente prima di iniziare il fade-in
        roundText.color = new Color(roundText.color.r, roundText.color.g, roundText.color.b, 0f);

        yield return StartCoroutine(FadeText(0f, 1f)); // Fade In (appare)
        yield return new WaitForSeconds(displayDuration); // Attende il tempo di lettura
        yield return StartCoroutine(FadeText(1f, 0f)); // Fade Out (scompare)

        roundText.gameObject.SetActive(false); // Disattiva l'oggetto UI per pulizia

        // Sblocca il gioco (i WaveManager ricominceranno a funzionare)
        isCutscene = false;

        // Se è una boss fight, aspetta ancora un po' e poi fa apparire la barra della vita del boss
        if (isBossFight)
        {
            yield return new WaitForSeconds(2f);
            healthBarBoss.setBoss();
        }
    }

    // Coroutine riutilizzabile per animare la trasparenza (Canale Alpha) del testo
    private IEnumerator FadeText(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = roundText.color;

        // Cicla finché il tempo trascorso non raggiunge la durata desiderata (fadeDuration)
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime; // Aggiunge il tempo passato dall'ultimo frame

            // Calcola il nuovo valore di alpha interpolando (Mathf.Lerp) tra il valore iniziale e finale
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            roundText.color = new Color(color.r, color.g, color.b, newAlpha);

            yield return null; // Aspetta il frame successivo prima di continuare il loop
        }

        // Assicurati che l'alpha arrivi esattamente al valore finale desiderato alla fine del ciclo
        roundText.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}