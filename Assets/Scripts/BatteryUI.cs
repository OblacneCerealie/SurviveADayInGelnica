using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BatteryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject batteryPanel;
    [SerializeField] private TextMeshProUGUI batteryText;
    [SerializeField] private Slider batterySlider;
    [SerializeField] private Image batteryFillImage;
    
    [Header("Visual Settings")]
    [SerializeField] private Color fullBatteryColor = Color.green;
    [SerializeField] private Color mediumBatteryColor = Color.yellow;
    [SerializeField] private Color lowBatteryColor = Color.red;
    [SerializeField] private Color depletedBatteryColor = Color.gray;
    [SerializeField] private float lowBatteryThreshold = 25f;
    [SerializeField] private float mediumBatteryThreshold = 50f;
    
    [Header("Animation")]
    [SerializeField] private bool useBlinkingEffect = true;
    [SerializeField] private float blinkSpeed = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    private PlayerInventory playerInventory;
    private bool hasFlashlight = false;
    private bool isBlinking = false;
    
    void Awake()
    {

        InitializeBatteryUI();
        SetupEventListeners();
    }
    
    void Start()
    {
        // Start method kept for any future initialization that needs the scene fully loaded
    }
    
    void InitializeBatteryUI()
    {
        // Find player inventory
        playerInventory = FindFirstObjectByType<PlayerInventory>();
        if (playerInventory == null)
        {

            
            // Try to find by player tag as backup
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerInventory = player.GetComponent<PlayerInventory>();
                if (playerInventory != null)
                {

                }
                else
                {

                }
            }
        }
        else
        {


        }
        
        // Auto-find UI components if not assigned
        if (batteryPanel == null)
        {
            batteryPanel = GameObject.Find("BatteryPanel");
        }
        
        if (batteryText == null && batteryPanel != null)
        {
            batteryText = batteryPanel.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (batterySlider == null && batteryPanel != null)
        {
            batterySlider = batteryPanel.GetComponentInChildren<Slider>();
        }
        
        if (batteryFillImage == null && batterySlider != null)
        {
            batteryFillImage = batterySlider.fillRect.GetComponent<Image>();
        }
        
        // Setup initial state
        if (batterySlider != null)
        {
            batterySlider.minValue = 0f;
            batterySlider.maxValue = 100f;
        }
        
        // Ensure panel is active for initialization, then hide it
        if (batteryPanel != null)
        {
            batteryPanel.SetActive(true);
        }
        SetPanelVisibility(false);
        
        if (showDebugInfo)
        {

        }
    }
    
    void SetupEventListeners()
    {
        // Subscribe to battery events
        FlashlightBattery.OnBatteryChanged += OnBatteryChanged;
        FlashlightBattery.OnBatteryDepleted += OnBatteryDepleted;
        FlashlightBattery.OnBatteryRestored += OnBatteryRestored;
    }
    
    void Update()
    {
        UpdateFlashlightStatus();
        UpdateBlinkingEffect();
    }
    
    void UpdateFlashlightStatus()
    {
        if (playerInventory == null)
        {

            playerInventory = FindFirstObjectByType<PlayerInventory>();
            return;
        }
        
        bool currentlyHasFlashlight = playerInventory.HasFlashlight();
        
        // Removed spam debug - only show when status changes
        
        if (hasFlashlight != currentlyHasFlashlight)
        {
            hasFlashlight = currentlyHasFlashlight;
            SetPanelVisibility(hasFlashlight);
            
            if (showDebugInfo)
            {

            }
            
            // When flashlight is first equipped, update the display immediately
            if (hasFlashlight && FlashlightBattery.Instance != null)
            {
                float current = FlashlightBattery.Instance.GetCurrentBattery();
                float max = FlashlightBattery.Instance.GetMaxBattery();
                UpdateBatteryDisplay(current, max);
                if (showDebugInfo)
                {

                }
            }
        }
    }
    
    void UpdateBlinkingEffect()
    {
        if (!isBlinking || batteryFillImage == null) return;
        
        // Create blinking effect by modulating alpha
        float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f; // 0 to 1
        alpha = Mathf.Lerp(0.3f, 1f, alpha); // Don't go completely transparent
        
        Color currentColor = batteryFillImage.color;
        currentColor.a = alpha;
        batteryFillImage.color = currentColor;
    }
    
    void SetPanelVisibility(bool visible)
    {
        if (batteryPanel != null)
        {
            batteryPanel.SetActive(visible);
        }
    }
    
    void OnBatteryChanged(float currentBattery, float maxBattery)
    {
        UpdateBatteryDisplay(currentBattery, maxBattery);
    }
    
    void OnBatteryDepleted()
    {
        if (useBlinkingEffect)
        {
            StartBlinking();
        }
        
        if (showDebugInfo)
        {

        }
    }
    
    void OnBatteryRestored()
    {
        StopBlinking();
        
        if (showDebugInfo)
        {

        }
    }
    
    void UpdateBatteryDisplay(float currentBattery, float maxBattery)
    {
        float percentage = (currentBattery / maxBattery) * 100f;
        
        // Update text
        if (batteryText != null)
        {
            batteryText.text = $"Battery: {percentage:F0}%";
        }
        
        // Update slider
        if (batterySlider != null)
        {
            batterySlider.value = percentage;
        }
        
        // Update color based on battery level
        UpdateBatteryColor(percentage);
        
        // Stop blinking if battery is restored above 0
        if (currentBattery > 0)
        {
            StopBlinking();
        }
    }
    
    void UpdateBatteryColor(float percentage)
    {
        if (batteryFillImage == null) return;
        
        Color targetColor;
        
        if (percentage <= 0)
        {
            targetColor = depletedBatteryColor;
        }
        else if (percentage <= lowBatteryThreshold)
        {
            targetColor = lowBatteryColor;
        }
        else if (percentage <= mediumBatteryThreshold)
        {
            targetColor = mediumBatteryColor;
        }
        else
        {
            targetColor = fullBatteryColor;
        }
        
        // Preserve alpha if blinking
        if (isBlinking)
        {
            targetColor.a = batteryFillImage.color.a;
        }
        
        batteryFillImage.color = targetColor;
        
        // Update text color too
        if (batteryText != null)
        {
            batteryText.color = targetColor;
        }
    }
    
    void StartBlinking()
    {
        isBlinking = true;
    }
    
    void StopBlinking()
    {
        isBlinking = false;
        
        // Restore full alpha
        if (batteryFillImage != null)
        {
            Color currentColor = batteryFillImage.color;
            currentColor.a = 1f;
            batteryFillImage.color = currentColor;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        FlashlightBattery.OnBatteryChanged -= OnBatteryChanged;
        FlashlightBattery.OnBatteryDepleted -= OnBatteryDepleted;
        FlashlightBattery.OnBatteryRestored -= OnBatteryRestored;
    }
    
    // Public methods for external control
    public void SetBatteryThresholds(float low, float medium)
    {
        lowBatteryThreshold = low;
        mediumBatteryThreshold = medium;
    }
    
    public void SetBatteryColors(Color full, Color medium, Color low, Color depleted)
    {
        fullBatteryColor = full;
        mediumBatteryColor = medium;
        lowBatteryColor = low;
        depletedBatteryColor = depleted;
    }
    
    // Method to manually update display (useful for testing)
    [ContextMenu("Test: Update Display")]
    public void TestUpdateDisplay()
    {
        if (FlashlightBattery.Instance != null)
        {
            float current = FlashlightBattery.Instance.GetCurrentBattery();
            float max = FlashlightBattery.Instance.GetMaxBattery();
            UpdateBatteryDisplay(current, max);
        }
    }
    
    [ContextMenu("Test: Show Panel")]
    public void TestShowPanel()
    {
        SetPanelVisibility(true);
        Debug.Log("TEST: Forcing panel to show");
    }
    
    [ContextMenu("Test: Hide Panel")]
    public void TestHidePanel()
    {
        SetPanelVisibility(false);
        Debug.Log("TEST: Forcing panel to hide");
    }
}
