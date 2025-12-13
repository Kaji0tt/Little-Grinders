# Enemy Ability System

## Übersicht

Das Enemy Ability System ermöglicht es Gegnern, spezielle Fähigkeiten zusätzlich zu ihren normalen Angriffen zu nutzen. Das System arbeitet parallel zum bestehenden `IAttackBehavior` und integriert sich nahtlos in die State Machine.

## Architektur

### Kernkomponenten

1. **IAbilityBehavior Interface** (`IAbilityBehaviour.cs`)
   - Definiert Basis-Methoden für alle Abilities
   - Analog zu `IAttackBehavior`

2. **AbilityBehavior Basisklasse** (`IAbilityBehaviour.cs`)
   - Abstrakte Klasse mit Cooldown-Management
   - Priority-System für Priorisierung vs. Angriffe
   - Automatische Integration mit `MobStats.castCD` Multiplier

3. **CastState** (`EntitieState.cs`)
   - Neuer State für Ability-Casting
   - Handhabt Cast-Animation und Timing
   - Transition zurück zu Attack/Chase State

4. **GameEvents Erweiterungen**
   - `OnEnemyRequestCast` - Fordert Wechsel zu CastState an
   - `OnEnemyStartCast` - Cast beginnt (Animation-Trigger)
   - `OnEnemyCastComplete` - Cast beendet

## Verwendung

### Ability zu einem Gegner hinzufügen

1. **Erstelle eine neue Ability-Klasse**:
```csharp
public class MyCustomAbility : AbilityBehavior
{
    protected override void ExecuteAbility()
    {
        // Deine Ability-Logik hier
        Debug.Log("Custom Ability ausgeführt!");
    }
}
```

2. **Füge die Komponente zum Enemy GameObject hinzu**:
   - In Unity: Enemy GameObject auswählen
   - Add Component → Deine Ability (z.B. `TeleportAbility`)
   - Das System erkennt die Ability automatisch via `AddEssentialComponents()`

3. **Konfiguriere die Parameter im Inspector**:
   - **Cooldown Time**: Zeit zwischen Ability-Nutzungen (Sekunden)
   - **Cast Time**: Dauer des Casts (0 = instant)
   - **Range**: Maximale Reichweite
   - **Priority**: 0-100 (höher = wird bevorzugt ausgeführt)
   - **Animation Type**: Welche Animation während der Ability abgespielt wird
   - **Execution Timing**: Wann (0.0-1.0) die Ability während der Animation ausgeführt wird

### Animation-System

Abilities unterstützen alle verfügbaren Enemy-Animationen:

```csharp
public enum AbilityAnimationType
{
    None,       // Keine Animation
    Idle,       // Idle Animation
    Walk,       // Walk Animation  
    Attack1,    // Attack1 Animation
    Attack2,    // Attack2 Animation
    Casting,    // Casting Animation (Standard)
    Die1,       // Die1 Animation
    Die2,       // Die2 Animation
    Hit1,       // Hit1 Animation
    Hit2,       // Hit2 Animation
    Open1,      // Open1 Animation
    Open2       // Open2 Animation
}
```

**Animation Type** bestimmt welche Animation abgespielt wird:
- Dropdown-Auswahl im Inspector
- Automatische Animation-Clip Zuordnung
- Geschwindigkeits-Anpassung basierend auf `castTime`

**Execution Timing** (0.0 - 1.0) steuert wann `ExecuteAbility()` aufgerufen wird:
- `0.0` = Sofort am Anfang der Animation
- `0.5` = In der Mitte der Animation (Standard für die meisten Casts)
- `0.7` = Bei 70% der Animation (gut für Projektil-Würfe)
- `1.0` = Am Ende der Animation

**Beispiele**:
```csharp
// Teleport: Instant, keine sichtbare Cast-Animation
animationType = AbilityAnimationType.Casting;
executionTiming = 0.0f; // Sofort ausführen

// Fireball: Casting-Animation, Projektil bei 70%
animationType = AbilityAnimationType.Casting;
executionTiming = 0.7f; // Feuerball wird bei 70% abgefeuert

// Buff: Attack-Animation als "Power-Up" Geste
animationType = AbilityAnimationType.Attack1;
executionTiming = 0.5f; // Buff bei Höhepunkt der Animation
```

### Priority-System

Das System vergleicht Prioritäten zwischen Attack und Ability:

- **Attack Priority (Standard)**: 50
- **Ability Priority (Standard)**: 75
- **TeleportAbility (Beispiel)**: 80

