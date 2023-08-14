using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Witch : MonoBehaviour, IDamageable
{
    [SerializeField] private int hp = 1;
    [SerializeField] private int moveSpeed = 4;
    [SerializeField] private GameObject[] players;
    public Transform camera;
    Vector3 moveDir = Vector3.right;
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
                attacker.GetComponent<Player>().AddPoints(5);
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

        float dist = transform.position.x - camera.position.x;

        // Move back and forth, annoying the player
        if (dist < -7f)
            moveDir = Vector3.right;         
        else if (dist > 7f)
            moveDir = -Vector3.right;

        rb.MovePosition(transform.position + moveDir * (moveSpeed + levelMultiplier * 1.5f) * Time.deltaTime);

        if (moveDir.x < 0)
            sr.flipX = false;
        else
            sr.flipX = true;
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
        attack.transform.forward = Quaternion.Euler(0f, 0f, Random.Range(-60, 60)) * attackDirection;

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
            players[i].GetComponent<Player>().TakeDamage(4, null);
        }

        // Instantiate nuke explosion
        explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(explosion, 0.5f);
        Destroy(gameObject);
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
        for (int i = 0; i < 5; i++)
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
