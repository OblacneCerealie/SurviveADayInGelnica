using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Script to automatically configure the environment for extreme darkness when lights are off.
/// This script should be attached to an empty GameObject in your scene and run once to set up the lighting.
/// </summary>
public class EnvironmentSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool removeAfterSetup = true;
    
    [Header("Darkness Settings")]
    [SerializeField] private Color extremeDarkAmbient = new Color(0.02f, 0.02f, 0.05f, 1f);
    [SerializeField] private float extremeDarkIntensity = 0.05f;
    [SerializeField] private Color extremeDarkFog = new Color(0.01f, 0.01f, 0.02f, 1f);
    [SerializeField] private float extremeDarkFogDensity = 0.15f;
    
    [Header("Lit Settings")]
    [SerializeField] private Color normalAmbient = new Color(0.3f, 0.3f, 0.4f, 1f);
    [SerializeField] private float normalIntensity = 0.8f;
    [SerializeField] private Color normalFog = new Color(0.4f, 0.4f, 0.6f, 1f);
    [SerializeField] private float normalFogDensity = 0.02f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupEnvironment();
        }
    }
    
    [ContextMenu("Setup Environment")]
    public void SetupEnvironment()
    {
        if (showDebugInfo)
        {
            Debug.Log("Setting up environment for extreme darkness...");
        }
        
        // Configure render settings for extreme darkness
        ConfigureRenderSettings();
        
        // Configure lighting manager if present
        ConfigureLightingManager();
        
        // Configure camera settings for darkness
        ConfigureCameras();
        
        // Configure post-processing for darkness
        ConfigurePostProcessing();
        
        if (showDebugInfo)
        {
            Debug.Log("Environment setup complete! The scene is now configured for extreme darkness when lights are off.");
        }
        
        // Remove this script after setup if requested
        if (removeAfterSetup)
        {
            if (showDebugInfo)
            {
                Debug.Log("Removing EnvironmentSetup script as requested.");
            }
            Destroy(this);
        }
    }
    
    void ConfigureRenderSettings()
    {
        // Enable fog for darkness effect
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        
        // Set initial ambient lighting (will be overridden by LightingManager)
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = extremeDarkAmbient;
        RenderSettings.ambientIntensity = extremeDarkIntensity;
        
        // Configure fog for darkness
        RenderSettings.fogColor = extremeDarkFog;
        RenderSettings.fogDensity = extremeDarkFogDensity;
        
        // Reduce reflection intensity
        RenderSettings.reflectionIntensity = 0.2f;
        
        if (showDebugInfo)
        {
            Debug.Log("Render settings configured for extreme darkness");
        }
    }
    
    void ConfigureLightingManager()
    {
        LightingManager lightingManager = FindFirstObjectByType<LightingManager>();
        
        if (lightingManager == null)
        {
            // Create a LightingManager if none exists
            GameObject lightingManagerObject = new GameObject("LightingManager");
            lightingManager = lightingManagerObject.AddComponent<LightingManager>();
            
            if (showDebugInfo)
            {
                Debug.Log("Created LightingManager GameObject");
            }
        }
        
        // The LightingManager will handle its own initialization
        if (showDebugInfo)
        {
            Debug.Log("LightingManager found and configured");
        }
    }
    
    void ConfigureCameras()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        
        foreach (Camera cam in cameras)
        {
            // Configure camera for better darkness visibility
            if (cam.GetComponent<UniversalAdditionalCameraData>() == null)
            {
                cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }
            
            UniversalAdditionalCameraData cameraData = cam.GetComponent<UniversalAdditionalCameraData>();
            
            // Enable post-processing for darkness effects
            cameraData.renderPostProcessing = true;
            
            // Adjust camera clear flags for better darkness
            cam.clearFlags = CameraClearFlags.Skybox;
            
            // Set background color to very dark for fallback
            cam.backgroundColor = new Color(0.01f, 0.01f, 0.02f, 1f);
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Configured {cameras.Length} cameras for darkness rendering");
        }
    }
    
    void ConfigurePostProcessing()
    {
        // Try to find existing post-process volume
        Volume postProcessVolume = FindFirstObjectByType<Volume>();
        
        if (postProcessVolume == null)
        {
            // Create a global post-process volume
            GameObject volumeObject = new GameObject("Global Post Process Volume");
            postProcessVolume = volumeObject.AddComponent<Volume>();
            postProcessVolume.isGlobal = true;
            postProcessVolume.priority = 0;
            
            if (showDebugInfo)
            {
                Debug.Log("Created global post-process volume");
            }
        }
        
        // Configure the volume for darkness effects
        postProcessVolume.isGlobal = true;
        
        if (showDebugInfo)
        {
            Debug.Log("Post-processing volume configured");
        }
    }
    
    [ContextMenu("Test Extreme Darkness")]
    public void TestExtremeDarkness()
    {
        RenderSettings.ambientLight = extremeDarkAmbient;
        RenderSettings.ambientIntensity = extremeDarkIntensity;
        RenderSettings.fogColor = extremeDarkFog;
        RenderSettings.fogDensity = extremeDarkFogDensity;
        
        // Turn off all lights
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in allLights)
        {
            light.enabled = false;
        }
        
        Debug.Log("Applied extreme darkness settings for testing");
    }
    
    [ContextMenu("Restore Normal Lighting")]
    public void RestoreNormalLighting()
    {
        RenderSettings.ambientLight = normalAmbient;
        RenderSettings.ambientIntensity = normalIntensity;
        RenderSettings.fogColor = normalFog;
        RenderSettings.fogDensity = normalFogDensity;
        
        // Turn on all lights
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in allLights)
        {
            light.enabled = true;
        }
        
        Debug.Log("Restored normal lighting settings");
    }
    
    [ContextMenu("Create Light Switch")]
    public void CreateLightSwitch()
    {
        // Create a cube that acts as a light switch
        GameObject switchCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        switchCube.name = "LightSwitch";
        switchCube.transform.position = Vector3.zero;
        switchCube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // Add the LightSwitch component
        LightSwitch lightSwitch = switchCube.AddComponent<LightSwitch>();
        
        // Position it at a reasonable height (like a wall switch)
        switchCube.transform.position = new Vector3(0, 1.2f, 0);
        
        Debug.Log("Created light switch cube at origin. Move it to your desired location!");
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw a sphere to show this is the environment setup object
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawIcon(transform.position, "Settings Icon", true);
    }
}
