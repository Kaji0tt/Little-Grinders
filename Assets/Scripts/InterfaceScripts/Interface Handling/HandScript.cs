using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HandScript : MonoBehaviour
{
    #region Singleton
    public static HandScript instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    public IMoveable MyMoveable { get; set; }

    private Image image;

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

    private void Update()
    {
        if (MyMoveable != null)
        {
            image.transform.position = Input.mousePosition;

            if (Input.GetKeyUp(KeyCode.Mouse0)) // Linksklick loslassen -> Ablegen
            {
                Put(); // Hier könnte spezifische Logik für die Actionbar hinzugefügt werden
            }
        }
    }

    public IMoveable Put()
    {
        IMoveable tmp = MyMoveable;
        MyMoveable = null;
        image.color = new Color(0, 0, 0, 0); // Mache das Bild unsichtbar
        return tmp;
    }
}
