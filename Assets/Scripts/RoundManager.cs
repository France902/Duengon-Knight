using System;
using System.Collections;
using UnityEngine;
using TMPro; // Necessario per TextMeshPro

public class RoundManager : MonoBehaviour
{
    public bool isCutscene = false;
    public int roundFought = 0;

    [Header("UI Settings")]
    public TextMeshProUGUI roundText; // Trascina qui l'oggetto testo dall'Inspector
    public float fadeDuration = 1.0f; // Durata della transizione
    public float displayDuration = 2.0f; // Quanto tempo resta visibile

    public void setIsCutscene(bool isCutscene)
    {
        this.isCutscene = isCutscene;
        if (isCutscene) StartCoroutine(ShowRoundSequence());
    }

    // Coroutine che gestisce l'intera sequenza
    private IEnumerator ShowRoundSequence()
    {
        roundText.text = (roundFought + 1) + "° ONDATA";
        roundFought++;
        // 1. Fade In
        yield return StartCoroutine(FadeText(0, 1));

        // 2. Attesa
        yield return new WaitForSeconds(displayDuration);

        // 3. Fade Out
        yield return StartCoroutine(FadeText(1, 0));

        isCutscene = false; // Fine della sequenza
    }

    // Metodo generico per cambiare l'opacità
    private IEnumerator FadeText(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = roundText.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            roundText.color = new Color(color.r, color.g, color.b, newAlpha);
            yield return null;
        }

        roundText.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}