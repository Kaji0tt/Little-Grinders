using UnityEngine;
using UnityEngine.UI;

public interface IMoveable //Kacknoob Solution würde wohl sein, das IMoveable zu IMoveableSpell umzubenennen und für jedes Bewegbare Element ein entsprechendes Interfacescript zu erstellen.
{
    Sprite icon
    {
        get;
    }

    Spell spell
    {
        get;
    }
}
