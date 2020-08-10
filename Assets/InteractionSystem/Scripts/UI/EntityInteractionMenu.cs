using InteractionSystem.Interactions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InteractionSystem.UI
{
    public class EntityInteractionMenu : MonoBehaviour
    {
        public WorldEntity EntityInteracted;
        public Sprite MenuItemImage;
        public Transform MenuHolder;
        public Transform StatsHolder;

        public Transform HealthView;
        // Start is called before the first frame update
        void Start()
        {
            var interactions = GetInteractionsFromEntity(EntityInteracted);

            CreateInteractionRadialMenuSlices(interactions);

            var healthText = HealthView.GetComponent<TextMeshProUGUI>();
            healthText.text = $"Health: {EntityInteracted.CurrentHealth}";
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                SetPlacementPositionInUISpace(transform);


                
            }
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

        void SetPlacementPositionInUISpace(Transform menuItem)
        {
            var BoundsForObject = EntityInteracted.GetComponent<SpriteRenderer>().bounds;
            


            transform.position = Camera.main.WorldToScreenPoint(BoundsForObject.center);
            transform.rotation = EntityInteracted.transform.rotation;
            transform.GetChild(0).gameObject.SetActive(true);   
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
            return newSlice;
        }

        public static List<Interactions.Interaction> GetInteractionsFromEntity(WorldEntity entity)
        {
            var components = entity.GetComponents<Interactions.Interaction>().ToList();

            return components;
        }
    }
}

