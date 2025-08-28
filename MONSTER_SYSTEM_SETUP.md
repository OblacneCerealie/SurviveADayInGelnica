# Monster System Setup Guide

This guide walks you through setting up the Monster system that activates when sanity reaches 0, featuring AI behavior, line of sight detection, patient freezing, and animations.

## Overview

When the player's sanity reaches 0:
- All patients freeze (stop moving)
- The Monster becomes active and starts patrolling
- The Monster will chase the player when they're spotted
- The Monster loses the player if they break line of sight and get far enough away
- Animation plays when the Monster is moving

## Files Created

- `MonsterAI.cs` - Main Monster AI behavior script
- `PatientManager.cs` - Manages freezing/unfreezing all patients
- `MonsterManager.cs` - Handles Monster spawning and activation
- `MonsterSetupHelper.cs` - Utility script for easy setup

## Quick Setup Steps

### 1. Run Auto-Setup
1. Add the `MonsterSetupHelper` script to any GameObject in your scene
2. In the Inspector, click the **"Full Monster System Setup"** button
3. This will automatically create and configure most components

### 2. Create Monster Prefab
1. Find your Monster model in the scene (should be auto-configured)
2. Drag it to your Prefabs folder to create a prefab
3. Assign this prefab to the MonsterManager's "Monster Prefab" field

### 3. Set Up Animation Controller
Create an Animator Controller for the Monster:

1. Right-click in Project window → Create → Animator Controller
2. Name it "MonsterAnimatorController"
3. Open the Animator window
4. Add a **Bool parameter** called `IsMoving`
5. Create two states:
   - **"Idle"** (default state) - assign your idle animation or leave empty
   - **"Walking"** - assign the walking animation from `model_Animation_Walking_withSkin.fbx`
6. Create transitions:
   - Idle → Walking: Condition `IsMoving` equals `true`
   - Walking → Idle: Condition `IsMoving` equals `false`
7. Assign this controller to the Monster's Animator component

### 4. Configure Spawn Point
1. Create an empty GameObject where you want the Monster to spawn
2. Assign this GameObject to the MonsterManager's "Monster Spawn Point" field

### 5. Test the System
1. Play the game
2. Use the SanityManager test function "Set Sanity to 0" or wait for sanity to naturally drain
3. Observe:
   - Patients should freeze
   - Monster should spawn and start patrolling
   - Monster should chase you when you're in line of sight
   - Walking animation should play when Monster moves

## Manual Setup (Alternative)

If you prefer manual setup or the auto-setup doesn't work:

### Monster GameObject Setup
1. Add your Monster model to the scene
2. Add these components:
   - `NavMeshAgent`
   - `MonsterAI`
   - `Animator`
3. Configure NavMeshAgent:
   - Speed: 2 (patrol), will be overridden by script
   - Angular Speed: 180
   - Acceleration: 8
   - Stopping Distance: 0.5
   - Radius: 0.5
   - Height: 2

### MonsterAI Configuration
- **Patrol Radius**: 30 (how far the Monster wanders)
- **Detection Range**: 15 (how far the Monster can see)
- **Lose Target Range**: 20 (distance to lose player)
- **Patrol Speed**: 2
- **Chase Speed**: 4
- **View Angle**: 60 (degrees of vision cone)
- **Player Transform**: Assign the Player GameObject
- **Eyes Position**: Create child GameObject at Monster's eye level for line of sight

### Manager Setup
1. Create GameObject called "MonsterManager"
2. Add `MonsterManager` script
3. Assign Monster prefab and spawn point
4. Create GameObject called "PatientManager"
5. Add `PatientManager` script (auto-finds patients)

## System Architecture

### Event Flow
1. **Sanity reaches 0** → `SanityManager.OnSanityReachedZero` event
2. **MonsterManager** activates Monster
3. **PatientManager** freezes all patients
4. **Monster AI** begins patrolling and hunting behavior

### AI States
- **Inactive**: Monster is disabled (sanity > 0)
- **Patrolling**: Random wandering, looking for player
- **Chasing**: Following player directly
- **Searching**: Lost player, searching last known location

### Line of Sight System
The Monster uses raycast-based line of sight:
- Checks if player is within detection range
- Verifies player is within view angle cone
- Casts ray to ensure no obstacles block vision
- Considers walls, objects on obstacle layer mask

## Configuration Options

### MonsterAI Settings
```csharp
[Header("AI Behavior")]
public float patrolRadius = 30f;        // Wandering area size
public float detectionRange = 15f;      // How far Monster can see
public float loseTargetRange = 20f;     // Distance to lose player
public float patrolSpeed = 2f;          // Speed while patrolling
public float chaseSpeed = 4f;           // Speed while chasing
public float minPatrolWaitTime = 2f;    // Min wait between patrol points
public float maxPatrolWaitTime = 6f;    // Max wait between patrol points

[Header("Line of Sight")]
public float viewAngle = 60f;           // Vision cone angle
public LayerMask obstacleMask = 1;      // What blocks vision
```

### Animation Parameters
The Animator Controller expects:
- **IsMoving** (Bool): True when Monster is moving, False when idle

## Testing Tools

### SanityManager Test Functions
- "Test: Set Sanity to 0" - Triggers Monster activation
- "Test: Set Sanity to 60" - Deactivates Monster, unfreezes patients

### MonsterManager Test Functions
- "Test: Activate Monster" - Force activate without sanity check
- "Test: Deactivate Monster" - Force deactivate
- "Test: Spawn Monster" - Spawn Monster without activating

### PatientManager Test Functions
- "Test: Freeze All Patients" - Manually freeze patients
- "Test: Unfreeze All Patients" - Manually unfreeze patients

## Troubleshooting

### Monster Doesn't Spawn
- Check MonsterManager has prefab assigned
- Verify spawn point is set
- Ensure NavMesh exists at spawn location

### Monster Doesn't Move
- Check NavMeshAgent component exists and is enabled
- Verify NavMesh is baked in scene
- Check that Monster is actually activated (isActive = true)

### Animation Doesn't Play
- Ensure Animator Controller is assigned
- Check "IsMoving" parameter exists in controller
- Verify walking animation is assigned to Walking state
- Check transitions between Idle and Walking states

### Line of Sight Issues
- Adjust view angle and detection range
- Check obstacle layer mask settings
- Verify "Eyes" position is at appropriate height
- Use Scene view gizmos to visualize sight lines (enable showGizmos)

### Patients Don't Freeze
- Check PatientManager exists in scene
- Verify patients have PatientNPC components
- Check console for PatientManager messages about found patients

## Performance Notes

- Monster AI only runs when active (sanity = 0)
- Line of sight checks are optimized with early distance/angle checks
- Patients are managed centrally for efficiency
- System uses events to minimize coupling between components

## Future Enhancements

Possible improvements you could add:
- Multiple Monster spawn points
- Different Monster behaviors based on sanity level
- Sound effects for Monster movement/detection
- Flashlight interaction with Monster
- Monster fear mechanics (Monster avoids light)
- Difficulty scaling with time/day progression
