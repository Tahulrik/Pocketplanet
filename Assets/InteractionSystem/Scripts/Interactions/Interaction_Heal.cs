using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionSystem.Interactions
{
    public class Interaction_Heal : Interaction
    {
        // Start is called before the first frame update
        void OnEnable()
        {
            parentObject = GetComponent<WorldEntity>();
        }

        public override void OnExecuteInteraction()
        {
            parentObject.CurrentHealth = parentObject.MaxHealth;
        }
    }
}
