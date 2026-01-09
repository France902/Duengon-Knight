using UnityEngine;
using TMPro; // Obbligatorio per TextMeshPro
using UnityEngine.UI; // Necessario per gestire il componente Image

public class HUDManager : MonoBehaviour
{
    [Header("Riferimenti")]
    public PlayerAttack playerScript; // Trascina qui il giocatore nell'ispettore

    [Header("UI Base (Slot 1)")]
    public TextMeshProUGUI textBase;
    public Image backgroundBase; // Trascina l'oggetto Image del quadrato base

    [Header("UI Heavy (Slot 2)")]
    public TextMeshProUGUI textHeavy;
    public Image backgroundHeavy;

    public float timerBase = 0f;
    public float timerHeavy = 0f;

    void Update()
    {
        // 1. Controllo se il player sta attaccando
        if (playerScript.isAttacking)
        {
            string type = playerScript.getTypeAttack();

            // 2. Switch per attivare il timer corretto
            switch (type)
            {
                case "base":
                    // Avvia il timer solo se non è già in corso
                    if (timerBase <= 0) timerBase = playerScript.shutdownAttack1;
                    break;

                case "heavy":
                    if (timerHeavy <= 0) timerHeavy = playerScript.shutdownAttack2;
                    break;
            }
        }

        // 3. Gestione del Countdown e visualizzazione
        UpdateCooldown(ref timerBase, textBase, backgroundBase);
        UpdateCooldown(ref timerHeavy, textHeavy, backgroundHeavy);
    }

    private void UpdateCooldown(ref float timer, TextMeshProUGUI UItext, Image bgImage)
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;

            // "f1" serve per mostrare solo un decimale (es: 1.5)
            UItext.text = timer.ToString("f1");

            // Opzionale: cambia colore o opacità quando attivo
            UItext.alpha = 1f;

            // Colore Rosso durante il cooldown
            bgImage.color = Color.red;
        }
        else
        {
            timer = 0;
            UItext.text = ""; // O lascia vuoto ""
            UItext.alpha = 0.5f;

            // Colore Verde quando è pronto
            bgImage.color = Color.green;
        }
    }
}