using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IsometricPlayer : MonoBehaviour
{

    public float movementSpeed = 30f;
    public float SlowFactor;
    IsometricCharacterRenderer isoRenderer;
    Rigidbody rbody;
    Vector3 forward, right;


    // Item Management
    private Inventory inventory;
    [SerializeField] private UI_Inventory uiInventory;


    //PlayerStat UI
    GameObject uiInventoryTab, uiHealthStat;
    private Text uiHealthText, ui_invHealthText, ui_invArmorText;
    //private CharStats charStats;
    //private CharStats charStats;
    public CharStats Hp, Armor, AttackPower, AbilityPower, MovementSpeed, AttackSpeed;



    private GameObject Schuhe, Hose, Brust, Kopf, Weapon, Schmuck;


    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();

        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;


        //Item Management
        inventory = new Inventory(UseItem);
        uiInventory.SetInventory(inventory);
        uiInventory.SetCharakter(this);
        //equipment = new Equipment();-- Tote Equipment.cs gefunden.
        //uiInventory.SetEquipment(equipment);-- Tote Equipment.cs gefunden.

        //PlayerStat UI
        uiInventoryTab = GameObject.Find("Inventory Tab");
        uiHealthStat = GameObject.Find("uiHealth");
        uiHealthText = uiHealthStat.GetComponent<Text>();
        //charStats = GetComponent<CharStats>();

        //PlayerStat Inventory
        ui_invHealthText = GameObject.Find("ui_invHp").GetComponent<Text>();
        ui_invArmorText = GameObject.Find("ui_invArmor").GetComponent<Text>();



        ItemWorld.SpawnItemWorld
            (
            new Vector3(transform.position.x + 5, transform.position.y, transform.position.z + 5),
            new Item { itemName = Item.ItemName.Einfacher_Hut, itemType = "Kopf" }
            );


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


        float horizontalInput = Input.GetAxis("HorizontalKey");
        float verticalInput = Input.GetAxis("VerticalKey");
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        isoRenderer.SetDirection(inputVector);

        uiHealthText.text = "Health:" + Hp.Value;
        ui_invHealthText.text = "Health:" + Hp.Value;
        ui_invArmorText.text = "Armor:" + Armor.Value;

    }
    void Move()
    {

        Vector3 ActualSpeed = right * Input.GetAxis("HorizontalKey") + forward * Input.GetAxis("VerticalKey");
        ActualSpeed = Vector3.ClampMagnitude(ActualSpeed, 1);
        rbody.AddForce(ActualSpeed * movementSpeed, ForceMode.Force);


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

    //public StatModifier
    //Hp,
    //Armor,
    //AttackPower,
    //AbilityPower,
   //movementSpeed,
    //AttackSpeed;

    private void UseItem(Item item)
    {

        //Er ist gut!
        //Hp.AddModifier(new StatModifier(item.ItemStats(item), StatModType.Flat, this));
        Hp.AddModifier(item.ItemStats(item)[0]);
        Armor.AddModifier(item.ItemStats(item)[1]);
        //AttackPower.AddModifier(item.ItemStats(item)[2]);
        //AbilityPower.AddModifier(item.ItemStats(item)[3]);
        //MovementSpeed.AddModifier(item.ItemStats(item)[4]);
        //AttackSpeed.AddModifier(item.ItemStats(item)[5]);


        //print(item.ItemStats(item)[1]);
    }

    //public EQSlotSchuhe eQSlotSchuhe; 
    public void Dequip (Item item)
    {
        //Hp.RemoveAllModifiersFromSource(item.ItemStats(item)[0]);
    }
    public Inventory Inventory
    {
        get { return inventory; }
    }



}