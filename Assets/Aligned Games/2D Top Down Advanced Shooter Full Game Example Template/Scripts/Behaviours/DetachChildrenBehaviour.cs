using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlignedGames
{
    public class DetachChildrenBehaviour : MonoBehaviour
    {
        void Start()
        {
            transform.DetachChildren(); // Detaches all child GameObjects from this GameObject
        }
    }
}
