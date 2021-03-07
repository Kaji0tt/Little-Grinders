using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    public CanvasGroup keybindOptions, soundOptions, gfxOptions, optionsInterface;


    public void ToggleOptionsInterface()
    {
        optionsInterface.alpha = optionsInterface.alpha > 0 ? 0 : 1;
        optionsInterface.blocksRaycasts = optionsInterface.blocksRaycasts == true ? false : true;
    }

    public void ToggleKeyBindOptions()
    {
        keybindOptions.alpha = keybindOptions.alpha > 0 ? 0 : 1;
        keybindOptions.blocksRaycasts = keybindOptions.blocksRaycasts == true ? false : true;
    }

    public void ToggleSoundOptions()
    {
        soundOptions.alpha = soundOptions.alpha > 0 ? 0 : 1;
        soundOptions.blocksRaycasts = soundOptions.blocksRaycasts == true ? false : true;
    }

    public void ToggleGfxOptions()
    {
        gfxOptions.alpha = gfxOptions.alpha > 0 ? 0 : 1;
        gfxOptions.blocksRaycasts = gfxOptions.blocksRaycasts == true ? false : true;
    }


}
