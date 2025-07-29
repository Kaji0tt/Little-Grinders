using UnityEngine;

public class MonoBehaviourHelper : MonoBehaviour
{
    public static MonoBehaviourHelper instance;
    
    private void Awake()
    {
        Debug.Log("[MonoBehaviourHelper] Awake() aufgerufen");
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[MonoBehaviourHelper] ✅ Instance erstellt und als DontDestroyOnLoad markiert");
        }
        else
        {
            Debug.Log("[MonoBehaviourHelper] ⚠️ Duplicate instance zerstört");
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        Debug.Log("[MonoBehaviourHelper] Start() - Helper ist bereit für Coroutines");
    }
}