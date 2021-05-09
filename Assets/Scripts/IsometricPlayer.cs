using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IsometricPlayer : MonoBehaviour
{
    #region Implementation - Basic 

    //Klasse, welche Isometrische Darstellungen kalkuliert.
    IsometricRenderer isoRenderer;

    //Wird im Inspektor eingestellt, um das Game-Objekt der Waffe und dessen Animator zu erkennen.
    [SerializeField]
    private GameObject weaponGameObject;

    //Wichtig, um die Sprites entsprechend der Isometrie für die Animationen zu kalkulieren
    private Vector2 inputVector;

    //Hinzufügen der PlayerStats (AttackPower, etc.)
    public PlayerStats playerStats { get; private set; }

    //Füge physischen Körper von Unity hinzu
    Rigidbody rbody;

    //Implementiere Vector für die Bewegung
    Vector3 forward, right;

    //Implementiere Zoom Distanz
    float zoom;

    //Noch nicht Verwendet - wird gebraucht um zu erkennen Attack im Fernkampf ausgeführt wird.
    public bool rangedWeapon;
    #endregion

    #region Implementation - Item und Inventory
    private Inventory inventory;

    public List<ItemInstance> equippedItems = new List<ItemInstance>();

    [SerializeField] private UI_Inventory uiInventory;

    /// <summary>
    /// 
    /// Eigene Classe Vorteile:
    /// Ausgerüstete Items könnten Aktive Fähigkeiten haben - voll cool.
    /// 
    /// Einzelne Liste:
    /// Einfach.
    /// 
    /// </summary>

    #endregion

    #region Implementation - Interface Objekte und Texte

    GameObject uiInventoryTab, uiHpOrb;
    private Text uiHealthText, ui_invHealthText, ui_invArmorText, ui_invAttackPowerText, ui_invAbilityPowerText, ui_invAttackSpeedText, ui_invMovementSpeedText, ui_Level, ui_Xp, ui_HpOrbTxt;
    public Slider HpSlider, XpSlider;
    private float maxHP;

    #endregion

    #region Implementation - Combat Stance

    //Ziel-Vector für Raycast der Maus
    private Vector3 targetDirection;

    //Combat-Stance Timer, um Attack entsprechend der Attacksped abzurufen
    private float combatStanceTime;

    #endregion

    #region Implementation - Idle Rotation

    // Die Dauer, bis die Rotation einsetzt.
    private float idle_time;

    // Die Geschwindigkeit, mit welcher rotiert wird.
    public float idleRotSpeed = 10;

    // Wie weit darf die Kamera nach Links rotieren
    private float xrotClamp = 15;

    //Wie weit neigt sich die Kamera
    private float yrotClamp = 50;

    //Wie weit zoomed die Kamera raus
    private float idleFOVClamp = 40;

    //Bool um zu überprüfen, ob der Spieler noch AFK ist.
    private bool idle = false;

    #endregion

    // Item which spawns upon load:
    //public Item test_item, test_item2, test_item3;

    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        isoRenderer = GetComponentInChildren<IsometricRenderer>();
        playerStats = GetComponent<PlayerStats>();
        rangedWeapon = false;

        //Isometric Camera
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        
        //Item Management
        inventory = new Inventory(UseItem);//UseItem = Welche Methode wird in Isometric Player bei Nutzung ausgelöst.
        uiInventory.SetInventory(inventory);
        uiInventory.SetCharakter(this);

        //PlayerStats & UI
        //GameEvents.current.LevelUpdate += UpdateLevel;
        uiInventoryTab = GameObject.Find("Inventory Tab");              
        ui_Level = GameObject.Find("LevelText").GetComponent<Text>();
        ui_Xp = GameObject.Find("XpText").GetComponent<Text>();
        uiHpOrb = GameObject.Find("HpOrbTxt");
        GameObject uiXp = GameObject.Find("XpText");
        //////////////////////Das ist ja fucking eklig, finde eine Alternative um die Texte zu initialisieren!!!!!
        playerStats.Set_currentHp(playerStats.Get_maxHp());

        //PlayerStat Inventory - ***********Initialisieren von Texten**********
        ui_invHealthText = GameObject.Find("ui_invHp").GetComponent<Text>();
        ui_invArmorText = GameObject.Find("ui_invArmor").GetComponent<Text>();
        ui_invAttackPowerText = GameObject.Find("ui_invAttP").GetComponent<Text>();
        ui_invAbilityPowerText = GameObject.Find("ui_invAbiP").GetComponent<Text>();
        ui_invAttackSpeedText = GameObject.Find("ui_invAttS").GetComponent<Text>();
        ui_invMovementSpeedText = GameObject.Find("ui_invMS").GetComponent<Text>();
        ui_HpOrbTxt = GameObject.Find("HpOrbTxt").GetComponent<Text>();

    }

    //Fixed Update wird gecalled, vor der physikalischen Berechning innerhalb eines Frames.
    void FixedUpdate()
    {


        Move();

        //Falls kein Input kommt, zählt die Idle_Time pro Frame 1 hoch
        if (!Input.anyKey)
            idle_time = idle_time + 1;
        else if (idle)
        {
            idle_time = 0;
            transform.rotation = Quaternion.Euler(45,0,0);
            Camera.main.fieldOfView = 15;
            idle = false;
        }


        //Falls die Idle-Time einen definierten 
        if (idle_time >= 2000)
            IdleRotation();

    }


    //Idle Rotation Funktion
    private void IdleRotation()
    {
        idle = true;

        if (transform.eulerAngles.x > xrotClamp)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x - 1 / idleRotSpeed, transform.eulerAngles.y, transform.eulerAngles.z); 

        if (transform.rotation.y > -yrotClamp)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 1 / idleRotSpeed, transform.eulerAngles.z);

        else if (transform.rotation.y > yrotClamp)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y - 1 / idleRotSpeed, transform.eulerAngles.z);

        if (Camera.main.fieldOfView < idleFOVClamp)
            Camera.main.fieldOfView += idleRotSpeed * Time.deltaTime / 3;

        

    }

    //Wird jeden Frame berechnet.
    private void Update()
    {

        //Input Methods


        if (Input.GetKey(KeyCode.LeftShift))  //Sollte über Menü einstellbar sein #UI_Manager
            ShowStatText();
        else
        {
            uiHpOrb.SetActive(false);
            ui_Xp.text = "";
        }

        #region Komplizierte Abfrage für den Attack und die Combat-Stance
        // Falls wir uns nicht bereits im Attack befinden, führe Angriff aus.
        if (Input.GetKey(KeyCode.Mouse0) && weaponGameObject.gameObject.GetComponent<Animator>().GetBool("isAttacking") == false)
        {
            //Setze den Timer für die Combat-Stance zurück.
            combatStanceTime = 1; // Später, attackSpeed länge.

            //Führe Angriff aus.
            Attack();
                
        }

        // Solange wir uns noch im Intervall des Combats befinden, soll die Zeit der Combat-Stance reduziert werden.
        else if (combatStanceTime >= 0)
        {
            combatStanceTime -= Time.deltaTime;

            //Setze die Combat-Stance des Iso-Renderers auf True, damit dieser die Animationen währe "isAttacking" nicht mehrfach versucht abzuspielen.
            isoRenderer.inCombatStance = true;
        }

        //Sobald die Combat-Stance Zeit abgelaufen ist, setzen wir den Animator zurück, um erneut animiert werden zu können.
        if (combatStanceTime <= 0)
        {
            weaponGameObject.gameObject.GetComponent<Animator>().SetBool("isAttacking", false);

            //Setze die Combat-Stance des Iso-Renderers zurück, um erneut für Attack-Animation bereit zu sein.
            isoRenderer.inCombatStance = false;
        }
        #endregion


        //UI Orb
        HpSlider.value = playerStats.Get_currentHp() / playerStats.Hp.Value;
        XpSlider.maxValue = playerStats.LevelUp_need(); //irgendwie unschön, vll kann man Max.Value einmalig einstellen, nachdem man levelUp gekommen ist.
        XpSlider.value = playerStats.xp;
        ui_Level.text = playerStats.level.ToString();




        //Input & WalkAnimation
        float horizontalInput = Input.GetAxis("HorizontalKey");
        float verticalInput = Input.GetAxis("VerticalKey");
        inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        isoRenderer.SetDirection(inputVector);
        isoRenderer.SetWeaponDirection(inputVector, weaponGameObject.GetComponent<Animator>());

        //print(inputVector);
        //Define Inventory Tab Values   ********schreiben von Interface Texten*********
        ui_invHealthText.text = "Health: " + (int)playerStats.Hp.Value;
        ui_invArmorText.text = "Armor: " + playerStats.Armor.Value;
        ui_invAttackPowerText.text = "Attack: " + playerStats.AttackPower.Value;
        ui_invAbilityPowerText.text = "Ability: " + playerStats.AbilityPower.Value;
        ui_invAttackSpeedText.text = "Speed: " + playerStats.AttackSpeed.Value;
        ui_invMovementSpeedText.text = "Movement: " + playerStats.MovementSpeed.Value;

    }

    //Bewege den Charakter über die HorizontalKeys / VerticalKeys aus den ProjectSettings
    void Move()
    {
        //Generiere einen Vector anhand der InputDaten aus den PlayerPrefs in Unity
        Vector3 ActualSpeed = right * Input.GetAxis("HorizontalKey") + forward * Input.GetAxis("VerticalKey");

        //Der Vector sollte auf 1 geclamped werden, da durch 2 Inputs (e.g. A + W) doppelte Geschwindigkeit erreicht werden.
        ActualSpeed = Vector3.ClampMagnitude(ActualSpeed, 1);

        //Bewege den Rigidbody anhand des Vectors, multipliziert durch MovementSpeed
        rbody.AddForce(ActualSpeed * playerStats.MovementSpeed.Value, ForceMode.Force);

    }

    public bool InLineOfSight()         //Abfrage, ob Spieler in LOS zum Geschoss / Spell ist.
    {
        
        //Generiere einen Raycast Array, der all Collider abspeichert, durch die er geht - der Rayc wird von der Camera zur Mouse-Position im 3D Raum gecasted.
        RaycastHit[] hits;
        hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100.0f);

        //Scanne die getroffenen Collider, nach dem entsprechenden Punkt im Boden
        for (int i = 0; i < hits.Length; i++)
        {

            RaycastHit hit = hits[i];
            if (hit.transform.tag == "Floor")
            {
                //Definiere den Ziel-Vector im Verhältnis zur Spieler Position
                Vector3 targetDirection = (hit.point - transform.position);
            }
        }

        //Falls sich der Direction-Collider vom Spieler, zwischen dem Spieler und dem Ziel-Vector befinden, return true, else false.
        RaycastHit dirCollider;
        int layerMask = 1 << 8;
        if (Physics.Raycast(transform.position, targetDirection, out dirCollider, Mathf.Infinity, layerMask))
            return true;
        else
            return false;
        
    }

    //Funktion um das erweiterte UI einzublenden (Statistiken)
    void ShowStatText() 
    {
        //Aktiviere entsprechende UI Elemente und schreibe diese.
        uiHpOrb.SetActive(true);
        ui_HpOrbTxt.text = Mathf.RoundToInt(playerStats.Get_currentHp()) + "\n" + Mathf.RoundToInt(playerStats.Hp.Value);
        ui_Xp.text = playerStats.xp + "/" + playerStats.LevelUp_need();
    }

    //Starten der Combat Stance
    void Attack()
    {

        //Play Animation - > Animation-Speed = AttackSpeed vom Schwert.
        weaponGameObject.gameObject.GetComponent<Animator>().SetBool("isAttacking", true);

        //Play Sound -> Incoming.        
        string[] attackSounds = new string[] { "Attack1", "Attack2", "Attack3" };

        if(AudioManager.instance != null)
        AudioManager.instance.Play(attackSounds[Random.Range(0, 3)]);
        

        //Detect all enemies in rage of attack by #DirectionCollider
        foreach (EnemyController enemy in DirectionCollider.instance.collidingEnemyControllers)
        {
            //Falls der Feind gestorben ist, wollen wir ihn nicht länger angreifen. Ohne != null kommt es sonst zur Null-Reference, nachdem entsprechende Feinde getötet wurden.
            if (enemy != null)
                enemy.TakeDamage(playerStats.AttackPower.Value, playerStats.Range);

        }

    }


    // ---- RANGED ATTACK ----- Not implemented yet.
    void RangedAttack(Vector3 worldPos)        
    {
        //Derzeit benutzen wir das für Fernkampf. Schau hier: https://youtu.be/wntKVHVwXnc?t=642 für Infos bzgl. Spell Index
        /* --Wird wieder enabled, sobald die Skills hinzugefügt wurden--
        float dist = Vector3.Distance(worldPos, transform.position);
        if (dist <= playerStats.Range)
        {
            Instantiate(skillPrefab[0], transform.position, Quaternion.identity);
        }
        */

    }


    private void OnTriggerStay(Collider collider)
    {                                                           //Die Abfrage sollte noch verbessert werden.
        
        //Versuche einen Collider mit der Item-World Klasse zu finden
        ItemWorld itemWorld = collider.GetComponent<ItemWorld>();

        //Falls ein entsprechender Collider gefunden wurde und die pickKey Taste (Default Q) gedrück wurde
        if (itemWorld != null && Input.GetKey(UI_Manager.instance.pickKey))
        {

            //Falls noch Platz im Inventar ist
            if(inventory.itemList.Count <= 14)
            {
                //Füge Item zum Inventar hinzu
                inventory.AddItem(itemWorld.GetItem());

                //Zerstöre den Collider
                itemWorld.DestroySelf();

                #region "Tutorial"
                if (itemWorld.GetItem().ItemID == "WP0001" && GameObject.FindGameObjectWithTag("TutorialScript").GetComponent<Tutorial>() != null)
                {
                    Tutorial tutorialScript = GameObject.FindGameObjectWithTag("TutorialScript").GetComponent<Tutorial>();
                    tutorialScript.ShowTutorial(4);

                }
                #endregion
            }

        }

    }

    
    private void UseItem(ItemInstance item)
    {
        item.Equip(playerStats);
        equippedItems.Add(item);

    }
    public void Dequip (ItemInstance item)
    {
        item.Unequip(playerStats);
        equippedItems.Remove(item);
    }
    
    public Inventory Inventory
    {
        get { return inventory; }
    }

}