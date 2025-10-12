using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MatthewAssets
{

    public class RotationParticle : MonoBehaviour
    {
        public Transform Orbit1;
        public Transform Static;
        //public Transform Orbit3;
        //public Transform Orbit;
        public float Speed;


        void Update()
        {
            Orbit1.RotateAround(Static.position, Vector3.up, Speed * Time.deltaTime);
            //Orbit3.RotateAround(Orbit2.position, Vector3.up, Speed * Time.deltaTime);
        }
    }
}
