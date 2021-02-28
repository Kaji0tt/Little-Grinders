using UnityEngine;
using UnityEngine.UI;

public interface IMoveable //Kacknoob Solution würde wohl sein, das IMoveable zu IMoveableSpell umzubenennen und für jedes Bewegbare Element ein entsprechendes Interfacescript zu erstellen.
{
    Sprite icon
    {
        get;
    }


    //Sollte hier nicht mit drin sein. Moveable sollten nichts außer ein Icon beinhalten.
    /*
    Spell spell
    {
        get;
    }
    */
}
