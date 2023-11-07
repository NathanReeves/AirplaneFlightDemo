using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Rigidbody Rigidbody;
    public TrailRenderer TrailRenderer;

    public Action<string> HitCallback;

    float startTime;
    float lifetime = 2f;

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time > startTime + lifetime)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        HitCallback?.Invoke(collision.gameObject.tag);
        if (collision.gameObject.tag != AirplaneController.PlayerTag && collision.gameObject.tag != AirplaneController.PlayerProjectileTag)
        {
            Destroy(gameObject);
        }
    }
}
