using UnityEngine;
using UnityEngine.AI;

public class MonsterSetupHelper : MonoBehaviour
{
    [Header("Monster Setup")]
    [SerializeField] private GameObject monsterModel; // The Shadowed_Stalker model
    [SerializeField] private GameObject walkingAnimation; // The walking animation FBX
    
    [Header("AI Configuration")]
    [SerializeField] private float patrolRadius = 30f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    
    [Header("Animation Configuration")]
    [SerializeField] private RuntimeAnimatorController animatorController;
    
    [ContextMenu("Auto-Setup Monster")]
    public void AutoSetupMonster()
    {
        Debug.Log("MonsterSetupHelper: Starting auto-setup...");
        
        // Find the monster model if not assigned
        if (monsterModel == null)
        {
            // Try to find the Shadowed_Stalker model
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Shadowed_Stalker") && obj.scene.name != null)
                {
                    monsterModel = obj;
                    Debug.Log($"Found monster model: {obj.name}");
                    break;
                }
            }
        }
        
        if (monsterModel == null)
        {
            Debug.LogError("MonsterSetupHelper: Could not find monster model! Please assign it manually.");
            return;
        }
        
        // Set up the monster components
        SetupMonsterComponents();
        
        Debug.Log("MonsterSetupHelper: Auto-setup completed!");
    }
    
    void SetupMonsterComponents()
    {
        // Add NavMeshAgent if not present
        NavMeshAgent agent = monsterModel.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = monsterModel.AddComponent<NavMeshAgent>();
            Debug.Log("Added NavMeshAgent to monster");
        }
        
        // Configure NavMeshAgent
        agent.speed = patrolSpeed;
        agent.angularSpeed = 180f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.5f;
        agent.radius = 0.5f;
        agent.height = 2f;
        
        // Add MonsterAI if not present
        MonsterAI monsterAI = monsterModel.GetComponent<MonsterAI>();
        if (monsterAI == null)
        {
            monsterAI = monsterModel.AddComponent<MonsterAI>();
            Debug.Log("Added MonsterAI to monster");
        }
        
        // Configure MonsterAI
        monsterAI.patrolRadius = patrolRadius;
        monsterAI.detectionRange = detectionRange;
        monsterAI.patrolSpeed = patrolSpeed;
        monsterAI.chaseSpeed = chaseSpeed;
        
        // Find and assign player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            monsterAI.playerTransform = player.transform;
            Debug.Log("Assigned player to monster AI");
        }
        
        // Set up animator
        SetupAnimator();
        
        // Create eyes position for line of sight
        CreateEyesPosition();
        
        Debug.Log("Monster components configured successfully");
    }
    
    void SetupAnimator()
    {
        // Find or create animator
        Animator animator = monsterModel.GetComponent<Animator>();
        if (animator == null)
        {
            animator = monsterModel.AddComponent<Animator>();
            Debug.Log("Added Animator to monster");
        }
        
        // Try to create a basic animator controller if none exists
        if (animator.runtimeAnimatorController == null && animatorController == null)
        {
            Debug.Log("MonsterSetupHelper: No animator controller assigned. You'll need to create one manually.");
            Debug.Log("Create an Animator Controller with:");
            Debug.Log("- Bool parameter: 'IsMoving'");
            Debug.Log("- Two states: 'Idle' and 'Walking'");
            Debug.Log("- Transition from Idle to Walking when IsMoving = true");
            Debug.Log("- Transition from Walking to Idle when IsMoving = false");
        }
        else if (animatorController != null)
        {
            animator.runtimeAnimatorController = animatorController;
            Debug.Log("Assigned animator controller to monster");
        }
        
        // Assign animator to MonsterAI
        MonsterAI monsterAI = monsterModel.GetComponent<MonsterAI>();
        if (monsterAI != null)
        {
            monsterAI.animator = animator;
        }
    }
    
    void CreateEyesPosition()
    {
        MonsterAI monsterAI = monsterModel.GetComponent<MonsterAI>();
        if (monsterAI == null) return;
        
        // Check if eyes position already exists
        Transform existingEyes = monsterModel.transform.Find("Eyes");
        if (existingEyes != null)
        {
            monsterAI.eyesPosition = existingEyes;
            Debug.Log("Using existing Eyes position");
            return;
        }
        
        // Create eyes position object
        GameObject eyesObject = new GameObject("Eyes");
        eyesObject.transform.SetParent(monsterModel.transform);
        eyesObject.transform.localPosition = new Vector3(0, 1.7f, 0.3f); // Adjust based on monster height
        
        monsterAI.eyesPosition = eyesObject.transform;
        Debug.Log("Created Eyes position for monster");
    }
    
    [ContextMenu("Setup Monster Manager")]
    public void SetupMonsterManager()
    {
        // Find or create MonsterManager
        MonsterManager monsterManager = FindFirstObjectByType<MonsterManager>();
        if (monsterManager == null)
        {
            GameObject managerObject = new GameObject("MonsterManager");
            monsterManager = managerObject.AddComponent<MonsterManager>();
            Debug.Log("Created MonsterManager");
        }
        
        // Create a prefab reference (this would need to be done manually in the editor)
        Debug.Log("MonsterManager created. You'll need to:");
        Debug.Log("1. Create a prefab from your configured monster");
        Debug.Log("2. Assign the prefab to MonsterManager's 'Monster Prefab' field");
        Debug.Log("3. Set a spawn point for the monster");
    }
    
    [ContextMenu("Setup Patient Manager")]
    public void SetupPatientManager()
    {
        // Find or create PatientManager
        PatientManager patientManager = FindFirstObjectByType<PatientManager>();
        if (patientManager == null)
        {
            GameObject managerObject = new GameObject("PatientManager");
            patientManager = managerObject.AddComponent<PatientManager>();
            Debug.Log("Created PatientManager");
        }
        else
        {
            Debug.Log("PatientManager already exists");
        }
    }
    
    [ContextMenu("Full Monster System Setup")]
    public void FullSetup()
    {
        Debug.Log("=== Starting Full Monster System Setup ===");
        
        AutoSetupMonster();
        SetupMonsterManager();
        SetupPatientManager();
        
        Debug.Log("=== Full Monster System Setup Complete ===");
        Debug.Log("Additional manual steps required:");
        Debug.Log("1. Create an Animator Controller for the monster with 'IsMoving' bool parameter");
        Debug.Log("2. Create a prefab from the configured monster");
        Debug.Log("3. Assign the monster prefab to MonsterManager");
        Debug.Log("4. Set monster spawn point in MonsterManager");
        Debug.Log("5. Test by setting sanity to 0 in SanityManager");
    }
    
    [ContextMenu("Debug Monster State")]
    public void DebugMonsterState()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        if (monster != null)
        {
            Debug.Log($"Monster found: {monster.gameObject.name}");
            Debug.Log($"Monster active: {monster.IsActive()}");
            Debug.Log($"Monster state: {monster.GetCurrentState()}");
            Debug.Log($"Monster position: {monster.transform.position}");
            Debug.Log($"Monster visible: {monster.gameObject.activeInHierarchy}");
            
            NavMeshAgent agent = monster.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                Debug.Log($"Agent enabled: {agent.enabled}");
                Debug.Log($"Agent stopped: {agent.isStopped}");
                Debug.Log($"Agent has path: {agent.hasPath}");
                Debug.Log($"Agent velocity: {agent.velocity}");
            }
        }
        else
        {
            Debug.Log("No MonsterAI found in scene");
        }
    }
    
    [ContextMenu("Force Monster Visible")]
    public void ForceMonsterVisible()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        if (monster != null)
        {
            monster.gameObject.SetActive(true);
            
            Renderer[] renderers = monster.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.enabled = true;
            }
            
            Debug.Log($"Forced monster visible. Position: {monster.transform.position}");
            Debug.Log($"Renderers found: {renderers.Length}");
            
            // Move monster to a visible position if it's underground
            if (monster.transform.position.y < 0)
            {
                Vector3 newPos = monster.transform.position;
                newPos.y = 2f;
                monster.transform.position = newPos;
                Debug.Log($"Moved monster above ground to: {newPos}");
            }
        }
        else
        {
            Debug.Log("No MonsterAI found to make visible");
        }
    }
    
    [ContextMenu("Teleport Monster to Player")]
    public void TeleportMonsterToPlayer()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            PlayerInventory playerInv = FindFirstObjectByType<PlayerInventory>();
            if (playerInv != null) player = playerInv.gameObject;
        }
        
        if (monster != null && player != null)
        {
            Vector3 playerPos = player.transform.position;
            Vector3 monsterPos = playerPos + player.transform.forward * 5f + Vector3.up * 3f; // 5 units in front, 3 units UP
            
            // Additional safety - ensure minimum height
            if (monsterPos.y < 2f)
            {
                monsterPos.y = 3f;
            }
            
            monster.transform.position = monsterPos;
            monster.gameObject.SetActive(true);
            
            Debug.Log($"Teleported monster to {monsterPos} (near player at {playerPos})");
        }
        else
        {
            Debug.Log($"Cannot teleport - Monster: {(monster != null ? "Found" : "Not found")}, Player: {(player != null ? "Found" : "Not found")}");
        }
    }
    
    [ContextMenu("Fix Monster Player Reference")]
    public void FixMonsterPlayerReference()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        if (monster != null)
        {
            // Try to find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                // Try alternative methods
                PlayerInventory playerInv = FindFirstObjectByType<PlayerInventory>();
                if (playerInv != null)
                {
                    player = playerInv.gameObject;
                }
            }
            
            if (player != null)
            {
                monster.playerTransform = player.transform;
                Debug.Log($"Fixed monster player reference: {player.name}");
            }
            else
            {
                Debug.LogError("Cannot fix player reference - no player found!");
            }
        }
        else
        {
            Debug.Log("No MonsterAI found to fix");
        }
    }
    
    [ContextMenu("Fix Animation Loop")]
    public void FixAnimationLoop()
    {
        Debug.Log("ANIMATION LOOP FIX INSTRUCTIONS:");
        Debug.Log("1. Select your walking animation in Project window");
        Debug.Log("2. In Inspector, check 'Loop Time' checkbox");
        Debug.Log("3. Click Apply");
        Debug.Log("4. In Animator Controller:");
        Debug.Log("   - Idle → Walking: IsMoving = true");
        Debug.Log("   - Walking → Idle: IsMoving = false");
        Debug.Log("   - Uncheck 'Has Exit Time' on both transitions");
    }
    
    [ContextMenu("FULL SYSTEM DIAGNOSTIC")]
    public void FullSystemDiagnostic()
    {
        Debug.Log("=== MONSTER SYSTEM DIAGNOSTIC ===");
        
        // Check SanityManager
        SanityManager sanityManager = FindFirstObjectByType<SanityManager>();
        Debug.Log($"SanityManager: {(sanityManager != null ? "Found" : "MISSING")}");
        if (sanityManager != null)
        {
            Debug.Log($"Current Sanity: {sanityManager.GetCurrentSanity()}");
        }
        
        // Check PatientManager
        PatientManager patientManager = FindFirstObjectByType<PatientManager>();
        Debug.Log($"PatientManager: {(patientManager != null ? "Found" : "MISSING")}");
        if (patientManager != null)
        {
            Debug.Log($"Patients Frozen: {patientManager.ArePatientsFrozen()}");
            Debug.Log($"Patient Count: {patientManager.GetPatientCount()}");
        }
        
        // Check MonsterManager
        MonsterManager monsterManager = FindFirstObjectByType<MonsterManager>();
        Debug.Log($"MonsterManager: {(monsterManager != null ? "Found" : "MISSING")}");
        if (monsterManager != null)
        {
            Debug.Log($"Monster Active: {monsterManager.IsMonsterActive()}");
            GameObject monster = monsterManager.GetSpawnedMonster();
            Debug.Log($"Monster Spawned: {(monster != null ? "Yes" : "No")}");
        }
        
        // Check Monster
        MonsterAI monsterAI = FindFirstObjectByType<MonsterAI>();
        Debug.Log($"MonsterAI: {(monsterAI != null ? "Found" : "MISSING")}");
        if (monsterAI != null)
        {
            Debug.Log($"Monster Position: {monsterAI.transform.position}");
            Debug.Log($"Monster Active: {monsterAI.IsActive()}");
            Debug.Log($"Monster State: {monsterAI.GetCurrentState()}");
            Debug.Log($"Player Reference: {(monsterAI.playerTransform != null ? monsterAI.playerTransform.name : "MISSING")}");
            Debug.Log($"Monster Visible: {monsterAI.gameObject.activeInHierarchy}");
        }
        
        // Check Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            PlayerInventory playerInv = FindFirstObjectByType<PlayerInventory>();
            if (playerInv != null) player = playerInv.gameObject;
        }
        Debug.Log($"Player: {(player != null ? player.name : "NOT FOUND")}");
        if (player != null)
        {
            Debug.Log($"Player Position: {player.transform.position}");
        }
        
        Debug.Log("=== END DIAGNOSTIC ===");
    }
    
    [ContextMenu("FORCE ACTIVATE MONSTER")]
    public void ForceActivateMonster()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        if (monster != null)
        {
            // Force activate regardless of sanity
            monster.SetActive(true);
            
            // Ensure it's visible and positioned correctly
            monster.gameObject.SetActive(true);
            
            // Move to visible position
            Vector3 pos = monster.transform.position;
            pos.y = 2f;
            monster.transform.position = pos;
            
            Debug.Log($"FORCE ACTIVATED Monster at position: {pos}");
            Debug.Log($"Monster state: {monster.GetCurrentState()}");
        }
        else
        {
            Debug.LogError("No MonsterAI found to force activate!");
        }
    }
    
    [ContextMenu("Test Monster Movement")]
    public void TestMonsterMovement()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        if (monster != null)
        {
            // Check NavMesh
            UnityEngine.AI.NavMeshAgent agent = monster.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                Debug.Log($"NavMeshAgent - Enabled: {agent.enabled}, OnNavMesh: {agent.isOnNavMesh}, Stopped: {agent.isStopped}");
                Debug.Log($"NavMeshAgent - Speed: {agent.speed}, HasPath: {agent.hasPath}");
                
                if (!agent.isOnNavMesh)
                {
                    Debug.LogError("Monster is NOT on NavMesh! Bake NavMesh or move monster to valid area.");
                }
                
                if (agent.isStopped)
                {
                    Debug.LogWarning("NavMeshAgent is STOPPED! This prevents movement.");
                }
            }
            
            // Check player reference
            if (monster.playerTransform != null)
            {
                float dist = Vector3.Distance(monster.transform.position, monster.playerTransform.position);
                Debug.Log($"Player found at distance: {dist:F1}");
            }
            else
            {
                Debug.LogError("Monster has no player reference!");
            }
            
            Debug.Log($"Monster AI Active: {monster.IsActive()}, State: {monster.GetCurrentState()}");
            
            // Check sanity system
            SanityManager sanity = FindFirstObjectByType<SanityManager>();
            if (sanity != null)
            {
                Debug.Log($"Current Sanity: {sanity.GetCurrentSanity()}");
            }
            
            if (!monster.IsActive())
            {
                Debug.LogError("PROBLEM: Monster AI is INACTIVE! Use 'FORCE ACTIVATE MONSTER' to test movement.");
            }
        }
        else
        {
            Debug.LogError("No MonsterAI found!");
        }
    }
    
    [ContextMenu("ACTIVATE AND TEST")]
    public void ActivateAndTest()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        if (monster != null)
        {
            // Force activate the monster
            monster.SetActive(true);
            Debug.Log("Monster forcefully activated for testing");
            
            // Wait a moment then test
            Invoke(nameof(TestMonsterMovement), 1f);
        }
    }
    
    [ContextMenu("Test Line of Sight")]
    public void TestLineOfSight()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            PlayerInventory playerInv = FindFirstObjectByType<PlayerInventory>();
            if (playerInv != null) player = playerInv.gameObject;
        }
        
        if (monster != null && player != null)
        {
            float distance = Vector3.Distance(monster.transform.position, player.transform.position);
            Vector3 direction = (player.transform.position - monster.transform.position).normalized;
            float angle = Vector3.Angle(monster.transform.forward, direction);
            
            Debug.Log($"=== LINE OF SIGHT TEST ===");
            Debug.Log($"Distance to player: {distance:F1} (Detection range: {monster.detectionRange})");
            Debug.Log($"Angle to player: {angle:F1} (Max angle: {monster.viewAngle/2f})");
            Debug.Log($"Monster forward: {monster.transform.forward}");
            Debug.Log($"Direction to player: {direction}");
            
            // Check if in range and angle
            bool inRange = distance <= monster.detectionRange;
            bool inAngle = angle <= monster.viewAngle / 2f;
            
            Debug.Log($"In range: {inRange}, In angle: {inAngle}");
            
            if (inRange && inAngle)
            {
                Debug.Log("Player should be VISIBLE to monster!");
            }
            else
            {
                if (!inRange) Debug.Log("PROBLEM: Player too far away - get closer!");
                if (!inAngle) Debug.Log("PROBLEM: Player outside view cone - monster needs to turn towards you!");
            }
        }
        else
        {
            Debug.LogError("Cannot test - missing monster or player");
        }
    }
    
    [ContextMenu("FORCE MONSTER CHASE")]
    public void ForceMonsterChase()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            PlayerInventory playerInv = FindFirstObjectByType<PlayerInventory>();
            if (playerInv != null) player = playerInv.gameObject;
        }
        
        if (monster != null && player != null)
        {
            // Force activate and start chasing
            monster.SetActive(true);
            
            // Directly set monster to chase the player (bypass line of sight)
            UnityEngine.AI.NavMeshAgent agent = monster.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = false;
                agent.speed = 4f; // Chase speed
                agent.SetDestination(player.transform.position);
                
                Debug.Log($"FORCING monster to chase player at {player.transform.position}");
                Debug.Log($"Monster position: {monster.transform.position}");
                Debug.Log($"Distance: {Vector3.Distance(monster.transform.position, player.transform.position):F1}");
            }
        }
        else
        {
            Debug.LogError("Cannot force chase - missing monster or player");
        }
    }
    
    [ContextMenu("Test Simple Detection")]
    public void TestSimpleDetection()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            PlayerInventory playerInv = FindFirstObjectByType<PlayerInventory>();
            if (playerInv != null) player = playerInv.gameObject;
        }
        
        if (monster != null && player != null)
        {
            float distance = Vector3.Distance(monster.transform.position, player.transform.position);
            bool inRange = distance <= monster.hearingRange;
            
            Debug.Log($"=== SIMPLE DETECTION TEST ===");
            Debug.Log($"Distance: {distance:F1} units");
            Debug.Log($"Hearing Range: {monster.hearingRange} units");
            Debug.Log($"Player in range: {inRange}");
            Debug.Log($"Monster active: {monster.IsActive()}");
            Debug.Log($"Use simple detection: {monster.useSimpleDetection}");
            
            if (inRange)
            {
                Debug.Log("✅ Player should be detected!");
            }
            else
            {
                Debug.Log($"❌ Player too far - get within {monster.hearingRange} units");
            }
        }
        else
        {
            Debug.LogError("Cannot test - missing monster or player");
        }
    }
    
    [ContextMenu("Debug Animation System")]
    public void DebugAnimationSystem()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        if (monster != null)
        {
            Animator animator = monster.GetComponent<Animator>();
            UnityEngine.AI.NavMeshAgent agent = monster.GetComponent<UnityEngine.AI.NavMeshAgent>();
            
            Debug.Log("=== ANIMATION SYSTEM DEBUG ===");
            
            // Check Animator
            if (animator != null)
            {
                Debug.Log($"✅ Animator found");
                Debug.Log($"Animator Controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "❌ NONE")}");
                Debug.Log($"Animator enabled: {animator.enabled}");
                
                // Check current animator state
                if (animator.runtimeAnimatorController != null)
                {
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    Debug.Log($"Current Animation State: {stateInfo.shortNameHash}");
                    Debug.Log($"Is in transition: {animator.IsInTransition(0)}");
                }
                
                // Check parameters
                bool hasIsMoving = false;
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    Debug.Log($"Parameter: {param.name} ({param.type})");
                    if (param.name == "IsMoving")
                    {
                        hasIsMoving = true;
                        Debug.Log($"✅ IsMoving parameter found, current value: {animator.GetBool("IsMoving")}");
                    }
                }
                
                if (!hasIsMoving)
                {
                    Debug.LogError("❌ IsMoving parameter NOT found!");
                }
            }
            else
            {
                Debug.LogError("❌ No Animator component found!");
            }
            
            // Check NavMeshAgent movement
            if (agent != null)
            {
                Debug.Log($"Agent velocity: {agent.velocity.magnitude:F2}");
                Debug.Log($"Agent moving: {agent.velocity.magnitude > 0.1f}");
                Debug.Log($"Agent isStopped: {agent.isStopped}");
                Debug.Log($"Agent hasPath: {agent.hasPath}");
            }
            
            Debug.Log("=== END ANIMATION DEBUG ===");
        }
        else
        {
            Debug.LogError("No MonsterAI found!");
        }
    }
    
    [ContextMenu("Force Test Animation")]
    public void ForceTestAnimation()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        if (monster != null)
        {
            Animator animator = monster.GetComponent<Animator>();
            if (animator != null)
            {
                Debug.Log("=== FORCING ANIMATION TEST ===");
                
                // Manually set IsMoving to true for 5 seconds
                animator.SetBool("IsMoving", true);
                Debug.Log("Set IsMoving to TRUE - walking animation should play now!");
                
                // Reset after 5 seconds
                Invoke(nameof(ResetAnimationTest), 5f);
            }
        }
    }
    
    private void ResetAnimationTest()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        if (monster != null)
        {
            Animator animator = monster.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                Debug.Log("Reset IsMoving to FALSE - should return to idle");
            }
        }
    }
    
    [ContextMenu("Animator Controller Diagnosis")]
    public void AnimatorControllerDiagnosis()
    {
        MonsterAI monster = FindFirstObjectByType<MonsterAI>();
        if (monster != null)
        {
            Animator animator = monster.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                Debug.Log("=== ANIMATOR CONTROLLER DIAGNOSIS ===");
                
                // Get current state info
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"Current State Hash: {stateInfo.shortNameHash}");
                Debug.Log($"State Length: {stateInfo.length}");
                Debug.Log($"Normalized Time: {stateInfo.normalizedTime}");
                Debug.Log($"Is Looping: {stateInfo.loop}");
                
                // Check if we're in a transition
                if (animator.IsInTransition(0))
                {
                    AnimatorTransitionInfo transInfo = animator.GetAnimatorTransitionInfo(0);
                    Debug.Log($"In Transition - Duration: {transInfo.duration}");
                }
                
                Debug.Log("=== DIAGNOSIS COMPLETE ===");
                Debug.Log("MANUAL CHECKS NEEDED:");
                Debug.Log("1. Open Animator window");
                Debug.Log("2. Select Walking state");
                Debug.Log("3. Check if Motion field has walking animation");
                Debug.Log("4. Select walking animation in Project");
                Debug.Log("5. Check 'Loop Time' checkbox");
                Debug.Log("6. Verify transitions: Idle->Walking (IsMoving=true), Walking->Idle (IsMoving=false)");
            }
            else
            {
                Debug.LogError("No Animator or Animator Controller found!");
            }
        }
    }
}
