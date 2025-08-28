using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MonsterAI : MonoBehaviour
{
    [Header("AI Behavior")]
    public float patrolRadius = 30f;
    public float detectionRange = 25f; // Increased from 15 to 25
    public float loseTargetRange = 30f; // Increased from 20 to 30
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float minPatrolWaitTime = 2f;
    public float maxPatrolWaitTime = 6f;
    
    [Header("Player Detection")]
    public float hearingRange = 20f; // Simple radius detection - no line of sight needed
    public float losePlayerRange = 25f; // Distance at which monster stops chasing
    public bool useSimpleDetection = true; // Use radius instead of line of sight
    
    [Header("Line of Sight (Optional)")]
    public float viewAngle = 60f;
    public LayerMask obstacleMask = 1; // Default layer for obstacles
    public Transform eyesPosition; // Position from where the monster "sees"
    
    [Header("Animation")]
    public Animator animator;
    
    [Header("Player Reference")]
    public Transform playerTransform;
    
    [Header("Debug")]
    public bool showGizmos = true;
    public bool debugLogs = true;
    
    private NavMeshAgent agent;
    private Vector3 startPosition;
    private bool isActive = false;
    private bool isChasing = false;
    private bool isWaiting = false;
    private float lastPlayerSeenTime;
    
    // Animation hash IDs for better performance
    private int isMovingHash;
    
    // AI States
    public enum MonsterState
    {
        Inactive,
        Patrolling,
        Chasing,
        Searching
    }
    
    private MonsterState currentState = MonsterState.Inactive;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple MonsterAI instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        // Cache animator hash IDs
        isMovingHash = Animator.StringToHash("IsMoving");
    }
    
    void Start()
    {
        InitializeComponents();
        
        // Check current sanity to see if monster should be active
        SanityManager sanityManager = SanityManager.Instance;
        if (sanityManager != null && sanityManager.GetCurrentSanity() == 0)
        {
            // Sanity is already 0, activate immediately
            SetActive(true);
            if (debugLogs)
            {
                Debug.Log("MonsterAI: Sanity is 0 - activating immediately");
            }
        }
        else
        {
            // Start inactive and wait for sanity to reach 0
            SetActive(false);
            if (debugLogs)
            {
                Debug.Log("MonsterAI: Initialized and set to inactive, waiting for sanity to reach 0");
            }
        }
    }
    
    void InitializeComponents()
    {
        // Get components
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogError("MonsterAI: NavMeshAgent component is required!");
                return;
            }
        }
        
        // Always refresh player reference to get current position
        RefreshPlayerReference();
        
        // Set up eyes position if not assigned
        if (eyesPosition == null)
        {
            eyesPosition = transform;
        }
        
        // Find animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
        
        // Store starting position
        startPosition = transform.position;
        
        // Set up agent
        if (agent != null)
        {
            agent.speed = patrolSpeed;
            agent.angularSpeed = 180f;
            agent.acceleration = 8f;
        }
    }
    
    void Update()
    {
        if (!isActive || agent == null) return;
        
        // Ensure agent is enabled and not stopped
        if (!agent.enabled)
        {
            agent.enabled = true;
        }
        
        if (agent.isStopped)
        {
            agent.isStopped = false;
        }
        
        switch (currentState)
        {
            case MonsterState.Patrolling:
                HandlePatrolling();
                break;
            case MonsterState.Chasing:
                HandleChasing();
                break;
            case MonsterState.Searching:
                HandleSearching();
                break;
        }
        
        // Update animation based on movement
        UpdateAnimation();
        
        // Debug current state
        if (debugLogs && Time.frameCount % 60 == 0) // Log every second
        {
            Debug.Log($"MonsterAI: State={currentState}, Position={transform.position}, HasPath={agent.hasPath}, Velocity={agent.velocity.magnitude:F2}");
            
            if (playerTransform != null)
            {
                float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                bool canSee = CanSeePlayer();
                Debug.Log($"MonsterAI: Player distance={distToPlayer:F1}, CanSeePlayer={canSee}");
            }
            else
            {
                Debug.LogWarning("MonsterAI: Player reference is null!");
            }
        }
    }
    
    void HandlePatrolling()
    {
        // Check if we can see the player
        if (CanSeePlayer())
        {
            StartChasing();
            return;
        }
        
        // Continue patrolling if not waiting and reached destination
        if (!isWaiting && (!agent.hasPath || (!agent.pathPending && agent.remainingDistance < 1.5f)))
        {
            if (debugLogs)
            {
                Debug.Log("MonsterAI: Patrol destination reached, finding new patrol point");
            }
            StartCoroutine(WaitAndPatrol());
        }
        else if (!isWaiting && !agent.hasPath)
        {
            // Force start patrolling if no path exists
            if (debugLogs)
            {
                Debug.Log("MonsterAI: No path found, forcing new patrol destination");
            }
            Vector3 patrolPoint = GetRandomPatrolPoint();
            if (patrolPoint != Vector3.zero)
            {
                agent.SetDestination(patrolPoint);
            }
        }
    }
    
    void HandleChasing()
    {
        if (playerTransform == null || agent == null) return;
        
        RefreshPlayerReference(); // Make sure we have current player position
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Check if player is too far away (lose chase)
        if (distanceToPlayer > losePlayerRange)
        {
            if (debugLogs)
            {
                Debug.Log($"MonsterAI: Player too far away ({distanceToPlayer:F1} > {losePlayerRange}) - stopping chase");
            }
            StartPatrolling(); // Go back to patrolling
            return;
        }
        
        // Check if we can still detect the player
        if (CanSeePlayer())
        {
            // Update chase target to current player position
            agent.SetDestination(playerTransform.position);
            lastPlayerSeenTime = Time.time;
            
            if (debugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log($"MonsterAI: Chasing player at {playerTransform.position}, distance: {distanceToPlayer:F1}");
            }
        }
        else
        {
            // Player outside hearing range but still within lose range - search briefly
            float timeSinceLastSeen = Time.time - lastPlayerSeenTime;
            
            if (timeSinceLastSeen > 3f)
            {
                if (debugLogs)
                {
                    Debug.Log("MonsterAI: Lost player for too long - returning to patrol");
                }
                StartPatrolling();
            }
        }
    }
    
    void HandleSearching()
    {
        // Check if we can see the player again
        if (CanSeePlayer())
        {
            StartChasing();
            return;
        }
        
        // Continue searching if not waiting and reached destination
        if (!isWaiting && !agent.pathPending && agent.remainingDistance < 1f)
        {
            // Search for a bit longer, then return to patrol
            float timeSinceLastSeen = Time.time - lastPlayerSeenTime;
            if (timeSinceLastSeen > 10f)
            {
                StartPatrolling();
            }
            else
            {
                StartCoroutine(WaitAndSearch());
            }
        }
    }
    
    void RefreshPlayerReference()
    {
        // Always try to find the current player to get updated position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            PlayerInventory playerInv = FindFirstObjectByType<PlayerInventory>();
            if (playerInv != null)
            {
                player = playerInv.gameObject;
            }
        }
        
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
    
    bool CanSeePlayer()
    {
        // Refresh player position every time we check
        RefreshPlayerReference();
        
        if (playerTransform == null) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (useSimpleDetection)
        {
            // Simple radius detection - much easier!
            bool canHear = distanceToPlayer <= hearingRange;
            
            if (debugLogs && Time.frameCount % 120 == 0) // Log every 2 seconds
            {
                Debug.Log($"MonsterAI: Player at distance {distanceToPlayer:F1}, Hearing range: {hearingRange}, Can hear: {canHear}");
            }
            
            return canHear;
        }
        else
        {
            // Original line of sight detection (if you want to use it later)
            Vector3 directionToPlayer = (playerTransform.position - eyesPosition.position).normalized;
            
            if (distanceToPlayer > detectionRange) return false;
            
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer > viewAngle / 2f) return false;
            
            Ray ray = new Ray(eyesPosition.position, directionToPlayer);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, distanceToPlayer, obstacleMask))
            {
                if (hit.collider.transform != playerTransform) return false;
            }
            
            return true;
        }
    }
    
    void StartChasing()
    {
        if (currentState == MonsterState.Chasing) return;
        
        if (agent == null)
        {
            Debug.LogError("MonsterAI: Cannot start chasing - NavMeshAgent is null!");
            return;
        }
        
        currentState = MonsterState.Chasing;
        isChasing = true;
        agent.speed = chaseSpeed;
        lastPlayerSeenTime = Time.time;
        
        if (debugLogs)
        {
            Debug.Log("MonsterAI: Started chasing player");
        }
    }
    
    void StartPatrolling()
    {
        // Ensure agent is initialized
        if (agent == null)
        {
            InitializeComponents();
        }
        
        if (agent == null)
        {
            Debug.LogError("MonsterAI: Cannot start patrolling - NavMeshAgent is null!");
            return;
        }
        
        currentState = MonsterState.Patrolling;
        isChasing = false;
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        agent.enabled = true;
        
        Vector3 patrolPoint = GetRandomPatrolPoint();
        if (patrolPoint != Vector3.zero)
        {
            agent.SetDestination(patrolPoint);
            
            if (debugLogs)
            {
                Debug.Log($"MonsterAI: Started patrolling to {patrolPoint}, distance: {Vector3.Distance(transform.position, patrolPoint):F1}");
            }
        }
        else
        {
            Debug.LogWarning("MonsterAI: Failed to find valid patrol point!");
        }
    }
    
    void StartSearching()
    {
        if (agent == null)
        {
            Debug.LogError("MonsterAI: Cannot start searching - NavMeshAgent is null!");
            return;
        }
        
        currentState = MonsterState.Searching;
        isChasing = false;
        agent.speed = patrolSpeed;
        
        // Go to last known player position
        if (playerTransform != null)
        {
            Vector3 searchPoint = GetRandomPointNear(playerTransform.position, 5f);
            agent.SetDestination(searchPoint);
        }
        
        if (debugLogs)
        {
            Debug.Log("MonsterAI: Started searching for player");
        }
    }
    
    Vector3 GetRandomPatrolPoint()
    {
        // Generate random point within patrol radius
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection.y = 0; // Keep on same Y level
        randomDirection += startPosition;
        
        // Make sure the point is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        // If no valid point found, return start position
        return startPosition;
    }
    
    Vector3 GetRandomPointNear(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection.y = 0;
        randomDirection += center;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return center;
    }
    
    IEnumerator WaitAndPatrol()
    {
        isWaiting = true;
        
        float waitTime = Random.Range(minPatrolWaitTime, maxPatrolWaitTime);
        yield return new WaitForSeconds(waitTime);
        
        Vector3 patrolPoint = GetRandomPatrolPoint();
        agent.SetDestination(patrolPoint);
        isWaiting = false;
    }
    
    IEnumerator WaitAndSearch()
    {
        isWaiting = true;
        
        float waitTime = Random.Range(1f, 3f);
        yield return new WaitForSeconds(waitTime);
        
        Vector3 searchPoint = GetRandomPointNear(transform.position, 8f);
        agent.SetDestination(searchPoint);
        isWaiting = false;
    }
    
    void UpdateAnimation()
    {
        if (animator == null) 
        {
            if (debugLogs && Time.frameCount % 300 == 0) // Log every 5 seconds
            {
                Debug.LogWarning("MonsterAI: No animator assigned - animations won't play");
            }
            return;
        }
        
        // Check if the monster is moving
        bool isMoving = agent != null && agent.velocity.magnitude > 0.1f;
        
        // Always update the animator parameter
        animator.SetBool(isMovingHash, isMoving);
        
        // Enhanced debugging for animation
        if (debugLogs && Time.frameCount % 60 == 0) // Log every second
        {
            Debug.Log($"MonsterAI: Animation - IsMoving: {isMoving}, Velocity: {(agent != null ? agent.velocity.magnitude : 0):F2}");
            Debug.Log($"MonsterAI: Animator Controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "None")}");
            
            // Check if IsMoving parameter exists
            bool hasParameter = false;
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "IsMoving")
                {
                    hasParameter = true;
                    Debug.Log($"MonsterAI: IsMoving parameter found, current value: {animator.GetBool(isMovingHash)}");
                    break;
                }
            }
            
            if (!hasParameter)
            {
                Debug.LogError("MonsterAI: 'IsMoving' parameter NOT found in Animator Controller!");
            }
        }
    }
    
    // Public methods for external control
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (active)
        {
            // CRITICAL: Ensure GameObject is active and visible
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
                if (debugLogs)
                {
                    Debug.Log("MonsterAI: GameObject was inactive - activated it!");
                }
            }
            
            // Ensure all renderers are enabled for visibility
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = true;
                }
                if (debugLogs)
                {
                    Debug.Log($"MonsterAI: Enabled {renderers.Length} renderers for visibility");
                }
            }
            else
            {
                if (debugLogs)
                {
                    Debug.LogWarning("MonsterAI: No renderers found! Monster may be invisible.");
                }
            }
            
            // Force position above ground if underground
            if (transform.position.y < 0.5f)
            {
                Vector3 pos = transform.position;
                pos.y = 2f;
                transform.position = pos;
                if (debugLogs)
                {
                    Debug.Log($"MonsterAI: Moved above ground to {pos}");
                }
            }
            
            // Start patrolling
            StartPatrolling();
            
            if (debugLogs)
            {
                Debug.Log($"MonsterAI: Activated - starting patrol at position {transform.position}");
            }
        }
        else
        {
            // Deactivate
            currentState = MonsterState.Inactive;
            
            // Stop the agent
            if (agent != null)
            {
                agent.isStopped = true;
            }
            
            // Don't hide GameObject completely, just disable AI
            // gameObject.SetActive(false);
            
            if (debugLogs)
            {
                Debug.Log("MonsterAI: Deactivated (AI stopped but GameObject remains visible)");
            }
        }
    }
    
    public bool IsActive()
    {
        return isActive;
    }
    
    public bool IsChasing()
    {
        return isChasing;
    }
    
    public MonsterState GetCurrentState()
    {
        return currentState;
    }
    
    // Singleton pattern for easy access
    public static MonsterAI Instance { get; private set; }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        Vector3 centerPosition = Application.isPlaying ? startPosition : transform.position;
        
        // Draw patrol radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPosition, patrolRadius);
        
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw lose target range
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // Orange color
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);
        
        // Draw view cone
        if (Application.isPlaying && isActive)
        {
            Vector3 viewAngleLeft = DirectionFromAngle(-viewAngle / 2, false);
            Vector3 viewAngleRight = DirectionFromAngle(viewAngle / 2, false);
            
            Gizmos.color = CanSeePlayer() ? Color.green : Color.red;
            Gizmos.DrawLine(eyesPosition.position, eyesPosition.position + viewAngleLeft * detectionRange);
            Gizmos.DrawLine(eyesPosition.position, eyesPosition.position + viewAngleRight * detectionRange);
            
            // Draw line to player if visible
            if (CanSeePlayer() && playerTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(eyesPosition.position, playerTransform.position);
            }
        }
        
        // Draw current destination
        if (Application.isPlaying && agent != null && agent.hasPath)
        {
            Gizmos.color = isChasing ? Color.red : Color.blue;
            Gizmos.DrawLine(transform.position, agent.destination);
            Gizmos.DrawWireCube(agent.destination, Vector3.one * 0.5f);
        }
    }
    
    Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
