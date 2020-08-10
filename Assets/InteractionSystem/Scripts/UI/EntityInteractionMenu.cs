using InteractionSystem.CameraSystem;
using InteractionSystem.Interactions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InteractionSystem.UI
{
    public class EntityInteractionMenu : MonoBehaviour, IDebugLogger
    {
        public bool LogInteractions = false;

        public Sprite MenuItemImage;
        public Transform MenuHolder;
        public Transform StatsHolder;

        public Transform HealthView;

        bool MenuActive;
        WorldEntity _entityInteracted;
        private void OnEnable()
        {
            InteractionController.EntitySelected += EnableInteractionMenuForInteraction;
            InteractionController.EntityDeselected += DisableInteractionMenuForInteraction;
        }

        private void OnDisable()
        {
            InteractionController.EntitySelected -= EnableInteractionMenuForInteraction;
            InteractionController.EntityDeselected -= DisableInteractionMenuForInteraction;
        }

        // Start is called before the first frame update
        void Start()
        {
            MenuActive = true; //Ensure that interactionMenu state can be set
            SetInteractionMenuActiveState(false);
        }

        // Update is called once per frame
        void LateUpdate() //What update?
        {
            if (MenuActive)
            {
                UpdatePlacementTransformInUISpace();
            }
        }

        void SetupInteractionMenuForInteractedObject(WorldEntity entity)
        {
            if (_entityInteracted == null || _entityInteracted != entity)
            {
                DeleteMenuButtons();
            }

            var interactions = GetInteractionsFromEntity(entity);

            CreateInteractionRadialMenuSlices(interactions);

            var healthText = HealthView.GetComponent<TextMeshProUGUI>();
            healthText.text = $"Health: {entity.CurrentHealth}";
        }

        void EnableInteractionMenuForInteraction(WorldEntity entity) // entity will always be null here
        {
            if (_entityInteracted != entity)
            {
                SetupInteractionMenuForInteractedObject(entity);
                _entityInteracted = entity; 
            }

            SetInteractionMenuActiveState(true);
        }

        void DisableInteractionMenuForInteraction(WorldEntity entity) // entity will always be null here
        {
            SetInteractionMenuActiveState(false);

            DeleteMenuButtons();
        }

        void CreateInteractionRadialMenuSlices(List<Interactions.Interaction> interactions)
        {
            for (int i = 1; i <= interactions.Count; i++)
            {
                int sliceAmount = interactions.Count;
                var newSlice = CreateNewMenuSlice(sliceAmount, 360, MenuHolder);
                newSlice.name = interactions[i - 1].ToString() + "Menu";
                var stepLength = (360f / sliceAmount) * (i - 1);
                var rotationAmount = -stepLength;

                newSlice.transform.rotation = Quaternion.Euler(0, 0, rotationAmount);
            }
        }

        void UpdatePlacementTransformInUISpace()
        {
            var BoundsForObject = _entityInteracted.GetComponent<SpriteRenderer>().bounds;

            var scaleAmount =  (CameraController.instance.CurrentZoomAmount) / (BoundsForObject.extents.x * 2);
            transform.position = BoundsForObject.center;//Camera.main.WorldToScreenPoint(BoundsForObject.center);
            transform.rotation = _entityInteracted.transform.rotation;
            transform.localScale = new Vector2(scaleAmount, scaleAmount);
            Log(scaleAmount.ToString());
        }

        GameObject CreateNewMenuSlice(int sliceAmount, float circumference, Transform parent)
        {
            if (circumference > 360)
            {
                circumference = 360;
            }
            else if (circumference < 0)
            {
                circumference = 0;
            }

            var fillAmount = circumference / 360f;
            var sliceSize = fillAmount / sliceAmount - 2f / circumference;

            GameObject newSlice = new GameObject();
            var imageComponent = newSlice.AddComponent<Image>();
            imageComponent.type = Image.Type.Filled;
            imageComponent.fillMethod = Image.FillMethod.Radial360;
            imageComponent.fillOrigin = 3;
            imageComponent.fillAmount = sliceSize;
            imageComponent.sprite = MenuItemImage;

            newSlice.transform.parent = parent;
            newSlice.transform.localPosition = Vector3.zero;
            newSlice.transform.localScale = Vector3.one;
            return newSlice;
        }

        public static List<Interactions.Interaction> GetInteractionsFromEntity(WorldEntity entity)
        {
            var components = entity.GetComponents<Interactions.Interaction>().ToList();

            return components;
        }

        private void DeleteMenuButtons()
        {
            if (MenuHolder.childCount > 0)
            {
                foreach (Transform slice in MenuHolder)
                {
                    Destroy(slice.gameObject);
                }

                _entityInteracted = null;
            }
        }

        private void SetInteractionMenuActiveState(bool activeState)
        {
            if (MenuActive != activeState)
            { 
                MenuHolder.parent.gameObject.SetActive(activeState);
                MenuActive = activeState;
            }
        }

        public void Log(string message)
        {
            if (LogInteractions)
                print(message);
        }
    }
}

