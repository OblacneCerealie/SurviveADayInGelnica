# Flashlight System Setup Guide

This guide explains how to set up the flashlight pickup and usage system in your Unity scene.

## Quick Setup

### Step 1: Setup Your Flashlight GameObject
1. Create a **Cube** in your scene and name it "Flashlight"
2. Add a **Light** component to it (or as a child object)
3. Set the Light type to **Spot Light** for best flashlight effect
4. Configure Light settings:
   - **Range**: 10-15
   - **Spot Angle**: 30-45 degrees
   - **Intensity**: 2-3
   - **Color**: White or warm white

### Step 2: Add Flashlight Scripts
1. Add the `FlashlightPickup` script to your flashlight GameObject
2. In the inspector, assign:
   - **Visual Object**: The flashlight cube GameObject
   - **Flashlight Light**: The Light component
   - **Interaction Range**: 3 (or desired pickup distance)

### Step 3: Setup Player Systems
1. Find your **Player** GameObject and ensure it has the "Player" tag
2. Add the `FlashlightController` script to your Player (or any persistent GameObject)
3. In the inspector, assign:
   - **Flashlight Attach Point**: Usually your main camera or player camera
   - **Toggle Key**: F (default)

### Step 4: Ensure PlayerInventory Exists
1. Make sure your Player has the updated `PlayerInventory` script
2. The script will automatically handle flashlight inventory management

## How It Works

### Pickup Phase
- Player approaches flashlight cube
- When in range (default 3 units), debug message shows: *"Flashlight nearby - Press E to pick up"*
- Press **E** to collect the flashlight
- Flashlight disappears from world and goes to inventory
- Light component gets attached to the player's camera

### Usage Phase
- Once collected, press **F** to toggle flashlight ON/OFF
- Flashlight starts OFF by default
- Debug messages show current state
- Light follows player's camera/view direction

## Configuration Options

### FlashlightPickup Settings
- `interactionRange`: How close player needs to be to pick up (default: 3)
- `interactionKey`: Key to pick up flashlight (default: E)
- `showDebugInfo`: Enable/disable debug messages

### FlashlightController Settings
- `toggleKey`: Key to toggle flashlight (default: F)
- `flashlightOnSound`: Audio clip for turning on
- `flashlightOffSound`: Audio clip for turning off
- `flashlightModel`: Optional 3D model to show when equipped

### Light Settings (Recommended)
- **Type**: Spot Light
- **Range**: 10-15 units
- **Spot Angle**: 30-45 degrees
- **Inner Spot Angle**: 20-35 degrees
- **Intensity**: 2-3
- **Color**: White (#FFFFFF) or warm white
- **Shadows**: Soft Shadows (if performance allows)

## Testing

### Debug Commands
The system provides automatic debug feedback:
- Pickup messages when in range
- Collection confirmations
- Toggle state messages
- Error messages if components are missing

### Expected Behavior
1. **Before Pickup**: Light is OFF and attached to world object
2. **During Pickup**: Light transfers to player, stays OFF
3. **After Pickup**: Press F toggles light ON/OFF, follows player view
4. **Inventory Integration**: PlayerInventory.GetInventoryStatus() includes "Flashlight"

## Troubleshooting

### Flashlight Not Picking Up
- Check player has "Player" tag
- Verify FlashlightPickup script is attached
- Ensure player is within interaction range
- Check console for error messages

### F Key Not Working
- Verify FlashlightController script is on a persistent GameObject
- Check that flashlight was successfully picked up
- Ensure Light component is assigned
- Verify toggleKey is set to F

### Light Not Visible
- Check Light component is enabled in inspector
- Verify Range and Intensity settings
- Make sure light is pointing in right direction
- Check for conflicting lighting setups

### Performance Tips
- Use one flashlight at a time
- Consider disabling shadows for mobile builds
- Limit light range to necessary distance
- Use Light Layers to control what gets lit

## Advanced Features

### Audio Integration
- Assign audio clips to flashlightOnSound and flashlightOffSound
- System automatically plays appropriate sound on toggle

### Visual Enhancement
- Add a 3D flashlight model to flashlightModel field
- Model will show/hide based on inventory status
- Consider adding glow effects or UI indicators

### Game Integration
- Flashlight state persists through scenes
- Integrates with existing PlayerInventory system
- Can be removed programmatically if needed

The flashlight system is designed to work seamlessly with your existing game mechanics and provide a realistic flashlight experience for players exploring dark environments.
