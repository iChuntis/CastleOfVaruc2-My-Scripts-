using System;
using UnityEngine;

public class MeleeAttack : AttackingBase , IAttackController
{
    [SerializeField] private Transform enemyCheck;
    [SerializeField] private float enemyCheckDistance;
    [SerializeField] private LayerMask whatIsPlayer;
    private bool player = true;

    private void Awake(){
        Messenger.AddListener("PlayerLive", PlayerLive);
        Messenger.AddListener("PlayerDie", PlayerDie);
    }
    private void PlayerLive() => player = true;
    private void PlayerDie() => player = false;

    public void Attack()
    {
        var enemyDetected = Physics2D.CircleCast(enemyCheck.position, enemyCheckDistance, Vector2.zero, 0, whatIsPlayer);

        Debug.Log("HERE I AM ");

        if (enemyDetected)
        {
            Debug.Log("Attack touches player");
            //DAMAGE
            if (player)
                Messenger<float, Transform>.Broadcast("DamageToPlayer", damage, enemyController.aliveTr);

            //Messenger<float,Transform>.Broadcast(meleeDamage, enemyController.aliveTr.transform);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyCheck.position, enemyCheckDistance);
    }

}
