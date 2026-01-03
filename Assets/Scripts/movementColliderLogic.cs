using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementColliderLogic : MonoBehaviour
{
    private Collider2D myCollider;
    void Start()
    {
        myCollider = GetComponent<Collider2D>();
    }

    public void setLeftOffset()
    {
        myCollider.offset = new Vector2(-0.8f, myCollider.offset.y);
    }
    public void setRightOffset()
    {
        myCollider.offset = new Vector2(0, myCollider.offset.y);
    }
}
