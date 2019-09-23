using UnityEngine;
using System.Collections;

public class AIZombieState_Patrol1 : AIZombieState
{
    // Inpsector Assigned 
    [SerializeField] float _turnOnSpotThreshold = 80.0f;
    [SerializeField] float _slerpSpeed = 5.0f;

    [SerializeField] [Range(0.0f, 3.0f)] float _speed = 1.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        if (_zombieStateMachine == null)
            return;
        //配置nav
        _zombieStateMachine.NavAgentControl(true,false);
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;
        if (_zombieStateMachine.currentTargetType != AITargetType.Waypoint)
        {
            _zombieStateMachine.ClearTarget();

            _zombieStateMachine.Agent.SetDestination(_zombieStateMachine.GetWayPointPosition(false));
            //重新-导航-路径
            _zombieStateMachine.Agent.Resume();
        }
    }



    public override AIStateType OnUpdate()
    {
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)
        {
            // If the distance to hunger ratio means we are hungry enough to stray off the path that far
            if ((1.0f - _zombieStateMachine.satisfaction) > (_zombieStateMachine.VisualThreat.distance / _zombieStateMachine.sensorRadius))
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                return AIStateType.Pursuit;
            }
        }

        // If path is still be computed then wait
        if (_zombieStateMachine.Agent.pathPending)
        {
            _zombieStateMachine.speed = 0;
            return AIStateType.Patrol;
        }
        else
            _zombieStateMachine.speed = _speed;

        // Calculate angle we need to turn through to be facing our target
        float angle = Vector3.Angle(_zombieStateMachine.transform.forward, (_zombieStateMachine.Agent.steeringTarget - _zombieStateMachine.transform.position));

        // If its too big then drop out of Patrol and into Altered
        if (angle > _turnOnSpotThreshold)
        {
            return AIStateType.Alerted;
        }

        // If root rotation is not being used then we are responsible for keeping zombie rotated
        // and facing in the right direction. 
        if (!_zombieStateMachine.useRootRotation)
        {
            // Generate a new Quaternion representing the rotation we should have
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.Agent.desiredVelocity);

            // Smoothly rotate to that new rotation over time
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }

        // If for any reason the nav agent has lost its path then call the NextWaypoint function
        // so a new waypoint is selected and a new path assigned to the nav agent.
        if (_zombieStateMachine.Agent.isPathStale ||
            !_zombieStateMachine.Agent.hasPath ||
            _zombieStateMachine.Agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            _zombieStateMachine.Agent.SetDestination(_zombieStateMachine.GetWayPointPosition(true));
        }

        return AIStateType.Patrol;
    }

    public override void OnDestinationReached(bool isReached)
    {
        // Only interesting in processing arricals not departures
        if (_zombieStateMachine == null || !isReached)
            return;

        // Select the next waypoint in the waypoint network
        if (_zombieStateMachine.currentTargetType == AITargetType.Waypoint)
            _zombieStateMachine.Agent.SetDestination(_zombieStateMachine.GetWayPointPosition(true));
    }
}