Wenn `abilityBehavior.GetPriority() > attackBehavior.GetPriority()` und die Ability bereit ist, wird ein Cast ausgelöst statt eines Angriffs.

### Cooldown-System

Abilities haben zwei Cooldown-Komponenten:

1. **Base Cooldown** (`cooldownTime` in AbilityBehavior)
2. **Global Multiplier** (`MobStats.castCD`)

**Effektive Cooldown-Berechnung**:
```csharp
float effectiveCooldown = cooldownTime * mobStats.castCD;
```

Dies ermöglicht globale Balance-Tweaks ohne jede Ability einzeln anzupassen.

## Beispiel: TeleportAbility

Die `TeleportAbility` demonstriert **Instant-Cast** mit sofortiger Ausführung:

```csharp
public class TeleportAbility : AbilityBehavior
{
    [Header("Teleport Settings")]
    public float minDistanceFromPlayer = 3f;
    public float maxDistanceFromPlayer = 8f;
    public bool preferBehindPlayer = true;
    
    // Default-Werte in Reset()
    private void Reset()
    {
        cooldownTime = 8f;
        castTime = 0f; // Instant cast
        range = 15f;
        priority = 80;
        
        // Animation: Casting mit sofortiger Ausführung
        animationType = AbilityAnimationType.Casting;
        executionTiming = 0.0f; // Sofort am Anfang
    }
    
    // Erweiterte Bereitschafts-Prüfung
    public override bool IsAbilityReady(EnemyController controller)
    {
        if (!base.IsAbilityReady(controller))
            return false;
        
        // Nur teleportieren wenn strategisch sinnvoll
        float distance = Vector3.Distance(
            controller.transform.position, 
            controller.Player.position
        );
        
        // Teleportiere wenn zu weit weg (> 10m) ODER zu nah dran (< 2m)
        return (distance > activateIfPlayerFartherThan) || 
               (distance < activateIfPlayerCloserThan);
    }
    
    // Ability-Logik
    protected override void ExecuteAbility()
    {
        Vector3 teleportPosition = CalculateTeleportPosition();
        // NavMesh-Check und Teleport...
    }
}
```

## Beispiel: FireballAbility

Die `FireballAbility` demonstriert **Projektil-Casting** mit Timing:

```csharp
public class FireballAbility : AbilityBehavior
{
    [Header("Fireball Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;
    public float damageMultiplier = 2.0f;
    
    private void Reset()
    {
        cooldownTime = 6f;
        castTime = 1.5f; // 1.5 Sekunden Cast
        range = 12f;
        priority = 70;
        
        // Animation: Casting mit Projektil bei 70%
        animationType = AbilityAnimationType.Casting;
        executionTiming = 0.7f; // Feuerball bei 70% der Animation
    }
    
    protected override void ExecuteAbility()
    {
        // Spawn Projektil
        Vector3 spawnPos = controller.transform.position + spawnOffset;
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, ...);
        
        // Konfiguriere Projektil
        _projectile proj = projectile.GetComponent<_projectile>();
        proj._pDamage = controller.mobStats.AbilityPower.Value * damageMultiplier;
        proj._pSpeed = projectileSpeed;
        proj.SetOrigin(controller);
    }
}
```

**Execution Timing Visualisierung**:
```
Cast-Animation (1.5s):
|-----------|-----------|
0.0s      0.75s      1.5s
^           ^          ^
Start    Execute    End
         (70%)
```

### Verwendung von Abilities:

**TeleportAbility** (Instant):
1. Enemy GameObject auswählen
2. Add Component → `TeleportAbility`
3. Parameter:
   - **Cooldown**: 8s
   - **Cast Time**: 0s (instant)
   - **Animation Type**: Casting
   - **Execution Timing**: 0.0 (sofort)
   - **Priority**: 80

**FireballAbility** (Getimed):
1. Enemy GameObject auswählen
2. Add Component → `FireballAbility`
3. Projektil-Prefab zuweisen
4. Parameter:
   - **Cooldown**: 6s
   - **Cast Time**: 1.5s
   - **Animation Type**: Casting
   - **Execution Timing**: 0.7 (bei 70%)
   - **Priority**: 70

## Animation-Integration

Das System nutzt die bestehende `AnimationState.Cast`:

```csharp
// IsometricRenderer
public enum AnimationState
{
    Idle, Walk, Attack, Cast, Hit, Die
}
```

### Cast-Animation Trigger-Flow:

