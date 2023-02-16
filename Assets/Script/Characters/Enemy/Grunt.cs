using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Grunt : EnemyController
{
    [Header("Skill")]
    public float kickForce;//力

    public void KickOff() {
        if (attackTarget!=null)
        {
            transform.LookAt(attackTarget.transform);
            Vector3 direction = attackTarget.transform.position - transform.position;//击飞方向
            direction.Normalize();//使该向量的 magnitude 为 1。

            attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
            attackTarget.GetComponent<NavMeshAgent>().velocity = direction * kickForce;//使攻击目标击飞
            attackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
        }
    }
}
