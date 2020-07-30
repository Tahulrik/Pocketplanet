using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InteractionSystem.Interactions
{
    public abstract class Interaction : MonoBehaviour, IEntityInteraction
    {
        Image InteractionIcon;
        protected WorldEntity parentObject;

        public virtual void OnExecuteInteraction()
        {
            throw new System.NotImplementedException();
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            parentObject = GetComponent<WorldEntity>();
        }

        public override string ToString()
        {
            return GetType().Name;
        }

    }
}
