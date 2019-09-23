using UnityEngine;
using System.Collections;

public class PlayerNav : MonoBehaviour
{
    public AINetWorkPoint _aiWays;

    [Header("Nav 公开参数:")]
    public bool _havePath = false;              //是否有路径
    public bool _isPathStale = false;           //当前路径,是否陈旧(不通)
    public NavMeshPathStatus _status;           //路径状态

    private NavMeshAgent _agent;
    private Animator _anim;
    private float _smoothAngle = 0;
    [Header("是否要有混合动画:")]
    public bool _isMixed = true;

    #region 动画Hash
    private int _angleHash = 0;
    private int _speedHash = 0;
    #endregion

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponent<Animator>();

        _angleHash = Animator.StringToHash("angle");
        _speedHash = Animator.StringToHash("speed");

        _agent.updateRotation = false;
        //_agent.updatePosition = false;

    }
    
    void Update()
    {
        _isPathStale = _agent.isPathStale;
        _havePath = _agent.hasPath;
        _status = _agent.pathStatus;

        //注意这里是以[局部坐标轴]作为参考。
        //三角函数,去求出anim动画运动【angle】需要的值
        Vector3 localDir = transform.InverseTransformDirection(_agent.desiredVelocity);         //转化为局部坐标,才能去计算角度。
        //look at here!!!  sin,cos,tan 求的值都是比值(也就是弧度).根据1°= 180/Π.得知:arcsin,arccos,arctan [unity这里简写成 Asin、Acos、Atan] 是反切、反弦. 结果是: 1弧度 = Π/180;
        //sin30° = √3/2  是个比值.
        float angle = Mathf.Atan2(localDir.x , localDir.z) * Mathf.Rad2Deg;                     //得到角度。
        Debug.Log("期望度数：" + angle);
        _smoothAngle = Mathf.MoveTowardsAngle(_smoothAngle, angle, 80.0f * Time.deltaTime);     //水平线插值
        _anim.SetFloat(_angleHash, _smoothAngle);
        //anim动画运动【speed】需要的值
        float speed = localDir.z;

        //把[_speedHash]的值--》在0.1f时间内以Time.deltaTime为时间增量增加,目标值:speed。
        _anim.SetFloat(_speedHash, speed, 0.1f,Time.deltaTime);                                 //线性插值

        //混合模式  : 人物动画 带有停滞。
        //不混合模式: 脚本去覆盖动画，实现人物的旋转偏移。
        if (_agent.desiredVelocity.sqrMagnitude > Mathf.Epsilon)  //用来杜绝 desiredVelocity = 0 情况。 【Mathf.Epsilon 一个微小浮点值】
        {
            if (!_isMixed || _isMixed && Mathf.Abs(angle) < 80.0f && _anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion"))
            {
                //不混合模式
                Quaternion lookRotation = Quaternion.LookRotation(_agent.desiredVelocity, Vector3.up);//目标旋转值
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3.0f);
            }
        }
       

        if (_agent.remainingDistance <= _agent.stoppingDistance || _agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            SetNextDestintion();
        }
    }

    private void OnAnimatorMove()        //OnAnimatorMove这个是在Update()调用之后。
    {
        //混合模式
        //TODO 自定义【混合模式】 开关,在 左、右转向的时候,由anim控制了
        if (_isMixed && !_anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion"))
        {
            //由root根运动控制,anim的移动.           就可以实现:左、右挪步动画效果.
            transform.rotation = _anim.rootRotation; //覆盖掉,update中的调用。 (相当于update中[rotation]没赋值过。)
        }

        _agent.velocity = _anim.deltaPosition / Time.deltaTime;    // (速度)agent = (位置增量)路程/(时间增量)时间
    }

    [Header("目标点下标:")]
    public int index = 0;
    void SetNextDestintion()
    {
        if (_aiWays == null) { return; }

        _agent.SetDestination(_aiWays.Points[index].position);
        index++;
        index %= _aiWays.Points.Length;
    }

}
