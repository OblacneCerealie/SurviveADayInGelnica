using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public KeyCode toggleKey = KeyCode.F;
    public Light flashlightLight;
    public Transform flashlightAttachPoint; // Where to attach the flashlight (usually player camera or hand)
    
    [Header("Audio")]
    public AudioClip flashlightOnSound;
    public AudioClip flashlightOffSound;
    public AudioSource audioSource;
    
    [Header("Visual Feedback")]
    public GameObject flashlightModel; // Optional: 3D model of flashlight to show when equipped
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private bool hasFlashlight = false;
    private bool isFlashlightOn = false;
    private PlayerInventory playerInventory;
    
    void Start()
    {
        InitializeFlashlight();
    }
    
    void InitializeFlashlight()
    {
        // Find player inventory
        playerInventory = FindFirstObjectByType<PlayerInventory>();
        if (playerInventory == null)
        {

        }
        
        // Find attach point if not assigned (usually the camera)
        if (flashlightAttachPoint == null)
        {
            Camera playerCamera = Camera.main;
            if (playerCamera != null)
            {
                flashlightAttachPoint = playerCamera.transform;
                if (showDebugInfo)
                {

                }
            }
            else
            {

            }
        }
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Hide flashlight model initially
        if (flashlightModel != null)
        {
            flashlightModel.SetActive(false);
        }
        
        // Ensure flashlight starts OFF
        if (flashlightLight != null)
        {
            flashlightLight.enabled = false;
            isFlashlightOn = false;
        }
    }
    
    void Update()
    {
        HandleFlashlightInput();
        UpdateFlashlightStatus();
    }
    
    void HandleFlashlightInput()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }
    }
    
    void UpdateFlashlightStatus()
    {
        // Check if player still has flashlight in inventory
        if (playerInventory != null)
        {
            bool inventoryHasFlashlight = playerInventory.HasFlashlight();
            
            // If inventory status changed, update local status
            if (hasFlashlight != inventoryHasFlashlight)
            {
                hasFlashlight = inventoryHasFlashlight;
                
                if (!hasFlashlight)
                {
                    // Lost flashlight - turn off light and hide model
                    SetFlashlightState(false);
                    if (flashlightModel != null)
                    {
                        flashlightModel.SetActive(false);
                    }
                }
                else
                {
                    // Gained flashlight - show model but keep light off by default
                    if (flashlightModel != null)
                    {
                        flashlightModel.SetActive(true);
                    }
                }
            }
        }
    }
    
    void ToggleFlashlight()
    {
        if (!hasFlashlight)
        {
            if (showDebugInfo)
            {

            }
            return;
        }
        
        if (flashlightLight == null)
        {
            if (showDebugInfo)
            {

            }
            return;
        }
        
        // Check battery level before turning on
        if (!isFlashlightOn && FlashlightBattery.Instance != null)
        {
            if (FlashlightBattery.Instance.IsBatteryDepleted())
            {
                if (showDebugInfo)
                {

                }
                return;
            }
        }
        
        // Toggle the flashlight state
        SetFlashlightState(!isFlashlightOn);
        
        if (showDebugInfo)
        {

        }
    }
    
    void SetFlashlightState(bool state)
    {
        isFlashlightOn = state;
        
        if (flashlightLight != null)
        {
            flashlightLight.enabled = isFlashlightOn;
        }
        
        // Play sound effect
        PlayFlashlightSound();
        
        // Update visual feedback if needed
        UpdateVisualFeedback();
    }
    
    void PlayFlashlightSound()
    {
        if (audioSource == null) return;
        
        AudioClip clipToPlay = isFlashlightOn ? flashlightOnSound : flashlightOffSound;
        if (clipToPlay != null)
        {
            audioSource.clip = clipToPlay;
            audioSource.Play();
        }
    }
    
    void UpdateVisualFeedback()
    {
        // Update flashlight model appearance if needed
        if (flashlightModel != null)
        {
            // Could change material, add glow effect, etc.
        }
    }
    
    // Method to set the flashlight light component (called by pickup)
    public void SetFlashlightLight(Light light)
    {
        flashlightLight = light;
        
        // Attach the light to the player if we have an attach point
        if (flashlightAttachPoint != null && light != null)
        {
            light.transform.SetParent(flashlightAttachPoint);
            light.transform.localPosition = Vector3.forward * 0.5f; // Slightly in front
            light.transform.localRotation = Quaternion.identity;
            
            if (showDebugInfo)
            {

            }
        }
        
        // Ensure it starts OFF
        light.enabled = false;
        isFlashlightOn = false;
    }
    
    // Public getters
    public bool HasFlashlight => hasFlashlight;
    public bool IsFlashlightOn => isFlashlightOn;
    
    // Public method to force flashlight state (for external controls)
    public void ForceFlashlightState(bool state)
    {
        if (hasFlashlight && flashlightLight != null)
        {
            SetFlashlightState(state);
        }
    }
    
    // Method to remove flashlight (if needed for game mechanics)
    public void RemoveFlashlight()
    {
        if (hasFlashlight)
        {
            SetFlashlightState(false);
            hasFlashlight = false;
            
            if (flashlightLight != null)
            {
                Destroy(flashlightLight.gameObject);
                flashlightLight = null;
            }
            
            if (flashlightModel != null)
            {
                flashlightModel.SetActive(false);
            }
            
            if (showDebugInfo)
            {

            }
        }
    }
}
