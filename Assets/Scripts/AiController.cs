using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiController : MonoBehaviour
{
    public ThirdPersonCamera CameraFollow;
    public AirplaneController PlayerController;

    public Airplane Airplane;
    public Transform Target;
    public LayerMask ObstacleLayer;
    public bool EnableObstacleAvoidance = true;

    const float delay = 2.2f;
    const float obstacleAvoidDistance = 25f;
    float followSmoothSpeed = 60f;


    Queue<Pose> targetPositions = new Queue<Pose>();
    bool isFollowing = false;

    void Start()
    {
        Airplane.IsAIPlane = true;
        StartCoroutine(FollowTarget());
        PlayerController.Airplane.PlaneCrashed += playersPlaneCrashed;
        Airplane.PlaneCrashed += aiPlaneCrashed;
    }

    void Update()
    {
        if (isFollowing && Airplane.CurrentPlaneState != PlaneState.Crashed)
        {
            if (targetPositions.Count > 0)
            {
                Pose pose = targetPositions.Dequeue();
                Airplane.transform.position = Vector3.MoveTowards(Airplane.transform.position, pose.position, followSmoothSpeed * Time.deltaTime);
                Airplane.transform.rotation = Quaternion.RotateTowards(Airplane.transform.rotation, pose.rotation, followSmoothSpeed * Time.deltaTime);
            }
            else
            {
                Airplane.transform.position = Vector3.MoveTowards(Airplane.transform.position, Target.position, followSmoothSpeed * Time.deltaTime);
                Airplane.transform.rotation = Quaternion.RotateTowards(Airplane.transform.rotation, Target.rotation, followSmoothSpeed * Time.deltaTime);
            }
        }
        if(EnableObstacleAvoidance)
        {
            avoidObstacles();
        }
    }

    void avoidObstacles()
    {
        Collider[] hitColliders = Physics.OverlapSphere(Airplane.transform.position, obstacleAvoidDistance, ObstacleLayer);
        foreach (var hit in hitColliders)
        {
            float distance = Vector3.Distance(hit.transform.position, Airplane.transform.position);
            if(distance < obstacleAvoidDistance)
            {
                Vector3 difference = Airplane.transform.position - hit.transform.position;
                Vector3 targetPosition = difference.normalized * obstacleAvoidDistance + hit.transform.position;
                targetPosition += Airplane.transform.forward;
                Airplane.transform.position = Vector3.MoveTowards(Airplane.transform.position, targetPosition, 5f * Time.deltaTime);
            }
        }
    }

    void aiPlaneCrashed()
    {
        PlayerController.HasFirstCrashed = true;
        PlayerController.Airplane.PlaneCrashed -= playersPlaneCrashed;
        enabled = false;
    }

    void playersPlaneCrashed()
    {
        isFollowing = false;
        CameraFollow.Target = Airplane.gameObject;
        Airplane.PlaneCrashed = PlayerController.Airplane.PlaneCrashed;
        PlayerController.Airplane = Airplane;
        PlayerController.Airplane.IsAIPlane = false;
        PlayerController.Airplane.PlaneCrashed -= playersPlaneCrashed;
        SoundManager.Instance.PlaySound(SoundType.Engine, true);
        EnableObstacleAvoidance = false;
    }

    IEnumerator FollowTarget()
    {
        float startTime = Time.time;
        while(Time.time < startTime + delay)
        {
            targetPositions.Enqueue(new Pose(Target.position, Target.rotation));
            yield return null;
        }
        followSmoothSpeed = 150f;
        isFollowing = true;
        while (true)
        {
            targetPositions.Enqueue(new Pose(Target.position, Target.rotation)); // Store the current position of the target
            yield return null;
        }
    }
}
