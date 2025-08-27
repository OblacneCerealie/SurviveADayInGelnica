using UnityEngine;
using System.Collections;

public class LightGenerator : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public Transform playerTransform;
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("Animation Settings")]
    public float shakeDuration = 4f;
    public float shakeIntensity = 0.1f;
    public AnimationCurve shakeIntensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    [Header("Lighting Control")]
    public bool turnOnLightsWhenActivated = true;
    public bool allowMultipleUses = true;
    public float cooldownTime = 2f;
    
    [Header("Visual Feedback")]
    public Material inactiveMaterial;
    public Material activeMaterial;
    public Material cooldownMaterial;
    public Renderer generatorRenderer;
    
    [Header("Audio")]
    public AudioClip generatorStartSound;
    public AudioClip generatorRunningSound;
    public AudioClip lightsOnSound;
    public AudioSource audioSource;
    
    [Header("Effects")]
    public ParticleSystem activationEffect;
    public Light indicatorLight;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool showGizmos = true;
    
    private bool playerInRange = false;
    private bool isActivating = false;
    private bool isOnCooldown = false;
    private Vector3 originalPosition;
    private float lastActivationTime;
    
    void Start()
    {
        InitializeGenerator();
        SetupComponents();
    }
    
    void InitializeGenerator()
    {
        // Store original position for shake animation
        originalPosition = transform.position;
        
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                if (showDebugInfo)
                {
                    Debug.Log("LightGenerator: Automatically found player with 'Player' tag");
                }
            }
            else
            {
                Debug.LogError("LightGenerator: No player found! Please assign playerTransform or tag your player with 'Player'");
            }
        }
        
        // Get renderer if not assigned
        if (generatorRenderer == null)
        {
            generatorRenderer = GetComponent<Renderer>();
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
        
        // Setup indicator light
        if (indicatorLight == null)
        {
            indicatorLight = GetComponentInChildren<Light>();
        }
        
        // Set initial visual state
        UpdateVisualState();
    }
    
    void SetupComponents()
    {
        // Ensure the generator has a collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider boxCol = gameObject.AddComponent<BoxCollider>();
            boxCol.isTrigger = false;
            if (showDebugInfo)
            {
                Debug.Log("LightGenerator: Added BoxCollider component");
            }
        }
    }
    
    void Update()
    {
        if (!isActivating)
        {
            CheckPlayerDistance();
            HandleInteractionInput();
            UpdateCooldown();
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
            if (CanActivate())
            {
                string message = $"Light generator nearby - Press {interactionKey} to turn on lights";
                
                // Add additional context if sanity is low
                if (SanityManager.Instance != null && !SanityManager.Instance.CanAutomaticallyRestoreLights())
                {
                    message += " (SANITY TOO LOW - Generator is the only way to restore power!)";
                }
                
                Debug.Log(message);
            }
            else if (SanityManager.Instance != null && !SanityManager.Instance.CanUseGenerator())
            {
                Debug.Log("Light generator nearby - GENERATOR DISABLED: Sanity is at 0!");
            }
        }
    }
    
    void HandleInteractionInput()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey) && CanActivate())
        {
            StartLightGeneration();
        }
    }
    
    void UpdateCooldown()
    {
        if (isOnCooldown && Time.time >= lastActivationTime + cooldownTime)
        {
            isOnCooldown = false;
            UpdateVisualState();
            
            if (showDebugInfo)
            {
                Debug.Log("LightGenerator: Cooldown finished, ready for next activation");
            }
        }
    }
    
    bool CanActivate()
    {
        if (isActivating) return false;
        if (!allowMultipleUses && HasBeenActivated()) return false;
        if (isOnCooldown) return false;
        
        // Check if generator can work based on sanity level
        if (SanityManager.Instance != null && !SanityManager.Instance.CanUseGenerator())
        {
            return false;
        }
        
        return true;
    }
    
    bool HasBeenActivated()
    {
        return lastActivationTime > 0;
    }
    
    void StartLightGeneration()
    {
        if (!CanActivate()) return;
        
        if (showDebugInfo)
        {
            Debug.Log("Starting light generator activation...");
        }
        
        lastActivationTime = Time.time;
        StartCoroutine(LightGenerationSequence());
    }
    
    IEnumerator LightGenerationSequence()
    {
        isActivating = true;
        UpdateVisualState();
        
        // Play activation sound
        PlaySound(generatorStartSound);
        
        // Start particle effect
        if (activationEffect != null)
        {
            activationEffect.Play();
        }
        
        // Shake animation
        yield return StartCoroutine(ShakeAnimation());
        
        // Activate lights
        ActivateLights();
        
        // Play success sound
        PlaySound(lightsOnSound);
        
        // Start cooldown if multiple uses allowed
        if (allowMultipleUses)
        {
            isOnCooldown = true;
        }
        
        isActivating = false;
        UpdateVisualState();
        
        if (showDebugInfo)
        {
            Debug.Log("Light generator activation completed!");
        }
    }
    
    IEnumerator ShakeAnimation()
    {
        float elapsed = 0f;
        
        // Play running sound during shake
        if (generatorRunningSound != null && audioSource != null)
        {
            audioSource.clip = generatorRunningSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Shake animation
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;
            
            // Use animation curve to control shake intensity over time
            float currentIntensity = shakeIntensity * shakeIntensityCurve.Evaluate(progress);
            
            // Generate random shake offset
            Vector3 randomOffset = new Vector3(
                Random.Range(-currentIntensity, currentIntensity),
                Random.Range(-currentIntensity, currentIntensity),
                Random.Range(-currentIntensity, currentIntensity)
            );
            
            // Apply shake
            transform.position = originalPosition + randomOffset;
            
            yield return null;
        }
        
        // Return to original position
        transform.position = originalPosition;
        
        // Stop running sound
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == generatorRunningSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }
    
    void ActivateLights()
    {
        if (LightingManager.Instance == null)
        {
            Debug.LogError("LightGenerator: No LightingManager found in scene! Cannot control lighting.");
            return;
        }
        
        if (turnOnLightsWhenActivated)
        {
            // Use ForceLightsOn to override sanity restrictions
            LightingManager.Instance.ForceLightsOn();
            
            // Also update sanity manager to track that lights are on
            if (SanityManager.Instance != null)
            {
                SanityManager.Instance.ForceLightsOn();
            }
            
            if (showDebugInfo)
            {
                Debug.Log("LightGenerator: FORCED lights ON (overriding sanity restrictions)");
            }
        }
        else
        {
            LightingManager.Instance.SetLights(false);
            if (showDebugInfo)
            {
                Debug.Log("LightGenerator: Turned OFF lights via LightingManager");
            }
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        
        audioSource.clip = clip;
        audioSource.loop = false;
        audioSource.Play();
    }
    
    void UpdateVisualState()
    {
        if (generatorRenderer == null) return;
        
        Material materialToUse = null;
        Color indicatorColor = Color.white;
        float indicatorIntensity = 1f;
        
        if (isActivating)
        {
            // Active state - bright, energetic colors
            indicatorColor = Color.cyan;
            indicatorIntensity = 2f;
        }
        else if (isOnCooldown)
        {
            // Cooldown state
            materialToUse = cooldownMaterial;
            indicatorColor = Color.yellow;
            indicatorIntensity = 0.5f;
        }
        else if (HasBeenActivated() && !allowMultipleUses)
        {
            // Used and can't be used again
            materialToUse = activeMaterial;
            indicatorColor = Color.green;
            indicatorIntensity = 0.3f;
        }
        else
        {
            // Ready to use
            materialToUse = inactiveMaterial;
            indicatorColor = Color.red;
            indicatorIntensity = 0.8f;
        }
        
        // Apply material
        if (materialToUse != null)
        {
            generatorRenderer.material = materialToUse;
        }
        
        // Update indicator light
        if (indicatorLight != null)
        {
            indicatorLight.color = indicatorColor;
            indicatorLight.intensity = indicatorIntensity;
            indicatorLight.enabled = true;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Draw interaction range
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw generator state indicator
        Color stateColor = Color.white;
        if (isActivating)
            stateColor = Color.cyan;
        else if (isOnCooldown)
            stateColor = Color.yellow;
        else if (HasBeenActivated() && !allowMultipleUses)
            stateColor = Color.green;
        else
            stateColor = Color.red;
            
        Gizmos.color = stateColor;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        
        // Draw light activation symbol
        if (turnOnLightsWhenActivated)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawIcon(transform.position + Vector3.up * 3f, "d_SceneViewLighting", true);
        }
    }
    
    // Public methods for external control
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }
    
    public void SetInteractionRange(float range)
    {
        interactionRange = range;
    }
    
    public bool IsPlayerInRange => playerInRange;
    public bool IsActivating => isActivating;
    public bool IsOnCooldown => isOnCooldown;
    public bool CanBeActivated => CanActivate();
    
    // Force activation (for external triggers)
    public void ForceActivate()
    {
        if (!isActivating)
        {
            StartLightGeneration();
        }
    }
    
    // Reset generator (for testing)
    [ContextMenu("Reset Generator")]
    public void ResetGenerator()
    {
        lastActivationTime = 0;
        isOnCooldown = false;
        isActivating = false;
        UpdateVisualState();
        
        if (showDebugInfo)
        {
            Debug.Log("LightGenerator: Reset to initial state");
        }
    }
}
