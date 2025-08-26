using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Components")]
    [SerializeField] private PatientSpawner patientSpawner;
    [SerializeField] private SanityManager sanityManager;
    [SerializeField] private LightingManager lightingManager;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    void Start()
    {
        InitializeGame();
    }
    
    void InitializeGame()
    {
        // Find components if not assigned
        if (patientSpawner == null)
        {
            patientSpawner = FindFirstObjectByType<PatientSpawner>();
        }
        
        if (sanityManager == null)
        {
            sanityManager = FindFirstObjectByType<SanityManager>();
        }
        
        if (lightingManager == null)
        {
            lightingManager = FindFirstObjectByType<LightingManager>();
        }
        
        // Subscribe to sanity events
        SanityManager.OnSanityChanged += OnSanityChanged;
        SanityManager.OnSanityBelowThreshold += OnSanityBelowThreshold;
        SanityManager.OnSanityAboveThreshold += OnSanityAboveThreshold;
        
        if (showDebugInfo)
        {
            Debug.Log("GameManager initialized. All systems ready.");
        }
    }
    
    void OnSanityChanged(int newSanity)
    {
        // Here you can add additional game logic based on sanity changes
        // For example, affect patient behavior, spawn events, etc.
    }
    
    void OnSanityBelowThreshold()
    {
        // Additional logic when entering dark mode
        // Could affect patient behavior, spawn additional threats, etc.
    }
    
    void OnSanityAboveThreshold()
    {
        // Additional logic when returning to normal mode
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        SanityManager.OnSanityChanged -= OnSanityChanged;
        SanityManager.OnSanityBelowThreshold -= OnSanityBelowThreshold;
        SanityManager.OnSanityAboveThreshold -= OnSanityAboveThreshold;
    }
    
    // Public methods for external control
    public PatientSpawner GetPatientSpawner() => patientSpawner;
    public SanityManager GetSanityManager() => sanityManager;
    public LightingManager GetLightingManager() => lightingManager;
}
