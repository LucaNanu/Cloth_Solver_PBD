using UnityEngine;

/// <summary>
/// Basic Position Based Dynamics solver for cloth.
///
/// Algorithm:
///
/// 1. Apply external forces
/// 2. Integrate predicted velocity
/// 3. Predict positions
/// 4. Iteratively solve constraints
/// 5. Update velocities from corrected positions
/// </summary>
public class PBDSolver
{
    public int iterations = 10;

    public Vector3 gravity =
        new Vector3(0.0f, -9.81f, 0.0f);

    public Vector3 externalAcceleration =
        Vector3.zero;

    private ClothSystem cloth;

    public PBDSolver(
        ClothSystem cloth,
        int iterations)
    {
        this.cloth = cloth;
        this.iterations = iterations;
    }

    public void Step(float dt)
    {
        if (cloth == null)
            return;

        if (dt <= 0.0f)
            return;

        // ========================================================
        // 1. FORCE / VELOCITY INTEGRATION
        // ========================================================

        foreach (ClothParticle particle
                 in cloth.particles)
        {
            if (particle.isFixed)
                continue;

            Vector3 acceleration =
                gravity +
                externalAcceleration;

            particle.velocity +=
                acceleration * dt;
        }

        // ========================================================
        // 2. PREDICT POSITIONS
        // ========================================================

        foreach (ClothParticle particle
                 in cloth.particles)
        {
            if (particle.isFixed)
                continue;

            particle.previousPosition =
                particle.position;

            particle.position +=
                particle.velocity * dt;
        }

        // ========================================================
        // 3. SOLVE CONSTRAINTS
        // ========================================================

        for (int iteration = 0;
             iteration < iterations;
             iteration++)
        {
            foreach (ClothConstraint constraint
                     in cloth.constraints)
            {
                constraint.Solve();
            }
        }

        // ========================================================
        // 4. UPDATE VELOCITIES
        // ========================================================

        foreach (ClothParticle particle
                 in cloth.particles)
        {
            if (particle.isFixed)
            {
                particle.velocity =
                    Vector3.zero;

                continue;
            }

            particle.velocity =
                (particle.position -
                 particle.previousPosition) / dt;
        }
    }
}