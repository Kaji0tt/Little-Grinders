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
    Vector3 forward, right;


    // Item Management
    private Inventory inventory;
    [SerializeField] private UI_Inventory uiInventory;

    // Item which spawns upon load:
    public Item test_item, test_item2, test_item3;


    //PlayerStats & UI
    GameObject uiInventoryTab, uiHealthStat;
    // *****Deklarieren von Interface Texten*********
    private Text uiHealthText, ui_invHealthText, ui_invArmorText, ui_invAttackPowerText, ui_invAbilityPowerText, ui_invAttackSpeedText, ui_invMovementSpeedText, ui_Level, ui_Xp;
    public Slider HpSlider, XpSlider;
    private float maxHP;



    //public int Range;
    //public float attackCD = 0f;
    Transform enemy;
    private GameObject Schuhe, Hose, Brust, Kopf, Weapon, Schmuck;


    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
        playerStats = GetComponent<PlayerStats>();

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
        uiHealthStat = GameObject.Find("uiHealth");
        uiHealthText = uiHealthStat.GetComponent<Text>();
        ui_Level = GameObject.Find("LevelText").GetComponent<Text>();
        ui_Xp = GameObject.Find("XpText").GetComponent<Text>();


        //PlayerStat Inventory - ***********Initialisieren von Texten**********
        ui_invHealthText = GameObject.Find("ui_invHp").GetComponent<Text>();
        ui_invArmorText = GameObject.Find("ui_invArmor").GetComponent<Text>();
        ui_invAttackPowerText = GameObject.Find("ui_invAttP").GetComponent<Text>();
        ui_invAbilityPowerText = GameObject.Find("ui_invAbiP").GetComponent<Text>();
        ui_invAttackSpeedText = GameObject.Find("ui_invAttS").GetComponent<Text>();
        ui_invMovementSpeedText = GameObject.Find("ui_invMS").GetComponent<Text>();



        //Spawning Random Items for Test purposes
        ItemWorld.SpawnItemWorld
            (new Vector3(transform.position.x + 5, transform.position.y, transform.position.z + 5),test_item);
        ItemWorld.SpawnItemWorld
            (new Vector3(transform.position.x + 3, transform.position.y, transform.position.z + 3),test_item2);
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

    }

    private void Update()
    {

        //UI Orb
        HpSlider.value = playerStats.Get_currentHp() / playerStats.Hp.Value;
        XpSlider.maxValue = playerStats.LevelUp_need(); //irgendwie unschön, vll kann man Max.Value einmalig einstellen, nachdem man levelUp gekommen ist.
        XpSlider.value = playerStats.xp;
        ui_Level.text = playerStats.level.ToString();
        ui_Xp.text = playerStats.xp + "/" + playerStats.LevelUp_need();



        //Input
        float horizontalInput = Input.GetAxis("HorizontalKey");
        float verticalInput = Input.GetAxis("VerticalKey");
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        isoRenderer.SetDirection(inputVector);

        //print(inputVector);
        //Define Inventory Tab Values   ********schreiben von Interface Texten*********
        uiHealthText.text = "Health: " + (int)playerStats.Hp.Value;
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