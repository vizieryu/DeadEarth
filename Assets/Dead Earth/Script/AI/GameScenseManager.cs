using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameScenseManager : MonoBehaviour
{
    private static GameScenseManager _instance;
    public static GameScenseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (GameScenseManager)FindObjectOfType(typeof(GameScenseManager));
            }
            return _instance;
        }
    }

    //private
    private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>();



    void Start()
    {

    }

    void Update()
    {

    }

    //public 
    //通过key,注册AIStateMachine
    public void RegisterAiStateMachine(int key, AIStateMachine aiMachine)
    {
        if (!_stateMachines.ContainsKey(key))
        {
            _stateMachines.Add(key,aiMachine);
        }
    }

    //通过key,来得到AIStateMachine
    public AIStateMachine GetAiStateMachine(int key)
    {
        AIStateMachine _aIStateMachine = null;
        if (_stateMachines.ContainsKey(key))
        {
            _stateMachines.TryGetValue(key,out _aIStateMachine);
        }
        return _aIStateMachine;
    }
}
