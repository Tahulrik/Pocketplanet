using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraSystem.StateMachine.States
{
    public class MoveTo : SceneLinkedSMB<CameraController>
    {
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
        {
            m_MonoBehaviour.CommandMoveTo();
        }

        public override void OnSLTransitionToStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //m_MonoBehaviour.CommandMoveTo();
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //m_MonoBehaviour.CommandMoveTo();
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }
    }
}
