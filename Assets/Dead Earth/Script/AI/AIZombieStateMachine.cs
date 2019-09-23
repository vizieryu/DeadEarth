using UnityEngine;
using System.Collections;

public class AIZombieStateMachine : AIStateMachine {

    //Inspector Assigned
    [SerializeField] [Range(0f, 360f)] float _fov = 50;       //视野夹角
    [SerializeField] [Range(0f, 1f)] float _hearing = 1;      //听觉
    [SerializeField] [Range(0f, 1f)] float _sight = 1;        //视觉
    [SerializeField] [Range(0.0f, 1.0f)] float _aggression = 0.5f;//侵略
    [SerializeField] [Range(0f, 100f)] int _health = 1;       //血量
    [SerializeField] [Range(0f, 1f)] float _intelligence = 1;       //智力
    [SerializeField] [Range(0f, 1f)] float _satisfaction = 1;       //饥饿值(满足感)
    [SerializeField] [Range(0f, 1f)] float _replenishRate = 0.5f;       //补充率
    [SerializeField] [Range(0f, 1f)] float _depletionRate = 0.1f;       //消耗率
       
    // Private
    private int _seeking = 0;
    private bool _feeding = false;
    private bool _crawling = false;
    private int _attackType = 0;
    private float _speed = 0.0f;

    // Hashes
    private int _speedHash = Animator.StringToHash("speed");
    private int _seekingHash = Animator.StringToHash("seeking");
    private int _feedingHash = Animator.StringToHash("feeding");
    private int _attackHash = Animator.StringToHash("attack");


    // Public Properties
    public float replenishRate { get { return _replenishRate; } }
    public float fov { get { return _fov; } }
    public float hearing { get { return _hearing; } }
    public float sight { get { return _sight; } }
    public bool crawling { get { return _crawling; } }
    public float intelligence { get { return _intelligence; } }
    public float satisfaction { get { return _satisfaction; } set { _satisfaction = value; } }
    public float aggression { get { return _aggression; } set { _aggression = value; } }
    public int health { get { return _health; } set { _health = value; } }
    public int attackType { get { return _attackType; } set { _attackType = value; } }
    public bool feeding { get { return _feeding; } set { _feeding = value; } }
    public int seeking { get { return _seeking; } set { _seeking = value; } }
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }


    // ---------------------------------------------------------
    // Name	:	Update
    // Desc	:	用最新的值刷新动画器
    // ---------------------------------------------------------
    protected override void Update()
    {
        base.Update();

        if (_anim != null)
        {
            _anim.SetFloat(_speedHash, _speed);
            _anim.SetBool(_feedingHash, _feeding);
            _anim.SetInteger(_seekingHash, _seeking);
            _anim.SetInteger(_attackHash, _attackType);
        }

        _satisfaction = Mathf.Max(0, _satisfaction - ((_depletionRate * Time.deltaTime) / 100.0f) * Mathf.Pow(_speed, 3.0f));
    }



}
