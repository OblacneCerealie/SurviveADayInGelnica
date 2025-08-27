using UnityEngine;
using System.Collections;

public class FlashlightBattery : MonoBehaviour
{
    [Header("Battery Settings")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float currentBattery = 100f;
    [SerializeField] private float drainRate = 1f; // Percentage per drain interval
    [SerializeField] private float drainInterval = 6f; // Seconds between drain
    
    [Header("Battery Pickup Settings")]
    [SerializeField] private float batteryRecharge = 25f; // How much a battery pickup restores
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Events
    public static System.Action<float, float> OnBatteryChanged; // current, max
    public static System.Action OnBatteryDepleted;
    public static System.Action OnBatteryRestored;
    
    // Private variables
    private bool isFlashlightOn = false;
    private bool hasFlashlight = false;
    private Coroutine batteryDrainCoroutine;
    private FlashlightController flashlightController;
    private PlayerInventory playerInventory;
    
    // Singleton pattern for easy access
    public static FlashlightBattery Instance { get; private set; }
    
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
        InitializeBattery();
    }
    
    void InitializeBattery()
    {
        // Find required components
        flashlightController = FindFirstObjectByType<FlashlightController>();
        playerInventory = FindFirstObjectByType<PlayerInventory>();
        
        // Set initial battery to full
        currentBattery = maxBattery;
        
        if (showDebugInfo)
        {
            Debug.Log($"FlashlightBattery initialized. Battery: {currentBattery}/{maxBattery}");
        }
        
        // Delay the initial UI notification to ensure UI is ready
        Invoke(nameof(SendInitialBatteryState), 0.1f);
    }
    
    void SendInitialBatteryState()
    {
        // Notify UI of initial state
        OnBatteryChanged?.Invoke(currentBattery, maxBattery);
        
        if (showDebugInfo)
        {
            Debug.Log($"FlashlightBattery: Sent initial battery state - {currentBattery}%");
        }
    }
    
    void Update()
    {
        UpdateFlashlightStatus();
    }
    
    void UpdateFlashlightStatus()
    {
        if (playerInventory == null || flashlightController == null) return;
        
        bool currentlyHasFlashlight = playerInventory.HasFlashlight();
        bool currentlyOn = flashlightController.IsFlashlightOn;
        
        // Check if flashlight status changed
        if (hasFlashlight != currentlyHasFlashlight)
        {
            hasFlashlight = currentlyHasFlashlight;
            HandleFlashlightStatusChange();
        }
        
        // Check if flashlight on/off status changed
        if (isFlashlightOn != currentlyOn)
        {
            isFlashlightOn = currentlyOn;
            HandleFlashlightToggle();
        }
    }
    
    void HandleFlashlightStatusChange()
    {
        if (hasFlashlight)
        {
            if (showDebugInfo)
            {
                Debug.Log("FlashlightBattery: Flashlight equipped - battery system active");
            }
        }
        else
        {
            // Stop draining when flashlight is not equipped
            StopBatteryDrain();
            if (showDebugInfo)
            {
                Debug.Log("FlashlightBattery: Flashlight unequipped - battery system inactive");
            }
        }
    }
    
    void HandleFlashlightToggle()
    {
        if (!hasFlashlight) return;
        
        if (isFlashlightOn)
        {
            // Check if battery is depleted
            if (currentBattery <= 0)
            {
                // Force flashlight off if battery is dead
                if (flashlightController != null)
                {
                    flashlightController.ForceFlashlightState(false);
                }
                
                if (showDebugInfo)
                {
                    Debug.Log("FlashlightBattery: Cannot turn on - battery depleted!");
                }
                return;
            }
            
            // Start draining battery
            StartBatteryDrain();
            
            if (showDebugInfo)
            {
                Debug.Log("FlashlightBattery: Flashlight ON - battery draining");
            }
        }
        else
        {
            // Stop draining battery
            StopBatteryDrain();
            
            if (showDebugInfo)
            {
                Debug.Log("FlashlightBattery: Flashlight OFF - battery drain stopped");
            }
        }
    }
    
