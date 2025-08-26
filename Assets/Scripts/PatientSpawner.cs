using UnityEngine;
using UnityEngine.AI;

public class PatientSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private GameObject patientPrefab;
    [SerializeField] private int numberOfPatients = 3;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float maxWanderRadius = 50f; // Extended radius for whole baked mesh
    
    [Header("Spawn Area")]
    [SerializeField] private Transform spawnCenter;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    
    private GameObject[] spawnedPatients;
    
    void Start()
    {
        SpawnPatients();
    }
    
    void SpawnPatients()
    {
        if (patientPrefab == null)
        {
            Debug.LogError("Patient prefab is not assigned!");
            return;
        }
        
        spawnedPatients = new GameObject[numberOfPatients];
        Vector3 centerPosition = spawnCenter != null ? spawnCenter.position : transform.position;
        
        for (int i = 0; i < numberOfPatients; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition(centerPosition);
            
            if (spawnPosition != Vector3.zero)
            {
                GameObject patient = Instantiate(patientPrefab, spawnPosition, Quaternion.identity);
                patient.name = $"Patient_{i + 1}";
                
                // Extend the patient's wander radius to cover the whole baked mesh
                PatientNPC patientNPC = patient.GetComponent<PatientNPC>();
                if (patientNPC != null)
                {
                    patientNPC.wanderRadius = maxWanderRadius;
                    Debug.Log($"Patient {patient.name} spawned with extended wander radius: {maxWanderRadius}");
                }
                
                spawnedPatients[i] = patient;
            }
            else
            {
                Debug.LogWarning($"Failed to find valid spawn position for patient {i + 1}");
            }
        }
        
        Debug.Log($"Successfully spawned {numberOfPatients} patients");
    }
    
    Vector3 GetValidSpawnPosition(Vector3 center)
    {
        // Try to find a valid position on the NavMesh within spawn radius
        for (int attempts = 0; attempts < 30; attempts++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
            randomDirection.y = 0; // Keep on the same Y level
            Vector3 targetPosition = center + randomDirection;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, spawnRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        
        // If no valid position found, try the center position
        NavMeshHit centerHit;
        if (NavMesh.SamplePosition(center, out centerHit, spawnRadius, NavMesh.AllAreas))
        {
            return centerHit.position;
        }
        
        return Vector3.zero; // Failed to find any valid position
    }
    
    public GameObject[] GetSpawnedPatients()
    {
        return spawnedPatients;
    }
    
    public int GetPatientCount()
    {
        return spawnedPatients != null ? spawnedPatients.Length : 0;
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        Vector3 centerPosition = spawnCenter != null ? spawnCenter.position : transform.position;
        
        // Draw spawn radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPosition, spawnRadius);
        
        // Draw extended wander radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerPosition, maxWanderRadius);
    }
}