1. **AttackBehavior.OnUpdateAttack()** prüft Ability-Priorität
2. Bei höherer Priorität: `GameEvents.EnemyRequestCast(controller)`
3. **EnemyController.OnRequestCast()** → Transition zu `CastState`
4. **CastState.Enter()** → `GameEvents.EnemyStartCast(controller, castTime)`
5. **IsometricRenderer.OnEnemyStartCast()** → spielt "Casting" Animation
6. Nach `castTime`: `ExecuteAbility()` aufgerufen
7. **CastState Complete** → `GameEvents.EnemyCastComplete()`
8. **IsometricRenderer.OnEnemyCastComplete()** → `isPerformingAction = false`

## State Machine Integration

### State-Übergänge mit Abilities:

```
IdleState → (Aggro) → ChaseState
                         ↓
                    (In Range)
                         ↓
                    AttackState ←→ CastState
                         ↓              ↓
                 (Ability Priority)  (Complete)
```

**AttackState Logik**:
```csharp
public override void Update()
{
    // Prüft automatisch ob Ability Priorität hat
    controller.attackBehavior?.OnUpdateAttack(controller);
    
    // Falls Ability ready: → CastState
    // Sonst: Normaler Angriff
}
```

## Eigene Abilities erstellen

### Template für neue Abilities:

```csharp
using UnityEngine;

public class MyAbility : AbilityBehavior
{
    [Header("My Ability Settings")]
    public float myCustomParameter = 10f;
    
    // Default-Werte setzen
    private void Reset()
    {
        cooldownTime = 5f;     // Sekunden zwischen Nutzungen
        castTime = 1.5f;       // Cast-Dauer (0 = instant)
        range = 10f;           // Maximale Reichweite
        priority = 70;         // Priorität vs. Angriffe
        
        // Animation Settings
        animationType = AbilityAnimationType.Casting; // Welche Animation?
        executionTiming = 0.5f; // Wann ausführen? (0.0 - 1.0)
    }
    
    // Optional: Erweiterte Bereitschafts-Prüfung
    public override bool IsAbilityReady(EnemyController controller)
    {
        if (!base.IsAbilityReady(controller))
            return false;
        
        // Deine zusätzlichen Bedingungen hier
        return true;
    }
    
    // PFLICHT: Ability-Logik implementieren
    protected override void ExecuteAbility()
    {
        // Was soll passieren wenn die Ability ausgeführt wird?
        Debug.Log($"{controller.name} führt MyAbility aus!");
        
        // Beispiele:
        // - Projektil spawnen
        // - Buff/Debuff anwenden
        // - Heal/Damage
        // - Teleport/Movement
        // - Summon andere Entities
    }
    
    // Optional: Zusätzliche Update-Logik
    protected override void UpdateAbility()
    {
        // Wird jeden Frame aufgerufen während Cooldown läuft
        // z.B. für visuelle Effekte, Counter, etc.
    }
}
```

### Animation-Timing Beispiele:

#### Instant-Cast (Teleport, Buffs):
```csharp
castTime = 0f;
animationType = AbilityAnimationType.Casting;
executionTiming = 0.0f; // Sofort
```

#### Schneller Cast (Heilung):
```csharp
castTime = 0.8f;
animationType = AbilityAnimationType.Casting;
executionTiming = 0.5f; // In der Mitte
```

#### Langsamer Cast (Großer Fireball):
```csharp
castTime = 2.5f;
animationType = AbilityAnimationType.Casting;
executionTiming = 0.8f; // Kurz vor Ende für maximale Spannung
```

#### Attack-Animation für Power-Up:
```csharp
castTime = 1.2f;
animationType = AbilityAnimationType.Attack1;
executionTiming = 0.6f; // Bei Schlag-Animation
```

#### Keine Animation (Passive Effekte):
```csharp
castTime = 0f;
animationType = AbilityAnimationType.None;
executionTiming = 0.0f; // Timing irrelevant
```

### Ability-Typen Beispiele:

#### 1. Projektil-Ability
```csharp
protected override void ExecuteAbility()
{
    GameObject projectile = Instantiate(projectilePrefab, 
        controller.transform.position, 
        Quaternion.identity);
    
    _projectile proj = projectile.GetComponent<_projectile>();
    proj.SetOrigin(controller);
    proj._pDamage = controller.mobStats.AbilityPower.Value * 2f;
}
```

#### 2. Buff/Heal-Ability
```csharp
protected override void ExecuteAbility()
{
    float healAmount = controller.mobStats.Hp.BaseValue * 0.2f; // 20% HP
    controller.mobStats.Hp.AddModifier(new StatModifier(healAmount, StatModType.Flat));
}
```

