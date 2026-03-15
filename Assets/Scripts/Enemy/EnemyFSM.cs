using UnityEngine;

public enum EnemyState
{
    Idle,
    Patrol,
    Chase
}

public class EnemyFSM : MonoBehaviour
{
    public EnemyState currentState = EnemyState.Idle;
    private EnemyController controller;

    void Start()
    {
        controller = GetComponent<EnemyController>();
        EnterState(currentState);
    }

    void Update()
    {
        UpdateState(currentState);
    }

    void EnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:   controller.StartIdle();   break;
            case EnemyState.Patrol: controller.StartPatrol(); break;
            case EnemyState.Chase:  controller.StartChase();  break;
        }
    }

    void UpdateState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle:
                if (controller.CanSeePlayer())
                    ChangeState(EnemyState.Chase);
                else if (controller.IdleTimeOver())
                    ChangeState(EnemyState.Patrol);
                break;
            case EnemyState.Patrol:
                if (controller.CanSeePlayer())
                    ChangeState(EnemyState.Chase);
                else if (controller.PatrolDone())
                    ChangeState(EnemyState.Idle);
                break;
            case EnemyState.Chase:
                if (!controller.CanSeePlayer())
                    ChangeState(EnemyState.Patrol);
                break;
        }
    }

    public void ChangeState(EnemyState newState)
    {
        currentState = newState;
        EnterState(newState);
    }
}