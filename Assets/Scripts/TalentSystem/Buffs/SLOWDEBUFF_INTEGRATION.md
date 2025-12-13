# Buff System Integration - SlowDebuff für AimedShotAbility

## Entscheidung: Bestehendes System beibehalten ✅

Das bestehende Buff-System mit ScriptableObjects ist **gut strukturiert** und wird beibehalten. Die neue `AimedShotSlowEffect` Komponenten-Lösung wurde durch die offizielle Buff-System Integration ersetzt.

---

## Neuer Buff: SlowDebuff

### Datei
`Assets/Scripts/TalentSystem/Buffs/SlowDebuff.cs`

### Features
- ✅ **Linear abklingender Slow-Effekt** (95% → 0% über Zeit)
- ✅ **ScriptableObject-basiert** (Unity Inspector konfigurierbar)
- ✅ **StatModifier Integration** für MovementSpeed
- ✅ **Tick-basiertes Update** (smooth transition)
- ✅ **Wiederverwendbar** für andere Abilities

### Lifecycle
```
Activated() → Initialer Slow wird angewendet
   ↓
OnTick() → Slow-Prozentsatz wird linear reduziert (alle 0.1s)
   ↓
Expired() → Slow-Modifier wird entfernt
```

---

## Setup in Unity

### 1. SlowDebuff ScriptableObject erstellen
1. Rechtsklick im Project Window → **Create → Buff → SlowDebuff**
2. Benenne es z.B. "AimedShot_Slow"
3. Konfiguriere im Inspector:
   ```
   Buff Name: "AimedShot_Slow"
   My Duration: 3.0 (Sekunden)
   Initial Slow Percentage: 0.95 (95% Slow)
   Tick Intervall: 0.1 (alle 0.1s Update für smooth transition)
   Stackable: ☐ false
   Particle Effect: (optional - z.B. Slow-VFX)
   Icon: (optional - für UI)
   ```

### 2. AimedShotAbility konfigurieren
Im Enemy GameObject mit `AimedShotAbility`:
```
[Aimed Shot Settings]
Projectile Prefab: Arrow_01 (Pfeil-Prefab)
Projectile Speed: 20
Damage Multiplier: 2.0 (doppelter Schaden)
Spawn Offset: (0, 1, 0)

[Slow Effect Settings]
Slow Debuff: AimedShot_Slow ← ScriptableObject hier reinziehen!
```

### 3. Fertig!
Der Pfeil-Prefab braucht **keine** Konfiguration:
- ✅ `buff` wird automatisch von AimedShotAbility gesetzt
- ✅ `_pSpecialEffect` wird automatisch aktiviert

---

## Code-Änderungen

### AimedShotAbility.cs
**Entfernt:**
- ❌ `AimedShotSlowEffect` Komponenten-Klasse
- ❌ `CreateAndAttachSlowBuff()` Methode
- ❌ Coroutine-basierte Slow-Logik
- ❌ `initialSlowPercentage` / `slowDuration` Felder (kommen jetzt aus SlowDebuff SO)
- ❌ BuffDatabase-Abhängigkeit

**Hinzugefügt:**
- ✅ `public SlowDebuff slowDebuff` (direkte SlowDebuff-Referenz)
- ✅ Direkte Integration mit `_projectile.buff`

**Vorher (Komponenten-basiert):**
```csharp
AimedShotSlowEffect slowEffect = projectileObj.AddComponent<AimedShotSlowEffect>();
slowEffect.Initialize(initialSlowPercentage, slowDuration);
```

**Nachher (Buff-System - Vereinfacht):**
```csharp
if (slowDebuff != null)
{
    projectile.buff = slowDebuff; // ✅ Nur 1 Zeile!
    projectile._pSpecialEffect = true;
}
```

**Alle Slow-Parameter sind im SlowDebuff SO gespeichert!**

---

## Vorteile der neuen Lösung

### 1. **Konsistenz**
- Alle Buffs nutzen das gleiche System (Poison, WeakArmor, SlowDebuff)
- Einheitliche Lifecycle-Methoden

### 2. **Unity-Editor Integration**
- SlowDebuff als ScriptableObject im Inspector konfigurierbar
- Keine Code-Änderungen für Balance-Tweaks nötig

### 3. **Wiederverwendbarkeit**
- SlowDebuff kann von anderen Abilities genutzt werden
- Einfach per Drag & Drop in Ability ziehen

### 4. **Vereinfachung**
- ✅ Keine BuffDatabase mehr nötig
- ✅ Keine doppelte Konfiguration (Ability + SO)
- ✅ Nur 1 Feld im Inspector: `slowDebuff`

### 5. **Debug-Freundlichkeit**
- UI_Buff System kann Slow-Effekt anzeigen
- BuffInstance in PlayerStats.activeBuffs sichtbar
- Klare Trennung: Ability → Projektil → Buff → Player

---

## Testing Checklist

- [ ] SlowDebuff ScriptableObject erstellt mit korrekten Werten (Slow%, Dauer, Tick-Intervall)
- [ ] AimedShotAbility hat slowDebuff-Referenz zugewiesen (Drag & Drop im Inspector)
- [ ] Enemy mit AimedShotAbility spawnt und schießt Projektil
- [ ] Spieler wird getroffen und verlangsamt (95% → 0% über 3s smooth)
- [ ] Slow-Effekt erscheint in UI (falls UI_Buff implementiert)
- [ ] Console zeigt: `[AimedShotAbility] ... feuert Aimed Shot mit ... Schaden und 'AimedShot_Slow' Debuff!`
- [ ] Keine Console-Errors oder NullReferenceExceptions

---

## Kompatibilität mit anderen Buffs

Alle bestehenden Buffs bleiben unverändert:
- ✅ **Poison** → Tick-basierter Schaden
- ✅ **WeakArmor** → Armor-Reduktion
- ✅ **Reflection** → Schaden reflektieren
- ✅ **LifePoison** → HP-Drain
- ✅ **SlowDebuff** → NEU: Verlangsamung

---

## Zukünftige Erweiterungen

Weitere Debuffs nach diesem Muster:
- **StunDebuff** → Bewegung & Angriff deaktivieren
- **BleedDebuff** → HP-Verlust über Zeit
- **SilenceDebuff** → Abilities deaktivieren
- **RootDebuff** → Bewegung deaktivieren, Angriff möglich

Alle nutzen das gleiche ScriptableObject-Pattern! 🎯
