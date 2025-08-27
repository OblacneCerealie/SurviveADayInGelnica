# Sanity System Setup Guide

This guide explains how to set up the patient spawning and sanity management system in your Unity scene.

## Overview

The system consists of several components that work together:

1. **PatientSpawner** - Spawns 3 patients with extended navigation range
2. **SanityManager** - Manages sanity levels and controls lighting
3. **GameManager** - Coordinates all systems
4. **SanitySystemSetup** - Helper script for easy setup

## Quick Setup

### Method 1: Automatic Setup (Recommended)

1. Add the `SanitySystemSetup` script to any GameObject in your scene
2. In the inspector, assign the Patient prefab to the "Patient Prefab" field
3. The system will automatically set up when you hit Play

### Method 2: Manual Setup

#### Step 1: Create GameManager
1. Create an empty GameObject named "GameManager"
2. Add the `GameManager` script to it

#### Step 2: Create PatientSpawner
1. Create an empty GameObject named "PatientSpawner"
2. Add the `PatientSpawner` script to it
3. Configure the following settings:
   - **Patient Prefab**: Assign your Patient prefab (Assets/Prefabs/Patient.prefab)
   - **Number Of Patients**: 3 (default)
   - **Spawn Radius**: 5f (default)
   - **Max Wander Radius**: 50f (extends walking to whole baked mesh)

#### Step 3: Create SanityManager
1. Create an empty GameObject named "SanityManager"
2. Add the `SanityManager` script to it
3. Configure the following settings:
   - **Max Sanity**: 100
   - **Current Sanity**: 100
   - **Sanity Decrease Interval**: 2f (decreases every 2 seconds)
   - **Sanity Decrease Amount**: 1
   - **Light Off Threshold**: 50 (lights turn off when sanity < 50)

#### Step 4: Create Sanity UI
1. Create a Canvas if one doesn't exist
2. Add a TextMeshPro - Text (UI) component as a child of the Canvas
3. Name it "SanityText"
4. Position it in the top-left corner
5. In the SanityManager, assign this TextMeshPro component to the "Sanity Display" field

## System Features

### Patient Spawning
- Spawns 3 patients automatically when the scene starts
- Patients have extended wander radius to cover the entire baked NavMesh
- Each patient is named "Patient_1", "Patient_2", "Patient_3"

### Sanity System
- Starts at 100 sanity
- Decreases by 1 every 2 seconds automatically
- **Draining stops when sanity reaches 0**
- **Draining resumes automatically when sanity is restored above 0** (e.g., through pills)
- Displays current sanity in UI as "Sanity: X/100"
- Text color changes based on sanity level:
  - White: Above 75%
  - Yellow: Between 50-75%
  - Red: Below 50%

### Lighting Control
- When sanity drops below 50, lights automatically turn off
- **NEW**: When sanity is below 50, lights can ONLY be restored using the Light Generator
- When sanity goes back above 50, lights turn back on automatically
- Uses the existing LightingManager system with enhanced sanity restrictions

## Configuration Options

### PatientSpawner Settings
- `numberOfPatients`: How many patients to spawn (default: 3)
- `spawnRadius`: Radius around spawn center for initial placement (default: 5f)
- `maxWanderRadius`: How far patients can wander (default: 50f)
- `spawnCenter`: Transform to use as spawn center (optional, uses own position if not set)

### SanityManager Settings
- `maxSanity`: Maximum sanity value (default: 100)
- `sanityDecreaseInterval`: How often sanity decreases in seconds (default: 2f)
- `sanityDecreaseAmount`: How much sanity decreases each interval (default: 1)
- `lightOffThreshold`: Sanity level below which lights turn off (default: 50)

## Events System

The SanityManager provides events you can subscribe to:

```csharp
SanityManager.OnSanityChanged += OnSanityChanged;
SanityManager.OnSanityBelowThreshold += OnSanityBelowThreshold;
SanityManager.OnSanityAboveThreshold += OnSanityAboveThreshold;
```

## Troubleshooting

### Patients Not Spawning
- Check that the Patient prefab is assigned in PatientSpawner
- Ensure there's a baked NavMesh in the scene
- Check console for error messages

### Sanity UI Not Showing
- Ensure there's a Canvas in the scene
- Check that the TextMeshPro component is assigned to SanityManager
- Verify the UI is not positioned off-screen

### Lights Not Turning Off
- Ensure LightingManager exists in the scene
- Check that the LightingManager is properly configured
- Verify the light off threshold is set correctly

### Navigation Issues
- Ensure the NavMesh is baked in the scene
- Check that the Patient prefab has a NavMeshAgent component
- Verify the wander radius allows for valid NavMesh positions

## Console Commands (Debug)

The system provides debug information. You can:

1. Use `SanitySystemSetup.ValidateSetup()` in the context menu to check if all components are present
2. Monitor console output for system status messages
3. Use the debug flags in each component to control log verbosity

## Integration with Existing Systems

This system integrates with:
- **LightingManager**: Controls lighting based on sanity with new force override capability
- **LightGenerator**: Can override sanity restrictions to force lights on
- **PatientNPC**: Extends navigation range
- **PatientInteraction**: Existing patient interaction system remains functional

The system is designed to work alongside existing game mechanics without conflicts.

## New Light Generator Integration

### Light Generator Override Feature
- **Sanity 50-100**: Normal operation - switches work automatically
- **Sanity 1-49**: Light switches disabled, only Light Generator can restore power
- **After Generator Activation (Sanity 1-49)**: Lights stay on permanently, switches work normally again
- **Sanity 0**: Complete power failure - even the Light Generator is disabled
- Provides layered gameplay progression based on sanity levels

### Testing the Integration
Use these context menu options on the respective components:

**SanityManager**:
- "Test: Set Sanity to 30 (Below Threshold)" - Forces low sanity to test restrictions
- "Test: Set Sanity to 60 (Above Threshold)" - Restores normal sanity
- "Test: Simulate Generator at Sanity 30" - Tests generator activation at low sanity
- "Test: Simulate Generator at Sanity 0" - Tests generator failure at zero sanity
- "Test: Check Switch Status" - Shows current system state and capabilities
- "Test: Sanity Recovery from Zero" - Tests sanity draining resumption after restoration

**LightingManager**:
- "Test: Try Normal Light Activation (Respects Sanity)" - Tests normal restrictions
- "Test: Force Light Activation (Ignores Sanity)" - Tests force override

**LightGenerator**:
- "Reset Generator" - Resets generator state for testing
