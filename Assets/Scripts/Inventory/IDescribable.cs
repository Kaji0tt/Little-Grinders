using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IDescribable : IPointerEnterHandler, IPointerExitHandler
{
    string GetDescription();
}
