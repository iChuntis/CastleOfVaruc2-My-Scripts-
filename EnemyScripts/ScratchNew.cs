using System;
using System.Collections;
using UnityEngine;
using Assets;
using UnityEditor;

public class ScratchNew : MonoBehaviour
{
    [SerializeField] private EnemyKind enemyType;
    public enum EnemyKind
    {
        Cat,
        Boar
    }

    public enum State
    {
        Moving,
        Aggro,
        Attack,
        Idle
    }

    [SerializeField] private Rigidbody2D aliveRb;

    [SerializeField] private MobsPrefs mobsPref;

    [SerializeField] private float chanceOfCoin;

    [SerializeField]
    private Transform
        wallCheck,
        groundCheck;

    [SerializeField]
    private LayerMask whatIsGround;

    [SerializeField]
    private float
        currentHealth,
        wallCheckDistance,
        groundCheckDistance,
        movementSpeed,
        distanceToAttack,
        attackCooldown;

    [SerializeField]
    public Animator aliveAnim;
    public float DistanceToAttack => distanceToAttack;
    public Transform aliveTr => aliveRb.transform;

    private float lastTime = -999f, lastShieldTime = -999f;

    private AttackingBase attackType;

    private LookingSide currentLooking;

    private Vector3 lastPlayerPosition;

    [SerializeField] private State currentState;

    [SerializeField] private bool shield;

    [DrawIf("shield",true)]
    public float ShieldTimePeriod;

    private float shieldPeriod;

    private enum LookingSide
    {
        RightSide,
        LeftSide
    }

    private void Awake()
    {
        //mobsPref = (MobsPrefs)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Enemies/TestWorking/Enemy/MobsParcticles.asset", typeof(MobsPrefs));
        Messenger<int>.AddListener("InitScene", InitScene);
    }

    private void InitScene(int i)
    {
        switch (enemyType)
        {
            case EnemyKind.Boar:
                IncreaseStats(1.5f * i, 0.5f * i);
                break;
            case EnemyKind.Cat:
                IncreaseStats(2f * i, 0.5f * i);
                break;
        }
    }

    private void IncreaseStats(float hp, float damage)
    {
        currentHealth += hp;

        attackType.Damage += damage;

    }


    private void OnEnable()
    {
        attackType = GetComponent<AttackingBase>();
        if (transform.rotation.eulerAngles.y == 0)
        {
            //Смотрит направо
            currentLooking = LookingSide.RightSide;
        }

        attackType.SetEnemyController(this);

        shieldPeriod = ShieldTimePeriod;
    }

    private void FixedUpdate()
    {
        if (currentState == State.Moving)
        {
            SimpleMovement();
        }
        else if (currentState == State.Aggro)
        {
            MoveToPlayer();
        }
        else if (currentState == State.Attack)
        {
            Attack();
        }
        else if (currentState == State.Idle)
        {
            IdleStay();
        }




        if (knockBack)
        {
            if(Time.time >= knockBack_BackTime + lastKnockBack)
            {
                knockBack = false;
                movementSpeed *= 2;
            }

        }
    }


    public void Animate(in string what)
    {
        aliveAnim.SetTrigger(what);
    }

    private void IdleStay()
    {
        var inDis = GetDistance();

        //Debug.Log("FROM IDLE DIS  " + inDis);

        if (inDis)
        {
            if (Time.time >= lastTime + attackCooldown)
            {
                lastTime = Time.time;
                Animate("AttackTr");
                currentState = State.Attack;
            }
        }
        else
        {
            //Debug.Log("FROM IDLE TO MOVE !!!! ");
            currentState = State.Aggro;
            //if (!aliveAnim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
               // Animate("MoveTr");
        }
    }

    private void Attack()
    {
        var inDis = GetDistance();

        Debug.Log(inDis + " WHILE ATTACK");
        
        if(inDis)
        {
            if (Time.time >= lastTime + attackCooldown)
            {
                lastTime = Time.time;
                Animate("AttackTr");
            }
            else
            {
                currentState = State.Idle;
                Animate("Idle");
            }

        }
    }


