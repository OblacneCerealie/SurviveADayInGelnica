# Cube Interaction Setup Guide

## Overview
The CubeInteraction script allows players to press E near a cube to trigger a 4-second shake animation followed by dropping a prefab.

## Setup Instructions

### 1. Create a Cube GameObject
1. In Unity, right-click in the Hierarchy
2. Select `3D Object > Cube`
3. Position it where you want in your scene

### 2. Add the CubeInteraction Script
1. Select your cube in the Hierarchy
2. In the Inspector, click `Add Component`
3. Type "CubeInteraction" and select it
4. The script will be added to your cube

### 3. Configure the Script Settings

#### Required Setup:
- **Player Transform**: Drag your player GameObject from the Hierarchy into this field
  - *Note: If your player has the "Player" tag, this will be found automatically*
- **Prefab To Drop**: Drag any prefab from your Project window into this field

#### Optional Settings:
- **Interaction Range**: How close the player needs to be (default: 3 units)
- **Shake Duration**: How long the shake lasts (default: 4 seconds)
- **Shake Intensity**: How violent the shake is (default: 0.1)
- **Shake Intensity Curve**: Controls how the shake intensity changes over time
- **Drop Point**: Where the prefab spawns (defaults to cube position)
- **Drop Force**: How much force is applied to the dropped object

### 4. Test the Interaction
1. Play your scene
2. Walk your player near the cube
3. Press E when close enough
4. Watch the cube shake for 4 seconds and drop the prefab!

## How It Works
- Uses the same input system as other interactions in your project (E key)
- Follows the existing code patterns from PatientInteraction and BedInteraction
- Automatically detects the player if tagged with "Player"
- Shakes using a coroutine with customizable intensity curve
- Drops prefab with realistic physics

## Debug Features
- Console logs show interaction status
- Gizmos in Scene view show interaction range and drop point
- Toggle debug info on/off in the inspector

## Notes
- The cube returns to its original position after shaking
- Dropped objects automatically get a Rigidbody if they don't have one
- The script prevents multiple simultaneous shake animations
- Works with any prefab - could be pills, coins, treasure, etc.
