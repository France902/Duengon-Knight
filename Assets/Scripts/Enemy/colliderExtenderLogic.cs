using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colliderExtenderLogic : MonoBehaviour
{
    private BoxCollider2D capsuleCollider;
    void Start()
    {
        capsuleCollider = GetComponent<BoxCollider2D>();
    }

    public void setLeftOffset()
    {
        capsuleCollider.offset = new Vector2(-0.23f, capsuleCollider.offset.y);
    }

    public void setRightOffset()
    {
        capsuleCollider.offset = new Vector2(0, capsuleCollider.offset.y);
    }
}
