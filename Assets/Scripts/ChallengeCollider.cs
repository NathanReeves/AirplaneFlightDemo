using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChallengeCollider : MonoBehaviour
{
    public Action<bool> ColliderCallback;

    private void OnTriggerEnter(Collider other)
    {
        ColliderCallback?.Invoke(true);
    }

    private void OnTriggerExit(Collider other)
    {
        ColliderCallback?.Invoke(false);
    }
}
