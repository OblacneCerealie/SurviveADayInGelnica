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
    
    // Private variables
    private bool lightsCurrentlyOff = false;
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
        }
    }
    
    void CheckLightingThreshold()
    {
        if (lightingManager == null) return;
        
        if (currentSanity < lightOffThreshold && !lightsCurrentlyOff)
        {
            // Turn off lights
            lightingManager.SetLights(false);
            lightsCurrentlyOff = true;
            OnSanityBelowThreshold?.Invoke();
        }
        else if (currentSanity >= lightOffThreshold && lightsCurrentlyOff)
        {
            // Turn on lights
            lightingManager.SetLights(true);
            lightsCurrentlyOff = false;
            OnSanityAboveThreshold?.Invoke();
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
}
