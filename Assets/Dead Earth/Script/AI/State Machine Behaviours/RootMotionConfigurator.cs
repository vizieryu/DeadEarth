using UnityEngine;
using System.Collections;

// ------------------------------------------------------------
// CLASS	:	RootMotionConfigurator
// DESC		:	与AIStateMachine派生类通信的状态机行为，
//              允许根据每个动画状态启用/禁用根运动。
// ------------------------------------------------------------
public class RootMotionConfigurator : AIStateMachineLink
{
    // Inspector Assigned Reference Incrementing Variables
    [SerializeField] private int _rootPosition = 0;
    [SerializeField] private int _rootRotation = 0;

    // --------------------------------------------------------
    // Name	:	OnStateEnter
    // Desc	:	在分配给此状态的动画的第一帧之前调用。
    // --------------------------------------------------------
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        // 请求为这个动画状态启用/禁用根运动
        if (_stateMachine)
            _stateMachine.AddRootMotionRequest(_rootPosition, _rootRotation);
    }

    // --------------------------------------------------------
    // Name	:	OnStateExit
    // Desc	:	调用动画离开前的最后一帧。
    // --------------------------------------------------------
    override public void OnStateExit(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        // Inform the AI State Machine that we wish to relinquish our root motion request.
        if (_stateMachine)
            _stateMachine.AddRootMotionRequest(-_rootPosition, -_rootRotation);
    }
}
