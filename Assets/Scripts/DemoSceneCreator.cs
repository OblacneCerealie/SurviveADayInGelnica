using UnityEngine;

/// <summary>
/// Creates a simple demo scene to showcase the dark environment and light switch system.
/// This is optional - just for demonstration purposes.
/// </summary>
public class DemoSceneCreator : MonoBehaviour
{
    [Header("Demo Scene Creation")]
    [SerializeField] private bool createDemoOnStart = false;
    
    [ContextMenu("Create Demo Scene")]
    public void CreateDemoScene()
    {
        Debug.Log("Creating demo scene for dark environment system...");
        
        // Create basic environment
        CreateBasicEnvironment();
        
        // Setup lighting system
        SetupLightingSystem();
        
        // Create multiple light switches
        CreateMultipleLightSwitches();
        
        // Create some basic lighting
        CreateBasicLights();
        
        Debug.Log("Demo scene created! Walk around and press E near the cube light switches to toggle lighting!");
    }
    
    void Start()
    {
        if (createDemoOnStart)
        {
            CreateDemoScene();
        }
    }
    
    void CreateBasicEnvironment()
    {
        // Create a floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(5, 1, 5);
        
        // Create some walls
        CreateWall("WallNorth", new Vector3(0, 2.5f, 25f), new Vector3(50, 5, 1));
        CreateWall("WallSouth", new Vector3(0, 2.5f, -25f), new Vector3(50, 5, 1));
        CreateWall("WallEast", new Vector3(25f, 2.5f, 0), new Vector3(1, 5, 50));
        CreateWall("WallWest", new Vector3(-25f, 2.5f, 0), new Vector3(1, 5, 50));
        
        // Create some obstacles/furniture
        CreateObstacle("Table1", new Vector3(5, 0.5f, 5), new Vector3(3, 1, 1.5f));
        CreateObstacle("Table2", new Vector3(-8, 0.5f, -3), new Vector3(2, 1, 3));
        CreateObstacle("Box1", new Vector3(10, 1, -8), new Vector3(2, 2, 2));
        
        Debug.Log("Basic environment created");
    }
    
    void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        
        // Make walls a bit darker
        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material wallMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            wallMat.color = new Color(0.3f, 0.3f, 0.3f);
            renderer.material = wallMat;
        }
    }
    
    void CreateObstacle(string name, Vector3 position, Vector3 scale)
    {
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = name;
        obstacle.transform.position = position;
        obstacle.transform.localScale = scale;
        
        // Add some color variation
        Renderer renderer = obstacle.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.8f), Random.Range(0.2f, 0.8f));
            renderer.material = mat;
        }
    }
    
    void SetupLightingSystem()
    {
        // Create LightingManager
        GameObject lightingManagerObj = new GameObject("LightingManager");
        lightingManagerObj.AddComponent<LightingManager>();
        
        // Create EnvironmentSetup
        GameObject envSetupObj = new GameObject("EnvironmentSetup");
        envSetupObj.AddComponent<EnvironmentSetup>();
        
        Debug.Log("Lighting system components created");
    }
    
    void CreateMultipleLightSwitches()
    {
        // Create materials for switches
        Material onMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        onMat.color = Color.green;
        onMat.name = "Switch_On";
        
        Material offMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        offMat.color = Color.red;
        offMat.name = "Switch_Off";
        
        // Create several light switches around the room
        CreateLightSwitch("MainLightSwitch", new Vector3(-23, 1.5f, 0), onMat, offMat);
        CreateLightSwitch("SecondarySwitch", new Vector3(0, 1.5f, 23), onMat, offMat);
        CreateLightSwitch("EmergencySwitch", new Vector3(23, 1.5f, 0), onMat, offMat);
        
        Debug.Log("Light switches created at various wall locations");
    }
    
    void CreateLightSwitch(string name, Vector3 position, Material onMat, Material offMat)
    {
        GameObject switchObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        switchObj.name = name;
        switchObj.transform.position = position;
        switchObj.transform.localScale = new Vector3(0.3f, 0.6f, 0.1f);
        
        LightSwitch lightSwitch = switchObj.AddComponent<LightSwitch>();
        lightSwitch.onMaterial = onMat;
        lightSwitch.offMaterial = offMat;
        
        // Set initial color to red (off)
        Renderer renderer = switchObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = offMat;
        }
    }
    
    void CreateBasicLights()
    {
        // Create a main directional light (sun/moon)
        GameObject dirLightObj = new GameObject("Main Directional Light");
        Light dirLight = dirLightObj.AddComponent<Light>();
        dirLight.type = LightType.Directional;
        dirLight.intensity = 1f;
        dirLight.color = Color.white;
        dirLightObj.transform.rotation = Quaternion.Euler(45f, 30f, 0f);
        
        // Create some point lights for interior lighting
        CreatePointLight("RoomLight1", new Vector3(0, 4, 0), Color.white, 15f, 2f);
        CreatePointLight("RoomLight2", new Vector3(10, 4, 10), Color.white, 12f, 1.8f);
        CreatePointLight("RoomLight3", new Vector3(-10, 4, -10), Color.white, 12f, 1.8f);
        
        // Create some accent lights
        CreatePointLight("AccentLight1", new Vector3(5, 2, 5), new Color(1f, 0.8f, 0.6f), 8f, 1f);
        CreatePointLight("AccentLight2", new Vector3(-8, 2, -3), new Color(0.8f, 0.9f, 1f), 6f, 0.8f);
        
        Debug.Log("Basic lighting setup created");
    }
    
    void CreatePointLight(string name, Vector3 position, Color color, float range, float intensity)
    {
        GameObject lightObj = new GameObject(name);
        lightObj.transform.position = position;
        
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = range;
        light.intensity = intensity;
        
        // Add a small visual indicator
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.name = name + "_Indicator";
        indicator.transform.parent = lightObj.transform;
        indicator.transform.localPosition = Vector3.zero;
        indicator.transform.localScale = Vector3.one * 0.1f;
        
        // Make the indicator glow
        Renderer indicatorRenderer = indicator.GetComponent<Renderer>();
        if (indicatorRenderer != null)
        {
            Material glowMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            glowMat.color = color;
            glowMat.SetFloat("_Metallic", 0f);
            glowMat.SetFloat("_Smoothness", 1f);
            indicatorRenderer.material = glowMat;
        }
        
        // Remove collider from indicator
        Collider indicatorCollider = indicator.GetComponent<Collider>();
        if (indicatorCollider != null)
        {
            DestroyImmediate(indicatorCollider);
        }
    }
    
    [ContextMenu("Clear Demo Scene")]
    public void ClearDemoScene()
    {
        // Find and destroy demo objects
        string[] demoObjectNames = {
            "Floor", "WallNorth", "WallSouth", "WallEast", "WallWest",
            "Table1", "Table2", "Box1",
            "MainLightSwitch", "SecondarySwitch", "EmergencySwitch",
            "Main Directional Light", "RoomLight1", "RoomLight2", "RoomLight3",
            "AccentLight1", "AccentLight2",
            "LightingManager", "EnvironmentSetup"
        };
        
        foreach (string objName in demoObjectNames)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        
        Debug.Log("Demo scene cleared");
    }
}
