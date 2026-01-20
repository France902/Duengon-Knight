using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeYAxis : MonoBehaviour
{
    private float lockedYPosition;

    void Start()
    {
        // Memorizza la posizione Y iniziale al momento dell'avvio
        lockedYPosition = transform.position.y;
    }

    // Usiamo LateUpdate per essere sicuri che questo avvenga DOPO 
    // che il tuo script di movimento originale ha agito
    void LateUpdate()
    {
        Vector3 currentPos = transform.position;

        // Sovrascriviamo solo la Y con quella memorizzata all'inizio
        transform.position = new Vector3(currentPos.x, lockedYPosition, currentPos.z);
    }
}