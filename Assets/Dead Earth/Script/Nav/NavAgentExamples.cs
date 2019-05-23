using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NavAgentExamples))]
public class NavAgentExamples : MonoBehaviour
{
    private NavMeshAgent agent;
    public AINetWorkPoint aINetWork;

    [Header("是否有路径")]
    public bool hasPath = false;                        //是否有路径
    [Header("是否正在计算路径，但还没有准备好?")]
    public bool pathPending = false;                    //是否等待路径
    [Header("当前路径是否过时？")]
    public bool isPathStale = false;                    //是否路径有效？
    [Header("当前路径状态")]
    public NavMeshPathStatus pathStatus;                //路径状态
    public AnimationCurve Curve;                        //抛物线


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (aINetWork == null) return;
        SetNextDestination(false);

    }

    void Update()
    {
        //更新属性,方便查看
        hasPath = agent.hasPath;
        pathPending = agent.pathPending;
        isPathStale = agent.isPathStale;
        pathStatus = agent.pathStatus;

        //如果代理在一个离线链接上，那么执行一个跳转
        if (agent.isOnOffMeshLink)
        {
            //Jump(1);
            StartCoroutine(Jump(1));
            return;
        }

        //【1】(是否到达目标点+下一个目标点是否计算好了)    ||   【2】当前路径目标点是否无效
        if ((agent.remainingDistance <= agent.stoppingDistance && !pathPending) || pathStatus == NavMeshPathStatus.PathInvalid)
        {
            SetNextDestination(true);//进入下一个导航点
        }

 
    }



    public int Index = 0;
    public void SetNextDestination(bool increment)
    {
        if (!aINetWork) return;

        Transform NextPoint = null;

        NextPoint = aINetWork.Points[Index];
        Index++;
        Index %= aINetWork.Points.Length;
        agent.destination = NextPoint.position;
    }

    IEnumerator Jump(float duration)
    {
        //注意:  Nav来控制人物的跳跃。  关键点: OffMeshLink
        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 start = transform.position;
        Vector3 end = data.endPos + (agent.baseOffset * Vector3.up);
        float timer = 0;

        while (timer <= duration)
        {
            float t = timer / duration;
            agent.transform.position = Vector3.Lerp(start, end, t) + Vector3.up * Curve.Evaluate(t);//求曲线的时间值。
            timer += Time.deltaTime;
            yield return null;
        }

        agent.CompleteOffMeshLink();//  完成 断裂网格连接
    }

}
