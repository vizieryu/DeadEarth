using UnityEngine;
using System.Collections;

public abstract class AIZombieState : AIState
{
    
    //protected
    protected int _playerLayerMask = -1;
    protected int _bodyPartLayer = -1;
    protected int _visualLayerMask = -1;
    protected AIZombieStateMachine _zombieStateMachine = null;
    


    public override void SetAIStateMachine(AIStateMachine aIStateMachine)
    {
        if (aIStateMachine.GetType() == typeof(AIZombieStateMachine))
        {
            _zombieStateMachine = (AIZombieStateMachine)aIStateMachine;
        }
        base.SetAIStateMachine(aIStateMachine);
    }

    void Awake()
    {
        // Get a mask for line of sight testing with the player. (+1) is a hack to include the
        // default layer in the current version of unity.
        _playerLayerMask = LayerMask.GetMask("Player", "AI Body Part") + 1;
        _visualLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Visual Aggravator") + 1;

        // Get the layer index of the AI Body Part layer
        _bodyPartLayer = LayerMask.NameToLayer("AI Body Part");
    }

    public override void OnTriggerEvent(AITriggerEventType aITriggerEventType, Collider other)
    {
        base.OnTriggerEvent(aITriggerEventType, other);

        if (aITriggerEventType != AITriggerEventType.Exit)
        {
            //1.player进入触发区。 (visual_player)
            if (other.CompareTag(Label.Tag.player))
            {
                //威胁距离
                float distanceToThreat = Vector3.Distance(_aIStateMachine.sensorPosition,other.transform.position);

                //视觉威胁不是player 或者 视觉威胁是player && 距离<distance.
                if (_aIStateMachine.VisualThreat.type != AITargetType.Visual_Player ||
                   _aIStateMachine.VisualThreat.type == AITargetType.Visual_Player && distanceToThreat < _aIStateMachine.VisualThreat.distance)
                {
                    RaycastHit hitInfo ;
                    if (ColliderIsVisible(other,out hitInfo, _playerLayerMask))
                    {
                        _aIStateMachine.VisualThreat.Set(AITargetType.Visual_Player, other, other.transform.position, distanceToThreat);
                    }
                }
            }
            //2.灯光触发
            else if (other.CompareTag(Label.Tag.flash_light) && _zombieStateMachine.VisualThreat.type != AITargetType.Visual_Player)
            {
                BoxCollider flashLightTrigger = (BoxCollider)other;

                //用,比例来算.
                float distanceToThreat = Vector3.Distance(_aIStateMachine.sensorPosition, flashLightTrigger.transform.position);
                float sizeZ = flashLightTrigger.size.z * flashLightTrigger.transform.lossyScale.z;
                float aggrFactor = distanceToThreat / sizeZ;
                if (aggrFactor <= _zombieStateMachine.sight && aggrFactor <= _zombieStateMachine.intelligence)
                {
                    _zombieStateMachine.VisualThreat.Set(AITargetType.Visual_Light, other, other.transform.position, distanceToThreat);
                }
            }
            //3.声音传递
            else if (other.CompareTag(Label.Tag.ai_sound_emitter) )
            {
                SphereCollider soundTrigger = (SphereCollider)other;
                if (soundTrigger == null) return;

                //把soundTrigger转到世界轴.
                Vector3 soundPos;
                float soundRadius;
                AIState.ConvertSphereColliderToWorldSpace(soundTrigger,out soundPos,out soundRadius);

                float distanceToSound = Vector3.Distance(_aIStateMachine.sensorPosition, soundPos);
                
                float distanceFactor = distanceToSound / soundRadius;
                //再计算,基于Agent听力能力的因素偏倚。
                distanceFactor += distanceFactor * (1.0f - _zombieStateMachine.hearing);
                //太远...
                if (distanceFactor > 1) {  return; }

                if (distanceToSound <= _zombieStateMachine.AudioThreat.distance)
                {
                    _zombieStateMachine.AudioThreat.Set(AITargetType.Audio, soundTrigger, soundPos, distanceToSound);
                }
            }
            //3.食物(尸体)
            else if (other.CompareTag(Label.Tag.ai_food) && curType != AITargetType.Visual_Player && curType != AITargetType.Visual_Light
                     && _zombieStateMachine.satisfaction <= 0.9f && _zombieStateMachine.AudioThreat.type == AITargetType.None)
            {
                float distanceToFood = Vector3.Distance(other.transform.position,_zombieStateMachine.sensorPosition);
                
                if (distanceToFood <= _zombieStateMachine.VisualThreat.distance)
                {
                    RaycastHit hitInfo;
                    if (ColliderIsVisible(other,out hitInfo, _visualLayerMask))
                    {
                        _zombieStateMachine.VisualThreat.Set(AITargetType.Visual_Food, other, other.transform.position, distanceToFood);
                    }
                }
            }
        }
    }


    /// <summary>
    /// 是否能检测到玩家的存在.(隔绝掉障碍物的阻挡)
    /// </summary>
    /// <returns></returns>
    public bool ColliderIsVisible(Collider other,out RaycastHit hitInfo, int layerMask = -1)
    {
        hitInfo = new RaycastHit();

        //1.先限制视野
        //判断是否处于FOV视野里面.
        Vector3 head =  _aIStateMachine.sensorPosition;
        Vector3 direction = other.transform.position - head;
        float angle = Vector3.Angle(transform.forward , direction);
        _aIStateMachine = (AIZombieStateMachine)_aIStateMachine;
        if (angle > _zombieStateMachine.fov/2)
        {
            return false;
        }

        float closestColliderDistance = float.MaxValue;
        Collider closestCollider = null;
        //2.再,剔除其他的物体(视觉)遮挡.
        RaycastHit[] raycastHits = Physics.RaycastAll(_aIStateMachine.sensorPosition, direction.normalized, _aIStateMachine.sensorRadius * _zombieStateMachine.sight, _playerLayerMask);
        for (int i = 0; i < raycastHits.Length; i++)
        {
            if (raycastHits[i].distance < closestColliderDistance)
            {
                //ai_body层
                if (raycastHits[i].transform.gameObject.layer == _bodyPartLayer)
                {
                    //并且,不是自身.
                    if (_aIStateMachine != GameScenseManager.Instance.GetAiStateMachine(other.GetComponent<Rigidbody>().GetInstanceID()))
                    {
                        closestColliderDistance = raycastHits[i].distance;
                        closestCollider = raycastHits[i].collider;
                        hitInfo = raycastHits[i];
                    }
                }
                else
                {
                    closestColliderDistance = raycastHits[i].distance;
                    closestCollider = raycastHits[i].collider;
                    hitInfo = raycastHits[i];
                }
            }
        }

        //满足:视野中存在player.
        if (closestCollider.gameObject == other.gameObject && closestCollider)
        {
            return true;
        }

        return false;
    }

}
