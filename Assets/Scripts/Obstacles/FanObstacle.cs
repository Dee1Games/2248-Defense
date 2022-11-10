using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanObstacle : MonoBehaviour
{
    public float fanSpeed = 300;

    void Update()
    {
        transform.eulerAngles += new Vector3(0, Time.deltaTime * fanSpeed, 0);
    }
}
