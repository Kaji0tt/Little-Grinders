using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLoad : MonoBehaviour
{

    //Ich weiß zwar, dass das ganz schön Spaghetti ist und ich einfach eine EQSlot Klasse schreiben sollte - ich tu's jetzt aber trotzdem nicht.
    public EQSlotBrust brust;
    public EQSlotHose hose;
    public EQSlotKopf kopf;
    public EQSlotSchmuck schmuck;
    public EQSlotSchuhe schuhe;
    public EQSlotWeapon weapon;

    private void Awake()    
    {
        LoadPlayer();
    }

    public void LoadPlayer()
    {
        PlayerSave data = SaveSystem.LoadPlayer();


        print(data.weapon + " im Ladevorgang.");

        //Die Gegenstände werden initialisiert und neu angezogen.
        if(data.brust != null)
        {
            brust.LoadItem(ItemDatabase.GetItemID(data.brust));
            print("brust checked");
        }


        if (data.hose != null)
        {
            hose.LoadItem(ItemDatabase.GetItemID(data.hose));
            print("hose checked");
        }


        if (data.kopf != null)
            kopf.LoadItem(ItemDatabase.GetItemID(data.kopf));

        if (data.schuhe != null)
            schuhe.LoadItem(ItemDatabase.GetItemID(data.schuhe));

        if (data.schmuck != null)
            schmuck.LoadItem(ItemDatabase.GetItemID(data.schmuck));

        if (data.weapon != null)
            weapon.LoadItem(ItemDatabase.GetItemID(data.weapon));

        print("Items angezogen: " + ItemSave.equippedItems.Count);

    }
}
