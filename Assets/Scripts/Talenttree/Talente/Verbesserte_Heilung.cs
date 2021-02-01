using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Verbesserte_Heilung : Spell
{
    //public Heilung heilung;


    //Dies sollte keinesfalls über Update geschehen! Finde lieber ein Workaround, damit das alles CPU schonender wird.

    private void Awake()
    {
        SetDescription("Erhöht die verursachte Heilung je Skillpunkt um 1% des maximalen Lebens.");
    }



}
