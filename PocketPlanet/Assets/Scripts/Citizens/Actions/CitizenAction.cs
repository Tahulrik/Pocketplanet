using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Buildings;

namespace Citizens.Actions{

    public enum WalkDirection
    {
        Left = 1,
        Right = -1,
        Stationary = 0
    }

	public abstract class CitizenAction{

		public CitizenAction previousAction;

		public GameObject CurrentActionTarget;
		protected Transform targetPosition;

		public CitizenBehaviour Self;

		public bool actionPaused = false;

        protected WalkDirection direction = WalkDirection.Stationary;
		protected float ObjectiveRange = 0.15f;

		protected float actionTimer;
		protected float timer;

		public bool isBusy = false;
		public bool returningHome = false;


        //remember to use hashes for animation states
        protected Animator animator;

		public bool ActionCompleted;

		public virtual bool GetNewTarget()
		{
			return false;
		}

		public virtual bool GetNewTarget(List<GameObject> targets)
        {
			if (targets == null) {
				return false;
			}

			if (targets.Contains(Self.gameObject))
			{
				targets.Remove(Self.gameObject);   
			}

            if (targets.Count == 0)
            {
				StopAction ();
				return false;
            }
            var randSelection = Random.Range(0, targets.Count);
            CurrentActionTarget = targets[randSelection];
			isBusy = true;
			return true;
        }

		public GameObject FindClosestTarget(List<GameObject> targets)
		{
			Vector3 currentPos = Self.transform.position;

			GameObject tMin = null;
			float minDist = Mathf.Infinity;

			foreach (var target in targets) {
				float dist = Vector3.Distance (target.transform.position, currentPos);
				if (dist < minDist) {
					tMin = target;
					minDist = dist;
				}
			}

			return tMin;
		}

		protected void FindDirectionToTarget(Vector2 targetPosition)
        {
            float dot = Vector2.Dot(targetPosition, Self.transform.right);
			var rotation = Self.transform.GetChild (0).localRotation;

            if (dot < 0)
            {
				rotation.eulerAngles = new Vector2(0,180f);
                direction = WalkDirection.Left;
            }
            else
            {
				rotation.eulerAngles = new Vector2(0, 0);
                direction = WalkDirection.Right;
            }

			//no need to get child every frame?
			Self.transform.GetChild (0).localRotation = rotation;
        }

        protected void MoveToTarget(float speedModifier)
        {
			if (Self.CurrentSubState != SubState.Moving) {
				Self.CurrentSubState = SubState.Moving;
			}

			FindDirectionToTarget(targetPosition.position);

            Self.transform.RotateAround(Planet.PlanetObject.transform.position, Vector3.forward, (int)direction * 5 * Time.deltaTime * speedModifier);

        }

		public void StartRunning()
		{
			if (!Self.isRunning) {
				Self.isRunning = true;
				Self.CurrentMoveSpeed = Self.RunningSpeed;
			}
		}

		public void StopRunning()
		{
			if (Self.isRunning) {
				Self.isRunning = false;
				Self.CurrentMoveSpeed = Self.WalkingSpeed;
			}
		}

		protected bool ReachedTarget(GameObject self, Vector2 targetDestination)
        {
			var dist = Vector2.Distance(self.transform.position, targetDestination);
            //Find threshold such that it fits with the target
			if (dist < ObjectiveRange)
            {
                direction = 0;
                return true;
            }

            return false;
        }

		protected float GetDistanceToTarget(GameObject self, Vector2 target)
		{
			var dist = Vector2.Distance(self.transform.position, target);

			return dist;
		}

		protected void ForceTargetToHalt(CitizenBehaviour target)
		{
			if (target.CurrentAction != null) {
				target.CurrentAction.previousAction = target.CurrentAction;
			}
			target.CurrentAction = null;
		
			target.CurrentSubState = SubState.Waiting;
		}

		public void EnterBuilding(Building target)
		{
			target.PeopleInside.Add(Self.gameObject);

			Self.inBuilding = true;
			Self.CurrentSubState = SubState.Waiting;

			Self.SetPersonActive(false);
			isBusy = false;
		}

		public void ExitBuilding(Building target)
		{
            try
            {
                target.PeopleInside.Remove(Self.gameObject);

            }
            catch (System.Exception)
            { }
			

			Self.inBuilding = false;

			Self.SetPersonActive(true);
		}
			
		public virtual void PauseAction(bool value)
		{
            animator.SetBool("ActionPaused", value);
            actionPaused = value;
		}

        public virtual void StopAction()
		{
			Self.CurrentState = ActionState.Idle;
			Self.CurrentSubState = SubState.Waiting;

			ActionCompleted = true;
			Self.CurrentAction = null;
		}

        public virtual void ExecuteAction()
        {
			if (actionPaused) {
				return;
			}

            if (animator == null)
            {
				animator = Self.anim;
            }

        }
	}
}

