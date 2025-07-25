using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class HandScript : MonoBehaviour, IEndDragHandler
{
    #region Singleton
    public static HandScript instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    public IMoveable MyMoveable { get; set; }

    public IUseable MyUseable { get; set; }

    private Image image;

    public int slotIndex;

    [SerializeField]
    private Vector3 offset;

    public bool onActionBar; // Optional: Prüfe, ob das Item auf die Actionbar gezogen wird.

    private void Start()
    {
        image = GetComponent<Image>();
        image.color = new Color(0, 0, 0, 0); // Start mit unsichtbarem Bild
    }

    public void TakeMoveable(IMoveable moveable)
    {
        this.MyMoveable = moveable;
        image.sprite = moveable.icon;
        image.color = Color.white; // Mache das Item sichtbar, wenn es gezogen wird.
    }

    public void TakeUseable(IUseable useable)
    {
        MyUseable = useable;
    }

    private void Update()
    {
        if (MyMoveable != null)
        {
            image.transform.position = Input.mousePosition;

            if (Input.GetKeyUp(KeyCode.Mouse0)) // Linksklick loslassen -> Ablegen
            {
                // Wenn kein Drop-Target gefunden wurde, Item zurück zum ursprünglichen Slot
                ReturnItemToOriginalSlot();
            }
        }
    }

    private void ReturnItemToOriginalSlot()
    {
        if (MyMoveable != null)
        {
            ItemInstance item = MyMoveable as ItemInstance;
            if (item != null)
            {
                // Item zurück zum ursprünglichen Slot
                if (slotIndex >= 0)
                {
                    UI_Inventory.instance.inventory.AddItemAtIndex(item, slotIndex);
                }
                else
                {
                    UI_Inventory.instance.inventory.AddItemToFirstFreeSlot(item);
                }
            }
            Put(); // Item aus Hand entfernen
        }
    }

    public IMoveable Put()
    {
        IMoveable tmp = MyMoveable;
        MyMoveable = null;
        image.color = new Color(0, 0, 0, 0); // Mache das Bild unsichtbar
        return tmp;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Handscript OnEndDrag triggered.");
    }
}

