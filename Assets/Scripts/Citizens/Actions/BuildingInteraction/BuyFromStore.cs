using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;

namespace Citizens.Actions
{
    public class BuyFromStore : CitizenAction
    {

        Building_Normal_Service targetScript;


        bool boughtItem = false;


        public override void ExecuteAction()
        {
            base.ExecuteAction();

            if (actionPaused)
            {
                return;
            }

            //Debug.Log ("Starting Store Extorsion Execute");
            if (targetScript.IsOnFire)
            {
                ExitBuilding(targetScript);
            }


            if (Self.inBuilding)
            {
                StartConsidering();
            }

            if (ReachedTarget(Self.gameObject, targetPosition.position))
            {
                if (!Self.inBuilding)
                {
                    EnterBuilding(targetScript);
                    
                }
            }
            else
            {
                MoveToTarget(Self.CurrentMoveSpeed);
            }
        }

        public override void StopAction()
        {
            ActionCompleted = true;
            Self.CurrentAction = null;
        }

        void StartConsidering()
        {
            timer += Time.deltaTime;
            if (timer >= actionTimer)
            {
                targetScript.BuyItem(Self);
                ExitBuilding(targetScript);
                StopAction();
            }
        }




        public override bool GetNewTarget(List<GameObject> targets)
        {
            return false;
        }


        public BuyFromStore(CitizenBehaviour self, Building_Normal_Service target)
        {
            Self = self;
            ActionCompleted = false;
            CurrentActionTarget = target.gameObject;
            ObjectiveRange = 0.15f;

            timer = Random.Range(7.5f, 15f);

            Self.SetPersonActive(true);

            targetScript = target as Building_Normal_Service;

            animator = Self.anim;

            targetPosition = target.transform;

            ExecuteAction();
        }
    }
}


