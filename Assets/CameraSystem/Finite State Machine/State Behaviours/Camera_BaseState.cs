using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraSystem.StateMachine.States
{
    public abstract class Camera_BaseState : SceneLinkedSMB<CameraController>
    {
        protected static CameraController controller;
        protected static Transform cameraBody;
        private void OnEnable()
        {
            

            
        }

        public override void OnStart(Animator animator)
        {
        
        }






    }

}
