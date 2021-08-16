using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObligatoryMonobehavior : MonoBehaviour
{
    void Start()
    {
        SessionManager.Initialize();
    }

    private void Update()
    {
        SessionManager.Update();
    }
}
