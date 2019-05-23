using UnityEngine;
using System.Collections;

public enum DisPlayMode
{
    None,
    Connected,
    Paths
}

public class AINetWorkPoint : MonoBehaviour
{

    public Transform[] Points;
    [HideInInspector] public DisPlayMode display = DisPlayMode.Connected;
    [HideInInspector] public int startIndex = 0;
    [HideInInspector] public int endIndex = 1;
    void Start()
    {

    }

    void Update()
    {

    }
}
