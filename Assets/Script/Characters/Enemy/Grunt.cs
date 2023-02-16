using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Grunt : EnemyController
{
    [Header("Skill")]
    public float kickForce;//��

    public void KickOff() {
        if (attackTarget!=null)
        {
            transform.LookAt(attackTarget.transform);
            Vector3 direction = attackTarget.transform.position - transform.position;//���ɷ���
            direction.Normalize();//ʹ�������� magnitude Ϊ 1��

            attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
            attackTarget.GetComponent<NavMeshAgent>().velocity = direction * kickForce;//ʹ����Ŀ�����
            attackTarget.GetComponent<Animator>().SetTrigger("Dizzy");
        }
    }
}
