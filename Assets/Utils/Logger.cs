using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public static void Log(string msg)
    {
        if (Application.isEditor)
        {
            Debug.Log(msg);
        } else
        {
            Debug.LogError(msg);
        }
    }


}
