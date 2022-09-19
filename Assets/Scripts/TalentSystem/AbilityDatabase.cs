using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDatabase : MonoBehaviour
{
    #region Singleton
    public static AbilityDatabase instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
