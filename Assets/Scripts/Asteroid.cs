using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Asteroid : MonoBehaviour
{
    public Asteroid AsteroidPrefab;

    Rigidbody rb;
    Vector3 orbitCenter;
    float distance;
    float randomXForce;
    float randomZForce;
    Vector3 randomDirection;

    public bool IsMainAsteroid = true;
    public float Speed = 1;
    public float BitCount = 6;

    public Action AsteroidDestroyed;

    public void Init(Vector3 startPosition, Vector3 worldCenter, Action asteroidDestroyed)
    {
        AsteroidDestroyed = asteroidDestroyed;
        rb = GetComponent<Rigidbody>();
        transform.position = startPosition;

        //Create an initial force along the XZ plane
        randomXForce = Random.Range(-30, 30);
        randomZForce = Random.Range(-30, 30);
        Vector3 initForce = new Vector3(randomXForce, 0, randomZForce);
        randomDirection = new Vector3(randomXForce, 0, randomZForce);
        rb.AddForce(initForce, ForceMode.VelocityChange);
        rb.AddTorque(new Vector3(randomXForce, Random.Range(-30, 30), randomZForce) / 10f, ForceMode.VelocityChange);
        orbitCenter = worldCenter;
        distance = Vector3.Distance(transform.position, orbitCenter);
        Speed *= Random.Range(.2f, 2f);
    }

    void FixedUpdate()
    {
        if (!IsMainAsteroid)
            return;

        Vector3 toOrbitCenter = transform.position - orbitCenter;
        Vector3 newAsteroidOffset = toOrbitCenter.normalized * distance;
        rb.MovePosition(orbitCenter + newAsteroidOffset);

        Vector3 orthogonalForceDirection = Vector3.Cross(toOrbitCenter, randomDirection).normalized;
        Vector3 addForce = orthogonalForceDirection;
        rb.velocity = addForce.normalized * Speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == AirplaneController.PlayerProjectileTag)
        {
            explodeAsteroid();
        }
    }

    void explodeAsteroid()
    {
        AsteroidDestroyed?.Invoke();
        if (IsMainAsteroid)
        {
            StartCoroutine(waitToSpawnAsteroidBits());
        }
        else
        {
            Destroy(gameObject);
        }
        SoundManager.Instance.PlaySound(SoundType.Asteroid);
    }

    IEnumerator waitToSpawnAsteroidBits()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < BitCount; i++)
        {
            Asteroid asteroidBit = Instantiate(AsteroidPrefab);
            asteroidBit.IsMainAsteroid = false;
            asteroidBit.transform.position = transform.position;
            asteroidBit.transform.localScale = transform.lossyScale * .3f;
            asteroidBit.Init(transform.position, orbitCenter, AsteroidDestroyed);
        }
        Destroy(gameObject);
    }
}
