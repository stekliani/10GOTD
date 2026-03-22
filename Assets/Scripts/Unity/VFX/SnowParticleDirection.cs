using UnityEngine;

public class SnowParticleDirection : MonoBehaviour
{
    public ParticleSystem ps;

    Vector3 lastPos;


    void Start()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        Vector3 velocity = (transform.position - lastPos) / Time.deltaTime;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;

        vel.x = -velocity.x * 0.3f;
        vel.y = -velocity.y * 0.3f;

        lastPos = transform.position;
    }
}