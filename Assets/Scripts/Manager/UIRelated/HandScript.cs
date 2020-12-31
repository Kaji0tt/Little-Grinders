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

    public bool onActionBar;


    private void Start()
    {
        image = GetComponent<Image>();
    }

    public void TakeMoveable(IMoveable moveable)
    {
        this.MyMoveable = moveable;
        image.sprite = moveable.icon;
        image.color = Color.white;
    }

    private void Update()
    {
        image.transform.position = Input.mousePosition;
    }

    //Schauen ob diese Funktion wirklich benutzt werden muss. Falls ich im Talent OnDragEnd benutz, muss dies vielleicht dort geschehen.

    public IMoveable Put()
    {
        IMoveable tmp = MyMoveable;

        MyMoveable = null;

        image.color = new Color(0, 0, 0, 0);

        return tmp;
    }



}
