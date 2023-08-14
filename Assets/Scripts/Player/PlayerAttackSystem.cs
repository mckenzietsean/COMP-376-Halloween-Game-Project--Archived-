using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttackSystem : MonoBehaviour
{
    [Header("References")]
    private Player player;
    private PlayerController playerController;
    [SerializeField] private GameManager manager;
    public LevelManager lm;

    [Header("Attacks")]
    public GameObject attackPrefab;
    public Attack attack;
    public bool attacking = false;
    [SerializeField] private bool fast = false;
    [SerializeField] private float timeBetweenAttacks;
    

    [Header("Fast Mode")]
    float usedTimeBetweenAttacks;
    float fastTimeBetweenAttacks = 0.2f;
    [SerializeField] private float fastCharge = 0;
    [SerializeField] private const float fastLimit = 100;
    [SerializeField] private Slider fastSlider;

    [Header("Aiming")]
    Vector3 targetPoint;
    Vector3 attackDirection;
    RaycastHit hit;

    [Header("Audio")]
    [SerializeField] private AudioSource shootSFX;
    [SerializeField] private AudioSource fastActiveSFX;
    [SerializeField] private AudioSource fastCappedSFX;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        player = GetComponent<Player>();
        usedTimeBetweenAttacks = timeBetweenAttacks;
        fastCharge = 0;
    }

    public void ResetStats()
    {
        usedTimeBetweenAttacks = timeBetweenAttacks;
        fastCharge = 0;
    }

    // Update is called once per frame
    void Update()
    {
        ReadInputs();

        if (!fast && player.alive && !player.disabled)
        {
            if (fastCharge < fastLimit)
            {
                fastCharge += Time.deltaTime * 3f;

                // Play sound if capped
                if (fastCharge >= fastLimit)
                    fastCappedSFX.Play();
            }        
            else
                fastCharge = fastLimit;
        }
        
        fastSlider.value = fastCharge;
    }

    void ReadInputs()
    {
        // Don't do anything if player is dead
        if (!player.alive || player.disabled)
            return;

        if (attackPrefab != null && Input.GetKey(player.shootKey) && !attacking)
        {
            attacking = true;
            Shoot();
            Invoke(nameof(ResetAttacking), usedTimeBetweenAttacks);
        }

        if (attackPrefab != null && Input.GetKey(player.fastKey) && fastCharge >= fastLimit)
        {
            fastActiveSFX.Play();
            fast = true;
            fastCharge = 0;
            player.defense = 2;
            usedTimeBetweenAttacks = fastTimeBetweenAttacks;
            lm.fastPlayers++;
            Invoke(nameof(ResetFastSpawns), 1);
            Invoke(nameof(ResetFastBoost), 3);
        }
    }

    public void Shoot()
    {
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

        attackDirection = playerController.GetFacingDirection();

        attack.transform.position = transform.position;
        attack.transform.forward = attackDirection;

        // Launch bullet
        attack.GetComponent<Rigidbody>().AddForce(attack.transform.forward * attack.speed, ForceMode.VelocityChange);
        shootSFX.Play();
    }


    private void ResetAttacking()
    {
        attacking = false;
    }

    private void ResetFastSpawns()
    {
        lm.fastPlayers--;
    }

    private void ResetFastBoost()
    {
        fast = false;
        player.defense = 1;
        usedTimeBetweenAttacks = timeBetweenAttacks;
    }
}
