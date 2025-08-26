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
        Debug.Log($"PatientInteraction START on {gameObject.name}");
        
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("PatientInteraction: Automatically found player with 'Player' tag");
            }
            else
            {
                Debug.LogError("PatientInteraction: No player found with 'Player' tag!");
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
            if (playerInventory != null)
            {
                Debug.Log("PatientInteraction: Auto-found PlayerInventory");
            }
            else
            {
                Debug.LogError("PatientInteraction: Could not find PlayerInventory in scene!");
            }
        }
        
        // Auto-find UI components if not assigned
        if (patientInteractionPanel == null)
        {
            // Try to find PatientInteraction, including inactive objects
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "PatientInteraction" && obj.scene.name != null) // Only scene objects, not prefabs
                {
                    patientInteractionPanel = obj;
                    break;
                }
            }
            
            if (patientInteractionPanel != null)
            {
                Debug.Log($"PatientInteraction: Auto-found UI panel named '{patientInteractionPanel.name}' (active: {patientInteractionPanel.activeInHierarchy})");
            }
            else
            {
                Debug.LogError("PatientInteraction: Could not find 'PatientInteraction' GameObject in scene!");
            }
        }
        
        if (dialogueText == null && patientInteractionPanel != null)
        {
            // Look specifically for "Greet" child - this is the dialogue text
            Transform greetChild = patientInteractionPanel.transform.Find("Greet");
            if (greetChild != null)
            {
                dialogueText = greetChild.GetComponent<TextMeshProUGUI>();
                if (dialogueText != null)
                {
                    Debug.Log("PatientInteraction: Auto-found dialogue text in 'Greet'");
                }
            }
            else
            {
                // Fallback to any TextMeshProUGUI that's NOT in a button
                TextMeshProUGUI[] allTexts = patientInteractionPanel.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (TextMeshProUGUI text in allTexts)
                {
                    // Skip if this text is inside a button (that would be the button label)
                    if (text.GetComponentInParent<Button>() == null)
                    {
                        dialogueText = text;
                        Debug.Log($"PatientInteraction: Auto-found dialogue text in '{dialogueText.gameObject.name}'");
                        break;
                    }
                }
            }
        }
        
        if (givePillsButton == null && patientInteractionPanel != null)
        {
            // Look specifically for "Button" child
            Transform buttonChild = patientInteractionPanel.transform.Find("Button");
            if (buttonChild != null)
            {
                givePillsButton = buttonChild.GetComponent<Button>();
                if (givePillsButton != null)
                {
                    Debug.Log("PatientInteraction: Auto-found give pills button");
                    // Set up button click event for auto-found button
                    givePillsButton.onClick.RemoveAllListeners();
                    givePillsButton.onClick.AddListener(OnGivePillsButtonClicked);
                    Debug.Log("Button click listener added to auto-found button");
                }
            }
            else
            {
                // Fallback to any Button
                givePillsButton = patientInteractionPanel.GetComponentInChildren<Button>();
                if (givePillsButton != null)
                {
                    Debug.Log($"PatientInteraction: Auto-found button in '{givePillsButton.gameObject.name}'");
                    // Set up button click event for fallback button
                    givePillsButton.onClick.RemoveAllListeners();
                    givePillsButton.onClick.AddListener(OnGivePillsButtonClicked);
                    Debug.Log("Button click listener added to fallback button");
                }
            }
        }
        
        // Validate components
        if (patientInteractionPanel == null)
        {
            Debug.LogError("PatientInteraction: No interaction panel found! Make sure there's a GameObject named 'PatientInteraction' in the scene.");
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
            givePillsButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            givePillsButton.onClick.AddListener(OnGivePillsButtonClicked);
            Debug.Log("Button click listener added successfully");
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
            
            // Manual button click detection as backup
            if (Input.GetMouseButtonDown(0) && givePillsButton != null && givePillsButton.interactable)
            {
                // Check if mouse is over the button
                Vector2 mousePos = Input.mousePosition;
                RectTransform buttonRect = givePillsButton.GetComponent<RectTransform>();
                if (RectTransformUtility.RectangleContainsScreenPoint(buttonRect, mousePos))
                {
                    Debug.Log("Manual button click detected!");
                    OnGivePillsButtonClicked();
                }
            }
        }
    }
    
    void CheckPlayerDistance()
    {
        if (playerTransform == null) 
        {
            // Debug.Log("PatientInteraction: playerTransform is null!");
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distanceToPlayer <= interactionRange;
        
        if (playerInRange && !wasInRange)
        {
            Debug.Log($"Patient {gameObject.name} nearby - Press E to interact (distance: {distanceToPlayer:F1})");
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
        Debug.Log($"StartInteraction called on {gameObject.name}");
        
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
            Debug.Log("UI panel activated");
        }
        else
        {
            Debug.LogError("patientInteractionPanel is null!");
        }
        
        // Unlock cursor for UI interaction
        UnlockCursor();
        
        // Update button state based on inventory
        UpdateButtonState();
        
        // Ensure button click listener is properly set up
        if (givePillsButton != null)
        {
            // Re-add the click listener to make sure it's connected
            givePillsButton.onClick.RemoveAllListeners();
            givePillsButton.onClick.AddListener(OnGivePillsButtonClicked);
            Debug.Log($"Button setup check: Button found, interactable: {givePillsButton.interactable}, listeners added");
        }
        else
        {
            Debug.LogError("Button is null during interaction start!");
        }
        
        // Start typing animation for greeting
        if (dialogueText != null)
        {
            StartTyping(greetingText);
            Debug.Log("Started typing greeting");
        }
        else
        {
            Debug.LogError("dialogueText is null!");
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
            
            Debug.Log($"Button state updated: {(hasPills ? "Has pills" : "No pills")}, Button interactable: {givePillsButton.interactable}");
        }
        else
        {
            if (givePillsButton == null)
                Debug.LogError("givePillsButton is null in UpdateButtonState!");
            if (playerInventory == null)
                Debug.LogError("playerInventory is null in UpdateButtonState!");
        }
    }
    
    void OnGivePillsButtonClicked()
    {
        Debug.Log("Give Pills button clicked!");
        
        if (playerInventory != null && playerInventory.HasPills())
        {
            // Remove pills from inventory
            playerInventory.RemovePills();
            
            // Increase sanity by 20 points (max 100)
            SanityManager sanityManager = SanityManager.Instance;
            if (sanityManager != null)
            {
                sanityManager.IncreaseSanity(20);
                Debug.Log("Sanity increased by 20 points");
            }
            
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
        else
        {
            if (playerInventory == null)
            {
                Debug.LogError("PlayerInventory is null!");
            }
            else if (!playerInventory.HasPills())
            {
                Debug.Log("Player has no pills!");
            }
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
