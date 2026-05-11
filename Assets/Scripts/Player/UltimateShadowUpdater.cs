using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Reflection;

public class UltimateShadowUpdater : MonoBehaviour
{
    [SerializeField] private ShadowCaster2D shadowCaster; // trascina il GameObject figlio "Shadow"
    private SpriteRenderer _spriteRenderer;
    private Sprite _lastSprite;
    private bool _lastFlipX;

    private FieldInfo _shapePathField;
    private FieldInfo _shapePathHashField;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // Se non assegnato nell'inspector, cerca nei figli
        if (shadowCaster == null)
            shadowCaster = GetComponentInChildren<ShadowCaster2D>();

        shadowCaster.useRendererSilhouette = true;

        _shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath",
            BindingFlags.NonPublic | BindingFlags.Instance);
        _shapePathHashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash",
            BindingFlags.NonPublic | BindingFlags.Instance);
    }

    void LateUpdate()
    {
        if (_spriteRenderer.sprite != _lastSprite || _spriteRenderer.flipX != _lastFlipX)
        {
            _lastSprite = _spriteRenderer.sprite;
            _lastFlipX = _spriteRenderer.flipX;

            FlipShadow();
            ForceUpdateShadow();
        }
    }

    void FlipShadow()
    {
        // Gira SOLO il transform del ShadowCaster2D figlio, non tocca il personaggio
        Vector3 scale = shadowCaster.transform.localScale;
        scale.x = _spriteRenderer.flipX ? -1f : 1f;
        shadowCaster.transform.localScale = scale;
    }

    void ForceUpdateShadow()
    {
        shadowCaster.enabled = false;

        if (_shapePathField != null)
            _shapePathField.SetValue(shadowCaster, null);

        if (_shapePathHashField != null)
            _shapePathHashField.SetValue(shadowCaster, 0);

        shadowCaster.enabled = true;
    }
}