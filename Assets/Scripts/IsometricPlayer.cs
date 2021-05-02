using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;

public class IsometricPlayer : MonoBehaviour
{


    IsometricCharacterRenderer isoRenderer;
    public PlayerStats playerStats { get; private set; }

    Rigidbody rbody;
    //EQSlotWeapon weapon;
    Vector3 forward, right;
    float zoom;
    public bool rangedWeapon;


    // Item Management
    private Inventory inventory;
    public List<ItemInstance> equippedItems = new List<ItemInstance>(); //Beim Load wird Player neu geladen, also doch nicht. Aber was ist am schlauesten?
    /// <summary>
    /// Eigene Classe Vorteile:
    /// Ausgerüstete Items könnten Aktive Fähigkeiten haben - voll cool.
    /// Einzelne Liste:
    /// Einfach.
    /// </summary>

    //private EquippedItems equippedItems;
    
    [SerializeField] private UI_Inventory uiInventory;


    // Item which spawns upon load:
    public Item test_item, test_item2, test_item3;


    //PlayerStats & UI
    GameObject uiInventoryTab, uiHpOrb;
    // *****Deklarieren von Interface Texten*********
    private Text uiHealthText, ui_invHealthText, ui_invArmorText, ui_invAttackPowerText, ui_invAbilityPowerText, ui_invAttackSpeedText, ui_invMovementSpeedText, ui_Level, ui_Xp, ui_HpOrbTxt;
    public Slider HpSlider, XpSlider;
    private float maxHP;


    //Talente und Kampf-System
    private Vector3 targetDirection;

    //Idle Rotation
    private float time;

    public float idleRotSpeed = 10;

    private float xrotClamp = 15;

    private float yrotClamp = 50;

    private float idleFOVClamp = 40;

    private bool idle = false;




    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
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

    void FixedUpdate()
    {


        Move();

        if (!Input.anyKey)
            time = time + 1;
        else if (idle)
        {
            time = 0;
            transform.rotation = Quaternion.Euler(45,0,0);
            Camera.main.fieldOfView = 15;
            idle = false;
        }



        if (time >= 2000)
            IdleRotation();

    }


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

    private void Update()
    {

        //Input Methods


        if (Input.GetKey(KeyCode.LeftShift))  //Sollte am Ende auf KeyCode.LeftAlt geändert werden.
            ShowStatText();
        else
        {
            uiHpOrb.SetActive(false);
            ui_Xp.text = "";
        }


        if (Input.GetKey(KeyCode.Mouse0))
        {
            Attack();
        }


        //UI Orb
        HpSlider.value = playerStats.Get_currentHp() / playerStats.Hp.Value;
        XpSlider.maxValue = playerStats.LevelUp_need(); //irgendwie unschön, vll kann man Max.Value einmalig einstellen, nachdem man levelUp gekommen ist.
        XpSlider.value = playerStats.xp;
        ui_Level.text = playerStats.level.ToString();




        //Input & WalkAnimation
        float horizontalInput = Input.GetAxis("HorizontalKey");
        float verticalInput = Input.GetAxis("VerticalKey");
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        isoRenderer.SetDirection(inputVector);

        //print(inputVector);
        //Define Inventory Tab Values   ********schreiben von Interface Texten*********
        ui_invHealthText.text = "Health: " + (int)playerStats.Hp.Value;
        ui_invArmorText.text = "Armor: " + playerStats.Armor.Value;
        ui_invAttackPowerText.text = "Attack: " + playerStats.AttackPower.Value;
        ui_invAbilityPowerText.text = "Ability: " + playerStats.AbilityPower.Value;
        ui_invAttackSpeedText.text = "Speed: " + playerStats.AttackSpeed.Value;
        ui_invMovementSpeedText.text = "Movement: " + playerStats.MovementSpeed.Value;

    }
    void Move()
    {

        Vector3 ActualSpeed = right * Input.GetAxis("HorizontalKey") + forward * Input.GetAxis("VerticalKey");
        ActualSpeed = Vector3.ClampMagnitude(ActualSpeed, 1);
        rbody.AddForce(ActualSpeed * playerStats.MovementSpeed.Value, ForceMode.Force);

    }

    public bool InLineOfSight()         //Abfrage, ob Spieler in LOS zum Geschoss / Spell ist.
    {
        
        RaycastHit[] hits;
        hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 100.0f);

        for (int i = 0; i < hits.Length; i++)
        {

            RaycastHit hit = hits[i];
            if (hit.transform.tag == "Floor")
            {

                targetDirection = (hit.point - transform.position);
            }
        }


        RaycastHit dirCollider;
        int layerMask = 1 << 8;
        if (Physics.Raycast(transform.position, targetDirection, out dirCollider, Mathf.Infinity, layerMask))
            return true;
        else
            return false;
        
    }

    void ShowStatText() 
    {
        uiHpOrb.SetActive(true);
        ui_HpOrbTxt.text = Mathf.RoundToInt(playerStats.Get_currentHp()) + "\n" + Mathf.RoundToInt(playerStats.Hp.Value);
        ui_Xp.text = playerStats.xp + "/" + playerStats.LevelUp_need();
    }

    void Attack()
    {


        //Play Animation


        //Detect all enemies in rage of attack



        //Apply Damage












        /*
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (rangedWeapon == true && InLineOfSight())           //Kladaradatsch. Die Abfrage nach Spells sollte eine eigene Methode sein.
                RangedAttack(hit.point);

        }
        */

    }

    // ---- RANGED ATTACK ----- Not implemented yet.


    void RangedAttack(Vector3 worldPos)        //Derzeit benutzen wir das für Fernkampf. Schau hier: https://youtu.be/wntKVHVwXnc?t=642 für Infos bzgl. Spell Index
    {
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
        
        ItemWorld itemWorld = collider.GetComponent<ItemWorld>();
        if (itemWorld != null && Input.GetKey(UI_Manager.instance.pickKey))
        {
            if(inventory.itemList.Count <= 14)
            {
                inventory.AddItem(itemWorld.GetItem());
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