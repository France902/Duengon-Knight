using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class RoundManager : MonoBehaviour
{
    public bool isCutscene = false;
    public int roundFought = 0;

    [Header("UI Settings")]
    public TextMeshProUGUI roundText;
    public float fadeDuration = 1.0f;
    public float displayDuration = 2.0f;
    public HUDManager HUD;
    public HealthBarUIBoss healthBarBoss;

    private bool victoryTriggered = false;

    public void setIsCutscene(bool isCutscene, bool isBossFight)
    {
        this.isCutscene = isCutscene;
        if (isCutscene && isBossFight) StartCoroutine(ShowRoundSequence(true));
        else if (isCutscene) StartCoroutine(ShowRoundSequence(false));
    }

    public void OnBossDefeated()
    {
        Debug.Log("[ROUND] OnBossDefeated chiamato, victoryTriggered=" + victoryTriggered + ", roundText=" + (roundText == null ? "NULL" : "OK"));
        if (victoryTriggered) return;
        victoryTriggered = true;
        StopAllCoroutines();
        StartCoroutine(ShowVictorySequence());
    }

    private IEnumerator ShowVictorySequence()
    {
        Debug.Log("[ROUND] ShowVictorySequence partita");
        yield return new WaitForSeconds(1.5f);
        Debug.Log("[ROUND] Dopo il delay, mostro VITTORIA");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
                playerRb.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            PlayerAttack playerAttack = player.GetComponent<PlayerAttack>();
            if (playerAttack != null) playerAttack.isVictory = true;
        }

        roundText.gameObject.SetActive(true);
        roundText.text = "VITTORIA";
        roundText.color = new Color(roundText.color.r, roundText.color.g, roundText.color.b, 0f);

        yield return StartCoroutine(FadeText(0f, 1f));
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(FadeText(1f, 0f));

        roundText.gameObject.SetActive(false);
    }

    private IEnumerator ShowRoundSequence(bool isBossFight)
    {
        roundText.gameObject.SetActive(true);

        if (!isBossFight) roundText.text = (roundFought + 1) + "° ONDATA";
        else
        {
            roundText.text = "BOSS FIGHT";
            HUD.activeBackgroundBoss(true);
        }
        roundFought++;
        roundText.color = new Color(roundText.color.r, roundText.color.g, roundText.color.b, 0f);
        yield return StartCoroutine(FadeText(0f, 1f));
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(FadeText(1f, 0f));

        roundText.gameObject.SetActive(false);
        isCutscene = false;

        if (isBossFight)
        {
            yield return new WaitForSeconds(2f);
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