using UnityEngine;
using System.Collections;

public abstract class AIState : MonoBehaviour
{
    //protected
    protected AIStateMachine _aIStateMachine;
    

    //abstract
    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();

    //private 
    private AITargetType _curType;

    //public Porperty
    public AITargetType curType { get { return _curType; } set { _curType = value; } }

    //Default Handlers 
    public virtual void OnEnterState() { }
    public virtual void OnExitState() { }
    public virtual void OnAnimatorUpdated() { }
    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEventType aITriggerEventType, Collider other) { }
    public virtual void OnDestinationReached(bool isReached) { }

    //public
    public virtual void SetAIStateMachine(AIStateMachine aIStateMachine)
    {
        _aIStateMachine = aIStateMachine;
    }

    /// <summary>
    /// 转换球形碰撞器到世界坐标轴
    /// </summary>
    public static void ConvertSphereColliderToWorldSpace(SphereCollider sphere,out Vector3 soundPos,out float soundRadius)
    {
        float x = sphere.center.x * sphere.transform.lossyScale.x;
        float y = sphere.center.y * sphere.transform.lossyScale.y;
        float z = sphere.center.z * sphere.transform.lossyScale.z;

        soundPos = new Vector3(x,y,z);

        float temp1 = Mathf.Max(sphere.radius * sphere.transform.lossyScale.x, sphere.radius * sphere.transform.lossyScale.y);
        float temp2 = Mathf.Max(temp1, sphere.radius * sphere.transform.lossyScale.z);
        soundRadius = temp2;
    }


}
