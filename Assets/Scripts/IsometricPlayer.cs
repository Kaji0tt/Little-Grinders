using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;

public class IsometricPlayer : MonoBehaviour
{

    //public float movementSpeed = 30f;
    public float SlowFactor;
    IsometricCharacterRenderer isoRenderer;
    PlayerStats playerStats;
    Rigidbody rbody;
    //EQSlotWeapon weapon;
    Vector3 forward, right;
    float zoom;
    public bool rangedWeapon;


    // Item Management
    private Inventory inventory;
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
    [SerializeField]
    private GameObject[] skillPrefab;
    private Vector3 targetDirection;

    Transform enemy;
    private GameObject Schuhe, Hose, Brust, Kopf, Weapon, Schmuck;


    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
        playerStats = GetComponent<PlayerStats>();
        //weapon = GetComponent<EQSlotWeapon>();
        rangedWeapon = false;

        //Isometric Camera
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        
        //Item Management
        inventory = new Inventory(UseItem);
        uiInventory.SetInventory(inventory);
        uiInventory.SetCharakter(this);
        

        //PlayerStats & UI
        uiInventoryTab = GameObject.Find("Inventory Tab");
        ui_Level = GameObject.Find("LevelText").GetComponent<Text>();
        ui_Xp = GameObject.Find("XpText").GetComponent<Text>();
        uiHpOrb = GameObject.Find("HpOrbTxt");
        GameObject uiXp = GameObject.Find("XpText");



        //PlayerStat Inventory - ***********Initialisieren von Texten**********
        ui_invHealthText = GameObject.Find("ui_invHp").GetComponent<Text>();
        ui_invArmorText = GameObject.Find("ui_invArmor").GetComponent<Text>();
        ui_invAttackPowerText = GameObject.Find("ui_invAttP").GetComponent<Text>();
        ui_invAbilityPowerText = GameObject.Find("ui_invAbiP").GetComponent<Text>();
        ui_invAttackSpeedText = GameObject.Find("ui_invAttS").GetComponent<Text>();
        ui_invMovementSpeedText = GameObject.Find("ui_invMS").GetComponent<Text>();
        ui_HpOrbTxt = GameObject.Find("HpOrbTxt").GetComponent<Text>();




        //Spawning Random Items for Test purposes
        ItemWorld.SpawnItemWorld
            (new Vector3(transform.position.x + 5, transform.position.y, transform.position.z + 5),test_item);
        ItemWorld.SpawnItemWorld
            (new Vector3(transform.position.x + 1, transform.position.y, transform.position.z + 1),test_item2);
        ItemWorld.SpawnItemWorld
            (new Vector3(transform.position.x + 6, transform.position.y, transform.position.z + 1),test_item3);

        //itemDatabase.GetDrop(transform.position);

    }

    public Vector3 GetPosition()
    {
        return new Vector3(transform.position.x, transform.position.y-0.3f, transform.position.z);
    }


    void FixedUpdate()
    {

        if (Input.anyKey)
        {
            Move();

        }
        Zoom();

        if (Input.GetKey(KeyCode.LeftShift))  //Sollte am Ende auf KeyCode.LeftAlt geändert werden.
            ShowStatText();
        else
        {
            uiHpOrb.SetActive(false);
            ui_Xp.text = "";
        }

        //Hier weiter machen - es muss abgefragt werden, ob es eine Fernkampfwaffe ist.
        if (Input.GetKey(KeyCode.Mouse0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (rangedWeapon == true && InLineOfSight())           //Kladaradatsch. Die Abfrage nach Spells sollte eine eigene Methode sein.
                    CastSpell(hit.point);
            }
        }
    }
    private void Update()
    {

        print(InLineOfSight());

        //UI Orb
        HpSlider.value = playerStats.Get_currentHp() / playerStats.Hp.Value;
        XpSlider.maxValue = playerStats.LevelUp_need(); //irgendwie unschön, vll kann man Max.Value einmalig einstellen, nachdem man levelUp gekommen ist.
        XpSlider.value = playerStats.xp;
        ui_Level.text = playerStats.level.ToString();




        //Input
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
            //print(hits[i].transform.name);
            if (hit.transform.tag == "Floor")
            {

                targetDirection = (hit.point - transform.position);
            }
        }

        //Debug.DrawRay(transform.position, targetDirection, Color.red);
        //DirectionCollider = Physics.Raycast(targetDirection, transform.position);

        RaycastHit dirCollider;
        int layerMask = 1 << 8;
        if (Physics.Raycast(transform.position, targetDirection, out dirCollider, Mathf.Infinity, layerMask))
            return true;
        else
            return false;

    }

    void Zoom()
    {
        //Versuch des Zoomens
        //float _zoom = Camera.main.fieldOfView;

        float max, min;
        max = 30.0f;
        min = 15.0f;

        if(Input.mouseScrollDelta.y < 0 &&  zoom > min)
        {
            Camera.main.fieldOfView = Camera.main.fieldOfView + Input.mouseScrollDelta.y;
        }

        if (Input.mouseScrollDelta.y > 0 && zoom < max)
        {
            Camera.main.fieldOfView = Camera.main.fieldOfView + Input.mouseScrollDelta.y;
        }
        zoom = Camera.main.fieldOfView;

    }
    void ShowStatText() 
    {
        uiHpOrb.SetActive(true);
        ui_HpOrbTxt.text = Mathf.RoundToInt(playerStats.Get_currentHp()) + "\n" + Mathf.RoundToInt(playerStats.Hp.Value);
        ui_Xp.text = playerStats.xp + "/" + playerStats.LevelUp_need();
    }
    void CastSpell(Vector3 worldPos)
    {

            float dist = Vector3.Distance(worldPos, transform.position);
            if (dist <= playerStats.Range)
            {
                Instantiate(skillPrefab[0], transform.position, Quaternion.identity);
            }



    }

    private void OnTriggerStay(Collider collider)
    {
        
        ItemWorld itemWorld = collider.GetComponent<ItemWorld>();
        if (itemWorld != null && Input.GetKey(KeyCode.Q))
        {
            inventory.AddItem(itemWorld.GetItem());
            itemWorld.DestroySelf();
        }
        

        

    }

    
    private void UseItem(Item item)
    {
        item.Equip(playerStats);

    }
    public void Dequip (Item item)
    {
        item.Unequip(playerStats, item);
    }
    
    public Inventory Inventory
    {
        get { return inventory; }
    }
    
    /*
    public void TakeDamage(float damage)
    {
        damage = 10 * (damage * damage) / (Armor.Value + (10 * damage));            // DMG & Armor als werte
        damage = Mathf.Clamp(damage, 1, int.MaxValue);
        Hp.AddModifier(new StatModifier(-damage, StatModType.Flat));
        if (Hp.Value <= 0)
            Die();

    }
    */



}