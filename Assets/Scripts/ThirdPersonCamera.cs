using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public GameObject Target;
    public float TrailDistance;
    public float YDistance;
    public float SmoothSpeed = 0.125f;
    public float RotationSpeed = 5f;

    private void LateUpdate()
    { 
        Vector3 desiredPosition = Target.transform.position - Target.transform.forward * TrailDistance;
        desiredPosition.y = desiredPosition.y + YDistance;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;

        Quaternion targetRotation = Quaternion.LookRotation(Target.transform.forward, Target.transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
    }

    public void CrashPlane()
    {
        StartCoroutine(crashPlane());
    }

    //Moves the camera out after crash for effect
    IEnumerator crashPlane()
    {
        float startFOV = Camera.main.fieldOfView;
        float targetFOV = 80;
        float moveDistance = 2f;
        Vector3 moveVector = -transform.forward * moveDistance;
        Vector3 targetPos= transform.position + moveVector;
        Vector3 startPos = transform.position;
        float time = 0;
        float totalTime = 3;
        while (time < totalTime)
        {
            time += Time.deltaTime;
            float progress = time / totalTime;
            transform.position = Vector3.Lerp(startPos, targetPos, progress);
            Camera.main.fieldOfView = Mathf.Lerp(startFOV, targetFOV, progress);
            yield return null;
        }
        yield return null;
    }
}