    //Если моб видит игрока то он должен идти к нему
    private void MoveToPlayer()
    {
        var isInDist = GetDistance();

        if (isInDist)
        {
            //если дистанция приемлимая , то он меняет стейт на аттаку .. кулдаун будет проверяться в другом месте
            if (Time.time >= lastTime + attackCooldown)
                currentState = State.Attack;
            else
            {
                Animate("Idle");
                currentState = State.Idle;
                
            }
        }

        if (!isInDist)
        {
              if (!aliveAnim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                Animate("MoveTr");
            aliveRb.velocity = new Vector2(0, aliveRb.velocity.y);
            var direction = (0.5f - (int)currentLooking) * 1;

            (bool, bool) wallGround = Check();

            //Debug.Log(wallGround + " WALL GROUND ");

            if (!wallGround.Item1 && wallGround.Item2 && !shieldActive && !knockBack)
            {
                if (currentLooking == LookingSide.RightSide)
                    //aliveRb.velocity = new Vector2(movementSpeed, aliveRb.velocity.y);
                    aliveRb.AddForce(new Vector2(movementSpeed * 2, 0));
                else
                    //aliveRb.velocity = new Vector2(movementSpeed * -1, aliveRb.velocity.y);
                    aliveRb.AddForce(new Vector2(movementSpeed * -2, 0));
            }

        }
    }


    //возвращает если дистанция приемлема для выполнение атаки
    public bool GetDistance()
    {
        var currentDist = Mathf.Abs((lastPlayerPosition - aliveRb.transform.position).magnitude);

        //Debug.Log("PLAYER POSITION ==== " + lastPlayerPosition);
        //Debug.Log("MOB POSITION ++++ " + aliveRb.transform.position);
        //Debug.Log("CURRENT DISTANCE " + currentDist);
        //Debug.Log("DISTANCE NEEDED " + distanceToAttack);

        //Debug.Log(currentDist + "   CURRENT DISTANCE ");
        //Debug.Log(distanceToAttack+ criticalDistance + "     DISTANCE TO ATTACK");

        if (currentDist <= distanceToAttack)
            return true;

        return false;
    }

    //обычное движение сторожа 
    private void SimpleMovement()
    {
        aliveRb.velocity = new Vector2(0, aliveRb.velocity.y);

        var direction = (0.5f - (int)currentLooking) * 2;

        (bool, bool) wallGround = Check();

        if (wallGround.Item1 || !wallGround.Item2)
        {
            if (currentLooking == LookingSide.RightSide)
                Rotate(180);
            else
                Rotate(0);

            currentLooking = LookingSide.LeftSide == currentLooking ? LookingSide.RightSide : LookingSide.LeftSide;
        }

        aliveRb.AddForce(new Vector2(movementSpeed * 2 * direction, 0));
    }



    //Проверка на наличие стены или пропасти
    private (bool, bool) Check()
    {
        var direction = (0.5f - (int)currentLooking) * 2;

        var wall = Physics2D.Raycast(wallCheck.position, direction * Vector2.right, wallCheckDistance, whatIsGround);

        var ground = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);

        return (wall, ground);
    }



    //Установка стейта 
    public void SetState(State state)
    {
        currentState = state;

    }


    //вход в агро зону , подписка на ивент игрока
    public void EnteredAggroZone()
    {
        ConnectToPlayer();
        //Debug.Log("ENETERED AGGRO ZONE");
        SetState(State.Aggro);
    }


