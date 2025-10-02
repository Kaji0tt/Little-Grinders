using System.Collections.Generic;
using UnityEngine;

public class InteractableManager : MonoBehaviour
{
    public static InteractableManager instance;
    
    private Dictionary<string, InteractableSaveData> interactableStates = new Dictionary<string, InteractableSaveData>();
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SaveInteractableState(string id, InteractableSaveData data)
    {
        interactableStates[id] = data;
    }
    
    public InteractableSaveData LoadInteractableState(string id)
    {
        return interactableStates.TryGetValue(id, out InteractableSaveData data) ? data : null;
    }
    
    public Dictionary<string, InteractableSaveData> GetAllStates()
    {
        return new Dictionary<string, InteractableSaveData>(interactableStates);
    }
    
    public void LoadAllStates(Dictionary<string, InteractableSaveData> states)
    {
        interactableStates = new Dictionary<string, InteractableSaveData>(states);
    }
}