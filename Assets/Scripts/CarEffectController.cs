using UnityEngine;

public class CarEffectController : MonoBehaviour
{
    [Header("Material Settings")]
    [Tooltip("Material to apply when pressing G key. Drag your CombinedShader material here. Press R to reset to original material.")]
    public Material shaderMaterial;

    private Material originalMaterial;
    private Renderer carRenderer;
    private bool isUsingEffectMaterial = false;

    void Start()
    {
        Transform child = transform.Find("Object010");
        if (child != null)
        {
            carRenderer = child.GetComponent<Renderer>();
        }

        if (carRenderer != null)
        {
            originalMaterial = carRenderer.material;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (carRenderer != null && shaderMaterial != null)
            {
                if (!isUsingEffectMaterial)
                {
                    isUsingEffectMaterial = true;
                    carRenderer.material = shaderMaterial;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (carRenderer != null && originalMaterial != null)
            {
                if (isUsingEffectMaterial)
                {
                    isUsingEffectMaterial = false;
                    carRenderer.material = originalMaterial;
                }
            }
        }
    }
}