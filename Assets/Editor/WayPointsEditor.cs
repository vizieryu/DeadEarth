using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AINetWorkPoint))]
public class WayPointsEditor : Editor
{
    AINetWorkPoint aiWayPoint;

   void OnSceneGUI()
    {
        
        //1.【绘制文字】绘制路标 名称
        aiWayPoint = (AINetWorkPoint)target;  //获取到自定义脚本的引用!

        for (int i = 0; i < aiWayPoint.Points.Length; i++)
        {
            Handles.Label(aiWayPoint.Points[i].position, "point" + i);
        }

        //2.【绘制连接类型】处理多种路标连接类型
        switch (aiWayPoint.display)
        {
            case DisPlayMode.None:
                break;
            case DisPlayMode.Connected:
                ConnectedType();
                break;
            case DisPlayMode.Paths:
                PathsType();
                break;
        }
    }


    public override void OnInspectorGUI()
    {
        GUILayout.Label("\t\t林大侠自定义编辑器!");
        base.OnInspectorGUI();
        
        aiWayPoint.display = (DisPlayMode)EditorGUILayout.EnumPopup("DisPlay Mode", aiWayPoint.display);
        if (aiWayPoint.display == DisPlayMode.Paths)
        {
            aiWayPoint.startIndex = EditorGUILayout.IntSlider("Waypoint Start", aiWayPoint.startIndex, 0, aiWayPoint.Points.Length - 1);
            aiWayPoint.endIndex = EditorGUILayout.IntSlider("Waypoint End", aiWayPoint.endIndex, 0, aiWayPoint.Points.Length - 1);
        }
    }

    /// <summary>
    /// 连接线路类型
    /// </summary>
    void ConnectedType()
    {
        Vector3[] newPoints = new Vector3[aiWayPoint.Points.Length + 1];
        for (int i = 0; i < newPoints.Length; i++)
        {
            if (i >= aiWayPoint.Points.Length)
            {
                newPoints[i] = aiWayPoint.Points[0].position;
            }
            else
            {
                newPoints[i] = aiWayPoint.Points[i].position;
            }
        }
        Handles.color = Color.cyan;
        Handles.DrawPolyLine(newPoints);
    }

    void PathsType()
    {
        NavMeshPath navMeshPath = new NavMeshPath();
        Vector3 start = aiWayPoint.Points[aiWayPoint.startIndex].position;
        Vector3 end = aiWayPoint.Points[aiWayPoint.endIndex].position;

        NavMesh.CalculatePath(start, end, NavMesh.AllAreas, navMeshPath);//利用Unity自带的寻路组件 NavMesh 来计算路径

        Handles.color = Color.yellow;
        Handles.DrawPolyLine(navMeshPath.corners);//再利用Handles来画 拐角-Corners 
    }
}
