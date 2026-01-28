using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBoxLogic: MonoBehaviour
{
    private CapsuleCollider2D capsuleCollider;
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    public void setLeftOffset() {
        capsuleCollider.offset = new Vector2(-0.23f, capsuleCollider.offset.y);
    }

    public void setRightOffset()
    {
        capsuleCollider.offset = new Vector2(0, capsuleCollider.offset.y);
    }
}
