using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IsometricPlayerMovementController : MonoBehaviour
{

    public float movementSpeed = 30f;
    public float SlowFactor;
    IsometricCharacterRenderer isoRenderer;
    Rigidbody rbody;
    Vector3 forward, right;

    // Item Management
    private Inventory inventory;
    [SerializeField] private UI_Inventory uiInventory;
    private Equipment equipment;

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
        equipment = new Equipment();
        uiInventory.SetEquipment(equipment);

        ItemWorld.SpawnItemWorld
            (
            new Vector3 (transform.position.x - 5, transform.position.y, transform.position.z + 3), 
            new Item { itemName = Item.ItemName.Einfacher_Hut, amount = 1 }
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

        //Vector2 currentPos = rbody.position;
        float horizontalInput = Input.GetAxis("HorizontalKey");
        float verticalInput = Input.GetAxis("VerticalKey");
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        //print(inputVector + "Peter stink");
        isoRenderer.SetDirection(inputVector);
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
        switch (item.itemName)
        {
            
            case Item.ItemName.Einfache_Sandalen:
                inventory.RemoveItem(item);
                //equipment.equip(item.itemType);
                //UseItem als Befehl an den Charakter ist viel sinniger um PlayerStats zu beeinflussen.. glaub. ansonsten 
                // kann equipment.equip auch über das UI inventory laufen.
                break;
            case Item.ItemName.Einfache_Hose:
                inventory.RemoveItem(item);
                break;
            case Item.ItemName.Einfache_Brust:
                inventory.RemoveItem(item);
                break;
            case Item.ItemName.Einfacher_Hut:
                inventory.RemoveItem(item);
                break;
            case Item.ItemName.Einfaches_Schwert:
                inventory.RemoveItem(item);
                break;
            case Item.ItemName.Anhänger:
                inventory.RemoveItem(item);
                break;
      
                // Das ist ein fauler Workaround.
                // Richtig wäre: Item an Equiment weiter geben.
        }



    }



}