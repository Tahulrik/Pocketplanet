using InteractionSystem.UI;
using Lean.Touch;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InteractionSystem
{
    public class InteractionController : BaseGameController
    {
        public delegate void EntityClicked(WorldEntity entity);
        public static event EntityClicked EntitySelected;
        public static event EntityClicked EntityDeselected;

        //public static InteractionController instance;

        public WorldEntity selectedEntity;
        public EntityInteractionMenu interactionMenu;


        // Start is called before the first frame update
        void Awake()
        {
            interactionMenu = GameObject.Find("EntityInteractionMenu").GetComponent<EntityInteractionMenu>();
        }

        // Update is called once per frame
        void Update()
        {
            var tapFingers = InputController.instance.TapFilter.GetFingers(true);
            if (tapFingers.Count > 0)
            {
                var tapFinger = tapFingers.First();
                var tapWorldPos = tapFinger.GetWorldPosition(Vector3.forward.magnitude, Camera.main);

                //print(tapWorldPos);
                if (tapFinger.Down)
                {
                    DetectClickOnWorldEntity(tapWorldPos);
                }

            }
        }

        private void DetectClickOnWorldEntity(Vector2 ClickPosition)
        {
            var result = Physics2D.Raycast(ClickPosition, Vector2.up);
            if (result != false)
            {
                selectedEntity = result.collider.gameObject.GetComponent<WorldEntity>();

                if (selectedEntity != null)
                {
                    Log("Entity Selected");
                    EntitySelected?.Invoke(selectedEntity);
                }
            }
            else
            {
                if (selectedEntity != null)
                {
                    Log("Entity Deselected");
                    selectedEntity = null;
                    EntityDeselected?.Invoke(null);
                }
            }

        }
    }
}

