using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

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

    // Item which spawns upon load:
    public Item test_item, test_item2, test_item3;


    //PlayerStat UI
    GameObject uiInventoryTab, uiHealthStat;
    private Text uiHealthText, ui_invHealthText, ui_invArmorText, ui_invAttackPowerText, ui_invAbilityPowerText, ui_invAttackSpeedText, ui_invMovementSpeedText;
    public CharStats Hp, Armor, AttackPower, AbilityPower, MovementSpeed, AttackSpeed;



    private GameObject Schuhe, Hose, Brust, Kopf, Weapon, Schmuck;


    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();

        //Isometric Camera
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;


        //Item Management
        inventory = new Inventory(UseItem);
        uiInventory.SetInventory(inventory);
        uiInventory.SetCharakter(this);

        //PlayerStat UI
        uiInventoryTab = GameObject.Find("Inventory Tab");
        uiHealthStat = GameObject.Find("uiHealth");
        uiHealthText = uiHealthStat.GetComponent<Text>();

        //PlayerStat Inventory
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


        //Define Inventory Tab Values
        uiHealthText.text = "Health: " + Hp.Value;
        ui_invHealthText.text = "Health: " + Hp.Value;
        ui_invArmorText.text = "Armor: " + Armor.Value;
        ui_invAttackPowerText.text = "Attack: " + AttackPower.Value;
        ui_invAbilityPowerText.text = "Ability: " + AbilityPower.Value;
        ui_invAttackSpeedText.text = "Speed: " + AttackSpeed.Value;
        ui_invMovementSpeedText.text = "Movement: " + MovementSpeed.Value;

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


    private void UseItem(Item item)
    {
        item.Equip(this);

    }


    public void Dequip (Item item)
    {
        item.Unequip(this, item);
    }
    public Inventory Inventory
    {
        get { return inventory; }
    }



}