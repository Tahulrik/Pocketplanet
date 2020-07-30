using InteractionSystem.Interactions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace InteractionSystem.UI
{
    public class EntityInteractionMenu : MonoBehaviour
    {
        public WorldEntity EntityInteracted;
        public Sprite MenuItemImage;
        public Transform MenuHolder;
        // Start is called before the first frame update
        void Start()
        {
            var interactions = GetInteractionsFromEntity(EntityInteracted);

            
            for(int i = 1; i <= interactions.Count; i++)
            {
                int sliceAmount = interactions.Count;
                var newSlice = CreateNewMenuSlice(sliceAmount);
                newSlice.name = interactions[i - 1].ToString() + "Menu";
                var stepLength = (180 / sliceAmount) * (i-1);
                var rotationAmount = -stepLength;

                newSlice.transform.rotation = Quaternion.Euler(0,0, rotationAmount);
            }

            transform.GetChild(0).gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                

                transform.position = Camera.main.WorldToScreenPoint(EntityInteracted.transform.position);
                transform.rotation = EntityInteracted.transform.rotation;
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }

        GameObject CreateNewMenuSlice(int sliceAmount)
        {
            var sliceSize = 0.5f / sliceAmount /*- 10 / 360f*/;

            GameObject newSlice = new GameObject();
            var imageComponent = newSlice.AddComponent<Image>();
            imageComponent.type = Image.Type.Filled;
            imageComponent.fillMethod = Image.FillMethod.Radial360;
            imageComponent.fillOrigin = 3;
            imageComponent.fillAmount = sliceSize;
            imageComponent.sprite = MenuItemImage;

            newSlice.transform.parent = MenuHolder;
            newSlice.transform.localPosition = -Vector3.up*25;
            return newSlice;
        }

        public static List<Interactions.Interaction> GetInteractionsFromEntity(WorldEntity entity)
        {
            var components = entity.GetComponents<Interactions.Interaction>().ToList();

            return components;
        }


    }
}

