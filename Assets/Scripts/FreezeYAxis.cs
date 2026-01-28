using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeYAxis : MonoBehaviour
{
    private float lockedYPosition;

    void Start()
    {
        lockedYPosition = transform.position.y;
    }

    void LateUpdate()
    {
        Vector3 currentPos = transform.position;

        transform.position = new Vector3(currentPos.x, lockedYPosition, currentPos.z);
    }
}