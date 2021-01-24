using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColliderManager : MonoBehaviour
{
    #region Singleton
    public static PlayerColliderManager instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion
    public GameObject player_collider;
}
