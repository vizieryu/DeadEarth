using UnityEngine;
using System.Collections;

public class AIZombieState_Alert1 : AIZombieState
{
    [SerializeField] [Range(1, 60)] float _maxDuration = 10.0f;


    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_aIStateMachine == null)
        {
            return;
        }

        //配置 state Machine
        _zombieStateMachine.NavAgentControl(true,false);
        _zombieStateMachine.speed = 1;



    }

    public override AIStateType OnUpdate()
    {


        return AIStateType.Alerted;
    }
}
