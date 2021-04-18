using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackingBase : MonoBehaviour
{
    protected ScratchNew enemyController;

    [SerializeField] protected float damage;

    public void SetEnemyController(ScratchNew enemyController)
    {
        this.enemyController = enemyController;
    }

    public float Damage { get => damage; set => damage = value; }
}
