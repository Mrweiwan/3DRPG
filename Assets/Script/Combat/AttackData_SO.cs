using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Attack",menuName ="Attack/Attack Data")]
public class AttackData_SO : ScriptableObject
{
    public float attackRange;//��������

    public float skillRange;//Զ�̹�������

    public float coolDown;//cdʱ��

    public int minDamage;//��С������

    public int maxDamage;//��󹥻���

    public float criticalMultiplier;//�����ӳ�

    public float criticalChance;//������
}
