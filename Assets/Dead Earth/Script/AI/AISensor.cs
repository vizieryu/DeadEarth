using UnityEngine;
using System.Collections;

public class AISensor : MonoBehaviour
{
    private AIStateMachine parentStateMachine;

    //public
    public void SetAIStateMachine(AIStateMachine aIStateMachine)
    {
        parentStateMachine = aIStateMachine;
    }


    void OnTriggerEnter(Collider col)
    {
        if (parentStateMachine != null)
            parentStateMachine.OnTriggerEvent(AITriggerEventType.Enter,col);
    }

    void OnTriggerStay(Collider col)
    {
        if (parentStateMachine != null)
            parentStateMachine.OnTriggerEvent(AITriggerEventType.Stay, col);
    }

    void OnTriggerExit(Collider col)
    {
        if (parentStateMachine != null)
            parentStateMachine.OnTriggerEvent(AITriggerEventType.Exit, col);
    }
}
