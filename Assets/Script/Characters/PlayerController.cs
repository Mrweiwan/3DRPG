using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    Animator animator;
    private GameObject attackTarget;//攻击对象
    private float lastAttackTime;//攻击CD（计时器）
    private CharacterStats characterStats;
    private bool isDead;
    public float force;
    private float stopDistance;//停止距离
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
        animator.SetFloat("Speed", agent.velocity.sqrMagnitude);//sqrMagnitude将Vector3转换成float
        animator.SetBool("Death", isDead);
    }
    public void MoveToTarget(Vector3 target) {
        StopAllCoroutines();//停止协程
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
    //协程
    IEnumerator MoveToAttackTarget() {
        agent.isStopped = false;
        agent.stoppingDistance = characterStats.attackData.attackRange;
        transform.LookAt(attackTarget.transform);
        //判断player与enemy的距离，并让player向enemy移动
        
        //修改攻击范围参数
        while (Vector3.Distance(attackTarget.transform.position,transform.position)>characterStats.attackData.attackRange) {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }
        agent.isStopped = true;
        //Attack
        if (lastAttackTime < 0) {
            animator.SetBool("Critical", characterStats.isCritical);
            animator.SetTrigger("Attack");
            //重置冷却时间
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
