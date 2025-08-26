using UnityEngine;
using TMPro;

/// <summary>
/// Helper script to automatically set up the sanity system in the scene.
/// This script helps configure all the necessary components for the patient spawning and sanity system.
/// </summary>
[System.Serializable]
public class SanitySystemSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createSanityUI = true;
    
    [Header("Prefab References")]
    [SerializeField] private GameObject patientPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private int numberOfPatients = 3;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float maxWanderRadius = 50f;
    
    [Header("UI Settings")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private Vector2 sanityUIPosition = new Vector2(20, -20);
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupSanitySystem();
        }
    }
    
    [ContextMenu("Setup Sanity System")]
    public void SetupSanitySystem()
    {
        Debug.Log("Setting up Sanity System...");
        
        // 1. Setup GameManager
        SetupGameManager();
        
        // 2. Setup PatientSpawner
        SetupPatientSpawner();
        
        // 3. Setup SanityManager
        SetupSanityManager();
        
        // 4. Setup UI if needed
        if (createSanityUI)
        {
            SetupSanityUI();
        }
        
        Debug.Log("Sanity System setup complete!");
    }
    
    void SetupGameManager()
    {
        GameManager existingGM = FindFirstObjectByType<GameManager>();
        if (existingGM == null)
        {
            GameObject gmObject = new GameObject("GameManager");
            gmObject.AddComponent<GameManager>();
            Debug.Log("GameManager created");
        }
        else
        {
            Debug.Log("GameManager already exists");
        }
    }
    
    void SetupPatientSpawner()
    {
        PatientSpawner existingSpawner = FindFirstObjectByType<PatientSpawner>();
        if (existingSpawner == null)
        {
            GameObject spawnerObject = new GameObject("PatientSpawner");
            PatientSpawner spawner = spawnerObject.AddComponent<PatientSpawner>();
            
            // Configure spawner
            if (patientPrefab != null)
            {
                var spawnerType = typeof(PatientSpawner);
                var patientPrefabField = spawnerType.GetField("patientPrefab", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                patientPrefabField?.SetValue(spawner, patientPrefab);
                
                var numberOfPatientsField = spawnerType.GetField("numberOfPatients", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                numberOfPatientsField?.SetValue(spawner, numberOfPatients);
                
                var spawnRadiusField = spawnerType.GetField("spawnRadius", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                spawnRadiusField?.SetValue(spawner, spawnRadius);
                
                var maxWanderRadiusField = spawnerType.GetField("maxWanderRadius", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                maxWanderRadiusField?.SetValue(spawner, maxWanderRadius);
                
                Debug.Log("PatientSpawner created and configured");
            }
            else
            {
                Debug.LogWarning("Patient prefab not assigned! Please assign it in the inspector.");
            }
        }
        else
        {
            Debug.Log("PatientSpawner already exists");
        }
    }
    
    void SetupSanityManager()
    {
        SanityManager existingSM = FindFirstObjectByType<SanityManager>();
        if (existingSM == null)
        {
            GameObject smObject = new GameObject("SanityManager");
            smObject.AddComponent<SanityManager>();
            Debug.Log("SanityManager created");
        }
        else
        {
            Debug.Log("SanityManager already exists");
        }
    }
    
    void SetupSanityUI()
    {
        // Find or create canvas
        Canvas canvas = targetCanvas;
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
        
        if (canvas == null)
        {
            // Create canvas
            GameObject canvasObject = new GameObject("SanityCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Check if sanity text already exists
        TextMeshProUGUI existingSanityText = FindTextWithName("SanityText");
        if (existingSanityText == null)
        {
            // Create sanity text
            GameObject sanityTextObject = new GameObject("SanityText");
            sanityTextObject.transform.SetParent(canvas.transform, false);
            
            TextMeshProUGUI sanityText = sanityTextObject.AddComponent<TextMeshProUGUI>();
            sanityText.text = "Sanity: 100/100";
            sanityText.fontSize = 24;
            sanityText.color = Color.white;
            sanityText.alignment = TextAlignmentOptions.TopLeft;
            
            // Position the text
            RectTransform rectTransform = sanityText.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = sanityUIPosition;
            rectTransform.sizeDelta = new Vector2(200, 50);
            
            Debug.Log("Sanity UI created");
            
            // Link to SanityManager
            SanityManager sanityManager = FindFirstObjectByType<SanityManager>();
            if (sanityManager != null)
            {
                var sanityManagerType = typeof(SanityManager);
                var sanityDisplayField = sanityManagerType.GetField("sanityDisplay", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                sanityDisplayField?.SetValue(sanityManager, sanityText);
                Debug.Log("Sanity UI linked to SanityManager");
            }
        }
        else
        {
            Debug.Log("Sanity UI already exists");
        }
    }
    
    TextMeshProUGUI FindTextWithName(string name)
    {
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var text in allTexts)
        {
            if (text.gameObject.name == name)
            {
                return text;
            }
        }
        return null;
    }
    
    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        Debug.Log("=== Sanity System Validation ===");
        
        // Check GameManager
        GameManager gm = FindFirstObjectByType<GameManager>();
        Debug.Log($"GameManager: {(gm != null ? "✓ Found" : "✗ Missing")}");
        
        // Check PatientSpawner
        PatientSpawner ps = FindFirstObjectByType<PatientSpawner>();
        Debug.Log($"PatientSpawner: {(ps != null ? "✓ Found" : "✗ Missing")}");
        
        // Check SanityManager
        SanityManager sm = FindFirstObjectByType<SanityManager>();
        Debug.Log($"SanityManager: {(sm != null ? "✓ Found" : "✗ Missing")}");
        
        // Check LightingManager
        LightingManager lm = FindFirstObjectByType<LightingManager>();
        Debug.Log($"LightingManager: {(lm != null ? "✓ Found" : "✗ Missing")}");
        
        // Check Patient Prefab
        Debug.Log($"Patient Prefab: {(patientPrefab != null ? "✓ Assigned" : "✗ Not assigned")}");
        
        // Check UI
        TextMeshProUGUI sanityUI = FindTextWithName("SanityText");
        Debug.Log($"Sanity UI: {(sanityUI != null ? "✓ Found" : "✗ Missing")}");
        
        Debug.Log("=== Validation Complete ===");
    }
}
