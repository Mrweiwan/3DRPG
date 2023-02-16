using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD,PATROL,CHASE,DEAD}//����״̬ö��
[RequireComponent(typeof(NavMeshAgent))]//Լ��������������ظýű��������Ҫĳ�����͵������������������Զ���ӣ�
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{
    private EnemyStates enemyStates;
    private NavMeshAgent agent;
    [Header("Basic settings")]//����
    public float sightRadius;//���ӷ�Χ
    protected GameObject attackTarget;//����Ŀ��
    private float speed;//��¼����ԭ�����ٶ�
    public bool isGuard;//�ж��Ƿ���GUARD״̬
    public float lookAtTime;
    private float remainLookAtTime;
    private float lastAttackTime;
    private Quaternion guardRotation;//��¼����ԭʼ��ת�Ƕ�
    private Collider coll;

    [Header("Patrol State")]
    public float patrolRange;
    protected CharacterStats characterStats;
    

    private Animator anim;
    //bool��϶���
    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;
    bool playerDead;
    private Vector3 wayPoint;//Ѳ�ߵ�
    private Vector3 guardPos;//���˳�ʼλ��
    
    
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
            GetNewWayPoint();//�õ��˻�ȡ��ʼ�ƶ��ĵ�
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
        //�������player �л���CHASE׷��״̬
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
                agent.speed = speed * 0.5f;//c#�ó˷��Ŀ����ȳ���С�����ó˷�
                //�ж��Ƿ񵽴�Ѳ�ߵ�
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
                //׷��player
                
                //�ڹ�����Χ���򹥻�Ŀ��
                //��϶���
                isWalk = false;
                isChase = true;

                agent.speed = speed;
                if (!FoundPlayer())
                {
                    //���ѻص���һ��״̬
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
                //������Χ���
                if (TargetInAttackRange() || TargetInSkillRange()) {
                    isFollow = false;
                    agent.isStopped = true;
                    if (lastAttackTime<0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;
                        //�����ж�
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                        //ִ�й���
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
            //����������
            anim.SetTrigger("Attack");
        }
        if (TargetInSkillRange()) {
            //���ܹ�������
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
        var coliders = Physics.OverlapSphere(transform.position, sightRadius);//�Ե��˵����ĵ�ΪԲ�ģ�����õİ뾶�ڲ�����ײ�壬������ײ����ӵ�������
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
        //��ʤ����
        anim.SetBool("Win", true);
        //ֹͣ�����ƶ�
        //ֹͣAgent
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}
