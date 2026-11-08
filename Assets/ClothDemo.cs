using UnityEngine;

/// <summary>
/// Main controller for the cloth PBD simulation.
/// </summary>
[RequireComponent(typeof(ClothSystem))]
public class ClothDemo : MonoBehaviour
{
    [Header("Simulation")]
    [Min(1)]
    public int solverIterations = 10;

    [Min(0.0001f)]
    public float fixedTimeStep = 0.01f;

    [Header("Forces")]
    public bool useGravity = true;

    public Vector3 externalAcceleration =
        Vector3.zero;

    [Header("Simulation control")]
    public bool simulate = true;

    private ClothSystem cloth;

    private PBDSolver solver;

    private float accumulatedTime;

    private void Awake()
    {
        cloth =
            GetComponent<ClothSystem>();

        solver =
            new PBDSolver(
                cloth,
                solverIterations);

        solver.gravity =
            useGravity
                ? Physics.gravity
                : Vector3.zero;

        solver.externalAcceleration =
            externalAcceleration;
    }

    private void Update()
    {
        if (!simulate)
            return;

        // Accumulate real elapsed time.
        accumulatedTime +=
            Time.deltaTime;

        // Avoid an excessive number of
        // simulation steps after a pause.
        accumulatedTime =
            Mathf.Min(
                accumulatedTime,
                0.1f);

        while (accumulatedTime >=
               fixedTimeStep)
        {
            solver.gravity =
                useGravity
                    ? Physics.gravity
                    : Vector3.zero;

            solver.externalAcceleration =
                externalAcceleration;

            solver.Step(
                fixedTimeStep);

            accumulatedTime -=
                fixedTimeStep;
        }

        cloth.UpdateMesh();
    }

    // ============================================================
    // RESET
    // ============================================================

    public void ResetSimulation()
    {
        cloth.Initialize();

        accumulatedTime = 0.0f;
    }

    // ============================================================
    // GIZMOS
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        if (cloth == null ||
            cloth.particles == null)
            return;

        foreach (ClothParticle particle
                 in cloth.particles)
        {
            if (particle.isFixed)
            {
                Gizmos.DrawSphere(
                    particle.position,
                    0.04f);
            }
        }
    }
}