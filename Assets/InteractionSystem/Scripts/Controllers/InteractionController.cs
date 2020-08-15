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
        public delegate void WorldClick(Vector2 clickPosition, WorldEntity entity);
        public static event WorldClick EntitySelected;
        public static event WorldClick MapClicked;
        public static event WorldClick EntityDeselected;

        //public static InteractionController instance;

        WorldEntity selectedEntity;
        public EntityInteractionMenu interactionMenu;

        public static InteractionController instance;
        // Start is called before the first frame update
        void Awake()
        {
            interactionMenu = GameObject.Find("EntityInteractionMenu")?.GetComponent<EntityInteractionMenu>();

            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            var tapFingers = InputController.instance.TapFilter.GetFingers();


            if (tapFingers.Count > 0)
            {
                var tapFinger = tapFingers.First();
                var tapWorldPos = tapFinger.GetWorldPosition(Vector3.forward.magnitude, Camera.main);
                var result = Physics2D.Raycast(tapWorldPos, Vector2.up);
                WorldEntity click = null;
                if (result != false)
                {
                    click = result.collider.gameObject.GetComponent<WorldEntity>();
                }

                

                if (InputController.instance.MultiTap)
                {
                    DetectIfClickOnWorldEntity(tapWorldPos, click);
                }
                else if(tapFinger.Down)
                {
                    DetectDeselect(tapWorldPos, click);
                }
            }
            
            
        }

        public float GetSizeOfSelectedWorldEntity()
        {
            var rend = selectedEntity.GetComponent<SpriteRenderer>();
            var largestAxisInPixels = rend.size.y * rend.sprite.pixelsPerUnit;
            return largestAxisInPixels;
        }

        public bool HasSelectedEntity()
        {
            bool result = (selectedEntity != null) ? true : false;
            return result;
        }

        public Vector3 GetCentreOfSelectedWorldEntity()
        {
            var rend = selectedEntity.GetComponent<SpriteRenderer>();
            return rend.bounds.center;
        }

        private void DetectIfClickOnWorldEntity(Vector2 worldClickPos, WorldEntity clickedEntity)
        {
            if (clickedEntity != null && clickedEntity != selectedEntity) //if clciked on world entity
            {
                Log($"New Entity: {clickedEntity.gameObject.name} Selected");
                selectedEntity = clickedEntity;
                EntitySelected?.Invoke(worldClickPos, clickedEntity);
            }
            else if (clickedEntity == null) //if clicked on map position
            {
                Log("Position on World Clicked");
                MapClicked?.Invoke(worldClickPos, clickedEntity);
            }
        }

        private void DetectDeselect(Vector2 worldClickPos, WorldEntity clickedEntity)
        {
            if (selectedEntity != null && clickedEntity == null)
            {
                Log($"Entity: {selectedEntity.gameObject.name} Deselected");
                selectedEntity = null;
                EntityDeselected?.Invoke(worldClickPos, null);
            }
        }
    }
}

