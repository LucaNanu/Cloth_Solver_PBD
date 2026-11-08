using UnityEngine;

/// <summary>
/// State of a single particle of the cloth.
/// </summary>
public class ClothParticle
{
    public Vector3 position;
    public Vector3 previousPosition;
    public Vector3 velocity;

    public float mass;
    public float inverseMass;

    public bool isFixed;

    public ClothParticle(
        Vector3 position,
        float mass,
        bool isFixed = false)
    {
        this.position = position;
        this.previousPosition = position;


        this.mass = mass;
        this.isFixed = isFixed;

        inverseMass = isFixed || mass <= 0.0f
            ? 0.0f
            : 1.0f / mass;
    }

    public void SetPosition(Vector3 newPosition)
    {
        position = newPosition;
        previousPosition = newPosition;
        velocity = Vector3.zero;
    }
}