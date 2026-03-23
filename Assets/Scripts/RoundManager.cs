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
    public HUDManager HUD;
    public HealthBarUIBoss healthBarBoss;

    public void setIsCutscene(bool isCutscene, bool isBossFight)
    {
        this.isCutscene = isCutscene;
        if (isCutscene && isBossFight) StartCoroutine(ShowRoundSequence(true));
        else if(isCutscene) StartCoroutine(ShowRoundSequence(false));
    }

    private IEnumerator ShowRoundSequence(bool isBossFight)
    {
        if (!isBossFight) roundText.text = (roundFought + 1) + "° ONDATA";
        else
        {
            
            roundText.text = "BOSS FIGHT";
            HUD.activeBackgroundBoss(true);
        }
        roundFought++;
        yield return StartCoroutine(FadeText(0, 1));

        yield return new WaitForSeconds(displayDuration);

        yield return StartCoroutine(FadeText(1, 0));

        isCutscene = false;

        if (isBossFight)
        {
            yield return new WaitForSeconds(2);
            healthBarBoss.setBoss();
        }
    }

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