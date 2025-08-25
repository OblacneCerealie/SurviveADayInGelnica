using UnityEngine;
using UnityEngine.AI;

public class PatientNPC : MonoBehaviour
{
    [Header("Movement Settings")]
    public float wanderRadius = 10f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;
    public float baseWalkSpeed = 1.5f;
    public float speedIncreasePerDay = 0.5f;
    
    private float currentWalkSpeed;
    
    [Header("Player Detection")]
    public float detectionRadius = 2f;
    public float lookSpeed = 2f;
    public Transform playerTransform;
    
    [Header("Debug")]
    public bool showGizmos = true;
    
    private NavMeshAgent agent;
    private Vector3 startPosition;
    private bool isWaiting = false;
    private bool isLookingAtPlayer = false;
    private Vector3 originalForward;
    private Quaternion targetRotation;
    private bool movementStopped = false;
    
    void Start()
    {
        // Get NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("PatientNPC requires a NavMeshAgent component!");
            return;
        }
        
        // Initialize speed (starting from day 1)
        currentWalkSpeed = baseWalkSpeed;
        
        // Set up agent
        agent.speed = currentWalkSpeed;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        
        // Store starting position
        startPosition = transform.position;
        
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
        // Start wandering
        StartWandering();
    }
    
    void Update()
    {
        if (agent == null || movementStopped) return;
        
        CheckPlayerDistance();
        HandlePlayerLooking();
        
        // Continue wandering if not waiting and reached destination
        if (!isWaiting && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAndWander());
        }
    }
    
    void CheckPlayerDistance()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer <= detectionRadius && !isLookingAtPlayer)
        {
            // Player is close, start looking at them
            isLookingAtPlayer = true;
            originalForward = transform.forward;
        }
        else if (distanceToPlayer > detectionRadius && isLookingAtPlayer)
        {
            // Player is far, stop looking at them
            isLookingAtPlayer = false;
        }
    }
    
    void HandlePlayerLooking()
    {
        if (!isLookingAtPlayer || playerTransform == null) return;
        
        // Calculate direction to player (only Y rotation)
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0; // Keep the NPC upright
        
        // Create target rotation
        if (directionToPlayer != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(directionToPlayer);
            
            // Smoothly rotate to look at player
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
        }
    }
    
    void StartWandering()
    {
        Vector3 randomDirection = GetRandomWanderPoint();
        agent.SetDestination(randomDirection);
    }
    
    Vector3 GetRandomWanderPoint()
    {
        // Generate random point within wander radius
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += startPosition;
        
        // Make sure the point is on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1))
        {
            return hit.position;
        }
        
        // If no valid point found, return current position
        return transform.position;
    }
    
    System.Collections.IEnumerator WaitAndWander()
    {
        isWaiting = true;
        
        // Wait for random time
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime);
        
        // Start wandering again
        StartWandering();
        isWaiting = false;
    }
    
    // Called by GameTimeUI when day changes
    public void OnDayChanged(int newDay)
    {
        // Calculate new speed based on day
        currentWalkSpeed = baseWalkSpeed + (speedIncreasePerDay * (newDay - 1));
        
        // Update agent speed
        if (agent != null)
        {
            agent.speed = currentWalkSpeed;
        }
        
        Debug.Log($"Patient {gameObject.name} speed updated to {currentWalkSpeed:F1} on day {newDay}");
    }
    
    // Utility method to get current speed
    public float GetCurrentSpeed()
    {
        return currentWalkSpeed;
    }
    
    // Movement control methods for interaction
    public void StopMovement()
    {
        movementStopped = true;
        if (agent != null)
        {
            agent.isStopped = true;
        }
        Debug.Log($"Patient {gameObject.name} movement stopped");
    }
    
    public void ResumeMovement()
    {
        movementStopped = false;
        if (agent != null)
        {
            agent.isStopped = false;
            // Resume wandering
            StartWandering();
        }
        Debug.Log($"Patient {gameObject.name} movement resumed");
    }
    
    public bool IsMovementStopped()
    {
        return movementStopped;
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Draw wander radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, wanderRadius);
        
        // Draw detection radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Draw line to player if looking
        if (isLookingAtPlayer && playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
        
        // Draw current destination
        if (Application.isPlaying && agent != null && agent.hasPath)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, agent.destination);
            Gizmos.DrawWireCube(agent.destination, Vector3.one * 0.5f);
        }
        
        // Display current speed info
        if (Application.isPlaying)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"Speed: {currentWalkSpeed:F1}");
        }
    }
}
