using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;

namespace Citizens.Actions
{
    public class IdleMovement : CitizenAction
    {

        float newDirectionTime = 10f;
        float directionTimer = 0;
        public override void ExecuteAction()
        {
			if (actionPaused) {
				return;
			}

            base.ExecuteAction();

            if (actionPaused)
            {
                return;
            }

            directionTimer += Time.deltaTime;
            if (directionTimer >= newDirectionTime)
            {
                GetNewDirection();
            }


            timer += Time.deltaTime;
            if (timer >= actionTimer)
            {
                StopAction();
            }

            Move(Self.CurrentMoveSpeed);
        }

        protected void Move(float speedModifier)
        {
            if (Self.CurrentSubState != SubState.Moving)
            {
                Self.CurrentSubState = SubState.Moving;
            }
			animator.SetInteger ("Movement", 0);
  
            Self.transform.RotateAround(Planet.PlanetObject.transform.position, Vector3.forward, (int)direction * 3 * Time.deltaTime * speedModifier);

        }

        protected void GetNewDirection()
        {
			//Debug.Log (Self.name + " Getting new Direction");
            var rand = Random.value;

            if (rand >= 0.5)
            {
                direction = WalkDirection.Left;
            }
            else
            {
                direction = WalkDirection.Right;
            }

			directionTimer = 0;
            newDirectionTime = Random.Range(5f, 15f);
        }


        public IdleMovement(CitizenBehaviour self, float time)
        {
            Self = self;
            ActionCompleted = false;

            ObjectiveRange = 0.15f;

            actionTimer = time;
            newDirectionTime = Random.Range(5f, 15f);
            Self.SetPersonActive(true);

            animator = Self.anim;


            ExecuteAction();
        }
    }


}

