# Chaos Portal System - Rouge Like Interactables

## Overview
The Chaos Portal system implements special "Rouge Like" interactables that teleport players to high-pressure chaos zones with enhanced rewards. This system provides breakthrough points in the player experience and rewards exploration.

## Features
- **Chaos Portals**: Special interactables that transport players to challenging zones
- **Pressure Mechanics**: Environmental effects that create intensity during gameplay
- **Enhanced Rewards**: Guaranteed socket drops and bonus loot for completion
- **Scene Integration**: Automated placement in intro and procedural map scenes
- **Configurable Zones**: Scriptable object-based configuration for different zone types

## Components

### Core System Files
- `ChaosPortal.cs` - Main interactable that extends the base Interactable class
- `ChaosZoneManager.cs` - Manages chaos zone gameplay, waves, and pressure effects
- `ChaosZoneRewardSystem.cs` - Handles enhanced rewards and socket drops
- `SceneLoader.cs` - Extended to support chaos zone scene loading

### Scene Integration Files
- `ChaosPortalPlacer.cs` - Automated portal placement system for existing scenes
- `IntroLevelChaosSetup.cs` - Specific integration for the intro level
- `ChaosZoneConfig.cs` - Scriptable object for configuring zone parameters

### Utility Files
- `ChaosPortalSystemValidator.cs` - Testing and validation framework

## Usage Instructions

### For Developers

#### Adding Chaos Portals to Scenes
1. Add a `ChaosPortalPlacer` component to any GameObject in your scene
2. Configure spawn points in the inspector
3. Set scene-specific settings (intro level, procedural map, etc.)
4. Portals will be automatically created when the scene loads

#### Intro Level Integration
1. Add an `IntroLevelChaosSetup` component to introduce chaos portals early
2. Configure the portal position and tutorial messaging
3. The system will automatically create the portal after a delay

#### Creating Custom Chaos Zones
1. Create a new scene named "ChaosZone" or with "Chaos" in the name
2. Add a `ChaosZoneManager` component to manage the zone
3. Optionally create a `ChaosZoneConfig` asset for custom parameters
4. The system will automatically detect and manage chaos zone scenes

### For Players

#### Using Chaos Portals
1. Approach any red, glowing portal in the game world
2. Press Q (or configured interact key) when near the portal
3. You'll be teleported to a special chaos zone

#### Surviving Chaos Zones
- **Pressure Effects**: Your health and energy will slowly drain over time
- **Enemy Waves**: Multiple waves of enhanced enemies will spawn
- **Time Limit**: Complete all waves before the time limit expires
- **Rewards**: Successfully completing a chaos zone grants:
  - Guaranteed socket drops
  - Enhanced experience (2.5x multiplier)
  - Bonus loot rolls
  - Possible chaos-exclusive items

## Integration with Existing Systems

### Interactable System
- `ChaosPortal` extends the existing `Interactable` base class
- Uses the same interaction key (Q) and trigger system
- Maintains compatibility with existing interactable patterns

### Scene Management
- Extends `SceneLoader` enum with chaos zone scene types
- Adds `LoadChaosZone()` and `LoadRandomChaosZone()` methods
- Maintains backward compatibility with existing scene loading

### Reward System
- Integrates with existing `ItemDatabase` for loot drops
- Uses existing `PlayerStats` for experience rewards
- Compatible with existing `AudioManager` for sound effects

### Player System
- Works with existing `PlayerManager` and `IsometricPlayer`
- Uses existing `PlayerStats` for pressure effects
- Maintains existing movement and combat systems

## Configuration Options

### ChaosPortal Settings
- `chaosZoneSceneName`: Target scene to load
- `activationRadius`: Distance for player interaction
- `portalEffect`: Visual particle system
- `portalGlow`: Lighting effects
- `activeColor` / `inactiveColor`: Visual state colors

### ChaosZone Settings
- `pressureIntensity`: How strong environmental pressure effects are
- `zoneDuration`: Maximum time limit for the zone
- `difficultyMultiplier`: Enemy difficulty scaling
- `waveCount`: Number of enemy waves to survive
- `experienceMultiplier`: Bonus XP multiplier

### Reward Settings
- `guaranteedSocketDrop`: Whether sockets always drop
- `bonusLootRolls`: Additional loot generation attempts
- `chaosUniqueDropChance`: Chance for exclusive items

## Testing and Validation

### Using the Validator
The `ChaosPortalSystemValidator` can be used to test system integration:
```csharp
// In Unity Editor, use menu items:
// "Little Grinders/Validate Chaos Portal System" - Full validation
// "Little Grinders/Quick Chaos Portal Test" - Basic systems check
```

### Manual Testing
1. Place an `IntroLevelChaosSetup` in the intro scene
2. Play the game and wait for the portal to appear
3. Interact with the portal to test teleportation
4. Verify pressure effects and enemy spawning in chaos zones
5. Complete a zone to test reward distribution

## Troubleshooting

### Common Issues
- **Portal doesn't appear**: Check that `ChaosPortalPlacer` or `IntroLevelChaosSetup` is in the scene
- **No teleportation**: Ensure chaos zone scenes exist or fallback spawning works
- **No rewards**: Verify `ItemDatabase.instance` and `PlayerStats` are available
- **Missing effects**: Check `AudioManager.instance` and particle system assignments

### Debugging
- Enable verbose logging in `ChaosPortalSystemValidator`
- Use the validation menu items to check system status
- Check Unity console for detailed error messages and status updates

## Future Enhancements

### Potential Additions
- Multiple chaos zone variants (forest, cave, ruins, etc.)
- Progressive difficulty based on player level
- Chaos zone specific enemies and bosses
- Portal network system for zone-to-zone travel
- Leaderboards for fastest chaos zone completions
- Seasonal chaos events with special rewards

### Extensibility
The system is designed to be easily extended:
- Create new `ChaosZoneConfig` assets for different zone types
- Add new chaos zone scenes following naming conventions
- Extend `ChaosZoneRewardSystem` for new reward types
- Create custom pressure effects in `ChaosZoneManager`

## Technical Notes

### Performance Considerations
- Chaos zones use object pooling for enemies when possible
- Portal effects are managed efficiently with appropriate cleanup
- Scene transitions are handled asynchronously when supported

### Compatibility
- Requires Unity 2019.4 LTS or later
- Compatible with existing Little Grinders architecture
- No external dependencies beyond Unity built-ins

### File Structure
```
Assets/Scripts/Manager/World/
├── ChaosPortal.cs
├── ChaosZoneManager.cs
├── ChaosZoneRewardSystem.cs
├── ChaosPortalPlacer.cs
├── IntroLevelChaosSetup.cs
├── ChaosZoneConfig.cs
└── ChaosPortalSystemValidator.cs

Assets/Scripts/Manager/SceneManagement/
└── SceneLoader.cs (extended)
```

This system provides the requested "Rouge Like" interactables that create breakthrough points in the player experience while maintaining full compatibility with the existing Little Grinders architecture.