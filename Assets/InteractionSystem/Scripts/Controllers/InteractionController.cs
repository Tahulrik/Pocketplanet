using Lean.Touch;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InteractionSystem
{
    public class InteractionController : MonoBehaviour
    {
        public delegate void EntityClicked(WorldEntity entity);
        public static event EntityClicked OnEntityClicked;

        //public static InteractionController instance;

        // Start is called before the first frame update
        void Awake()
        {

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
                var clickedEntity = result.collider.gameObject.GetComponent<WorldEntity>();

                if (clickedEntity != null)
                    OnEntityClicked?.Invoke(clickedEntity);
            }
        }
    }
}

