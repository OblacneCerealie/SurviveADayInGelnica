using UnityEngine;
using TMPro;
using System.Collections;

public class SanityManager : MonoBehaviour
{
    [Header("Sanity Settings")]
    [SerializeField] private int maxSanity = 100;
    [SerializeField] private int currentSanity = 100;
    [SerializeField] private float sanityDecreaseInterval = 2f; // Decrease every 2 seconds
    [SerializeField] private int sanityDecreaseAmount = 1;
    [SerializeField] private int lightOffThreshold = 50; // Turn off lights when below this value
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI sanityDisplay;
    
    [Header("Lighting Control")]
    [SerializeField] private LightingManager lightingManager;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Events
    public static System.Action<int> OnSanityChanged;
    public static System.Action OnSanityBelowThreshold;
    public static System.Action OnSanityAboveThreshold;
    public static System.Action OnSanityReachedZero;
    public static System.Action OnSanityRestoredFromZero;
    
    // Private variables
    private bool lightsCurrentlyOff = false;
    private bool generatorOverrideActive = false; // Track if generator has overridden lighting
    private Coroutine sanityDecreaseCoroutine;
    
    // Singleton pattern for easy access
    public static SanityManager Instance { get; private set; }
    
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
        Initialize();
        StartSanityDecrease();
    }
    
    void Initialize()
    {
        // Find LightingManager if not assigned
        if (lightingManager == null)
        {
            lightingManager = LightingManager.Instance;
            if (lightingManager == null)
            {
                lightingManager = FindFirstObjectByType<LightingManager>();
            }
        }
        
        // Find sanity display if not assigned
        if (sanityDisplay == null)
        {
            // Try to find TextMeshPro component in children or scene
            sanityDisplay = GetComponentInChildren<TextMeshProUGUI>();
            if (sanityDisplay == null)
            {
                sanityDisplay = FindFirstObjectByType<TextMeshProUGUI>();
            }
        }
        
        // Set initial sanity
        currentSanity = maxSanity;
        UpdateSanityDisplay();
        
        if (showDebugInfo)
        {
            Debug.Log($"SanityManager initialized. Starting sanity: {currentSanity}");
        }
    }
    
    void StartSanityDecrease()
    {
        if (sanityDecreaseCoroutine != null)
        {
            StopCoroutine(sanityDecreaseCoroutine);
        }
        
        sanityDecreaseCoroutine = StartCoroutine(SanityDecreaseRoutine());
    }
    
    IEnumerator SanityDecreaseRoutine()
    {
        while (currentSanity > 0)
        {
            yield return new WaitForSeconds(sanityDecreaseInterval);
            
            DecreaseSanity(sanityDecreaseAmount);
        }
        
        if (showDebugInfo)
        {
            Debug.Log("Sanity has reached 0! Game over condition might be triggered here.");
        }
    }
    
    public void DecreaseSanity(int amount)
    {
        int oldSanity = currentSanity;
        currentSanity = Mathf.Max(0, currentSanity - amount);
        
        if (oldSanity != currentSanity)
        {
            OnSanityChanged?.Invoke(currentSanity);
            UpdateSanityDisplay();
            CheckLightingThreshold();
            
            // Trigger special event when sanity reaches 0
            if (oldSanity > 0 && currentSanity == 0)
            {
                OnSanityReachedZero?.Invoke();
                if (showDebugInfo)
                {
                    Debug.Log("SanityManager: Sanity reached 0 - Monster should activate, patients should freeze");
                }
            }
        }
    }
    
    public void IncreaseSanity(int amount)
    {
        int oldSanity = currentSanity;
        currentSanity = Mathf.Min(maxSanity, currentSanity + amount);
        
        if (oldSanity != currentSanity)
        {
            OnSanityChanged?.Invoke(currentSanity);
            UpdateSanityDisplay();
            CheckLightingThreshold();
            
            // If sanity was at 0 and is now above 0, restart the draining routine
            if (oldSanity == 0 && currentSanity > 0)
            {
                OnSanityRestoredFromZero?.Invoke();
                StartSanityDecrease();
                if (showDebugInfo)
                {
                    Debug.Log("SanityManager: Sanity restored above 0 - Monster should deactivate, patients should unfreeze, resuming sanity decrease");
                }
            }
        }
    }
    
    void CheckLightingThreshold()
    {
        if (lightingManager == null) return;
        
        // If sanity drops to 0, turn off lights and disable generator override
        if (currentSanity == 0 && !lightsCurrentlyOff)
        {
            lightingManager.SetLights(false, true); // Force override to turn off
            lightsCurrentlyOff = true;
            generatorOverrideActive = false; // Disable generator override at 0 sanity
            OnSanityBelowThreshold?.Invoke();
            
            if (showDebugInfo)
            {
                Debug.Log("SanityManager: Sanity at 0 - turning off lights, generator disabled");
            }
        }
        // If sanity is below threshold but above 0, and no generator override, turn off lights
        else if (currentSanity < lightOffThreshold && currentSanity > 0 && !lightsCurrentlyOff && !generatorOverrideActive)
        {
            lightingManager.SetLights(false);
            lightsCurrentlyOff = true;
            OnSanityBelowThreshold?.Invoke();
            
            if (showDebugInfo)
            {
                Debug.Log("SanityManager: Sanity below threshold - turning off lights");
            }
        }
        // If sanity is restored to normal levels, turn on lights automatically and clear override
        else if (currentSanity >= lightOffThreshold && lightsCurrentlyOff)
        {
            lightingManager.SetLights(true);
            lightsCurrentlyOff = false;
            generatorOverrideActive = false; // Clear override state when sanity is restored
            OnSanityAboveThreshold?.Invoke();
            
            if (showDebugInfo)
            {
                Debug.Log("SanityManager: Sanity restored - turning on lights automatically");
            }
        }
    }
    
    void UpdateSanityDisplay()
    {
        if (sanityDisplay != null)
        {
            sanityDisplay.text = $"Sanity: {currentSanity}/{maxSanity}";
            
            // Optional: Change text color based on sanity level
            if (currentSanity < lightOffThreshold)
            {
                sanityDisplay.color = Color.red;
            }
            else if (currentSanity < (maxSanity * 0.75f))
            {
                sanityDisplay.color = Color.yellow;
            }
            else
            {
                sanityDisplay.color = Color.white;
            }
        }
    }
    
    // Public getters
    public int GetCurrentSanity() => currentSanity;
    public int GetMaxSanity() => maxSanity;
    public float GetSanityPercentage() => (float)currentSanity / maxSanity;
    public bool AreLightsOff() => lightsCurrentlyOff;
    public int GetLightOffThreshold() => lightOffThreshold;
    
    // Method to check if lights can be automatically restored
    public bool CanAutomaticallyRestoreLights() => currentSanity >= lightOffThreshold;
    
    // Method to check if switches can work normally (after generator activation or high sanity)
    public bool CanUseLightSwitches() => currentSanity >= lightOffThreshold || generatorOverrideActive;
    
    // Method to check if generator can work
    public bool CanUseGenerator() => currentSanity > 0;
    
    // Method for light generator to force lights on regardless of sanity
    public void ForceLightsOn()
    {
        if (lightingManager == null) return;
        
        // Check if generator can work (sanity must be > 0)
        if (currentSanity == 0)
        {
            if (showDebugInfo)
            {
                Debug.Log("SanityManager: Generator cannot work - sanity is at 0!");
            }
            return;
        }
        
        lightingManager.SetLights(true, true); // Use force override
        lightsCurrentlyOff = false;
        generatorOverrideActive = true; // Mark that generator override is active (permanent until sanity changes)
        
        if (showDebugInfo)
        {
            Debug.Log($"SanityManager: Lights forced ON by generator (sanity: {currentSanity}) - switches now work normally");
        }
    }
    
    // Public methods for external control
    public void SetSanity(int newSanity)
    {
        int oldSanity = currentSanity;
        currentSanity = Mathf.Clamp(newSanity, 0, maxSanity);
        
        if (oldSanity != currentSanity)
        {
            OnSanityChanged?.Invoke(currentSanity);
            UpdateSanityDisplay();
            CheckLightingThreshold();
            
            // Trigger special events for sanity reaching/leaving 0
            if (oldSanity > 0 && currentSanity == 0)
            {
                OnSanityReachedZero?.Invoke();
                if (showDebugInfo)
                {
                    Debug.Log("SanityManager: Sanity reached 0 - Monster should activate, patients should freeze");
                }
            }
            else if (oldSanity == 0 && currentSanity > 0)
            {
                OnSanityRestoredFromZero?.Invoke();
                StartSanityDecrease();
                if (showDebugInfo)
                {
                    Debug.Log("SanityManager: Sanity restored above 0 - Monster should deactivate, patients should unfreeze, resuming sanity decrease");
                }
            }
        }
    }
    
    public void PauseSanityDecrease()
    {
        if (sanityDecreaseCoroutine != null)
        {
            StopCoroutine(sanityDecreaseCoroutine);
            sanityDecreaseCoroutine = null;
        }
    }
    
    public void ResumeSanityDecrease()
    {
        if (sanityDecreaseCoroutine == null)
        {
            StartSanityDecrease();
        }
    }
    
    public void ResetSanity()
    {
        SetSanity(maxSanity);
    }
    
    void OnDestroy()
    {
        if (sanityDecreaseCoroutine != null)
        {
            StopCoroutine(sanityDecreaseCoroutine);
        }
    }
    
    // Debug/Test methods
    [ContextMenu("Test: Set Sanity to 30 (Below Threshold)")]
    public void TestSetSanityLow()
    {
        SetSanity(30);
        Debug.Log("TEST: Sanity set to 30 - lights should turn off and only generator can restore them");
    }
    
    [ContextMenu("Test: Set Sanity to 60 (Above Threshold)")]
    public void TestSetSanityHigh()
    {
        SetSanity(60);
        Debug.Log("TEST: Sanity set to 60 - lights should automatically turn on");
    }
    
    [ContextMenu("Test: Try to force lights on manually")]
    public void TestForceLights()
    {
        ForceLightsOn();
        Debug.Log("TEST: Attempted to force lights on regardless of sanity level");
    }
    
    [ContextMenu("Test: Simulate Generator at Sanity 30")]
    public void TestGeneratorWithLowSanity()
    {
        SetSanity(30); // Set low sanity
        ForceLightsOn(); // Simulate generator activation
        Debug.Log("TEST: Generator activated at sanity 30 - lights should stay on permanently, switches should work");
    }
    
    [ContextMenu("Test: Simulate Generator at Sanity 0")]
    public void TestGeneratorAtZeroSanity()
    {
        SetSanity(0); // Set zero sanity
        ForceLightsOn(); // Try to activate generator
        Debug.Log("TEST: Attempted generator activation at sanity 0 - should fail completely");
    }
    
    [ContextMenu("Test: Check Switch Status")]
    public void TestSwitchStatus()
    {
        bool canUse = CanUseLightSwitches();
        bool canAuto = CanAutomaticallyRestoreLights();
        bool canGen = CanUseGenerator();
        bool isDraining = (sanityDecreaseCoroutine != null);
        Debug.Log($"TEST: Sanity {currentSanity} - Can use switches: {canUse}, Auto restore: {canAuto}, Generator works: {canGen}, Override active: {generatorOverrideActive}, Draining: {isDraining}");
    }
    
    [ContextMenu("Test: Sanity Recovery from Zero")]
    public void TestSanityRecoveryFromZero()
    {
        SetSanity(0); // Set to zero
        Debug.Log("TEST: Set sanity to 0 - draining should stop");
        
        // Wait a moment then restore sanity
        Invoke(nameof(RestoreSanityAfterDelay), 2f);
    }
    
    [ContextMenu("Test: Force Sanity to 0")]
    public void TestForceSanityToZero()
    {
        SetSanity(0);
        Debug.Log("TEST: Forced sanity to 0 - Monster should activate, patients should freeze");
    }
    
    private void RestoreSanityAfterDelay()
    {
        SetSanity(20); // Restore to 20
        Debug.Log("TEST: Restored sanity to 20 - draining should resume");
    }
}
