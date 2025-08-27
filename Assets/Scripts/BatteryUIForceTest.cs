using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BatteryUIForceTest : MonoBehaviour
{
    public GameObject batteryPanel;
    public TextMeshProUGUI batteryText;
    public Slider batterySlider;
    
    private PlayerInventory playerInventory;
    
    void Start()
    {
        Debug.Log("BatteryUIForceTest: Starting!");
        
        // Find player inventory
        playerInventory = FindFirstObjectByType<PlayerInventory>();
        if (playerInventory != null)
        {
            Debug.Log($"BatteryUIForceTest: Found PlayerInventory on {playerInventory.gameObject.name}");
        }
        else
        {
            Debug.LogError("BatteryUIForceTest: No PlayerInventory found!");
        }
        
        // Hide panel content initially but keep panel active so Update() runs
        if (batteryPanel != null)
        {
            // Keep panel active but hide its content
            batteryPanel.SetActive(true);
            if (batteryText != null) batteryText.gameObject.SetActive(false);
            if (batterySlider != null) batterySlider.gameObject.SetActive(false);
            Debug.Log("BatteryUIForceTest: Panel content hidden initially, but panel stays active");
        }
        
        // Setup slider
        if (batterySlider != null)
        {
            batterySlider.minValue = 0f;
            batterySlider.maxValue = 100f;
        }
    }
    
    void Update()
    {
        if (playerInventory != null)
        {
            bool hasFlashlight = playerInventory.HasFlashlight();
            
            // Debug every few seconds to see what's happening
            if (Time.frameCount % 180 == 0) // Every 3 seconds at 60fps
            {
                Debug.Log($"BatteryUIForceTest: Has flashlight: {hasFlashlight}, Panel active: {(batteryPanel != null ? batteryPanel.activeSelf : false)}");
            }
            
            if (hasFlashlight)
            {
                // Show panel content when flashlight is equipped
                if (batteryText != null && !batteryText.gameObject.activeSelf)
                {
                    batteryText.gameObject.SetActive(true);
                    Debug.Log("BatteryUIForceTest: Battery text shown - flashlight equipped!");
                }
                if (batterySlider != null && !batterySlider.gameObject.activeSelf)
                {
                    batterySlider.gameObject.SetActive(true);
                    Debug.Log("BatteryUIForceTest: Battery slider shown - flashlight equipped!");
                }
            }
            else
            {
                // Hide panel content when flashlight is not equipped
                if (batteryText != null && batteryText.gameObject.activeSelf)
                {
                    batteryText.gameObject.SetActive(false);
                    Debug.Log("BatteryUIForceTest: Battery text hidden - flashlight removed!");
                }
                if (batterySlider != null && batterySlider.gameObject.activeSelf)
                {
                    batterySlider.gameObject.SetActive(false);
                    Debug.Log("BatteryUIForceTest: Battery slider hidden - flashlight removed!");
                }
            }
            
            // Update battery display
            if (FlashlightBattery.Instance != null && hasFlashlight)
            {
                float current = FlashlightBattery.Instance.GetCurrentBattery();
                
                if (batteryText != null)
                {
                    batteryText.text = $"Battery: {current:F0}%";
                    
                    // Add color coding
                    if (current <= 0)
                    {
                        batteryText.color = Color.gray;
                    }
                    else if (current <= 25f)
                    {
                        batteryText.color = Color.red;
                    }
                    else if (current <= 50f)
                    {
                        batteryText.color = Color.yellow;
                    }
                    else
                    {
                        batteryText.color = Color.green;
                    }
                }
                
                if (batterySlider != null)
                {
                    batterySlider.value = current;
                    
                    // Color the slider fill too
                    UnityEngine.UI.Image fillImage = batterySlider.fillRect.GetComponent<UnityEngine.UI.Image>();
                    if (fillImage != null)
                    {
                        if (current <= 0)
                        {
                            fillImage.color = Color.gray;
                        }
                        else if (current <= 25f)
                        {
                            fillImage.color = Color.red;
                        }
                        else if (current <= 50f)
                        {
                            fillImage.color = Color.yellow;
                        }
                        else
                        {
                            fillImage.color = Color.green;
                        }
                    }
                }
            }
        }
    }
}
