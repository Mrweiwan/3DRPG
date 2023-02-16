using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    Animator animator;
    private GameObject attackTarget;//��������
    private float lastAttackTime;//����CD����ʱ����
    private CharacterStats characterStats;
    private bool isDead;
    public float force;
    private float stopDistance;//ֹͣ����
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();

        stopDistance = agent.stoppingDistance;
    }
    private void OnEnable()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RigisterPlayer(characterStats);
    }
    private void Start()
    {        
        
        SaveManager.Instance.LoadPlayerData();
    }
    private void OnDisable()
    {
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EventAttack;
    }


    private void Update()
    {
        isDead = characterStats.CurrentHealth == 0;
        if (isDead)
        {
            GameManager.Instance.NodifyObserver();
        }
        SwitchAnimation();
        lastAttackTime -= Time.deltaTime;
    }
    private void SwitchAnimation()
    {
        animator.SetFloat("Speed", agent.velocity.sqrMagnitude);//sqrMagnitude��Vector3ת����float
        animator.SetBool("Death", isDead);
    }
    public void MoveToTarget(Vector3 target) {
        StopAllCoroutines();//ֹͣЭ��
        if (isDead)return;
        agent.stoppingDistance = stopDistance;  
        agent.isStopped = false;
        agent.destination = target;
    }
    private void EventAttack(GameObject target)
    {
        if (isDead)return;
        
        if (target != null) {
            attackTarget = target;
            characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
        
    }
    //Э��
    IEnumerator MoveToAttackTarget() {
        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;
        transform.LookAt(attackTarget.transform);
        //�ж�player��enemy�ľ��룬����player��enemy�ƶ�
        
        //�޸Ĺ�����Χ����
        while (Vector3.Distance(attackTarget.transform.position,transform.position)>characterStats.attackData.attackRange) {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }
        agent.isStopped = true;
        //Attack
        if (lastAttackTime < 0) {
            animator.SetBool("Critical", characterStats.isCritical);
            animator.SetTrigger("Attack");
            //������ȴʱ��
            lastAttackTime = characterStats.attackData.coolDown;
        }
    }
    //Animation Event
    void Hit() {
        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>()&& attackTarget.GetComponent<Rock>().rockStates == Rock.RockStates.HitNothing)
            {
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * force, ForceMode.Impulse);
            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
        
    }
    
}
