using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    //public Transform lookPoint;
    //private Vector3 offset;
    public Transform target;
    public float speed = 3;

    void Start()
    {
        //offset = transform.position - lookPoint.position;
    }

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * speed);
        Quaternion tar = Quaternion.Slerp(transform.rotation, target.rotation,Time.deltaTime * speed);
        transform.rotation = tar;
    }
}
