using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD,PATROL,CHASE,DEAD}//敌人状态枚举
[RequireComponent(typeof(NavMeshAgent))]//约束条件：代表挂载该脚本的物价需要某个类型的组件（若该无组件会自动添加）
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{
    private EnemyStates enemyStates;
    private NavMeshAgent agent;
    [Header("Basic settings")]//区分
    public float sightRadius;//可视范围
    protected GameObject attackTarget;//攻击目标
    private float speed;//记录敌人原来的速度
    public bool isGuard;//判断是否处于GUARD状态
    public float lookAtTime;
    private float remainLookAtTime;
    private float lastAttackTime;
    private Quaternion guardRotation;//记录敌人原始旋转角度
    private Collider coll;

    [Header("Patrol State")]
    public float patrolRange;
    protected CharacterStats characterStats;
    

    private Animator anim;
    //bool配合动画
    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;
    bool playerDead;
    private Vector3 wayPoint;//巡逻点
    private Vector3 guardPos;//敌人初始位置
    
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();
        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
        
    }
    private void Start()
    {
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();//让敌人获取初始移动的点
        }
        GameManager.Instance.AddObserver(this);
    }
    private void Update()
    {
        if (characterStats.CurrentHealth==0)
        {
            isDead = true;
        }
        if (!playerDead)
        {
            SwitchStates();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
        
    }
    void SwitchAnimation() {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Death", isDead);
        anim.SetBool("Critical", characterStats.isCritical);
    }
    void SwitchStates() {

        if (isDead)
        {
            enemyStates = EnemyStates.DEAD;
        }
        //如果发现player 切换成CHASE追击状态
        else if (FoundPlayer())
        {
            enemyStates = EnemyStates.CHASE;
           
        }

        switch (enemyStates) {
            case EnemyStates.GUARD:
                isChase = false;
                if (transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;
                    if (Vector3.SqrMagnitude(guardPos-transform.position)<=agent.stoppingDistance)
                    {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.01f);
                    }
                }
                break;
            case EnemyStates.PATROL:
                isChase = false;
                agent.speed = speed * 0.5f;//c#用乘法的开销比除法小尽量用乘法
                //判断是否到达巡逻点
                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else {
                        GetNewWayPoint();
                    }
                    
                }
                else {
                    isWalk = true;
                    agent.destination = wayPoint;
                }
                break;
            case EnemyStates.CHASE:
                //追击player
                
                //在攻击范围内则攻击目标
                //配合动画
                isWalk = false;
                isChase = true;

                agent.speed = speed;
                if (!FoundPlayer())
                {
                    //拉脱回到上一个状态
                    isFollow = false;
                    if (remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else if (isGuard)
                    {
                        enemyStates = EnemyStates.GUARD;
                    }
                    else {
                        enemyStates = EnemyStates.PATROL;
                    }
                    
                }
                else {
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }
                //攻击范围检测
                if (TargetInAttackRange() || TargetInSkillRange()) {
                    isFollow = false;
                    agent.isStopped = true;
                    if (lastAttackTime<0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;
                        //暴击判断
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                        //执行攻击
                        Attack();
                    }
                }
                break;
            case EnemyStates.DEAD:
                coll.enabled = false;
                //agent.enabled = false;
                agent.radius = 0;
                Destroy(gameObject, 2f);
                break;
        }
    }
    void Attack() {
        transform.LookAt(attackTarget.transform);
        if (TargetInAttackRange())
        {
            //近身攻击动画
            anim.SetTrigger("Attack");
        }
        if (TargetInSkillRange()) {
            //技能攻击动画
            anim.SetTrigger("Skill");
        }
    }
    private void OnEnable()
    {
        //GameManager.Instance.AddObserver(this);
    }
    private void OnDisable()
    {
        if (!GameManager.IsInitialized)
        {
            return;
        }
        GameManager.Instance.RemoveObserver(this);
    }
    private bool FoundPlayer() {
        var coliders = Physics.OverlapSphere(transform.position, sightRadius);//以敌人的中心点为圆心，在设好的半径内查找碰撞体，并把碰撞体添加到数组中
        foreach (var target in coliders) {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }
    bool TargetInAttackRange() {
        if (attackTarget != null)       
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;      
        else
            return false;
    }

    bool TargetInSkillRange() {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        else
            return false;
    }
    void GetNewWayPoint() {
        remainLookAtTime = lookAtTime;
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);
        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ?hit.position:transform.position;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }
    //Animation Event
    void Hit() {
        if (attackTarget!=null&&transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void EndNodify()
    {
        //获胜动画
        anim.SetBool("Win", true);
        //停止所有移动
        //停止Agent
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}
