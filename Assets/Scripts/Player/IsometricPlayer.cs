using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


///Bezogen auf das Debuff / Bufff System, bzw. die Abilities:
///Au    dem Spieler liegt eine Liste von Abilities - wenn der Spieler ein AbilityTalent benutzt, wird nach der Ability in der entsprechenden Liste gesucht, welche sich mit der BaseAbility vom AbilityTalent gleicht.
///Eine InterfaceKlasse DeBuffSystem könnte auf die OnTicks(), OnUse(), und OnCooldowns() von Abilities zugreifen und die... - Ne das stupid.
public class IsometricPlayer : MonoBehaviour //,DeBuffSystem
{
    //private List<Ability> abilities = new List<Ability>();
    //private HashSet<Ability> abilities;
    private Dictionary<string, Ability> abilities;

    #region Implementation - Basic 

    //Klasse, welche Isometrische Darstellungen kalkuliert.
    public IsometricRenderer isoRenderer;

    //Wird im Inspektor eingestellt, um das Game-Objekt der Waffe und dessen Animator zu erkennen.
    //public GameObject weaponGameObject; 

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
    public Inventory inventory { get; private set; }

    public List<ItemInstance> equippedItems = new List<ItemInstance>();


    #endregion

    #region Implementation - Interface Objekte und Texte

    GameObject uiInventoryTab;
    private TextMeshProUGUI uiHealthText, ui_invHealthText, ui_invArmorText, ui_invAttackPowerText, ui_invAbilityPowerText, ui_invAttackSpeedText, ui_invMovementSpeedText, ui_invRegText, ui_invCritChanceText;
    private TextMeshProUGUI xp_Text;
    private Text ui_Level;
    private GameObject hp_Text;
    private Image HpImage, XpImage;
    private float maxHP;

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

    public float userFOV;

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
        if (Camera.main != null)
        {
            forward = Camera.main.transform.forward;
            forward.y = 0;
            forward = Vector3.Normalize(forward);
            right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
        }
        else
        {
            Debug.LogError("Camera.main not found! Movement vectors not initialized.");
            forward = Vector3.forward;
            right = Vector3.right;
        }
        //weaponGameObject = GameObject.Find("WeaponAnim");


        //PlayerStats & UI
        //GameEvents.current.LevelUpdate += UpdateLevel;
        uiInventoryTab = GameObject.Find("Inventory Tab");
        ui_Level = GameObject.Find("LevelText").GetComponent<Text>();
        GameObject uiXp = GameObject.Find("XpText");
        //////////////////////Das ist ja fucking eklig, finde eine Alternative um die Texte zu initialisieren!!!!!
        playerStats.Set_currentHp(playerStats.Get_maxHp());

        //PlayerStat Inventory - ***********Initialisieren von Texten**********
        ui_invHealthText = GameObject.Find("ui_invHp").GetComponent<TextMeshProUGUI>();
        ui_invArmorText = GameObject.Find("ui_invArmor").GetComponent<TextMeshProUGUI>();
        ui_invAttackPowerText = GameObject.Find("ui_invAttP").GetComponent<TextMeshProUGUI>();
        ui_invAbilityPowerText = GameObject.Find("ui_invAbiP").GetComponent<TextMeshProUGUI>();
        ui_invAttackSpeedText = GameObject.Find("ui_invAttS").GetComponent<TextMeshProUGUI>();
        ui_invMovementSpeedText = GameObject.Find("ui_invMS").GetComponent<TextMeshProUGUI>();
        ui_invRegText = GameObject.Find("ui_invReg").GetComponent<TextMeshProUGUI>();
        ui_invCritChanceText = GameObject.Find("ui_invCrC").GetComponent<TextMeshProUGUI>();
        //ui_HpOrbTxt = GameObject.Find("HpOrbTxt").GetComponent<Text>();


