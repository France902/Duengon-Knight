using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image healthBarFill;
    public PlayerAttack playerScript;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerScript = GameObject.FindObjectOfType<PlayerAttack>();
        }
    }

    void Update()
    {
        if (playerScript != null && healthBarFill != null)
        {
            
            float fillValue = playerScript.health / playerScript.maxHealth;

            healthBarFill.fillAmount = fillValue;
        }
    }
}