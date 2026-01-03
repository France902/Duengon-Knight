// Script sull'oggetto Arma o sul sensore
using Unity.VisualScripting;
using UnityEngine;

public class WeaponLogic : MonoBehaviour
{
    private Collider2D myCollider;

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
    }

    public void SetEnemyExclusion(bool exclude)
    {
        int enemyLayerIndex = LayerMask.NameToLayer("enemyLayer");

        if (exclude)
        {
            // Esclude il layer nemico
            myCollider.excludeLayers = (1 << enemyLayerIndex);
        }
        else
        {
            // Rimuove tutte le esclusioni
            myCollider.excludeLayers = 0;
        }
    }

    public void setLeftOffset()
    {
        myCollider.offset = new Vector2(-0.75f, myCollider.offset.y);
    }

    public void setRightOffset()
    {
        myCollider.offset = new Vector2(0f, myCollider.offset.y);
    }
}