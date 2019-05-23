using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum AIStateType
{
    None,           //无
    Idle,           //闲置
    Alerted,        //警觉
    Chase,          //追踪
    Attack,         //攻击
    Patrol,         //巡逻
    Dead            //死亡
}

public enum AITargetType
{
    None,           //无
    Waypoint,       //路线
    Visual_Player,  //视觉_Player
    Visual_Light,   //视觉_Light
    Visual_Food,    //视觉_Food
    Audio           //声音
}

public enum AITriggerEventType
{
    Enter,
    Stay,
    Exit
}

public struct AITarget
{
    private AITargetType _type;
    private Collider _collider;
    private Vector3 _position;
    private float _distance;
    private float _time;

    public AITargetType Type { get { return _type; } }

    public Collider Collider { get { return _collider; } }

    public Vector3 Position { get { return _position; } }

    public float Distance { set { _distance = value; } get { return _distance; } }

    public float Times { get { return _time; } }

    public void Set(AITargetType type, Collider collider, Vector3 position, float distance)
    {
        _type = type;
        _collider = collider;
        _position = position;
        _distance = distance;
        _time = Time.time;
    }

    public void Clear()
    {
        _type = AITargetType.None;
        _collider = null;
        _position = Vector3.zero;
        _distance = Mathf.Infinity;
        _time = 0.0f;
    }

}

public abstract class AIStateMachine : MonoBehaviour
{
    //public
    public AITarget VisualThreat = new AITarget();
    public AITarget AudioThreat = new AITarget();
    public AIStateType _currentStateType = AIStateType.Idle; 

    //protected
    protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();
    protected AITarget _target = new AITarget();
    protected AIState _currentState;


    [SerializeField] private SphereCollider _targetColiiderTrigger = null;  //目标触发
    [SerializeField] private SphereCollider _sensorColiiderTrigger = null;  //警觉触发

    [SerializeField] [Range(0, 15)] protected float _stoppingDistance = 1.0f;

    //component
    protected Animator _anim;
    protected NavMeshAgent _agent;
    protected Collider _collider;
    protected Transform _transform;

    //public Properties
    public Animator Anim { get { return _anim; } }
    public NavMeshAgent Agent { get { return _agent; } }
    public Vector3 sensorPosition
    {
        get
        {
            if (_sensorColiiderTrigger == null) return Vector3.zero;
            Vector3 point = _sensorColiiderTrigger.transform.position;
            point.x += _sensorColiiderTrigger.center.x * _sensorColiiderTrigger.transform.lossyScale.x;
            point.y += _sensorColiiderTrigger.center.y * _sensorColiiderTrigger.transform.lossyScale.y;
            point.z += _sensorColiiderTrigger.center.z * _sensorColiiderTrigger.transform.lossyScale.z;
            return point;
        }
    }

    public float sensorRaduis
    {
        get
        {
            //求取,三坐标轴. 得出最佳 raduis
            if (_sensorColiiderTrigger == null) return 0.0f;
            float radius = Mathf.Max(_sensorColiiderTrigger.radius * _sensorColiiderTrigger.transform.lossyScale.x,
                          _sensorColiiderTrigger.radius * _sensorColiiderTrigger.transform.lossyScale.y);
            return Mathf.Max(radius, _sensorColiiderTrigger.radius * _sensorColiiderTrigger.transform.lossyScale.z);
        }
    }

    protected virtual void Awake()
    {
        //为组件去赋值
        _anim = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();

        if (GameScenseManager.Instance != null)
        {
            //每一个物体的InstanceID在场景中是唯一的
            //注册
            if (_targetColiiderTrigger) GameScenseManager.Instance.RegisterAiStateMachine(_targetColiiderTrigger.GetInstanceID(),this);
            if (_sensorColiiderTrigger) GameScenseManager.Instance.RegisterAiStateMachine(_sensorColiiderTrigger.GetInstanceID(),this);
        }
    }

    protected virtual void Start()
    {
        if (_sensorColiiderTrigger != null)
        {
            AISensor script = _sensorColiiderTrigger.GetComponent<AISensor>();
            if (script!=null)
            {
                script.parentStateMachine = this;
            }
        }

        AIState[] states = GetComponents<AIState>(); //我们在怪物本身上挂载,多个【AIState】来分别控制。

        foreach (AIState state in states)
        {
            if (state != null && !_states.ContainsKey(state.GetStateType()))
            {
                _states.Add(state.GetStateType(), state);
                state.SetAIStateMachine(this);
            }
        }
        if (_states.ContainsKey(_currentStateType))
        {
            _currentState = _states[_currentStateType];
            _currentState.OnEnterState();
        }
        else
        {
            _currentState = null;
        }
    }

    protected virtual void Update()
    {
        if (_currentState == null)
            return;
        AIStateType newAIStateType = _currentState.OnUpdate();
        if (newAIStateType != _currentStateType)
        {
            AIState newState = null;
            if (_states.TryGetValue(newAIStateType,out newState))
            {
                _currentState.OnExitState();//当前状态退出
                newState.OnEnterState();    //new状态进入
                _currentState = newState;   //当前状态更新
            }
            //增加一层,检验层。状态不见,idle接上.
            else if (_states.TryGetValue(AIStateType.Idle, out newState))
            {
                _currentState.OnExitState();
                newState.OnEnterState();    
                _currentState = newState;   
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        VisualThreat.Clear();
        AudioThreat.Clear();

        if (_target.Type != AITargetType.None)
        {
            _target.Distance = Vector3.Distance(transform.position, _target.Position);
        }
    }
    
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d)
    {
        _target.Set(t, c, p, d);

        if (_targetColiiderTrigger != null)
        {
            _targetColiiderTrigger.radius = _stoppingDistance;
            _targetColiiderTrigger.transform.position = _target.Position;
            _targetColiiderTrigger.enabled = true;
        }
    }

    public void SetTarget(AITarget t)
    {
        _target = t;

        if (_targetColiiderTrigger != null)
        {
            _targetColiiderTrigger.radius = _stoppingDistance;
            _targetColiiderTrigger.transform.position = _target.Position;
            _targetColiiderTrigger.enabled = true;
        }
    }

    public void ClearTarget()
    {
        _target.Clear();
        if (_targetColiiderTrigger != null)
        {
            _targetColiiderTrigger.enabled = false;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (_targetColiiderTrigger == null || _targetColiiderTrigger != other) return;

        if (_currentState == null)
        {
            _currentState.OnDestinationReached(true);//到达目的地
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (_targetColiiderTrigger == null || _targetColiiderTrigger != other) return;

        if (_currentState != null)
        {
            _currentState.OnDestinationReached(false);//
        }
    }



    protected virtual void OnTriggerEvent(AITriggerEventType type,Collider other)
    {
        if (_currentState!=null) //当前,有状态。
        {
            _currentState.OnTriggerEvent(type, other);
        }
    }

    protected virtual void OnAnimatorMove()
    {
        if (_currentState != null) //当前,有状态。
        {
            _currentState.OnAnimatorUpdated();
        }
    }

    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (_currentState != null) //当前,有状态。
        {
            _currentState.OnAnimatorIKUpdated();
        }
    }

    protected virtual void NavAgentControl(bool positionUpdate,bool rotationUpdate)
    {
        if (_agent != null) //当前,有状态。
        {
            _agent.updatePosition = positionUpdate;
            _agent.updateRotation = rotationUpdate;
        }
    }
}
