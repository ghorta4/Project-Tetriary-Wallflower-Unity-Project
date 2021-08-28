using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObligatoryMonobehavior : MonoBehaviour
{
    void Start()
    {
        SessionManager.Initialize();
        CommandLine.Initialze();
    }

    private void Update()
    {
        SessionManager.Update(Time.deltaTime);
        CommandLine.Update(Time.deltaTime);
    }

    private void OnGUI()
    {
        CommandLine.DrawCommandLine();
    }
}
