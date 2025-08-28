using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    [Header("Monster Reference")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private Transform monsterSpawnPoint;
    
    [Header("Settings")]
    [SerializeField] private bool spawnMonsterAtStart = false;
    [SerializeField] private bool debugLogs = true;
    
    private GameObject spawnedMonster;
    private MonsterAI monsterAI;
    private bool monsterActive = false;
    
    // Singleton pattern for easy access
    public static MonsterManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Subscribe to sanity events
        SanityManager.OnSanityChanged += OnSanityChanged;
        
        // Set up monster spawn point if not assigned
        if (monsterSpawnPoint == null)
        {
            monsterSpawnPoint = transform;
        }
        
        // Spawn monster if needed
        if (spawnMonsterAtStart)
        {
            SpawnMonster();
        }
        
        if (debugLogs)
        {
            Debug.Log("MonsterManager: Initialized and listening for sanity changes");
        }
    }
    
    void OnSanityChanged(int newSanity)
    {
        if (newSanity == 0 && !monsterActive)
        {
            ActivateMonster();
        }
        else if (newSanity > 0 && monsterActive)
        {
            DeactivateMonster();
        }
    }
    
    void SpawnMonster()
    {
        if (monsterPrefab == null)
        {
            Debug.LogError("MonsterManager: No monster prefab assigned!");
            return;
        }
        
        if (spawnedMonster != null)
        {
            if (debugLogs)
            {
                Debug.Log("MonsterManager: Monster already spawned");
            }
            return;
        }
        
        // Calculate proper spawn position (ensure it's on the ground)
        Vector3 spawnPosition = monsterSpawnPoint.position;
        
        // Raycast down to find ground
        RaycastHit hit;
        if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            spawnPosition = hit.point + Vector3.up * 1f; // 1 unit above ground to ensure visibility
            if (debugLogs)
            {
                Debug.Log($"MonsterManager: Ground found at {hit.point}, spawning at {spawnPosition}");
            }
        }
        else
        {
            // No ground found, spawn higher
            spawnPosition.y += 2f;
            if (debugLogs)
            {
                Debug.Log($"MonsterManager: No ground found, spawning at {spawnPosition}");
            }
        }
        
        // Spawn the monster
        spawnedMonster = Instantiate(monsterPrefab, spawnPosition, monsterSpawnPoint.rotation);
        spawnedMonster.name = "Monster";
        
        // Get the MonsterAI component
        monsterAI = spawnedMonster.GetComponent<MonsterAI>();
        if (monsterAI == null)
        {
            Debug.LogError("MonsterManager: Spawned monster doesn't have MonsterAI component!");
            return;
        }
        
        // Ensure the monster starts inactive but visible
        spawnedMonster.SetActive(true);
        monsterAI.SetActive(false);
        
        if (debugLogs)
        {
            Debug.Log("MonsterManager: Monster spawned at " + spawnPosition);
        }
    }
    
    public void ActivateMonster()
    {
        if (monsterActive) return;
        
        // Spawn monster if not already spawned
        if (spawnedMonster == null)
        {
            SpawnMonster();
        }
        else
        {
            // Monster already exists, just activate it at current position
            if (debugLogs)
            {
                Debug.Log($"MonsterManager: Monster already spawned at {spawnedMonster.transform.position}, activating in place");
            }
        }
        
        if (monsterAI != null)
        {
            monsterAI.SetActive(true);
            monsterActive = true;
            
            if (debugLogs)
            {
                Debug.Log("MonsterManager: Monster activated (sanity reached 0)");
            }
        }
        else
        {
            Debug.LogError("MonsterManager: Cannot activate monster - MonsterAI component not found!");
        }
    }
    
    public void DeactivateMonster()
    {
        if (!monsterActive) return;
        
        if (monsterAI != null)
        {
            monsterAI.SetActive(false);
            monsterActive = false;
            
            if (debugLogs)
            {
                Debug.Log("MonsterManager: Monster deactivated (sanity restored)");
            }
        }
    }
    
    public void SetMonsterPrefab(GameObject prefab)
    {
        monsterPrefab = prefab;
    }
    
    public void SetSpawnPoint(Transform spawnPoint)
    {
        monsterSpawnPoint = spawnPoint;
    }
    
    public bool IsMonsterActive()
    {
        return monsterActive;
    }
    
    public GameObject GetSpawnedMonster()
    {
        return spawnedMonster;
    }
    
    public MonsterAI GetMonsterAI()
    {
        return monsterAI;
    }
    
    // Force spawn monster for testing
    public void ForceSpawnMonster()
    {
        SpawnMonster();
    }
    
    // Remove monster completely
    public void DestroyMonster()
    {
        if (spawnedMonster != null)
        {
            Destroy(spawnedMonster);
            spawnedMonster = null;
            monsterAI = null;
            monsterActive = false;
            
            if (debugLogs)
            {
                Debug.Log("MonsterManager: Monster destroyed");
            }
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        SanityManager.OnSanityChanged -= OnSanityChanged;
    }
    
    // Debug/Test methods
    [ContextMenu("Test: Activate Monster")]
    public void TestActivateMonster()
    {
        ActivateMonster();
        Debug.Log("TEST: Manually activated monster");
    }
    
    [ContextMenu("Test: Deactivate Monster")]
    public void TestDeactivateMonster()
    {
        DeactivateMonster();
        Debug.Log("TEST: Manually deactivated monster");
    }
    
    [ContextMenu("Test: Spawn Monster")]
    public void TestSpawnMonster()
    {
        SpawnMonster();
        Debug.Log("TEST: Manually spawned monster");
    }
    
    [ContextMenu("Test: Destroy Monster")]
    public void TestDestroyMonster()
    {
        DestroyMonster();
        Debug.Log("TEST: Manually destroyed monster");
    }
}
