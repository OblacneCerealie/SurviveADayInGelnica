# Battery System Setup Guide

This guide explains how to set up the flashlight battery system with UI panel, battery drain mechanics, and battery pickups.

## Overview

The battery system adds realistic power management to your flashlight:
- **Battery drains 1% every 6 seconds** when flashlight is ON
- **UI panel shows battery percentage** when flashlight is equipped
- **Battery pickups restore 25% battery** (max 100%)
- **Flashlight disabled when battery reaches 0%**

## Quick Setup

### Step 1: Create Battery System Manager
1. Create an empty GameObject named "BatterySystem"
2. Add the `FlashlightBattery` script to it
3. Configure settings in inspector:
   - **Max Battery**: 100 (default)
   - **Current Battery**: 100 (starts full)
   - **Drain Rate**: 1 (percentage per interval)
   - **Drain Interval**: 6 (seconds between drain)
   - **Battery Recharge**: 25 (pickup restoration amount)

### Step 2: Create Battery UI Panel
1. **Create Canvas** (if you don't have one):
   - Right-click in Hierarchy → UI → Canvas
   - Set Canvas Scaler to "Scale With Screen Size"

2. **Create Battery Panel**:
   - Right-click Canvas → UI → Panel
   - Name it "BatteryPanel"
   - Position in corner of screen (e.g., top-right)
   - Resize to appropriate size (e.g., 200x60)

3. **Add Battery Text**:
   - Right-click BatteryPanel → UI → Text - TextMeshPro
   - Name it "BatteryText"
   - Set text to "Battery: 100%"
   - Center align and adjust font size

4. **Add Battery Slider** (Optional but recommended):
   - Right-click BatteryPanel → UI → Slider
   - Name it "BatterySlider"
   - Set Min Value: 0, Max Value: 100
   - Customize fill colors for visual appeal

5. **Add BatteryUI Script**:
   - Add `BatteryUI` script to BatteryPanel
   - Assign references in inspector:
     - **Battery Panel**: The panel GameObject
     - **Battery Text**: The TextMeshPro component
     - **Battery Slider**: The slider component
     - **Battery Fill Image**: The slider's fill image

### Step 3: Create Battery Pickups
1. **Create Battery Model**:
   - Create a Cube (or import battery 3D model)
   - Name it "BatteryPickup"
   - Scale it appropriately (e.g., 0.3, 0.6, 0.2 for battery shape)
   - Position it where players can find it

2. **Add BatteryPickup Script**:
   - Add `BatteryPickup` script to the battery GameObject
   - Configure in inspector:
     - **Visual Object**: The battery model
     - **Battery Recharge**: 25 (percentage restored)
     - **Interaction Range**: 3 (pickup distance)

3. **Visual Enhancement** (Optional):
   - Change material color to yellow/orange for battery look
   - Add emissive properties for visibility
   - Add particle effects for better feedback

### Step 4: Integration Check
1. Ensure your **Player** has:
   - `PlayerInventory` script (updated version)
   - `FlashlightController` script
   - Player tag set to "Player"

2. Your scene should have:
   - FlashlightBattery system (singleton)
   - BatteryUI on Canvas
   - At least one BatteryPickup in the world

## How It Works

### Battery Drainage
- **Starts**: When flashlight is turned ON with F key
- **Rate**: Loses 1% every 6 seconds (configurable)
- **Stops**: When flashlight is turned OFF or battery reaches 0%
- **Automatic Shutoff**: Flashlight force-disabled at 0% battery

### UI Display
- **Visibility**: Panel only shows when flashlight is equipped
- **Color Coding**:
  - Green: 51-100% battery
  - Yellow: 26-50% battery  
  - Red: 1-25% battery
  - Gray: 0% battery (depleted)
- **Blinking**: Red blinking effect when battery is depleted

### Battery Pickups
- **Requirement**: Must have flashlight equipped to use batteries
- **Interaction**: Walk near battery, press E to collect
- **Effect**: Restores 25% battery (doesn't exceed 100%)
- **Feedback**: Audio/visual effects on pickup

## Configuration Options

### FlashlightBattery Settings
```csharp
maxBattery = 100f;          // Maximum battery capacity
currentBattery = 100f;      // Starting battery level
drainRate = 1f;             // Percentage lost per interval
drainInterval = 6f;         // Seconds between drain ticks
batteryRecharge = 25f;      // Amount restored by pickups
```

### BatteryUI Visual Settings
```csharp
fullBatteryColor = Green;     // 51-100% color
mediumBatteryColor = Yellow;  // 26-50% color
lowBatteryColor = Red;        // 1-25% color
depletedBatteryColor = Gray;  // 0% color
lowBatteryThreshold = 25f;    // When to show red
mediumBatteryThreshold = 50f; // When to show yellow
```

### BatteryPickup Settings
```csharp
interactionRange = 3f;        // Pickup distance
batteryRecharge = 25f;        // Restoration amount
```

## Expected Behavior

### Normal Flow
1. **Equip Flashlight**: UI panel appears showing 100%
2. **Turn ON (F key)**: Battery starts draining every 6 seconds
3. **Turn OFF (F key)**: Battery drain stops
4. **Find Battery**: Approach pickup, press E to collect
5. **Battery Depleted**: Flashlight automatically turns off at 0%

### Visual Feedback
- **100%**: Green "Battery: 100%"
- **50%**: Yellow "Battery: 50%"
- **10%**: Red "Battery: 10%"
- **0%**: Gray blinking "Battery: 0%"

## Testing Features

### FlashlightBattery Debug Methods
- `"Test: Set Battery to 10%"` - Test low battery state
- `"Test: Deplete Battery"` - Test empty battery behavior
- `"Test: Recharge Battery"` - Simulate battery pickup
- `"Test: Full Battery"` - Reset to 100%

### BatteryUI Debug Methods
- `"Test: Update Display"` - Manually refresh UI display

### BatteryPickup Debug Methods
- `"Test: Pickup Battery"` - Force pickup battery
- `"Test: Set Recharge to 50%"` - Change pickup amount

## Troubleshooting

### UI Not Showing
- Check Canvas exists and has proper setup
- Verify BatteryUI script is on BatteryPanel
- Ensure player has flashlight equipped
- Check batteryPanel reference is assigned

### Battery Not Draining
- Verify FlashlightBattery script is in scene
- Check flashlight is actually turned ON (F key)
- Ensure FlashlightController integration is working
- Look for error messages in console

### Pickups Not Working
- Confirm player has flashlight first
- Check interaction range and player distance
- Verify BatteryPickup script is attached
- Ensure FlashlightBattery.Instance exists

### Battery Drain Too Fast/Slow
- Adjust `drainInterval` (6 = every 6 seconds)
- Modify `drainRate` (1 = 1% per interval)
- Example: 0.5% every 10 seconds = longer battery life

## Integration Notes

- **Singleton Pattern**: FlashlightBattery uses singleton for easy access
- **Event System**: UI updates automatically via events
- **Persistence**: Battery level maintained during gameplay
- **Safety Checks**: System prevents flashlight use when battery depleted
- **Performance**: Efficient coroutine-based drain system

The battery system adds strategic resource management to your flashlight, encouraging players to explore and find battery pickups while managing their light usage carefully.