    //выход из агро зоны , где и отписывается от ивента
    public void ExitedAggroZone()
    {
        DisconnectFromPlayer();

        if (!aliveAnim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            Animate("MoveTr");

        
        SetState(State.Moving);
    }

    private bool connectedToPlayer;

    //подключение и отключение на ивент (положение игрока)

    public void ConnectToPlayer()
    {
        connectedToPlayer = true;
        Messenger<Vector2>.AddListener("PlayerPositionChanged", PlayerPositionChanged);
    }
    public void DisconnectFromPlayer()
    {
        connectedToPlayer = false;
        Messenger<Vector2>.RemoveListener("PlayerPositionChanged", PlayerPositionChanged);
    }

    //Подписка на движение игрока
    private void PlayerPositionChanged(Vector2 playerPos)
    {
        lastPlayerPosition = playerPos;

        if (currentState != State.Attack)
        {
            RotateToPlayer(lastPlayerPosition.x);
       
            if(currentState != State.Idle)
            {
                if (!aliveAnim.GetCurrentAnimatorStateInfo(0).IsName("Walk") && !aliveAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    Animate("MoveTr");
            }
        
        }


    }

    //если моб видет игрока он поворачивается в его сторону

    private void RotateToPlayer(float xPositionOfPlayer)
    {
        if (aliveRb.transform.position.x < xPositionOfPlayer)
        {
            currentLooking = LookingSide.RightSide;
            Rotate(0);
        }
        else
        {
            currentLooking = LookingSide.LeftSide;
            Rotate(180);
        }
    }


    //Поворачивать в сторону , куда задано
    private void Rotate(in float to)
    {
        aliveRb.transform.rotation = Quaternion.Euler(0, to, 0);
    }

    private void OnDrawGizmos()
    {
        var dir = (0.5f - (int)currentLooking) * 2;
        //Gizmos.DrawLine(aliveRb.position + offset, (Vector2)aliveRb.transform.position + offset + new Vector2(dir, 0) * frontView);
        //Gizmos.DrawLine(aliveRb.position + offset, (Vector2)aliveRb.transform.position + offset + new Vector2(-dir, 0) * backView);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(wallCheck.position, new Vector2(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.color = Color.blue;
        Vector2 pos = aliveRb.transform.position;
        pos.y -= 0.5f;
        //Gizmos.DrawLine(pos + new Vector2(dir, 0) * distanceToAttack, pos + new Vector2(dir, 0) * (distanceToAttack + criticalDistance));


        if (connectedToPlayer)
            Gizmos.DrawLine(aliveRb.transform.position, lastPlayerPosition);

    }

    public void ShieldAnimFinish() => shieldActive = false;

    [SerializeField] private bool shieldActive = false; 

    [SerializeField] private Vector2 knockbackSpeed;

    [SerializeField] private float knockBack_BackTime;

    private float lastKnockBack = -99f;

    private bool knockBack;

    private void KnockBackTime(float dir)
    {
        movementSpeed /= 2;
        lastKnockBack = Time.time;
        knockBack = true;
        aliveRb.velocity = new Vector2(knockbackSpeed.x * dir, knockbackSpeed.y);
    }

    private void Damage(float[] attackDetails)
    {


        var dir = (0.5f - (int)currentLooking) * 2;


        if (attackDetails[1] > aliveTr.position.x)
        {
            if (dir == -1)
            {
                Rotate(0);
            }
        }
        else
        {
            if (dir == 1)
            {
                Rotate(180);
            }
        }


        if (shield && Time.time >= lastShieldTime + shieldPeriod && !aliveAnim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {

            Animate("Shield");
            lastShieldTime = Time.time;
            lastTime = Time.time - attackCooldown / 2;
            shieldActive = true;

        }
        else
        {
            KnockBackTime(dir);

            Animate("Knockback");

            currentHealth -= attackDetails[0];
            //UpdateKnockbackState();

            //Instantiate(hitParticle, alive.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));
            Instantiate(mobsPref.slashParticle, aliveTr.position, Quaternion.Euler(0.0f, 0.0f, UnityEngine.Random.Range(0.0f, 360.0f)));
            Instantiate(mobsPref.bloodSmash, aliveTr.position, Quaternion.Euler(0.0f, 0.0f, UnityEngine.Random.Range(0.0f, 180.0f)));
            Instantiate(mobsPref.floatingPoints, aliveTr.position, transform.rotation);


            //UIDamage();
            //ShootAudioSource.Play();
            //Hit particle

            if (currentHealth > 0.0f)
            {
                //SwitchState(State.Knockback);
            }
            else if (currentHealth <= 0.0f)
            {
                //ReloadAudioSource.Play();
                //SwitchState(State.Dead);
                EnterDeadState();
            }
        }

    }

    private void EnterDeadState()
    {
        Instantiate(mobsPref.deathChunkParticle, aliveTr.position, mobsPref.deathChunkParticle.transform.rotation);
        Instantiate(mobsPref.deathBloodParticle, aliveTr.position, mobsPref.deathChunkParticle.transform.rotation);
        //Instantiate(bloodSmash, transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 180.0f)));

        var rand = UnityEngine.Random.Range(1, 100);
        if (rand <= chanceOfCoin)
        {
            Instantiate(mobsPref.coinsDrop, aliveTr.position, aliveTr.rotation);
        }

        //GameObject points = Instantiate(floatingPoints, alive.transform.position, alive.transform.rotation) as GameObject;
        //points.transform.GetChild(0).GetComponent<TextMesh>().text = "250";
        //Instantiate(coinsDrop[Random.Range(1, 3)], alive.transform.position, Quaternion.identity);
        //Debug.Log("Coin Drop");
        Destroy(gameObject);
    }

}
