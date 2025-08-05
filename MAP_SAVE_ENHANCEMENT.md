# Map Save Interactable Enhancement

## Overview
This enhancement adds support for saving and loading interactable objects (like chests, totems, etc.) in the Map Save system. When a map is saved, all interactables on that map are stored along with their current state (used/unused). When the map is loaded again, the saved interactables are restored instead of generating new random ones.

## Features
- **Persistent Interactables**: Interactables maintain their positions and states across save/load cycles
- **State Preservation**: Used/unused state is preserved (opened chests stay opened)
- **Visual State**: Used interactables maintain their visual appearance (sprite changes)
- **Automatic Integration**: Works seamlessly with existing save/load system

## Technical Implementation

### New Classes
- **InteractableSave**: Serializable data structure storing position, type, and used state
- **MapSaveTest**: Test script for manual testing (optional)

### Modified Classes
- **MapSave**: Added interactables list and save/restore methods
- **PrefabCollection**: Added method to get specific interactable by name
- **MapGenHandler**: Added flag to prevent duplicate generation during loading
- **OutsideVegLoader**: Skip interactable generation when loading existing maps
- **GlobalMap**: Added method to update current map interactables
- **PlayerSave**: Added interactable save during player save

### Key Methods
- `MapSave.SaveInteractables()`: Collects and saves all interactables in the scene
- `MapSave.RestoreInteractables()`: Recreates saved interactables in the scene
- `PrefabCollection.GetInteractableByName()`: Gets specific interactable prefab by name
- `GlobalMap.UpdateCurrentMapInteractables()`: Updates current map with latest states

## How It Works

### Map Creation (New Maps)
1. Map generates normally with random interactables
2. `CreateAndSaveNewMap()` calls `SaveInteractables()` 
3. Interactables are stored in the MapSave

### Map Loading (Existing Maps)
1. `LoadMap()` sets flag to skip new interactable generation
2. Map loads with terrain and environment
3. `RestoreInteractables()` recreates saved interactables
4. Used state and sprites are restored

### Player Save
1. Player save automatically updates current map interactables
2. All explored maps (with their interactables) are saved to PlayerSave
3. Interactable states persist in save file

## Usage

### Automatic (Recommended)
The system works automatically with no additional code required:
- Create/explore maps normally
- Use interactables (open chests, activate totems)
- Save/load game normally
- Interactables will persist automatically

### Manual Testing
Use the `MapSaveTest` script for debugging:
1. Attach `MapSaveTest` to any GameObject in the scene
2. F1: Manually save current interactables
3. F2: Clear and restore saved interactables  
4. F3: Clear all interactables
5. Check console for detailed logs

### Integration Notes
- Interactable prefabs must be in PrefabCollection.interactablesPF array
- Interactables must inherit from the base Interactable class
- New interactable types work automatically (no code changes needed)

## Benefits
- **Player Experience**: Opened chests stay opened, activated totems stay activated
- **Game Balance**: Prevents exploitation by reloading maps for new loot
- **Immersion**: World feels more persistent and real
- **Technical**: Minimal performance impact, robust error handling

## Compatibility
- Fully backward compatible with existing save files
- New saves include interactable data
- Old saves continue to work (generate new interactables as before)
- No breaking changes to existing gameplay systems