        /*
        ItemWorld.SpawnItemWorld(transform.position, new ItemInstance(test_item));
        ItemWorld.SpawnItemWorld(new Vector3(transform.position.x + 1, transform.position.y, transform.position.z), new ItemInstance(test_item));
        ItemWorld.SpawnItemWorld(new Vector3(transform.position.x, transform.position.y, transform.position.z + .5f), new ItemInstance(test_item));
        */
    }

    private void Start()
    {
        xp_Text = PlayerManager.instance.xp_Text;
        hp_Text = PlayerManager.instance.hp_Text;
        HpImage = PlayerManager.instance.hpFill;
        XpImage = PlayerManager.instance.xpFill;
    }

    //Fixed Update wird gecalled, vor der physikalischen Berechning innerhalb eines Frames.
    void FixedUpdate()
    {

        MapView();

        Move();

        //Falls kein Input kommt, zählt die Idle_Time pro Frame 1 hoch
        if (!Input.anyKey)
        {
            //userFOV = Camera.main.fieldOfView;
            idle_time = idle_time + 1;
        }

        //Falls wir uns im Idle-State befindet, nun jedoch eine Taste gedrückt wird (also !Input.anyKey (nicht keine Taste gedrückt wird) springen wir zum spieler zurück.
        else if (idle && !Input.GetKey(UI_Manager.instance.toggleCamKey))
        {        
            idle_time = 0;
            transform.rotation = Quaternion.Euler(45,0,0);
            //05.04.22 Derzeit wird nicht die UserFOV verwendet, da die Kamera im Idlestate ganz nah den spieler zu snappen scheint.
            Camera.main.fieldOfView = 15;
            idle = false;           
        }
       



        //Falls die Idle-Time einen definierten 
        if (idle_time >= 2000)
            StartIdleRotation();

    }


    //Idle Rotation Funktion
    private void StartIdleRotation()
    {
        idle = true;

        if (transform.eulerAngles.x > xrotClamp)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x - 1 / idleRotSpeed, transform.eulerAngles.y, transform.eulerAngles.z); 

        if (transform.rotation.y > -yrotClamp)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 1 / idleRotSpeed, transform.eulerAngles.z);

        else if (transform.rotation.y > yrotClamp)
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y - 1 / idleRotSpeed, transform.eulerAngles.z);

        if (CameraManager.instance.mainCam.fieldOfView < idleFOVClamp)
            CameraManager.instance.mainCam.fieldOfView += idleRotSpeed * Time.deltaTime / 3;

        

    }

    private void MapView()
    {
        if (Input.GetKey(UI_Manager.instance.toggleCamKey))
        {
            if (CameraManager.instance.mainCamGO.activeSelf)
            {
                CameraManager.instance.mainCamGO.SetActive(false);
                CameraManager.instance.mapCamGO.SetActive(true);
            }
        }
        else
        {   //Brutal ist das hässlich bruder, ganz dringend beim CameraManager 
            CameraManager.instance.mainCamGO.SetActive(true);
            if(userFOV <= 20 && userFOV >= 10)
                CameraManager.instance.mainCam.fieldOfView = userFOV;
            CameraManager.instance.mapCamGO.SetActive(false);
        }
    }

    //Wird jeden Frame berechnet.
    private void Update()
    {
        //GodMode
        if (Input.GetKeyDown(KeyCode.G))
        {
            TroubleShootGodMode();

        }

        // Quick Fix: Spieler aus dem Boden teleportieren
        if (transform.position.y < -5f)
        {
            transform.position = new Vector3(transform.position.x, 3f, transform.position.z);
            Debug.LogWarning("Player fell through ground - teleported back up!");
        }


        //Input Methods
        if (Input.GetKey(KeyCode.LeftShift))  //Sollte über Menü einstellbar sein #UI_Manager
            ShowStatText();
        else
        {
            hp_Text.SetActive(false);
            xp_Text.text = "";
        }




        // UI Orb
        HpImage.fillAmount = playerStats.Get_currentHp() / playerStats.Hp.Value;
        XpImage.fillAmount = XpImage.fillAmount = (playerStats.LevelUp_need() > 0f) ? (float)playerStats.xp / playerStats.LevelUp_need() : 0f;
        ui_Level.text = playerStats.level.ToString();
        PPVolumeManager.instance.LowHealthPP(HpImage.fillAmount);
        //Postprocessing for LowHealth


        float horizontalInput = Input.GetAxis("HorizontalKey");
        float verticalInput = Input.GetAxis("VerticalKey");

        // Prüfe, ob keine Tastatureingabe vorliegt
        if (Mathf.Approximately(horizontalInput, 0) && Mathf.Approximately(verticalInput, 0))
        {
            // Berechne die Blickrichtung basierend auf der Mausposition
            Vector3 directionToMouse = DirectionCollider.instance.dirVector; // Verwende den Richtungsvektor, den du im vorherigen Code für den DirectionCollider berechnet hast

            // Konvertiere die Richtung in eine 2D-Richtung, um sie mit dem IsoRenderer zu verwenden
            Vector2 mouseDirection2D = new Vector2(directionToMouse.x, directionToMouse.z).normalized;

            // Minimiere den Input von mouseDirection2D, damit die IsoRenderer Idle-Array auswählt.
            mouseDirection2D = Vector2.ClampMagnitude(mouseDirection2D, 0.1f);
            isoRenderer.SetPlayerDirection(mouseDirection2D); // Setze die Blickrichtung des IsoRenderers basierend auf der Mausposition
        }
        else
        {
            // Wenn Tastatureingabe vorliegt, verwende diese für die Blickrichtung
            Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
            inputVector = Vector2.ClampMagnitude(inputVector, 1);
            isoRenderer.SetPlayerDirection(inputVector); // Setze die Blickrichtung des IsoRenderers basierend auf der Tastatureingabe

            //Animate Weapon
            isoRenderer.AnimateIdleWeapon(inputVector);
        }
        isoRenderer.AnimateIdleWeapon(DirectionCollider.instance.dirVector);
        //isoRenderer.PlayWeaponAttack()

        //print(inputVector);
        //Define Inventory Tab Values   ********schreiben von Interface Texten*********
        ui_invHealthText.text = "Health: " + (int)playerStats.Hp.Value;
        ui_invArmorText.text = "Armor: " + playerStats.Armor.Value;
        ui_invAttackPowerText.text = "Attack: " + playerStats.AttackPower.Value;
        ui_invAbilityPowerText.text = "Ability: " + playerStats.AbilityPower.Value;
        ui_invAttackSpeedText.text = "Speed: " + playerStats.AttackSpeed.Value;
        ui_invMovementSpeedText.text = "Movement: " + playerStats.MovementSpeed.Value;
        ui_invRegText.text = "Regeneration: " + playerStats.Regeneration.Value;
        ui_invCritChanceText.text = "Crit. Chance: " + playerStats.CriticalChance.Value;

    }

    void TroubleShootGodMode()
    {

        foreach (Talent_UI talent in TalentTreeManager.instance.allTalents)
        {

            Debug.Log(talent.name + talent.currentCount);

        }
    }
    //Bewege den Charakter über die HorizontalKeys / VerticalKeys aus den ProjectSettings
    void Move()
    {
        Vector3 ActualSpeed = right * Input.GetAxis("HorizontalKey") + forward * Input.GetAxis("VerticalKey");
        ActualSpeed = Vector3.ClampMagnitude(ActualSpeed, 1);

        Vector3 targetPosition = transform.position + Time.deltaTime * ActualSpeed * playerStats.MovementSpeed.Value;

        // Debug: Prüfe alle relevanten Werte auf NaN
        if (
            float.IsNaN(targetPosition.x) || float.IsNaN(targetPosition.y) || float.IsNaN(targetPosition.z) ||
            float.IsNaN(ActualSpeed.x) || float.IsNaN(ActualSpeed.y) || float.IsNaN(ActualSpeed.z) ||
            float.IsNaN(playerStats.MovementSpeed.Value) ||
            float.IsNaN(transform.position.x) || float.IsNaN(transform.position.y) || float.IsNaN(transform.position.z)
        )
        {
            Debug.LogError(
                $"[MovePosition NaN] transform.position: {transform.position}, " +
                $"ActualSpeed: {ActualSpeed}, MovementSpeed: {playerStats.MovementSpeed.Value}, " +
                $"targetPosition: {targetPosition}"
            );
            return; // Verhindere MovePosition bei NaN
        }

        rbody.MovePosition(targetPosition);
    }

    //Funktion um das erweiterte UI einzublenden (Statistiken)
    void ShowStatText()
    {
        //Aktiviere entsprechende UI Elemente und schreibe diese.
        hp_Text.SetActive(true);
        hp_Text.GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(playerStats.Get_currentHp()) + "\n" + Mathf.RoundToInt(playerStats.Hp.Value);
        xp_Text.text = playerStats.xp + "/" + playerStats.LevelUp_need();
    }
    
    public Inventory Inventory
    {
        get { return inventory; }
    }

}