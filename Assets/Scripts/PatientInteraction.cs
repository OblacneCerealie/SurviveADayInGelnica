using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PatientInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 2f;
    public Transform playerTransform;
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("UI Elements")]
    public GameObject patientInteractionPanel;
    public TextMeshProUGUI dialogueText;
    public Button givePillsButton;
    
    [Header("References")]
    public PatientNPC patientNPC;
    public PlayerInventory playerInventory;
    
    [Header("Dialogue")]
    public string greetingText = "Hey boss";
    public string thanksText = "Thanks boss";
    public float typingSpeed = 0.05f;
    public float exitDelay = 2f;
    
    [Header("Debug")]
    public bool showGizmos = true;
    
    private bool playerInRange = false;
    private bool isInteracting = false;
    private Coroutine typingCoroutine;
    
    void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("PatientInteraction: Automatically found player with 'Player' tag");
            }
        }
        
        // Find PatientNPC if not assigned
        if (patientNPC == null)
        {
            patientNPC = GetComponent<PatientNPC>();
        }
        
        // Find PlayerInventory if not assigned
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }
        
        // Validate components
        if (patientInteractionPanel == null)
        {
            Debug.LogError("PatientInteraction: No interaction panel assigned!");
        }
        
        if (dialogueText == null)
        {
            Debug.LogError("PatientInteraction: No dialogue text assigned!");
        }
        
        if (givePillsButton == null)
        {
            Debug.LogError("PatientInteraction: No give pills button assigned!");
        }
        else
        {
            // Set up button click event
            givePillsButton.onClick.AddListener(OnGivePillsButtonClicked);
        }
        
        // Hide interaction panel initially
        if (patientInteractionPanel != null)
        {
            patientInteractionPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        CheckPlayerDistance();
        
        if (!isInteracting)
        {
            HandleKeyboardInput();
        }
        else
        {
            // Check if player walked away during interaction
            if (!playerInRange)
            {
                Debug.Log("Player walked away - ending interaction");
                EndInteraction();
            }
        }
    }
    
    void CheckPlayerDistance()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distanceToPlayer <= interactionRange;
        
        if (playerInRange && !wasInRange)
        {
            Debug.Log("Patient nearby - Press E to interact");
        }
    }
    
    void HandleKeyboardInput()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            StartInteraction();
        }
    }
    
    void StartInteraction()
    {
        if (isInteracting) return;
        
        isInteracting = true;
        
        // Stop patient movement
        if (patientNPC != null)
        {
            patientNPC.StopMovement();
        }
        
        // Show interaction panel
        if (patientInteractionPanel != null)
        {
            patientInteractionPanel.SetActive(true);
        }
        
        // Unlock cursor for UI interaction
        UnlockCursor();
        
        // Update button state based on inventory
        UpdateButtonState();
        
        // Start typing animation for greeting
        if (dialogueText != null)
        {
            StartTyping(greetingText);
        }
        
        Debug.Log("Started interaction with patient");
    }
    
    void UpdateButtonState()
    {
        if (givePillsButton != null && playerInventory != null)
        {
            bool hasPills = playerInventory.HasPills();
            givePillsButton.interactable = hasPills;
            
            // Update button text based on inventory
            TextMeshProUGUI buttonText = givePillsButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = hasPills ? "Give Pills" : "No Pills";
            }
        }
    }
    
    void OnGivePillsButtonClicked()
    {
        if (playerInventory != null && playerInventory.HasPills())
        {
            // Remove pills from inventory
            playerInventory.RemovePills();
            
            // Hide the button completely
            if (givePillsButton != null)
            {
                givePillsButton.gameObject.SetActive(false);
            }
            
            // Show thanks message
            StartTyping(thanksText);
            
            // End interaction after typing finishes + delay
            StartCoroutine(EndInteractionAfterThanks());
            
            Debug.Log("Pills given to patient");
        }
    }
    
    void StartTyping(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        typingCoroutine = StartCoroutine(TypeText(text));
    }
    
    IEnumerator TypeText(string text)
    {
        if (dialogueText == null) yield break;
        
        dialogueText.text = "";
        
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    
    IEnumerator EndInteractionAfterThanks()
    {
        // Wait for "Thanks boss" typing to complete
        float typingDuration = thanksText.Length * typingSpeed;
        yield return new WaitForSeconds(typingDuration);
        
        // Then wait additional delay
        yield return new WaitForSeconds(exitDelay);
        
        // End the interaction
        EndInteraction();
    }
    
    void EndInteraction()
    {
        isInteracting = false;
        
        // Hide interaction panel
        if (patientInteractionPanel != null)
        {
            patientInteractionPanel.SetActive(false);
        }
        
        // Lock cursor back for gameplay
        LockCursor();
        
        // Resume patient movement
        if (patientNPC != null)
        {
            patientNPC.ResumeMovement();
        }
        
        // Stop typing if still running
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        
        // Show button again for next interaction (will be updated based on inventory)
        if (givePillsButton != null)
        {
            givePillsButton.gameObject.SetActive(true);
            givePillsButton.interactable = true;
        }
        
        Debug.Log("Ended interaction with patient");
    }
    
    // Public method to force end interaction (for external use)
    public void ForceEndInteraction()
    {
        EndInteraction();
    }
    
    // Check if currently interacting
    public bool IsInteracting()
    {
        return isInteracting;
    }
    
    // Cursor control methods
    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Cursor unlocked for UI interaction");
    }
    
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Cursor locked for gameplay");
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Draw interaction range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Draw line to player if in range
        if (Application.isPlaying && playerInRange && playerTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}
