using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour, IDamageable
{
    public int hp = 1;
    [SerializeField] private int moveSpeed = 4;
    [SerializeField] private GameObject[] players;
    [SerializeField] private float[] playerDamageDealt;
    Vector3 moveDir;
    [SerializeField] private Vector3[] movePositions;
    int moveIndex = 0;
    float moveTimer = 0;
    float moveTimeCap = 5;
    bool alive = true;

    [Header("References")]
    public LevelManager lm;
    public Transform camera;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource shootSFX;
    [SerializeField] private AudioSource deathSFX;

    Rigidbody rb;
    [SerializeField] private GameObject explosionPrefab;
    GameObject explosion;

    [Header("Attacks")]
    public GameObject attackPrefab;
    public Attack attack;
    public bool attacking = false;
    public float timeBetweenAttacks;
    public int numOfAttacks;

    [Header("Aiming")]
    Vector3 targetPoint;
    Vector3 attackDirection;
    RaycastHit hit;

    public void Death()
    {
        // Grant 20 points to all players
        for (int i = 0; i < players.Length; i++)
        {
            players[i].GetComponent<Player>().AddPoints(20);
        }

        // Now determine who gets the bonus 10 points
        int mvp = 0;
        for (int i = 1; i < players.Length; i++)
        {
            if (playerDamageDealt[i] > playerDamageDealt[mvp])
                mvp = i;
        }
        players[mvp].GetComponent<Player>().AddPoints(10);

        // SINGLE TO LEVEL MANAGER THAT LEVEL IS DONE
        lm.StageClear();

        // Instantiate excessive explosion
        explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(explosion, 0.5f);

        alive = false;
        if (anim != null)
            anim.SetBool("alive", false);
        Destroy(rb);
        GetComponent<CapsuleCollider>().enabled = false;
        Destroy(gameObject, 1f);
    }

    public void Heal(int heal)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(int damage, GameObject attacker)
    {
        hp -= damage;
        deathSFX.Play();

        // Check which player dealt this damage
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == attacker)
                playerDamageDealt[i]++;
        }

        if (hp <= 0)
        {
            Death();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (anim != null)
        {
            anim.SetBool("alive", true);
            anim.SetBool("moving", false);
        }  

        // Find all players to interact with
        players = GameObject.FindGameObjectsWithTag("Player");
        playerDamageDealt = new float[players.Length];

        // Only start attacking after 2 seconds
        attacking = true;
        Invoke(nameof(ResetAttacking), 2);
    }

    // Update is called once per frame
    void Update()
    {
        // Don't do anything if player is dead
        if (!alive)
            return;

        if (attackPrefab != null && !attacking)
        {
            attacking = true;
            for(int i = 0; i < numOfAttacks; i++)
            {
                Shoot();
            }   
            Invoke(nameof(ResetAttacking), timeBetweenAttacks);
        }


        moveTimer += Time.deltaTime;

        if (moveTimer > moveTimeCap)
        {
            moveTimer = 0;
            moveIndex = (moveIndex + 1) % movePositions.Length;
        }
    }

    void FixedUpdate()
    {
        if (!alive)
            return;

        BossAI();
    }

    void BossAI()
    {
        Vector3 destination = new Vector3(camera.position.x, 0, 0) + movePositions[moveIndex];
        moveDir = (destination - transform.position).normalized;

        if (moveDir.x < 0)
            sr.flipX = false;
        else
            sr.flipX = true;

        if (Vector3.Distance(destination, transform.position) > 1)
        {
            rb.MovePosition(transform.position + moveDir * moveSpeed * Time.deltaTime);
            if(anim != null)
                anim.SetBool("moving", true);
        }
        else if (anim != null)
            anim.SetBool("moving", false);
    }

    private void ResetAttacking()
    {
        attacking = false;
    }

    public void Shoot()
    {
        if (!alive)
            return;

        shootSFX.Play();

        // If there's no attack to use, ignore
        if (attackPrefab == null)
        {
            Debug.Log("NO ATTACK TO USE");
            attacking = false;
            return;
        }

        // Create attack
        attack = Instantiate(attackPrefab, transform.position, Quaternion.identity).GetComponent<Attack>();
        attack.owner = gameObject;
        attack.ownerLayer = gameObject.layer;
        attack.ownerTag = tag;

        // Find a player to target
        int target = Random.Range(0, players.Length);
        attackDirection = players[target].transform.position - transform.position;

        attack.transform.position = transform.position;
        attack.transform.forward = Quaternion.Euler(0f, 0f, Random.Range(-30, 30)) * attackDirection;

        // Launch bullet
        attack.GetComponent<Rigidbody>().AddForce(attack.transform.forward * attack.speed, ForceMode.VelocityChange);
    }
}
