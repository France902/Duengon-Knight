using UnityEngine;
using TMPro;      
using UnityEngine.UI; 

public class HUDManager : MonoBehaviour
{
    [Header("Riferimenti")]
    public PlayerAttack playerScript; 

    [Header("UI Base (Slot 1)")]
    public TextMeshProUGUI textBase;
    public Image backgroundBase; 

    [Header("UI Heavy (Slot 2)")]
    public TextMeshProUGUI textHeavy;
    public Image backgroundHeavy;

    public Image backgroundBossBase;

    public float timerBase = 0f;
    public float timerHeavy = 0f;

    void Start()
    {
        backgroundBossBase.enabled = false;
    }



    void Update()
    {
        if (playerScript.isAttacking)
        {
            string type = playerScript.getTypeAttack();

            switch (type)
            {
                case "base":
                    if (timerBase <= 0) timerBase = playerScript.shutdownAttack1;
                    break;

                case "heavy":
                    if (timerHeavy <= 0) timerHeavy = playerScript.shutdownAttack2;
                    break;
            }
        }

        UpdateCooldown(ref timerBase, textBase, backgroundBase);
        UpdateCooldown(ref timerHeavy, textHeavy, backgroundHeavy);
    }

    private void UpdateCooldown(ref float timer, TextMeshProUGUI UItext, Image bgImage)
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;

            UItext.text = timer.ToString("f1");

            UItext.alpha = 1f;

            bgImage.color = Color.gray;
        }
        else
        {
            timer = 0;
            UItext.text = ""; 
            UItext.alpha = 0.5f;
            bgImage.color = Color.white;
        }
    }

    public void activateBackgroundBoss()
    {
        backgroundBossBase.enabled = false;
    }
}