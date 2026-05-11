using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlipShadow : MonoBehaviour
{
    [SerializeField] private Transform shadowTransform; // trascina qui il GameObject "Shadow"
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Modifica SOLO lo scale X del figlio Shadow, il personaggio non viene toccato
        Vector3 scale = shadowTransform.localScale;
        scale.x = spriteRenderer.flipX ? -1f : 1f;
        shadowTransform.localScale = scale;
    }
}