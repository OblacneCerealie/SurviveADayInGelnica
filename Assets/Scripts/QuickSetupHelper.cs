using UnityEngine;
using UnityEditor;

/// <summary>
/// Quick setup helper for the dark environment and lighting system.
/// This script provides easy buttons to set up everything automatically.
/// </summary>
public class QuickSetupHelper : MonoBehaviour
{
    [Header("Quick Setup")]
    [SerializeField] private bool showInstructions = true;
    
    [Header("Setup Options")]
    [SerializeField] private bool setupLightingManager = true;
    [SerializeField] private bool setupEnvironment = true;
    [SerializeField] private bool createLightSwitch = true;
    [SerializeField] private Vector3 lightSwitchPosition = new Vector3(0, 1.2f, 2f);
    
    [Header("Materials for Light Switch")]
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;
    
    [TextArea(10, 20)]
    [SerializeField] private string instructions = @"DARK ENVIRONMENT & LIGHT SWITCH SETUP INSTRUCTIONS:

1. Click 'Complete Setup' button below to automatically set up everything!

OR follow these manual steps:

MANUAL SETUP:
1. Add 'LightingManager' script to an empty GameObject in your scene
2. Add 'EnvironmentSetup' script to an empty GameObject and let it run
3. Create a cube for the light switch and add 'LightSwitch' script to it
4. Position the light switch cube where you want it (like on a wall)
5. Assign materials to the light switch for on/off visual feedback (optional)

FEATURES:
- Press E near any cube with LightSwitch script to toggle lights
- Extremely dark environment when lights are off (can barely see 1 meter)
- Smooth lighting transitions
- Works with your existing GeneratorInteraction script
- Visual and audio feedback for light switches

GENERATOR INTEGRATION:
- Check 'Controls Lighting' in your GeneratorInteraction component
- Set 'Turn On Lights When Activated' to turn on lights when generator activates

TESTING:
- Use the 'Test Darkness' and 'Test Normal Lighting' buttons below
- In play mode, press E near light switch cubes to toggle lighting

NOTES:
- Make sure your player GameObject has the 'Player' tag
- Light switches will automatically find the player
- The LightingManager is a singleton and persists between scenes";
    
    void Start()
    {
        if (showInstructions)
        {
            Debug.Log("QuickSetupHelper: " + instructions.Replace("\n", " "));
        }
    }
    
    [ContextMenu("Complete Setup")]
    public void CompleteSetup()
    {
        Debug.Log("Starting complete dark environment setup...");
        
        if (setupLightingManager)
        {
            SetupLightingManager();
        }
        
        if (setupEnvironment)
        {
            SetupEnvironment();
        }
        
        if (createLightSwitch)
        {
            CreateLightSwitch();
        }
        
        Debug.Log("Complete setup finished! Your dark environment is ready!");
        Debug.Log("Instructions: Press E near the light switch cube to toggle lights on/off.");
    }
    
    [ContextMenu("Setup Lighting Manager")]
    public void SetupLightingManager()
    {
        LightingManager existing = FindFirstObjectByType<LightingManager>();
        if (existing != null)
        {
            Debug.Log("LightingManager already exists in scene");
            return;
        }
        
        GameObject lightingManagerObj = new GameObject("LightingManager");
        LightingManager manager = lightingManagerObj.AddComponent<LightingManager>();
        
        Debug.Log("Created LightingManager in scene");
    }
    
    [ContextMenu("Setup Environment")]
    public void SetupEnvironment()
    {
        GameObject envSetupObj = new GameObject("EnvironmentSetup");
        EnvironmentSetup envSetup = envSetupObj.AddComponent<EnvironmentSetup>();
        
        Debug.Log("Created EnvironmentSetup - it will automatically configure the scene for darkness");
    }
    
    [ContextMenu("Create Light Switch")]
    public void CreateLightSwitch()
    {
        GameObject switchCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        switchCube.name = "LightSwitch";
        switchCube.transform.position = lightSwitchPosition;
        switchCube.transform.localScale = new Vector3(0.3f, 0.6f, 0.1f); // Wall switch proportions
        
        LightSwitch lightSwitch = switchCube.AddComponent<LightSwitch>();
        
        // Assign materials if available
        if (onMaterial != null)
        {
            lightSwitch.onMaterial = onMaterial;
        }
        if (offMaterial != null)
        {
            lightSwitch.offMaterial = offMaterial;
        }
        
        // Create simple materials if none assigned
        if (onMaterial == null && offMaterial == null)
        {
            CreateSimpleSwitchMaterials(lightSwitch);
        }
        
        Debug.Log($"Created light switch at position {lightSwitchPosition}. Move it to your desired location!");
    }
    
    void CreateSimpleSwitchMaterials(LightSwitch lightSwitch)
    {
        // Create simple colored materials for the switch
        Material greenMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        greenMat.color = Color.green;
        greenMat.name = "LightSwitch_On";
        
        Material redMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        redMat.color = Color.red;
        redMat.name = "LightSwitch_Off";
        
        lightSwitch.onMaterial = greenMat;
        lightSwitch.offMaterial = redMat;
        
        Debug.Log("Created simple green/red materials for light switch");
    }
    
    [ContextMenu("Test Extreme Darkness")]
    public void TestExtremeDarkness()
    {
        if (LightingManager.Instance != null)
        {
            LightingManager.Instance.SetLights(false);
            Debug.Log("Applied extreme darkness via LightingManager");
        }
        else
        {
            // Fallback manual setup
            RenderSettings.ambientLight = new Color(0.02f, 0.02f, 0.05f, 1f);
            RenderSettings.ambientIntensity = 0.05f;
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.01f, 0.01f, 0.02f, 1f);
            RenderSettings.fogDensity = 0.15f;
            
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                light.enabled = false;
            }
            
            Debug.Log("Applied extreme darkness manually (no LightingManager found)");
        }
    }
    
    [ContextMenu("Test Normal Lighting")]
    public void TestNormalLighting()
    {
        if (LightingManager.Instance != null)
        {
            LightingManager.Instance.SetLights(true);
            Debug.Log("Applied normal lighting via LightingManager");
        }
        else
        {
            // Fallback manual setup
            RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.4f, 1f);
            RenderSettings.ambientIntensity = 0.8f;
            RenderSettings.fogColor = new Color(0.4f, 0.4f, 0.6f, 1f);
            RenderSettings.fogDensity = 0.02f;
            
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                light.enabled = true;
            }
            
            Debug.Log("Applied normal lighting manually (no LightingManager found)");
        }
    }
    
    [ContextMenu("Find and Setup Existing Cubes as Light Switches")]
    public void SetupExistingCubesAsLightSwitches()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int switchesCreated = 0;
        
        foreach (GameObject obj in allObjects)
        {
            // Look for cube objects that don't already have LightSwitch
            if (obj.name.ToLower().Contains("cube") && obj.GetComponent<LightSwitch>() == null)
            {
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.name.Contains("Cube"))
                {
                    obj.AddComponent<LightSwitch>();
                    switchesCreated++;
                    Debug.Log($"Added LightSwitch to existing cube: {obj.name}");
                }
            }
        }
        
        Debug.Log($"Added LightSwitch component to {switchesCreated} existing cube objects");
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw where the light switch will be created
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(lightSwitchPosition, new Vector3(0.3f, 0.6f, 0.1f));
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}
