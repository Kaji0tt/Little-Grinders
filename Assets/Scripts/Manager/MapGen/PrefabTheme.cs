using UnityEngine.AI;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PrefabTheme : MonoBehaviour
{
    [SerializeField]
    public GameObject[] smalGreenPF;
    [SerializeField]
    public GameObject[] midGreenPF;
    [SerializeField]
    public GameObject[] highGreenPF;
    [SerializeField]
    public GameObject[] horizntalFencePF;
    [SerializeField]
    public GameObject[] verticalFencePF;
    [SerializeField]
    public GameObject[] enemiesPF;
    [SerializeField]
    public GameObject[] groundTexture;
    [SerializeField]
    public GameObject[] interactablesPF;

    public int requiredLevel;

    public WorldType themeType;


    private void Start()
    {
       
        
    }
}
