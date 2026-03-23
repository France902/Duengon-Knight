using UnityEngine;
using UnityEngine.UI;

public class HealthBarUIBoss : MonoBehaviour
{
    public Image healthBarFill;
    public GameObject BossScript;

    void Update()
    {
        
        if (BossScript != null && healthBarFill != null)
        {
            BossScript.GetComponent<EnemySlime>().hp = GameObject.FindGameObjectWithTag("Enemy").GetComponent<EnemySlime>().hp;

            float fillValue = BossScript.GetComponent<EnemySlime>().hp / BossScript.GetComponent<EnemySlime>().MaxHp;

            healthBarFill.fillAmount = fillValue;
        }
    }

    public void setBoss()
    {
        BossScript = GameObject.FindGameObjectWithTag("Enemy");
    }
}