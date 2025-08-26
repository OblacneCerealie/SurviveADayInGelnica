using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public Transform playerTransform;
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("Visual Feedback")]
    public Material onMaterial;
    public Material offMaterial;
    public Renderer switchRenderer;
    public GameObject switchObject; // The actual switch/cube object
    
    [Header("Audio")]
    public AudioClip switchOnSound;
    public AudioClip switchOffSound;
    public AudioSource audioSource;
    
    [Header("Animation")]
    public bool useScaleAnimation = true;
    public float scaleAnimationDuration = 0.2f;
    public Vector3 pressedScale = new Vector3(0.9f, 0.8f, 0.9f);
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool showGizmos = true;
    
    private bool playerInRange = false;
    private bool isAnimating = false;
    private Vector3 originalScale;
    private bool lightsCurrentlyOn = false;
    
    void Start()
    {
        InitializeSwitch();
        SetupComponents();
    }
    
    void InitializeSwitch()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                if (showDebugInfo)
                {
                    Debug.Log("LightSwitch: Automatically found player with 'Player' tag");
                }
            }
            else
            {
                Debug.LogError("LightSwitch: No player found! Please assign playerTransform or tag your player with 'Player'");
            }
        }
        
        // Setup switch object reference
        if (switchObject == null)
        {
            switchObject = gameObject;
        }
        
        // Get renderer if not assigned
        if (switchRenderer == null)
        {
            switchRenderer = switchObject.GetComponent<Renderer>();
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
        
        // Store original scale
        originalScale = switchObject.transform.localScale;
        
        // Get initial lighting state
        if (LightingManager.Instance != null)
        {
            lightsCurrentlyOn = LightingManager.Instance.AreLightsOn;
        }
        
        // Set initial visual state
        UpdateSwitchVisuals();
    }
    
    void SetupComponents()
    {
        // Ensure the switch has a collider for interaction
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
            boxCol.isTrigger = false; // Keep solid for visual feedback
            if (showDebugInfo)
            {
                Debug.Log("LightSwitch: Added BoxCollider component");
            }
        }
        
        // Validate materials
        if (onMaterial == null || offMaterial == null)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning("LightSwitch: On/Off materials not assigned. Visual feedback will be limited.");
            }
        }
    }
    
    void Update()
    {
        if (!isAnimating)
        {
            CheckPlayerDistance();
            HandleInteractionInput();
        }
        
        // Update lighting state if it changed externally
        if (LightingManager.Instance != null)
        {
            bool newLightState = LightingManager.Instance.AreLightsOn;
            if (newLightState != lightsCurrentlyOn)
            {
                lightsCurrentlyOn = newLightState;
                UpdateSwitchVisuals();
            }
        }
    }
    
    void CheckPlayerDistance()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distanceToPlayer <= interactionRange;
        
        if (playerInRange && !wasInRange && showDebugInfo)
        {
            Debug.Log($"Light switch nearby - Press {interactionKey} to toggle lights");
        }
    }
    
    void HandleInteractionInput()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            ActivateSwitch();
        }
    }
    
    void ActivateSwitch()
    {
        if (isAnimating) return;
        
        // Check if LightingManager exists
        if (LightingManager.Instance == null)
        {
            Debug.LogError("LightSwitch: No LightingManager found in scene!");
            return;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("Light switch activated!");
        }
        
        // Toggle the lights
        LightingManager.Instance.ToggleLights();
        lightsCurrentlyOn = LightingManager.Instance.AreLightsOn;
        
        // Visual and audio feedback
        StartCoroutine(SwitchPressAnimation());
        PlaySwitchSound();
        UpdateSwitchVisuals();
    }
    
    System.Collections.IEnumerator SwitchPressAnimation()
    {
        if (!useScaleAnimation || switchObject == null) yield break;
        
        isAnimating = true;
        float elapsed = 0f;
        Vector3 startScale = originalScale;
        
        // Scale down (press)
        while (elapsed < scaleAnimationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (scaleAnimationDuration / 2f);
            switchObject.transform.localScale = Vector3.Lerp(startScale, pressedScale, progress);
            yield return null;
        }
        
        // Scale back up (release)
        elapsed = 0f;
        while (elapsed < scaleAnimationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (scaleAnimationDuration / 2f);
            switchObject.transform.localScale = Vector3.Lerp(pressedScale, originalScale, progress);
            yield return null;
        }
        
        // Ensure final scale is correct
        switchObject.transform.localScale = originalScale;
        isAnimating = false;
    }
    
    void PlaySwitchSound()
    {
        if (audioSource == null) return;
        
        AudioClip clipToPlay = lightsCurrentlyOn ? switchOnSound : switchOffSound;
        if (clipToPlay != null)
        {
            audioSource.clip = clipToPlay;
            audioSource.Play();
        }
    }
    
    void UpdateSwitchVisuals()
    {
        if (switchRenderer == null) return;
        
        Material materialToUse = lightsCurrentlyOn ? onMaterial : offMaterial;
        if (materialToUse != null)
        {
            switchRenderer.material = materialToUse;
        }
        
        // Optional: Change switch color if no materials are assigned
        if (onMaterial == null && offMaterial == null)
        {
            Color switchColor = lightsCurrentlyOn ? Color.green : Color.red;
            switchRenderer.material.color = switchColor;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Draw interaction range
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw switch state indicator
        Gizmos.color = lightsCurrentlyOn ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
    }
    
    // Public methods for external control
    public void SetInteractionRange(float range)
    {
        interactionRange = range;
    }
    
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }
    
    public bool IsPlayerInRange => playerInRange;
    public bool IsAnimating => isAnimating;
    
    // Force update switch state (useful when lighting changes externally)
    public void RefreshSwitchState()
    {
        if (LightingManager.Instance != null)
        {
            lightsCurrentlyOn = LightingManager.Instance.AreLightsOn;
            UpdateSwitchVisuals();
        }
    }
}
