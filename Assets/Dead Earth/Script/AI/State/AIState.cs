using UnityEngine;
using System.Collections;

public abstract class AIState
{
    protected AIStateMachine _aIStateMachine;

    //abstract
    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();

    //Default Handlers 
    public virtual void OnEnterState() { }
    public virtual void OnExitState() { }
    public virtual void OnAnimatorUpdated() { }
    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEventType aITriggerEventType, Collider other) { }
    public virtual void OnDestinationReached(bool isReached) { }

    //public
    public void SetAIStateMachine(AIStateMachine aIStateMachine)
    {
        _aIStateMachine = aIStateMachine;
    }

}
