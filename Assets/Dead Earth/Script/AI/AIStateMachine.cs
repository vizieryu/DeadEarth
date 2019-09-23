using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum AIStateType
{
    None,           //无
    Idle,           //闲置
    Alerted,        //警觉
    Pursuit,        //追踪
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

    public AITargetType type { get { return _type; } }

    public Collider collider { get { return _collider; } }

    public Vector3 position { get { return _position; } }

    public float distance { set { _distance = value; } get { return _distance; } }

    public float times { get { return _time; } }

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


    //protected
    protected Dictionary<AIStateType, AIState> _states = new Dictionary<AIStateType, AIState>();
    protected AITarget _target = new AITarget();
    protected AIState _currentState;
    protected int _rootPositionRefCount = 0;
    protected int _rootRotationRefCount = 0;


    // Protected Inspector Assigned
    [SerializeField] protected AIStateType _currentStateType = AIStateType.Idle;
    [SerializeField] protected SphereCollider _targetColiiderTrigger = null;  //目标触发
    [SerializeField] protected SphereCollider _sensorColiiderTrigger = null;  //警觉触发
    [SerializeField] protected AINetWorkPoint _waypointNetwork = null;
    //[SerializeField] protected bool _randomPatrol = false;
    //[SerializeField] protected int _currentWayPoint = -1;
    [SerializeField] [Range(0, 15)] protected float _stoppingDistance = 1.0f;

    // Component Cache
    protected Animator _anim;
    protected NavMeshAgent _agent;
    protected Collider _collider;

    // Public Properties
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
    public float sensorRadius
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
    public bool useRootPosition { get { return _rootPositionRefCount > 0; } }
    public bool useRootRotation { get { return _rootRotationRefCount > 0; } }
    public AITargetType currentTargetType
    {
        get
        {
            return _target.type;
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
            //注册到 [游戏场景管理]
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
                script.SetAIStateMachine(this);
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

        // 获取从动画器派生的所有AIStateMachineLink行为，
        // 并将它们的状态机引用设置为该状态机.
        if (_anim)
        {
            AIStateMachineLink[] scripts = _anim.GetBehaviours<AIStateMachineLink>();
            foreach (AIStateMachineLink script in scripts)
            {
                script.stateMachine = this;
            }
        }
    }

    protected virtual void Update()
    {
        if (_currentState == null)
            return;
        AIStateType newAIStateType = _currentState.OnUpdate();
        //每次,更新了[AIState] 并初始化掉,起始方法
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

            _currentState = newState;
        }
    }

    protected virtual void FixedUpdate()
    {
        VisualThreat.Clear();
        AudioThreat.Clear();

        if (_target.type != AITargetType.None)
        {
            _target.distance = Vector3.Distance(transform.position, _target.position);
        }
    }

    int _currentIndex = 0;
    public Vector3 GetWayPointPosition(bool increment,bool isRandom = false)
    {
        Transform target;
        float distance = 0;

        //随机,巡逻点.
        if (isRandom)
        {
            int randIndex =  Random.Range(0, _waypointNetwork.Points.Length);
            target = _waypointNetwork.Points[randIndex];
            distance = Vector3.Distance(transform.position,target.position);

            SetTarget(AITargetType.Waypoint, null, target.position, distance);
            return target.position;
        }
        else
        {
            //有递增,index++;
            if (increment)
            {
                _currentIndex++;
            }
            _currentIndex %= _waypointNetwork.Points.Length;
            target = _waypointNetwork.Points[_currentIndex];
            distance = Vector3.Distance(transform.position, target.position);
        }

        SetTarget(AITargetType.Waypoint, null, target.position, distance);
        return target.position;
    }
    
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d)
    {
        _target.Set(t, c, p, d);

        if (_targetColiiderTrigger != null)
        {
            _targetColiiderTrigger.radius = _stoppingDistance;
            _targetColiiderTrigger.transform.position = _target.position;
            _targetColiiderTrigger.enabled = true;
        }
    }

    public void SetTarget(AITarget t)
    {
        _target = t;

        if (_targetColiiderTrigger != null)
        {
            _targetColiiderTrigger.radius = _stoppingDistance;
            _targetColiiderTrigger.transform.position = _target.position;
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

    public virtual void OnTriggerEvent(AITriggerEventType type,Collider other)
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

    public virtual void NavAgentControl(bool positionUpdate,bool rotationUpdate)
    {
        if (_agent != null) //当前,有状态。
        {
            _agent.updatePosition = positionUpdate;
            _agent.updateRotation = rotationUpdate;
        }
    }

    public void AddRootMotionRequest(int rootPosition, int rootRotation)
    {
        _rootPositionRefCount += rootPosition;
        _rootRotationRefCount += rootRotation;
    }
}
