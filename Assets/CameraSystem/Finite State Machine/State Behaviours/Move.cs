using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraSystem.StateMachine.States
{
    public class Move : SceneLinkedSMB<CameraController>
    {
        public override void OnSLTransitionToStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.CommandMoveCamera();
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_MonoBehaviour.CommandMoveCamera();
        }
    }
}

