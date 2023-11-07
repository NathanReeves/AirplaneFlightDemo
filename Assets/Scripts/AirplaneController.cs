using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirplaneController : MonoBehaviour
{
    public static string PlayerTag = "Player";
    public static string PlayerProjectileTag = "PlayerProjectile";
    public static string AsteroidTag = "Asteroid";

    public JoystickController Joystick;
    public ThirdPersonCamera CameraFollower;
    public ChallengeSystem ChallengeSystem;
    public UIController UIController;

    public Airplane Airplane;

    public List<ParticleSystem> JetEngines;
    public List<GameObject> MissileOrigins;

    public float ForwardForce;
    public float PitchSpeed;
    public float RollSpeed;
    public float TapThrustMultiplier;
    public float BoostThrustMultiplier;
    public bool InvertControls;

    public Projectile MissilePrefab;

    Coroutine barrelRollCheck;

    Vector3 finalThrust;
    Vector3 finalRotateTorque;
    public bool HasFirstCrashed = false;

    void Start()
    {
        Airplane.PlaneCrashed += onPlaneCrashed;
    }

    void Update()
    {
        handleInput();
    }

    void FixedUpdate()
    {
        Airplane.PlaneMotion(finalThrust, finalRotateTorque);
    }

    void onPlaneCrashed()
    {
        if(HasFirstCrashed)
        {
            UIController.OnPlayerCrashed();
            if (barrelRollCheck != null)
            {
                StopCoroutine(barrelRollCheck);
            }
            CameraFollower.enabled = false;
            CameraFollower.CrashPlane();
        }
        else
        {
            UIController.DisableBackupCam();
            HasFirstCrashed = true;
        }
    }

    void handleInput()
    {
        float pitch = Joystick.InputDirection.y;
        float roll = Joystick.InputDirection.x;
        Vector3 pitchAxis = Airplane.transform.right;
        Vector3 rollAxis = Airplane.transform.forward;
        Vector3 rotateTorque = pitchAxis * pitch * PitchSpeed + rollAxis * roll * RollSpeed;

        finalRotateTorque = rotateTorque * (InvertControls ? 1 : -1);

        bool isThrusting = Input.GetMouseButton(0);
        bool isBoosting = Input.GetKey(KeyCode.B);
        float tapThrust = isThrusting ? TapThrustMultiplier : 1f;
        float boostThrust = isThrusting && isBoosting ? BoostThrustMultiplier : 1f;

        finalThrust = Airplane.transform.forward * ForwardForce * tapThrust * boostThrust;

        if(isBoosting)
        {
            Airplane.SetJetState(JetState.Boost);
        }
        else if(isThrusting)
        {
            Airplane.SetJetState(JetState.Thrust);
        }
        else
        {
            Airplane.SetJetState(JetState.Idle);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if(barrelRollCheck != null)
            {
                StopCoroutine(barrelRollCheck);
            }
            barrelRollCheck = StartCoroutine(barrelRollChecker());
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Airplane.FireMissile();
        }
    }

    IEnumerator barrelRollChecker()
    {
        float timeout = 2.5f;
        float time = 0;
        float totalZ = 0;
        Vector3 previousRight = Airplane.transform.right;
        while(time < timeout)
        {
            time += Time.deltaTime;
            totalZ += Mathf.Abs(Vector3.Angle(previousRight, Airplane.transform.right));
            previousRight = Airplane.transform.right;
            if (totalZ >= 320)
            {
                ChallengeSystem.PlayerDidBarrellRoll();
                yield break;
            }
            yield return null;
        }
        if(totalZ >= 300)
        {
            ChallengeSystem.PlayerDidBarrellRoll();
        }
    }
}
