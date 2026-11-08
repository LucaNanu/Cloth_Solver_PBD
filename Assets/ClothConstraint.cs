using UnityEngine;

/// <summary>
/// Distance constraint between two cloth particles.
///
/// C(x_i, x_j) = |x_i - x_j| - L0 = 0
/// </summary>
public class ClothConstraint
{
    public ClothParticle particleA;
    public ClothParticle particleB;

    /// <summary>
    /// Rest length of the constraint.
    /// </summary>
    public float restLength;

    public ClothConstraint(
        ClothParticle particleA,
        ClothParticle particleB)
    {
        this.particleA = particleA;
        this.particleB = particleB;

        restLength =
            Vector3.Distance(
                particleA.position,
                particleB.position);
    }

    /// <summary>
    /// Solve the distance constraint.
    /// </summary>
    public void Solve()
    {
        Vector3 delta =
            particleA.position -
            particleB.position;

        float distance = delta.magnitude;

        // Avoid division by zero.
        if (distance < 1e-8f)
            return;

        float wA = particleA.inverseMass;
        float wB = particleB.inverseMass;

        float wSum = wA + wB;

        // Both particles are fixed.
        if (wSum <= 0.0f)
            return;

        Vector3 direction = delta / distance;

        // Constraint:
        //
        // C = |xA - xB| - L0
        //
        float C = distance - restLength;

        // PBD position correction.
        //
        // Δλ = -C / (wA + wB)
        //
        float lambda = -C / wSum;

        Vector3 correction = lambda * direction;

        if (!particleA.isFixed)
        {
            particleA.position +=
                wA * correction;
        }

        if (!particleB.isFixed)
        {
            particleB.position -=
                wB * correction;
        }
    }
}