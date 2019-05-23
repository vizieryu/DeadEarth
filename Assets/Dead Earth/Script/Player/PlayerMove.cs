using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    public float speed = 2;

    private float x = 0, y = 0;

    private Animator anim;
    private int _horizontalHash = 0, _verticalHash = 0;
    private int _attackHash = 0;
    private int _ismoveHash = 0;

    void Start()
    {
        anim = GetComponent<Animator>();

        _horizontalHash = Animator.StringToHash("Horizontal");
        _verticalHash = Animator.StringToHash("Vertical");
        _attackHash = Animator.StringToHash("Attack");
        _ismoveHash = Animator.StringToHash("IsMove");
    }

    void Update()
    {

        

        #region isMove
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            anim.SetBool(_ismoveHash, true);
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            anim.SetBool(_ismoveHash, false);
            
        } 
        #endregion

        x = Input.GetAxis("Horizontal") *2.32f;
        y = Input.GetAxis("Vertical") * 5.66f * speed;

        anim.SetFloat(_horizontalHash, x, 0.1f, Time.deltaTime);
        anim.SetFloat(_verticalHash, y, 1, Time.deltaTime);

        if (Mathf.Abs(x) >= 0.01f ||Mathf.Abs(y) >= 0.01f)
        {
            
        }

        

        if (Input.GetKeyDown(KeyCode.J))
        {
            anim.SetTrigger(_attackHash);
        }
    }
}