#### 3. AoE-Ability
```csharp
protected override void ExecuteAbility()
{
    Collider[] hits = Physics.OverlapSphere(
        controller.transform.position, 
        aoeRadius, 
        LayerMask.GetMask("Player")
    );
    
    foreach (var hit in hits)
    {
        // Schaden/Effekt anwenden
    }
}
```

## Debugging

### Debug-Logs aktivieren:

Kommentiere Debug-Zeilen in folgenden Dateien ein:

- `CastState.cs`: Cast-State Transitions
- `IsometricRenderer.cs`: Animation-Events
- `TeleportAbility.cs`: Teleport-Positionen

### Gizmos für Ability-Ranges:

```csharp
private void OnDrawGizmosSelected()
{
    if (controller == null || controller.Player == null)
        return;
    
    Gizmos.color = Color.cyan;
    Gizmos.DrawWireSphere(controller.transform.position, range);
}
```

## Troubleshooting

### Ability wird nicht ausgeführt:

1. **Priority zu niedrig**: Erhöhe `priority` über Attack-Priority (>50)
2. **Cooldown aktiv**: Warte bis `GetCooldownRemaining() == 0`
3. **Range zu klein**: Erhöhe `range` oder prüfe Spieler-Distanz
4. **IsAbilityReady() false**: Prüfe Custom-Bedingungen in Override

### Animation spielt nicht:

1. **"Casting" Clip fehlt**: Stelle sicher dass Enemy-Animator "Casting" State hat
2. **isPerformingAction stuck**: Prüfe ob `OnEnemyCastComplete` Event ausgelöst wird
3. **turnOffIsometricRenderer**: Muss `false` sein für Animation-Events

### Cast-State funktioniert nicht:

1. **abilityBehavior == null**: Component vergessen hinzuzufügen
2. **GameEvents nicht registriert**: Prüfe `OnEnemyRequestCast` Listener
3. **State Machine deaktiviert**: `turnOffStandardBehaviour` muss `false` sein

## Performance-Überlegungen

- **Cooldown-Updates**: Laufen in `Update()`, optimiert für viele Enemies
- **Ability-Ready-Checks**: Nur wenn bereit wird Cast-Request ausgelöst
- **NavMesh Sampling**: `TeleportAbility` nutzt `NavMesh.SamplePosition()` effizient

## Erweiterungsmöglichkeiten

### Mehrere Abilities pro Enemy (Zukunft):

```csharp
public List<IAbilityBehavior> abilities;

// In OnUpdateAttack():
IAbilityBehavior bestAbility = null;
int highestPriority = GetPriority();

foreach (var ability in abilities)
{
    if (ability.IsAbilityReady(controller) && 
        ability.GetPriority() > highestPriority)
    {
        bestAbility = ability;
        highestPriority = ability.GetPriority();
    }
}

if (bestAbility != null)
    GameEvents.Instance?.EnemyRequestCast(controller);
```

### Ability-Chains (Combo-System):

```csharp
public IAbilityBehavior followUpAbility;
public float followUpChance = 0.5f;

protected override void ExecuteAbility()
{
    // Normale Ability-Logik...
    
    if (Random.value < followUpChance && followUpAbility != null)
    {
        StartCoroutine(ExecuteFollowUp());
    }
}
```

## Changelog

**Version 1.1** (Dezember 2025):
- ✨ **Animation-System**: `AbilityAnimationType` Enum für flexible Animation-Auswahl
- ✨ **Execution Timing**: Präzise Steuerung wann `ExecuteAbility()` während Animation aufgerufen wird (0.0 - 1.0)
- ✨ **11 Animation-Typen**: Idle, Walk, Attack1, Attack2, Casting, Die1, Die2, Hit1, Hit2, Open1, Open2, None
- 📝 `FireballAbility` als Beispiel für getimte Projektil-Abilities
- 🔧 `CastState` erweitert für präzises Timing basierend auf `executionTiming`
- 🎨 `IsometricRenderer` unterstützt alle Animation-Typen über `GetAnimationNameFromType()`

**Version 1.0** (Dezember 2025):
- Initial Release
- `IAbilityBehavior` Interface und `AbilityBehavior` Base Class
- `CastState` Integration
- Priority-System
- Cooldown-Management mit MobStats Multiplier
- GameEvents für Cast-Lifecycle
- `TeleportAbility` als Beispiel-Implementierung
- Animation-Integration via `AnimationState.Cast`
