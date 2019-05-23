using UnityEngine;
using System.Collections;

public class AISensor : MonoBehaviour
{
    public AIStateMachine parentStateMachine;

    void OnTriggerEnter(Collider col)
    {
        if (parentStateMachine != null)
            //parentStateMachine.trig
            return ;
    }

    void OnTriggerStay(Collider col)
    {
        
    }

    void OnTriggerExit(Collider col)
    {
        
    }
}
