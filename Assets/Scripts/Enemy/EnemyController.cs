using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour, IEntitie
{
    //Create Variabel for the Player (through PlayerManager Singleton)
    //Transform character_transform;

    //Create Reference to currents GO Agent
    [Header("Welt Daten")]
    public NavMeshAgent myNavMeshAgent { get; private set; }

    //Create Reference to currents GO isoRenderer
    public IsometricRenderer myIsoRenderer { get; private set; }

    private EntitieState myEntitieState;

    public Transform Player => PlayerManager.instance.player.transform;

    //public Transform myTarget;


    [Header("Interface Visualisierung")]
    public GameObject hpBar { get; private set; }
    public Canvas combatCanvas { get; private set; }

    public Slider enemyHpSlider { get; private set; }

    public GameObject targetIndicator;

    /// <summary>
    /// Schaltet den Zielindikator für diesen Gegner an oder aus.
    /// </summary>
    /// <param name="active">true = anzeigen, false = verstecken</param>
    public void SetTargetIndicatorActive(bool active)
    {
        if (targetIndicator != null)
            targetIndicator.SetActive(active);
    }


    /// <summary>
    /// Bullet Types - finde eine bessere Methode die entsprechenden Scripte abzurufen.
    /// </summary>
    //private Steinwurf steinwurf;


    [Header("Combat Referenzen")]
    [Tooltip("Statistiken des Gegners")]
    public MobStats mobStats { get; private set; }
    [Tooltip("Angriffsverhalten des Gegners")]
    public IAttackBehavior attackBehavior;
    [Tooltip("Reichweite des Angriffs und die Distanz und wie nah der Gegner an den Spieler heran darf, bevor er stoppt")]
    public float attackRange, aggroRange, stoppingDistance;
    private float maxHpForUI;
    [HideInInspector]
    public bool isDead = false;
    [HideInInspector]
    public bool pulled = false; // Flag for wave-spawned enemies to immediately chase player
    //public int level;

    [Space]
    [Header("Custom Range")]
    [Tooltip("Benutzerdefinierte Reichweite für Tests")]
    public float customRange = 5f;
    [Tooltip("Farbe für Custom Range Gizmo")]
    public Color customRangeColor = Color.green;
    [Tooltip("Custom Range Gizmo anzeigen")]
    public bool showCustomRange = false;


    //public LootTable lootTable;


    ///----Position/Ziel/Direction----
    ///Controller Variables
    Vector3 myForward, myRight;




    //Erstelle einen Kreis aus der Aggro-Range für den Editor Modus
    void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, aggroRange);
            
        Vector3 position = transform.position;
        // Custom Range
        if (showCustomRange)
        {
            Gizmos.color = customRangeColor;
            Gizmos.DrawWireSphere(position, customRange);
        }
    }

    void Start()
    {
        mobStats = GetComponent<MobStats>();

        //UI Display
        TextMeshProUGUI[] statText = GetComponentsInChildren<TextMeshProUGUI>();

        // Spätestens wenn ich mehrere Level habe und der Spawn vom Player neu gesetzt wird, wird ein durchgehender Singleton nötig sein.
        SetLocalDirectionVariables();

        AddEssentialComponents();

        //Recalculate BaseStats in dependency of Map-Level
        maxHpForUI = mobStats.Hp.Value;

        //Setze die NavMeshAgent-Speed Eigenschaft entsprechend der MobStats.MovementSpeed.Value 
        if (myNavMeshAgent != null && mobStats != null)
            myNavMeshAgent.speed = mobStats.MovementSpeed.Value;

        // Audio Setup
        SetupAudioSource();

        TransitionTo(new IdleState(this));
    }
    void Update()
    {

        myEntitieState?.Update();
        UpdateAudioVolume();

        if (!mobStats.isDead)
        {
            CalculateHPCanvas();


            // 👉 Statt TargetDirection verwenden wir die echte Bewegung
            Vector3 moveDirection = myNavMeshAgent.velocity;

            // Optional glätten, falls nötig
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                myIsoRenderer.SetFacingDirection(new Vector2(moveDirection.x, moveDirection.z));
            }
        }
        else
            hpBar.GetComponent<CanvasGroup>().alpha = 0;

    }

    public void SetLocalDirectionVariables()
    {
        //Adapt the Isometric-Camera angles
        myForward = Camera.main.transform.forward;
        myForward.y = 0;
        myForward = Vector3.Normalize(myForward);
        myRight = Quaternion.Euler(new Vector3(0, 90, 0)) * myForward;
    }

    public virtual void AddEssentialComponents()
    {
        //Get Renderer Component.
        if (myIsoRenderer == null)
            myIsoRenderer = GetComponentInChildren<IsometricRenderer>();

        //Add the MobCamScript.
        if (gameObject.GetComponent<MobsCamScript>() == null)
            gameObject.AddComponent<MobsCamScript>();

        if (myNavMeshAgent == null)
            myNavMeshAgent = GetComponent<NavMeshAgent>();

        // Automatische Referenzierung der UI-Elemente
        if (hpBar == null)
            hpBar = transform.GetChild(2).gameObject;

        if (enemyHpSlider == null && hpBar != null)
            enemyHpSlider = hpBar.GetComponentInChildren<Slider>();

        // Attack Behavior Setup
        if (attackBehavior == null)
        {
            // Suche nach einem IAttackBehavior Component
            var behaviorComponent = GetComponent<IAttackBehavior>();
            if (behaviorComponent != null)
            {
                attackBehavior = behaviorComponent;
            }
            else
            {
                // Fallback: Component automatisch hinzufügen
                attackBehavior = gameObject.AddComponent<PendingAttack>();
            }
        }
    }

    #region Audio Management

    [Header("Audio Settings")]
    [Tooltip("Distanz unter der das Audio-Volume auf 0 gesetzt wird")]
    public float audioMuteDistance = 70f;
    private AudioSource myAudioSource;


    private void SetupAudioSource()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    private void UpdateAudioVolume()
    {
        if (myAudioSource == null) return;
        float distanceToPlayer = GetDistanceToPlayer();
        
        // AudioSource ein-/ausschalten basierend auf Distanz
        if (distanceToPlayer >= audioMuteDistance)
        {
            // Wenn weit entfernt: AudioSource deaktivieren
            if (myAudioSource.enabled)
                myAudioSource.enabled = false;
        }
        else
        {
            // Wenn nah genug: AudioSource aktivieren
            if (!myAudioSource.enabled)
                myAudioSource.enabled = true;
        }
    }
    #endregion

    #region Distance Calculations
    /// <summary>
    /// Berechnet die Entfernung zum Spieler
    /// </summary>
    /// <returns>Entfernung als float</returns>
    /// 
        public float GetDistanceToPlayer()
    {
        if (Player == null) return float.MaxValue;
        return Vector3.Distance(Player.position, transform.position);
    }

        public float TargetDistance()
    {
        return GetDistanceToPlayer();
    }

    /// <summary>
    /// Intervalle für den Zufälligkeitswert von zu wählenenden Richtungen
    /// </summary>
    private float directionTimer = 0f;
    private readonly float directionInterval = 0.5f;
    private Vector3 currentDirection = Vector3.zero;


    // Alte Funktion für Backward Compatibility (optional entfernen)

    
    public void MoveToPlayer()
    {
        if (myNavMeshAgent == null)
            myNavMeshAgent = GetComponent<NavMeshAgent>();

        // Wenn gepullt: Direkt zum Spieler laufen
        if (pulled)
        {
            if (!mobStats.isDead)
                myNavMeshAgent.SetDestination(Player.position);
            return;
        }

        // Normale Bewegung mit gewichteter Richtung (für Aggro-Verhalten)
        directionTimer -= Time.deltaTime;

        if (directionTimer <= 0f)
        {
            //Die Laufrichtung, welche in die Wahrschienlichkeitsrechnung als Hauptziel vergeben wird
            Vector3 preferredDirection = (Player.position - transform.position).normalized;

            // Gewichtet anhand von Wahrscheinlichkeiten Richtung Spieler laufen
            currentDirection = MovementHelper.GetWeightedDirection(preferredDirection);

            // Setze das Intervall für die Richtungsdefinition zurück
            directionTimer = directionInterval;
        }

        // Laufziel berechnen – z. B. 2 Meter in diese Richtung
        Vector3 destination = transform.position + currentDirection * 2f;

        if(!mobStats.isDead)
        myNavMeshAgent.SetDestination(destination);
        //Vector3 toPlayer = PlayerManager.instance.player.transform.position - transform.position;

    }

    public bool IsPlayerInAttackRange()
    {
        if (Player == null) return false;
        return GetDistanceToPlayer() <= attackRange;
    }

    public void StopMoving()
    {
        if (myNavMeshAgent != null && myNavMeshAgent.isActiveAndEnabled)
        {
            myNavMeshAgent.SetDestination(transform.position);
        }

    }

        public Vector2 TargetDirection()
    {
        Vector3 Direction = PlayerManager.instance.player.transform.position - transform.position;

        Vector2 outputVector = new Vector2(Direction.x * -1, Direction.z);

        outputVector = Vector2.ClampMagnitude(outputVector, 1);

        return outputVector;
    }

    #endregion

    #region Action StateMachine
    public void TransitionTo(EntitieState newState)
    {
        if (!mobStats.isDead)
        {
            myEntitieState?.Exit();
            myEntitieState = newState;
            myEntitieState.Enter();
        }
    }

    public void PerformAttack()
    {
        // TODO: Schadenslogik, Cooldowns etc.
        PlayerStats playerStats = PlayerManager.instance.player.transform.GetComponent<PlayerStats>();

        if (playerStats != null)
        {
            //Falls der AudioManager aus dem Hauptmenü nicht vorhanden ist, soll kein Sound abgespielt werden.
            //if (AudioManager.instance != null)
                //Spiele einen zufälligen Sound ab aus der Sound Gruppe "Grunt_Hit"
                //AudioManager.instance.PlaySound("Grunt_Hit");

            // 3% Chance auf kritischen Treffer
            bool isCrit = UnityEngine.Random.value < 0.03f;
            float damage = mobStats.AttackPower.Value;
            if (isCrit)
                damage *= 2;

            //Füge dem Spieler Schaden entsprechend der AttackPower hinzu. int Range derzeit irrelevant (0)
            playerStats.TakeDamage(damage, isCrit);

            //Calle Außerdem das GameEvent, dass der Spieler angegriffen wurde.
            //GameEvents.current.PlayerWasAttacked(mobStats, mobStats.AttackPower.Value);
            
            //Der Versuch einen AttackSpeed zu integrieren - je kleiner der mobStats.AttackSpeed.Value, desto mehr Zeit zwischen den Angriffen.
            //mobStats.attackCD = 1f / mobStats.AttackSpeed.Value;
        }
    }




    #endregion


    private void CalculateHPCanvas()
    {

        //Re-Calculate maxHp - this seems necessary, since inheriting Classes do not seem to run EnemyContrller Start Functions to initialize certain values.
        if (maxHpForUI == 0)
        {
                mobStats = GetComponent<MobStats>();
                maxHpForUI = mobStats.Hp.Value;
        }

        enemyHpSlider.value = mobStats.Hp.Value / maxHpForUI;

        if (mobStats.Hp.Value < maxHpForUI)
        {
            if (!mobStats.isDead)
                hpBar.GetComponent<CanvasGroup>().alpha = 1;


            if (KeyManager.MyInstance?.Keybinds != null && 
                KeyManager.MyInstance.Keybinds.ContainsKey("SHOW VALUES") && 
                Input.GetKey(KeyManager.MyInstance.Keybinds["SHOW VALUES"]) && !mobStats.isDead)
            {
                TextMeshProUGUI[] statText = GetComponentsInChildren<TextMeshProUGUI>();

                {
                    statText[1].text = Mathf.RoundToInt(mobStats.Hp.Value) + "/" + Mathf.RoundToInt(maxHpForUI);
                    statText[0].text = mobStats.level.ToString();
                }
            }
            else if(!mobStats.isDead)
            {
                TextMeshProUGUI[] statText = GetComponentsInChildren<TextMeshProUGUI>();

                {
                    statText[1].text = " ";
                    statText[0].text = " ";
                }

            }

        }


        if (mobStats.Hp.Value <= 0)
        {


            if(!mobStats.isDead)
            {
                //StopMoving();
                TransitionTo(new DeadState(this));
            }

            //StopMoving();
        }

    }

    public string GetBasePrefabName()
    {
        // Entfernt ggf. "(Clone)" und Ziffern am Ende
        string n = gameObject.name;
        int parenIndex = n.IndexOf(" (");
        if (parenIndex > 0)
            n = n.Substring(0, parenIndex);
        // Optional: Noch weitere Bereinigung, falls nötig
        return n;
    }

    public void TakeDamage(float incoming_damage, int range_radius_ofDMG, bool isCrit)
    {

        if (GetDistanceToPlayer() <= range_radius_ofDMG)
        {
            incoming_damage = 10 * (incoming_damage * incoming_damage) / (mobStats.Armor.Value + (10 * incoming_damage));

            incoming_damage = Mathf.Clamp(incoming_damage, 1, int.MaxValue);

            //Schadesberechnung
            mobStats.Hp.AddModifier(new StatModifier(-incoming_damage, StatModType.Flat));


            ///Upsi
            GameEvents.Instance.EnemyWasAttacked(incoming_damage, transform, isCrit);
            
            // NEU: Event für Bladevortex - Schaden erhalten
            GameEvents.Instance.EnemyTookDamage(this, isCrit);

            // Knockback
            if (!mobStats.isDead && gameObject.activeSelf)
                ApplyKnockback();

            //Setze den Entitie State auf "Hit"
            // Nur Hit-Animation, wenn NICHT im Angriff
            if (!(myEntitieState is HitState) && !isDead)
            {
                TransitionTo(new HitState(this));
            }

            //pulled = true; // Alles in AggroRange sollte ebenfalls gepulled werden.
        }
    }


    //Take Direct Damage ignoriert die Rüstungswerte der Entitie - besonders relevant für AP Schaden.
    public virtual void TakeDirectDamage(float incoming_damage, float range_radius_ofDMG)
    {
        float player_distance = GetDistanceToPlayer();

        if (player_distance <= range_radius_ofDMG)
        {

            mobStats.Hp.AddModifier(new StatModifier(-incoming_damage, StatModType.Flat));

            //Setze den Entitie State auf "Hit"
            myIsoRenderer.Play(AnimationState.Hit);

            if (!(myEntitieState is HitState) && !isDead)
            {
                TransitionTo(new HitState(this));
            }

            //pulled = true; // Alles in AggroRange sollte ebenfalls gepulled werden.
            //Damage Popup für direkten Schaden - aktuell hardcoded auf false für Crits
            GameEvents.Instance.EnemyTookDirectDamage(incoming_damage, this, false);
        }
    }

    private void ApplyKnockback()
    {
        //if (DirectionCollider.instance == null) return;

        Vector3 knockbackDirection = (transform.position - PlayerManager.instance.player.transform.position).normalized;

        // Knockback-Distanz
        float knockbackDistance = 0.2f;

        // Stelle sicher, dass wir nur horizontal verschieben
        knockbackDirection.y = 0;

        // Berechne neues Ziel
        Vector3 targetPosition = transform.position + knockbackDirection * knockbackDistance;

        // Bewegung direkt setzen oder animieren
        StartCoroutine(KnockbackRoutine(targetPosition, 0.3f));
    }

    private IEnumerator KnockbackRoutine(Vector3 targetPos, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        // Optionale Deaktivierung der NavMesh-Steuerung
        //myNavMeshAgent.SetDestination(transform.position);

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        // NavMeshAgent reaktivieren
        //myNavMeshAgent.enabled = true;
    }

    //Überarbeitungswürdig. Soll schließlich eine Abfrage für Collision mit sämtlichen Projektilen ergeben.
    public virtual void OnTriggerEnter(Collider collider)
    {
        //18.09.22 Die Projektile erhalten derzeit ihre eigene Klasse because logical Reasons.
        if (collider.gameObject.tag == "Bullet") // Andere Lösung finden zur Liebe des CPU. // Singleton für DirectionCollider?
        {

            /*
            //Derzeit müssten die entsprechenden Bullet-Scripts hier abgefragt werden, dats stupid.
            Steinwurf_Bullet steinwurf_bullet = collider.GetComponent<Steinwurf_Bullet>();

            TakeDirectDamage(steinwurf_bullet.steinwurf.damage, steinwurf_bullet.steinwurf.range); // <- Die Bullets werden endlos fliegen können, jedoch erst ab spell.range schaden machen.  
            */

        }

        if (collider.gameObject.tag == "DirCollider")
        {
            collider.gameObject.GetComponent<DirectionCollider>().collidingEnemyControllers.Add(this);
        }
    }

    public virtual void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.tag == "DirCollider")
        {
            collider.gameObject.GetComponent<DirectionCollider>().collidingEnemyControllers.Remove(this);
        }
    }


    //Damit EnemyController als IEntite gehandelt werden kann, hat VisualStudio hier diesen sexy absatz empfohlen. Danke VS Code.
    public void Die()
    {
        // NEU: Event für Bladevortex - Gegner gestorben
        GameEvents.Instance.EnemyDied(this);
        
        ((IEntitie)mobStats).Die();
    }

    public float Get_currentHp()
    {
        return ((IEntitie)mobStats).Get_currentHp();
    }

    public float Get_maxHp()
    {
        return ((IEntitie)mobStats).Get_maxHp();
    }

    public CharStats GetStat(EntitieStats stat)
    {
        return ((IEntitie)mobStats).GetStat(stat);
    }

    public Transform GetTransform()
    {
        return this.transform;
    }

    public List<BuffInstance> GetBuffs()
    {
        return ((IEntitie)mobStats).GetBuffs();
    }

    public void Heal(int healAmount)
    {
        ((IEntitie)mobStats).Heal(healAmount);
    }

    public void ApplyBuff(BuffInstance buff)
    {
        ((IEntitie)mobStats).ApplyBuff(buff);
    }

    public void RemoveBuff(BuffInstance buff)
    {
        ((IEntitie)mobStats).RemoveBuff(buff);
    }
}


