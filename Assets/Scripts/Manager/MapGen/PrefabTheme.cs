using UnityEngine.AI;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PrefabTheme : MonoBehaviour
{
    //Kleine Umgebungsobjekte
    [SerializeField]
    public GameObject[] smalGreenPF;

    //Mittlere Umgebungsobjekte
    [SerializeField]
    public GameObject[] midGreenPF;

    //Große Umgebungsobjekte
    [SerializeField]
    public GameObject[] highGreenPF;

    //Zaun für die Horizontale Begrenzung der Karte
    [SerializeField]
    public GameObject[] horizntalFencePF;

    //Zaun für die Vertikale Begrenzung
    [SerializeField]
    public GameObject[] verticalFencePF;

    //Feinde, welche in diesem Theme spawnen können
    [SerializeField]
    public GameObject[] enemiesPF;

    //Ground-Textures. Derzeit noch nicht in Verwendung.
    [SerializeField]
    public GameObject[] groundTexture;

    //Dinge, mit denen in diesem Theme interagiert werden können (Totem, Truhen, ...)
    [SerializeField]
    public GameObject[] interactablesPF;

    //Vorgebaute Tiles für Low-Veg-Fields
    public GameObject[] preBuildTiles;

    //Das benötigte Karten-Level, damit dieses theme spawnen kann.
    public int requiredLevel;

    public WorldType themeType;


    private void Start()
    {
       
        
    }
}
