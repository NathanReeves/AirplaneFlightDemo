using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airplane : MonoBehaviour
{
    public PlaneState CurrentPlaneState = PlaneState.Flying;

    public List<ParticleSystem> JetEngines;
    public List<GameObject> MissileOrigins;
    public Projectile MissilePrefab;
    public GameObject ExplosionEffectPrefab;

    public float MissileSpeed;

    public Action PlaneCrashed;
    public bool IsAIPlane;

    List<GameObject> planeParts;
    Rigidbody planeRigidbody;

    const float jetIdleSpeed = 10f;
    const float jetThrustSpeed = 20f;
    const float jetBoostSpeed = 60f;
    const float jetEffectSmoothSpeed = 30f;

    const float jetIdlePitch = 1f;
    const float jetThrustPitch = 1.5f;
    const float jetBoostPitch = 2f;

    float currentJetPitch = 1f;
    float finalJetSpeed;
    float finalJetPitch;

    JetState currentJetState = JetState.Idle;

    void Start()
    {
        planeRigidbody = GetComponent<Rigidbody>();
        planeParts = new List<GameObject>();
        for(int i = 0; i < transform.childCount; i++)
        {
            planeParts.Add(transform.GetChild(i).gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(CurrentPlaneState == PlaneState.Flying && collision.gameObject.tag != AirplaneController.PlayerProjectileTag)
        {
            Debug.Log($"{gameObject.name} Crashed into {collision.gameObject.name}");
            CrashPlane();
        }
    }

    public void CrashPlane()
    {
        foreach(GameObject part in planeParts)
        {
            Rigidbody rb = part.AddComponent<Rigidbody>();
            rb.angularDrag = .25f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        CurrentPlaneState = PlaneState.Crashed;
        DisableJets();
        SoundManager.Instance.PlaySound(SoundType.PlaneCrash);
        PlaneCrashed?.Invoke();
        GameObject explosion = Instantiate(ExplosionEffectPrefab);
        explosion.transform.position = transform.position;

    }

    public void PlaneMotion(Vector3 thrust, Vector3 rotateTorque)
    {
        if (CurrentPlaneState == PlaneState.Crashed)
            return;
        planeRigidbody.AddForce(thrust * Time.deltaTime);
        planeRigidbody.AddTorque(rotateTorque * Time.deltaTime);
    }

    public void PlaneMotionPose(Pose pose)
    {
        if (CurrentPlaneState == PlaneState.Crashed)
            return;
        planeRigidbody.MovePosition(pose.position);
        planeRigidbody.MoveRotation(pose.rotation);
    }

    public void FireMissile()
    {
        foreach(GameObject originPoint in MissileOrigins)
        {
            Projectile missile = Instantiate(MissilePrefab);
            missile.transform.position = originPoint.transform.position;
            missile.Rigidbody.MovePosition(originPoint.transform.position);
            missile.transform.forward = originPoint.transform.forward;
            missile.Rigidbody.velocity = missile.transform.forward * MissileSpeed;
            missile.TrailRenderer.emitting = true;
        }
        SoundManager.Instance.PlaySound(SoundType.Missile);
    }

    void Update()
    {
        updateJetState();
        if(currentJetState != JetState.Off)
        {
            updateJetVisual();
            updateJetSound();
        }
    }

    void updateJetVisual()
    {
        foreach (var engine in JetEngines)
        {
            var particles = engine.main;
            particles.startSpeed = new ParticleSystem.MinMaxCurve(Mathf.MoveTowards(particles.startSpeed.constant, finalJetSpeed, jetEffectSmoothSpeed * Time.deltaTime));
        }
    }

    void updateJetSound()
    {
        if (IsAIPlane)
            return;
        float smoothPitch = Mathf.MoveTowards(currentJetPitch, finalJetPitch, jetEffectSmoothSpeed * Time.deltaTime);
        SoundManager.Instance.AdjustSoundPitch(SoundType.Engine, smoothPitch);
        currentJetPitch = smoothPitch;
    }

    void updateJetState()
    {
        switch (currentJetState)
        {
            case JetState.Off:
                finalJetSpeed = 0;
                finalJetPitch = 0;
                break;
            case JetState.Idle:
                finalJetSpeed = jetIdleSpeed;
                finalJetPitch = jetIdlePitch;
                break;
            case JetState.Thrust:
                finalJetSpeed = jetThrustSpeed;
                finalJetPitch = jetThrustPitch;
                break;
            case JetState.Boost:
                finalJetSpeed = jetBoostSpeed;
                finalJetPitch = jetBoostPitch;
                break;
        }
    }

    public void SetJetState(JetState jetLevel)
    {
        currentJetState = jetLevel;
    }

    public void DisableJets()
    {
        finalJetSpeed = 0;
        finalJetPitch = 0;
        foreach (var engine in JetEngines)
        {
            engine.gameObject.SetActive(false);
        }
        if(!IsAIPlane)
            SoundManager.Instance.StopSound(SoundType.Engine);
    }
}