//Mob Calculation. From my understanding, this is currently handled somewhere else.

/*
private void CalculateMobStats()
{
    //Alles noch nicht durchdacht, für pre alpha 0.3 solls reichen.
    if (GlobalMap.instance != null)
    {
        mobStats.level = mobStats.level + GlobalMap.instance.currentMap.mapLevel;

        if (mobStats.level > GlobalMap.instance.currentMap.mapLevel)
        {
            mobStats.Hp.AddModifier(new StatModifier(mobStats.Hp.Value + (GlobalMap.instance.currentMap.mapLevel * 10 / 2), StatModType.Flat));
            //Hp.AddModifier(new StatModifier(Hp.Value + (GlobalMap.instance.currentMap.mapLevel * 0.01f), StatModType.PercentMult));
            mobStats.Armor.AddModifier(new StatModifier((PlayerManager.instance.player.GetComponent<PlayerStats>().Get_level() / (GlobalMap.instance.currentMap.mapLevel + 1)), StatModType.Flat));
            mobStats.AttackPower.AddModifier(new StatModifier(mobStats.AttackPower.Value + (GlobalMap.instance.currentMap.mapLevel * 2), StatModType.Flat));

            //Print do double-check the values of increasing modifiers.
            //print("modifiers of mobs should have been added, resulting in:" + Hp.Value + " for Hp on " + gameObject.name + ". AttackPower: " + AttackPower.Value);
        }



        experience += mobStats.Get_xp() + GlobalMap.instance.currentMap.mapLevel * 10;

    }

}
*/