using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Reflection;

[RequireComponent(typeof(ShadowCaster2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class UltimateShadowUpdater : MonoBehaviour
{
    private ShadowCaster2D _shadowCaster;
    private SpriteRenderer _spriteRenderer;
    private Sprite _lastSprite;
    private bool _lastFlipX;
    
    // Cache della FieldInfo per le performance
    private FieldInfo _shapePathField;
    private FieldInfo _shapePathHashField;

    void Awake()
    {
        _shadowCaster = GetComponent<ShadowCaster2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Forza l'uso della silhouette
        _shadowCaster.useRendererSilhouette = true;
        
        // Cache i campi interni (non cercarli ogni frame)
        _shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        _shapePathHashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash",
            BindingFlags.NonPublic | BindingFlags.Instance);
    }

    void LateUpdate()
    {
        // Controlla se lo sprite o la direzione è cambiata
        if (_spriteRenderer.sprite != _lastSprite || _spriteRenderer.flipX != _lastFlipX)
        {
            _lastSprite = _spriteRenderer.sprite;
            _lastFlipX = _spriteRenderer.flipX;
            
            Debug.Log($"🔄 Shadow Update - Sprite: {_spriteRenderer.sprite?.name}, FlipX: {_spriteRenderer.flipX}");
            
            ForceUpdateShadow();
        }
    }

    void ForceUpdateShadow()
    {
        // Step 1: Disabilita
        _shadowCaster.enabled = false;
        
        // Step 2: Resetta il cache interno
        if (_shapePathField != null)
            _shapePathField.SetValue(_shadowCaster, null);
        
        // Step 3: Resetta anche l'hash (per sicurezza)
        if (_shapePathHashField != null)
            _shapePathHashField.SetValue(_shadowCaster, 0);
        
        // Step 4: Riabilita (forza la rigenerazione)
        _shadowCaster.enabled = true;
    }
}