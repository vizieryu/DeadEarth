using UnityEngine;
using System.Collections;

public enum DoorType { Open, Close, Animation }

public class AutoDoor : MonoBehaviour
{

    public float OpenDistance = 4;
    public float Duration = 2;
    public DoorType doorType = DoorType.Open;
    public bool isOpen = false;
    public AnimationCurve aniCurve;

    private Vector3 _startPos = Vector3.zero;
    private Vector3 _endPos = Vector3.zero;

    void Start()
    {
        _startPos = transform.position;
        _endPos = transform.position - Vector3.right * OpenDistance;
    }

    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.Space) && doorType != DoorType.Animation)
            {
                StartCoroutine("DoorState", doorType == DoorType.Open ? DoorType.Close : DoorType.Open);
            }

            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    isOpen = !isOpen;
            //    StartCoroutine(DoorState2(isOpen));
            //}
        }
    }

    #region 测试
    //IEnumerator DoorState2(bool open)
    //{
    //    float timer = 0;
    //    Vector3 ori = open ? _startPos : _endPos;
    //    Vector3 tar = open ? _endPos : _startPos;

    //    while (timer<= Duration)
    //    {
    //        float t = timer / Duration;
    //        transform.position = Vector3.Lerp(ori, tar, aniCurve.Evaluate(t));
    //        timer += Time.deltaTime;
    //        yield return null;
    //    }

    //} 
    #endregion

    IEnumerator DoorState(DoorType newTYPE)
    {
        doorType = DoorType.Animation;
        float timer = 0;
        //起始点 《====》 结束点
        Vector3 startPos = (newTYPE == DoorType.Open) ? _endPos : _startPos;
        Vector3 endPos = (newTYPE == DoorType.Open) ? _startPos : _endPos;

        while (timer <= Duration)
        {
            float t = timer / Duration;
            transform.position = Vector3.Lerp(startPos, endPos, aniCurve.Evaluate(t));
            timer += Time.deltaTime;
            yield return null;
        }

        doorType = newTYPE;
    }

}
