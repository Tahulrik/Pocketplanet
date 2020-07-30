using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem
{
    public abstract class WorldEntity : MonoBehaviour, IClickable
    {
        public int MaxHealth;
        public int CurrentHealth;

        void Update()
        { 
            if(CurrentHealth <= 0)
            {
                print("dead " + gameObject.name);
            }
        }

        public virtual void OnInteraction()
        {
            throw new System.NotImplementedException();
        }
    }
}