    void StartBatteryDrain()
    {
        if (batteryDrainCoroutine != null)
        {
            StopCoroutine(batteryDrainCoroutine);
        }
        
        batteryDrainCoroutine = StartCoroutine(BatteryDrainRoutine());
    }
    
    void StopBatteryDrain()
    {
        if (batteryDrainCoroutine != null)
        {
            StopCoroutine(batteryDrainCoroutine);
            batteryDrainCoroutine = null;
        }
    }
    
    IEnumerator BatteryDrainRoutine()
    {
        while (currentBattery > 0 && isFlashlightOn && hasFlashlight)
        {
            yield return new WaitForSeconds(drainInterval);
            
            DrainBattery(drainRate);
            
            // Check if battery is now depleted
            if (currentBattery <= 0)
            {
                HandleBatteryDepletion();
                break;
            }
        }
    }
    
    void DrainBattery(float amount)
    {
        float oldBattery = currentBattery;
        currentBattery = Mathf.Max(0, currentBattery - amount);
        
        if (oldBattery != currentBattery)
        {
            OnBatteryChanged?.Invoke(currentBattery, maxBattery);
            
            if (showDebugInfo)
            {
                Debug.Log($"FlashlightBattery: Battery drained to {currentBattery:F1}%");
            }
        }
    }
    
    void HandleBatteryDepletion()
    {
        // Force flashlight off
        if (flashlightController != null)
        {
            flashlightController.ForceFlashlightState(false);
        }
        
        OnBatteryDepleted?.Invoke();
        
        if (showDebugInfo)
        {
            Debug.Log("FlashlightBattery: Battery depleted! Flashlight disabled.");
        }
    }
    
    public void RechargeBattery(float amount)
    {
        float oldBattery = currentBattery;
        currentBattery = Mathf.Min(maxBattery, currentBattery + amount);
        
        if (oldBattery != currentBattery)
        {
            OnBatteryChanged?.Invoke(currentBattery, maxBattery);
            
            // If battery was at 0 and now has charge, notify restoration
            if (oldBattery <= 0 && currentBattery > 0)
            {
                OnBatteryRestored?.Invoke();
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"FlashlightBattery: Battery recharged by {amount}%. Current: {currentBattery:F1}%");
            }
        }
    }
    
    public void UseBatteryPickup()
    {
        RechargeBattery(batteryRecharge);
    }
    
    // Public getters
    public float GetCurrentBattery() => currentBattery;
    public float GetMaxBattery() => maxBattery;
    public float GetBatteryPercentage() => currentBattery / maxBattery;
    public bool IsBatteryDepleted() => currentBattery <= 0;
    public bool CanUseFlashlight() => hasFlashlight && currentBattery > 0;
    
    // Public methods for external control
    public void SetBattery(float newBattery)
    {
        float oldBattery = currentBattery;
        currentBattery = Mathf.Clamp(newBattery, 0, maxBattery);
        
        if (oldBattery != currentBattery)
        {
            OnBatteryChanged?.Invoke(currentBattery, maxBattery);
            
            if (oldBattery <= 0 && currentBattery > 0)
            {
                OnBatteryRestored?.Invoke();
            }
            else if (oldBattery > 0 && currentBattery <= 0)
            {
                HandleBatteryDepletion();
            }
        }
    }
    
    public void ResetBattery()
    {
        SetBattery(maxBattery);
    }
    
    void OnDestroy()
    {
        StopBatteryDrain();
    }
    
    // Debug/Test methods
    [ContextMenu("Test: Set Battery to 10%")]
    public void TestSetBatteryLow()
    {
        SetBattery(10f);
        Debug.Log("TEST: Battery set to 10%");
    }
    
    [ContextMenu("Test: Deplete Battery")]
    public void TestDepleteBattery()
    {
        SetBattery(0f);
        Debug.Log("TEST: Battery depleted");
    }
    
    [ContextMenu("Test: Recharge Battery")]
    public void TestRechargeBattery()
    {
        UseBatteryPickup();
        Debug.Log("TEST: Battery pickup used (+25%)");
    }
    
    [ContextMenu("Test: Full Battery")]
    public void TestFullBattery()
    {
        ResetBattery();
        Debug.Log("TEST: Battery reset to 100%");
    }
}
