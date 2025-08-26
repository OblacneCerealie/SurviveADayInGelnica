using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class LightingManager : MonoBehaviour
{
    [Header("Lighting Settings")]
    [SerializeField] private bool lightsOn = false;
    [SerializeField] private float transitionSpeed = 2f;
    
    [Header("Dark Environment Settings")]
    [SerializeField] private Color darkAmbientColor = new Color(0.05f, 0.05f, 0.1f, 1f);
    [SerializeField] private float darkAmbientIntensity = 0.1f;
    [SerializeField] private float darkFogDensity = 0.08f;
    [SerializeField] private Color darkFogColor = new Color(0.02f, 0.02f, 0.05f, 1f);
    
    [Header("Lit Environment Settings")]
    [SerializeField] private Color litAmbientColor = new Color(0.4f, 0.4f, 0.5f, 1f);
    [SerializeField] private float litAmbientIntensity = 1f;
    [SerializeField] private float litFogDensity = 0.01f;
    [SerializeField] private Color litFogColor = new Color(0.5f, 0.5f, 0.7f, 1f);
    
    [Header("Light Sources")]
    [SerializeField] private Light[] controllableLights;
    [SerializeField] private Light mainDirectionalLight;
    
    [Header("Post Processing")]
    [SerializeField] private Volume postProcessVolume;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Private variables for smooth transitions
    private Color currentAmbientColor;
    private float currentAmbientIntensity;
    private float currentFogDensity;
    private Color currentFogColor;
    private float[] originalLightIntensities;
    private bool isTransitioning = false;
    
    // Singleton pattern for easy access
    public static LightingManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        InitializeLighting();
        SetupInitialState();
    }
    
    void InitializeLighting()
    {
        // Find main directional light if not assigned
        if (mainDirectionalLight == null)
        {
            Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in allLights)
            {
                if (light.type == LightType.Directional)
                {
                    mainDirectionalLight = light;
                    break;
                }
            }
        }
        
        // Find all controllable lights if not assigned
        if (controllableLights == null || controllableLights.Length == 0)
        {
            controllableLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        }
        
        // Store original light intensities
        if (controllableLights != null)
        {
            originalLightIntensities = new float[controllableLights.Length];
            for (int i = 0; i < controllableLights.Length; i++)
            {
                if (controllableLights[i] != null)
                {
                    originalLightIntensities[i] = controllableLights[i].intensity;
                }
            }
        }
        
        // Find post process volume if not assigned
        if (postProcessVolume == null)
        {
            postProcessVolume = FindFirstObjectByType<Volume>();
        }
        
        // Enable fog
        RenderSettings.fog = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"LightingManager initialized with {controllableLights?.Length ?? 0} controllable lights");
        }
    }
    
    void SetupInitialState()
    {
        // Start with lights on
        lightsOn = true;
        ApplyLightingState(true); // Immediate application
    }
    
    void Update()
    {
        if (isTransitioning)
        {
            UpdateTransition();
        }
    }
    
    void UpdateTransition()
    {
        // Smoothly transition lighting values
        Color targetAmbient = lightsOn ? litAmbientColor : darkAmbientColor;
        float targetIntensity = lightsOn ? litAmbientIntensity : darkAmbientIntensity;
        float targetFogDensity = lightsOn ? litFogDensity : darkFogDensity;
        Color targetFogColor = lightsOn ? litFogColor : darkFogColor;
        
        // Lerp values
        currentAmbientColor = Color.Lerp(currentAmbientColor, targetAmbient, Time.deltaTime * transitionSpeed);
        currentAmbientIntensity = Mathf.Lerp(currentAmbientIntensity, targetIntensity, Time.deltaTime * transitionSpeed);
        currentFogDensity = Mathf.Lerp(currentFogDensity, targetFogDensity, Time.deltaTime * transitionSpeed);
        currentFogColor = Color.Lerp(currentFogColor, targetFogColor, Time.deltaTime * transitionSpeed);
        
        // Apply values
        RenderSettings.ambientLight = currentAmbientColor;
        RenderSettings.ambientIntensity = currentAmbientIntensity;
        RenderSettings.fogDensity = currentFogDensity;
        RenderSettings.fogColor = currentFogColor;
        
        // Check if transition is complete
        float colorDiff = Vector4.Distance(currentAmbientColor, targetAmbient);
        float intensityDiff = Mathf.Abs(currentAmbientIntensity - targetIntensity);
        float fogDiff = Mathf.Abs(currentFogDensity - targetFogDensity);
        
        if (colorDiff < 0.01f && intensityDiff < 0.01f && fogDiff < 0.001f)
        {
            isTransitioning = false;
            if (showDebugInfo)
            {
                Debug.Log($"Lighting transition completed. Lights are now: {(lightsOn ? "ON" : "OFF")}");
            }
        }
    }
    
    public void ToggleLights()
    {
        lightsOn = !lightsOn;
        ApplyLightingState(false);
        
        if (showDebugInfo)
        {
            Debug.Log($"Lights toggled: {(lightsOn ? "ON" : "OFF")}");
        }
    }
    
    public void SetLights(bool state)
    {
        if (lightsOn != state)
        {
            lightsOn = state;
            ApplyLightingState(false);
            
            if (showDebugInfo)
            {
                Debug.Log($"Lights set to: {(lightsOn ? "ON" : "OFF")}");
            }
        }
    }
    
    void ApplyLightingState(bool immediate = false)
    {
        if (immediate)
        {
            // Apply immediately without transition
            currentAmbientColor = lightsOn ? litAmbientColor : darkAmbientColor;
            currentAmbientIntensity = lightsOn ? litAmbientIntensity : darkAmbientIntensity;
            currentFogDensity = lightsOn ? litFogDensity : darkFogDensity;
            currentFogColor = lightsOn ? litFogColor : darkFogColor;
            
            RenderSettings.ambientLight = currentAmbientColor;
            RenderSettings.ambientIntensity = currentAmbientIntensity;
            RenderSettings.fogDensity = currentFogDensity;
            RenderSettings.fogColor = currentFogColor;
        }
        else
        {
            // Start smooth transition
            isTransitioning = true;
        }
        
        // Handle individual lights
        if (controllableLights != null)
        {
            for (int i = 0; i < controllableLights.Length; i++)
            {
                if (controllableLights[i] != null)
                {
                    if (lightsOn)
                    {
                        controllableLights[i].enabled = true;
                        controllableLights[i].intensity = originalLightIntensities[i];
                    }
                    else
                    {
                        controllableLights[i].enabled = false;
                    }
                }
            }
        }
        
        // Handle main directional light (sun/moon)
        if (mainDirectionalLight != null)
        {
            if (lightsOn)
            {
                mainDirectionalLight.intensity = 1f;
                mainDirectionalLight.color = Color.white;
            }
            else
            {
                mainDirectionalLight.intensity = 0.05f; // Very dim moonlight
                mainDirectionalLight.color = new Color(0.1f, 0.1f, 0.2f, 1f); // Dark blue tint
            }
        }
    }
    
    // Public getters
    public bool AreLightsOn => lightsOn;
    public bool IsTransitioning => isTransitioning;
    
    // Method to add lights to control
    public void RegisterLight(Light lightToAdd)
    {
        if (lightToAdd == null) return;
        
        // Expand array if needed
        Light[] newArray = new Light[controllableLights.Length + 1];
        float[] newIntensities = new float[originalLightIntensities.Length + 1];
        
        for (int i = 0; i < controllableLights.Length; i++)
        {
            newArray[i] = controllableLights[i];
            newIntensities[i] = originalLightIntensities[i];
        }
        
        newArray[controllableLights.Length] = lightToAdd;
        newIntensities[originalLightIntensities.Length] = lightToAdd.intensity;
        
        controllableLights = newArray;
        originalLightIntensities = newIntensities;
        
        if (showDebugInfo)
        {
            Debug.Log($"Registered new light: {lightToAdd.name}");
        }
    }
    
    // Debug method for testing
    [System.Obsolete("Use ToggleLights() instead")]
    public void TestToggle()
    {
        ToggleLights();
    }
}
