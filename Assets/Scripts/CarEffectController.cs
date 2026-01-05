using UnityEngine;

public class CarEffectController : MonoBehaviour
{
    [Header("Material Settings")]
    [Tooltip("Material to apply when pressing G key. Drag your CombinedShader material here. Press R to reset to original material.")]
    public Material shaderMaterial;

    [Header("Animation Settings")]
    [Tooltip("Transition duration in seconds when switching materials (currently not used, but reserved for future fade effects).")]
    public float transitionDuration = 0.5f;

    private Material originalMaterial;
    private Renderer carRenderer;
    private bool isUsingEffectMaterial = false;
    private float transitionTimer = 0f;

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
                    transitionTimer = 0f;
                    carRenderer.material = shaderMaterial;
                    // Set initial time
                    if (carRenderer.material.HasProperty("_ShaderTime"))
                    {
                        carRenderer.material.SetFloat("_ShaderTime", Time.time);
                    }
                    Debug.Log($"Applied effect material: {shaderMaterial.name}");
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
                    transitionTimer = 0f;
                    carRenderer.material = originalMaterial;
                    Debug.Log($"Reset to original material: {originalMaterial.name}");
                }

            }

        }
    }
}