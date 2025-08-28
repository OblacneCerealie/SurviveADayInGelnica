using UnityEngine;
using System.Collections.Generic;

public class PatientManager : MonoBehaviour
{
    [Header("Patient Management")]
    public bool debugLogs = true;
    
    private List<PatientNPC> allPatients = new List<PatientNPC>();
    private bool patientsFrozen = false;
    
    // Singleton pattern for easy access
    public static PatientManager Instance { get; private set; }
    
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
        
        // Find all patients in the scene after a short delay to ensure they're spawned
        Invoke(nameof(RefreshPatientList), 1f);
    }
    
    void OnSanityChanged(int newSanity)
    {
        if (newSanity == 0 && !patientsFrozen)
        {
            FreezeAllPatients();
        }
        else if (newSanity > 0 && patientsFrozen)
        {
            UnfreezeAllPatients();
        }
    }
    
    public void RefreshPatientList()
    {
        allPatients.Clear();
        
        // Find all PatientNPC components in the scene
        PatientNPC[] foundPatients = FindObjectsByType<PatientNPC>(FindObjectsSortMode.None);
        allPatients.AddRange(foundPatients);
        
        if (debugLogs)
        {
            Debug.Log($"PatientManager: Found {allPatients.Count} patients in the scene");
        }
        
        // Also try to get patients from PatientSpawner if it exists
        PatientSpawner spawner = FindFirstObjectByType<PatientSpawner>();
        if (spawner != null)
        {
            GameObject[] spawnedPatients = spawner.GetSpawnedPatients();
            if (spawnedPatients != null)
            {
                foreach (GameObject patient in spawnedPatients)
                {
                    if (patient != null)
                    {
                        PatientNPC patientNPC = patient.GetComponent<PatientNPC>();
                        if (patientNPC != null && !allPatients.Contains(patientNPC))
                        {
                            allPatients.Add(patientNPC);
                        }
                    }
                }
            }
        }
        
        if (debugLogs)
        {
            Debug.Log($"PatientManager: Total patients registered: {allPatients.Count}");
        }
    }
    
    public void FreezeAllPatients()
    {
        if (patientsFrozen) return;
        
        patientsFrozen = true;
        
        foreach (PatientNPC patient in allPatients)
        {
            if (patient != null)
            {
                patient.StopMovement();
            }
        }
        
        if (debugLogs)
        {
            Debug.Log($"PatientManager: Froze {allPatients.Count} patients (sanity reached 0)");
        }
    }
    
    public void UnfreezeAllPatients()
    {
        if (!patientsFrozen) return;
        
        patientsFrozen = false;
        
        foreach (PatientNPC patient in allPatients)
        {
            if (patient != null)
            {
                patient.ResumeMovement();
            }
        }
        
        if (debugLogs)
        {
            Debug.Log($"PatientManager: Unfroze {allPatients.Count} patients (sanity restored)");
        }
    }
    
    public void RegisterPatient(PatientNPC patient)
    {
        if (patient != null && !allPatients.Contains(patient))
        {
            allPatients.Add(patient);
            
            // If patients are currently frozen, freeze this new patient too
            if (patientsFrozen)
            {
                patient.StopMovement();
            }
            
            if (debugLogs)
            {
                Debug.Log($"PatientManager: Registered new patient {patient.gameObject.name}");
            }
        }
    }
    
    public void UnregisterPatient(PatientNPC patient)
    {
        if (patient != null && allPatients.Contains(patient))
        {
            allPatients.Remove(patient);
            
            if (debugLogs)
            {
                Debug.Log($"PatientManager: Unregistered patient {patient.gameObject.name}");
            }
        }
    }
    
    public bool ArePatientsFrozen()
    {
        return patientsFrozen;
    }
    
    public int GetPatientCount()
    {
        return allPatients.Count;
    }
    
    public List<PatientNPC> GetAllPatients()
    {
        return new List<PatientNPC>(allPatients); // Return a copy to prevent external modification
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        SanityManager.OnSanityChanged -= OnSanityChanged;
    }
    
    // Debug/Test methods
    [ContextMenu("Test: Freeze All Patients")]
    public void TestFreezePatients()
    {
        FreezeAllPatients();
        Debug.Log("TEST: Manually froze all patients");
    }
    
    [ContextMenu("Test: Unfreeze All Patients")]
    public void TestUnfreezePatients()
    {
        UnfreezeAllPatients();
        Debug.Log("TEST: Manually unfroze all patients");
    }
    
    [ContextMenu("Test: Refresh Patient List")]
    public void TestRefreshPatients()
    {
        RefreshPatientList();
        Debug.Log($"TEST: Refreshed patient list - found {allPatients.Count} patients");
    }
}
