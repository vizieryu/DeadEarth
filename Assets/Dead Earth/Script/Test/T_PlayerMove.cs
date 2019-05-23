using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
//MonoBehaviour
public class T_PlayerMove : NetworkBehaviour
{
    //public
    public float _moveSpeed = 2;

    //private
    private Rigidbody _rig;
    private float _x;
    private float _y;
    private float _g = 9.8f;

    void Awake()
    {
        _rig = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (isLocalPlayer == false) //不是当前客户端.直接return.
        {
            return;
        }

        _x = Input.GetAxis("Horizontal") * _moveSpeed * Time.fixedDeltaTime;
        _y = Input.GetAxis("Vertical") * _moveSpeed * Time.fixedDeltaTime;
        if (Mathf.Abs(_x) > 0.01f || Mathf.Abs(_y) > 0.01f)
        {
            transform.Translate(new Vector3(_x, 0, _y));
            //_rig.velocity = new Vector3(_x, 0, _y);  //会影响跳跃.
        }

        //_rig.position -= 1/2 * Vector3.up *_g * 
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _rig.AddForce(Vector3.up  * 200);
        }
    }

    //当第一次加载,PlyaerPref
    public override void OnStartLocalPlayer()
    {
        GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        base.OnStartLocalPlayer();
    }

}
