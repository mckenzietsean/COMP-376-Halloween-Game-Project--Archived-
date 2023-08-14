using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{  
    [SerializeField] private int hp = 1;
    [SerializeField] private int moveSpeed = 4;
    [SerializeField] private GameObject[] players;
    [SerializeField] private Transform closestPlayer;
    bool alive = true;
    public int levelMultiplier = 1;

    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource shootSFX;
    [SerializeField] private AudioSource deathSFX;
    private Material originalMaterial;
    [SerializeField] private Material flashMaterial;
    Rigidbody rb;
    [SerializeField] private GameObject explosionPrefab;
    GameObject explosion;
    [SerializeField] private BlockadeHandler blockade;

    [Header("Attacks")]
    public GameObject attackPrefab;
    public Attack attack;
    public bool attacking = false;
    public float timeBetweenAttacks;

    [Header("Aiming")]
    Vector3 targetPoint;
    Vector3 attackDirection;
    RaycastHit hit;

    [SerializeField] private float enemyLifeTime;
    private Coroutine flashRoutine;

    public void Death()
    {
        if (blockade != null)
            blockade.Explode();

        alive = false;
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
        if (hp <= 0)
        {
            if(attacker != null)
                attacker.GetComponent<Player>().AddPoints(1);
            Death();
        }         
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalMaterial = sr.material;
        anim.SetBool("alive", true);

        // Find all players to interact with
        players = GameObject.FindGameObjectsWithTag("Player");
        closestPlayer = players[0].transform;   // Placeholder in case player is dead when enemy spawns

        // Set up explosion timers
        Invoke(nameof(TimeOutExplosion), enemyLifeTime);
        Invoke(nameof(ExplodeFlashing), enemyLifeTime - 1.5f);

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
            Shoot();
            Invoke(nameof(ResetAttacking), timeBetweenAttacks - levelMultiplier * 0.5f);
        }

    }

    void FixedUpdate()
    {
        if (!alive)
            return;

        DetermineClosest();
        Vector3 direction = (closestPlayer.position - transform.position).normalized;
        if(moveSpeed > 0)
            rb.MovePosition(transform.position + direction * (moveSpeed + levelMultiplier * 1.5f) * Time.deltaTime);

        if (direction.x < 0)
            sr.flipX = false;
        else
            sr.flipX = true;
    }

    void DetermineClosest()
    {
        float closestDistance = 99;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<Player>().alive)
            {
                float newDistance = Vector3.Distance(players[i].transform.position, transform.position);

                if (newDistance < closestDistance)
                {
                    closestDistance = newDistance;
                    closestPlayer = players[i].transform;
                }
            }             
        }
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
        attack.GetComponent<Rigidbody>().AddForce(attack.transform.forward * (attack.speed + levelMultiplier * 1.5f), ForceMode.VelocityChange);
    }


    void TimeOutExplosion()
    {
        if (!alive)
            return;

        // Make all players take damage
        for (int i = 0; i < players.Length; i++)
        {
            players[i].GetComponent<Player>().TakeDamage(2, null);
        }

        // Instantiate nuke explosion
        explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(explosion, 0.5f);
        Death();
    }

    private void ExplodeFlashing()
    {
        // Can cause issues if we have multiple active coroutines at once
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(HitFlashing());
    }

    private IEnumerator HitFlashing()
    {
        for(int i = 0; i < 5; i++)
        {
            sr.material = flashMaterial;
            yield return new WaitForSeconds(0.2f);
            sr.material = originalMaterial;
            yield return new WaitForSeconds(0.2f);
        }

        // Signal that the hit flash routine is done
        flashRoutine = null;
    }
}
