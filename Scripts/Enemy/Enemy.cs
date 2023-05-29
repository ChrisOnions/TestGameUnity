using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;


public class Enemy : MonoBehaviour
{

  //[Header("Enemy Parameters")]
  [SerializeField] private int maxHealth = 100;
  //[SerializeField] private int enemyDammage = 10;
  [SerializeField] private int enemySpeed = 10;
  //[SerializeField] private int aggressionLevel = 1;
  [SerializeField] private float currentHealth;
  // private bool playerInRange = false;
  private bool isDead;

  [Header("Health Bar Parameters")]


  [Header("AI Parameters")]
  [SerializeField] private float distanceToPlayer = 40f;
  private NavMeshAgent navMeshAgent;
  private Transform player;
  private float friction = 100f;

  [Header("Group Parameters")]
  [SerializeField] private Enemy groupLeader;
  [SerializeField] private float patrolRange = 50f;
  [SerializeField] private float groupToFormDetectionRange = 10f;
  [SerializeField] private float patrolDelay = 5f;
  [SerializeField] private bool isGroupLeader;
  [SerializeField] private bool isGrouped;
  [SerializeField] private bool isGroupMember;
  private List<Enemy> groupMembers = new List<Enemy>();
  private List<Enemy> neighboringGroupMembers = new List<Enemy>();

  [Header("Group movement Parameters")]
  [SerializeField] private float updateInterval = .1f;

  [Header("Path Visualization")]
  [SerializeField] private LineRenderer lineRenderer;
  [SerializeField] private Color pathColor = Color.red;
  [SerializeField] private float pathWidth = 0.5f;

  [Header("Timers")]
  [SerializeField] float timeToGroup = 600f;
  [SerializeField] float groupFromTimer = 0f;

  [Header("Coroutines")]
  private Coroutine groupMovementCoroutine;
  private Coroutine soloPatrolCoroutine;

  private void Start()
  {
    currentHealth = maxHealth;
    player = GameObject.FindGameObjectWithTag("Player").transform;
    //Food = GameObject.FindGameObjectWithTag("Food").transform;
    navMeshAgent = GetComponent<NavMeshAgent>();
    navMeshAgent.isStopped = true;
    navMeshAgent.speed = enemySpeed;
    StartSoloPatrol();
  }

  private void Update()
  {

    if (isDead)
      return;

    if (player != null && Vector3.Distance(transform.position, player.position) <= distanceToPlayer)
      SetDestination(player.position);

    // if (groupFromTimer >= timeToGroup)
    // {
    //   FormGroup();
    //   groupFromTimer = 0;
    // }

    // if (isGrouped)
    // {
    //   groupMovementCoroutine = StartCoroutine(GroupMovementCoroutine());
    // }

    groupFromTimer += Time.deltaTime;
    GetComponent<Rigidbody>().AddForce(-GetComponent<Rigidbody>().velocity * friction * Time.fixedDeltaTime);
    DrawPath();
  }

  // private void FormGroup()
  // {
  //   Collider[] colliders = Physics.OverlapSphere(transform.position, groupToFormDetectionRange);

  //   foreach (Collider collider in colliders)
  //   {
  //     Enemy enemy = collider.GetComponent<Enemy>();
  //     if (enemy != null && enemy != this && !enemy.isGrouped)
  //     {
  //       if (Random.value <= 0.99f)
  //       {
  //         enemy.JoinGroup(this);
  //         groupMembers.Add(enemy);
  //       }
  //     }
  //   }

  //   if (groupMembers.Count > 0)
  //   {
  //     groupLeader = this;
  //     isGrouped = true;
  //     foreach (Enemy member in groupMembers)
  //     {
  //       member.isGrouped = true;
  //       member.groupLeader = this;
  //     }
  //   }
  // }

  // public void JoinGroup(Enemy leader)
  // {
  //   groupLeader = leader;
  //   isGrouped = true;
  // }

  private Vector3 GetRandomPatrolDestination()
  {
    return transform.position + Random.insideUnitSphere * patrolRange;
  }

  public void TakeDamage(float damageAmount)
  {
    if (isDead)
      return;
    currentHealth -= damageAmount;
    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    if (currentHealth <= 0)
      Die();
  }

  private void Die()
  {
    isDead = true;
    // if (isGrouped)
    // {
    //   groupLeader.RemoveFromGroup(this);
    // }
    isDead = true;
    Destroy(gameObject);
  }

  // private void RemoveFromGroup(Enemy member)
  // {
  //   groupMembers.Remove(member);
  //   if (groupMembers.Count == 0)
  //   {
  //     isGrouped = false;
  //   }
  // }


  private void DrawPath()
  {
    if (navMeshAgent.path.corners.Length < 2)
    {
      lineRenderer.positionCount = 0;
      return;
    }

    lineRenderer.positionCount = navMeshAgent.path.corners.Length;
    lineRenderer.SetPosition(0, transform.position);

    for (int i = 1; i < navMeshAgent.path.corners.Length; i++)
    {
      Vector3 pointPosition = navMeshAgent.path.corners[i];
      lineRenderer.SetPosition(i, pointPosition);
    }

    lineRenderer.startWidth = pathWidth;
    lineRenderer.endWidth = pathWidth;
    lineRenderer.material.color = pathColor;
  }

  private IEnumerator PatrolCoroutine()
  {
    while (!isGrouped)
    {
      if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
      {
        navMeshAgent.stoppingDistance = 0f;
        yield return new WaitForSeconds(patrolDelay);
        SetDestination(GetRandomPatrolDestination());
      }
      yield return null;
    }
  }

  private void StartSoloPatrol()
  {
    if (soloPatrolCoroutine == null)
    {
      soloPatrolCoroutine = StartCoroutine(PatrolCoroutine());
    }
  }

  private void StopSoloPatrol()
  {
    if (soloPatrolCoroutine != null)
    {
      StopCoroutine(soloPatrolCoroutine);
      soloPatrolCoroutine = null;
    }
  }

  // private IEnumerator GroupMovementCoroutine()
  // {
  //   if (isGroupLeader)
  //   {

  //   }
  //   else
  //   {
  //     print("poop");
  //   }
  //   yield return new WaitForSeconds(updateInterval);
  // }

  private void SetDestination(Vector3 destination)
  {
    navMeshAgent.SetDestination(destination);
  }
}
