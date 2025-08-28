using UnityEngine;

[CreateAssetMenu(fileName = "MonsterAnimationHelper", menuName = "Monster System/Animation Helper")]
public class MonsterAnimationHelper : ScriptableObject
{
    [Header("Animation Setup Instructions")]
    [TextArea(10, 20)]
    public string instructions = @"MONSTER ANIMATION CONTROLLER SETUP:

1. CREATE ANIMATOR CONTROLLER:
   - Right-click in Project → Create → Animator Controller
   - Name it 'MonsterAnimatorController'

2. SET UP PARAMETERS:
   - Open Animator window
   - In Parameters tab, add Bool parameter: 'IsMoving'

3. CREATE STATES:
   - Right-click in Animator graph → Create State → Empty
   - Name first state 'Idle' (set as default state)
   - Create second state 'Walking'

4. ASSIGN ANIMATIONS:
   - Select 'Walking' state
   - In Inspector, assign 'model_Animation_Walking_withSkin' to Motion field
   - Leave 'Idle' empty or assign idle animation if you have one

5. CREATE TRANSITIONS:
   - Right-click 'Idle' → Make Transition → Click 'Walking'
   - In transition settings:
     * Uncheck 'Has Exit Time'
     * Add Condition: IsMoving equals true
   
   - Right-click 'Walking' → Make Transition → Click 'Idle'  
   - In transition settings:
     * Uncheck 'Has Exit Time'
     * Add Condition: IsMoving equals false

6. ASSIGN TO MONSTER:
   - Select Monster GameObject
   - Assign controller to Animator component
   - The MonsterAI script will automatically control the IsMoving parameter

TESTING:
- When Monster patrols/chases, IsMoving = true → Walking animation
- When Monster stops/waits, IsMoving = false → Idle state";

    [ContextMenu("Print Instructions")]
    public void PrintInstructions()
    {
        Debug.Log(instructions);
    }
}
