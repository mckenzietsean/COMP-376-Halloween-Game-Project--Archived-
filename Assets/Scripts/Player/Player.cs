using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Player : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Animator anim;
    private Material originalMaterial;
    [SerializeField] private Material flashMaterial;
    public LevelManager lm;
    public GameManager gm;

    [Header("States")]
    public bool disabled = true;
    public bool alive = true;
    public bool wasHit = false;
    public bool immuneToDamage = false;
    public bool onContinueScreen = false;
    public bool gameOver = false;
    private Coroutine flashRoutine;

    [Header("Stats")]
    [SerializeField] private int hp = 6;
    [SerializeField] private int lives = 3;
    public int points = 0;
    public int defense = 1;

    [Header("Timers")]
    [SerializeField] private float invinTime = 1;
    [SerializeField] private float hitFlashDuration = 0.125f;
    [SerializeField] private float reviveTime = 3;
    [SerializeField] private float continueTimer = 10;

    [Header("UI")]
    [SerializeField] private GameObject UICanvas;
    [SerializeField] private GameObject[] allUIs = new GameObject[3];
    [SerializeField] private TextMeshProUGUI playerLives;
    [SerializeField] private TextMeshProUGUI playerScore;
    [SerializeField] private Sprite[] heartSprites = new Sprite[3];
    [SerializeField] private Image[] playerHearts = new Image[3];
    [SerializeField] private TextMeshProUGUI continueTimerText;

    [Header("Audio")]
    [SerializeField] private AudioSource hitSFX;
    [SerializeField] private AudioSource deathSFX;
    [SerializeField] private AudioSource reviveSFX;

    // Honestly it's just easier to track inputs this way
    [Header("Keycodes")]
    public string inputKeyIdentifier;
    public KeyCode shootKey;
    public KeyCode jumpKey;
    public KeyCode slideKey;
    public KeyCode fastKey;

    public void EnablePlayer()
    {
        Debug.Log("ENABLE PLAYER.");
        disabled = false;
        UICanvas.SetActive(true);  
        lm = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
        gm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        GetComponent<PlayerAttackSystem>().lm = lm;
        GetComponent<CharacterController>().enabled = true;
        immuneToDamage = false;
        defense = 1;
    }

    public void DisablePlayer()
    {
        disabled = true;
        GetComponent<CharacterController>().enabled = false;
        UICanvas.SetActive(false);
    }

    public void ResetPlayerStats()
    {
        hp = 6;
        lives = 3;
        points = 0;
        defense = 1;
        gameOver = false;
        alive = true;
        immuneToDamage = false;
        wasHit = false;
        onContinueScreen = false;
        SwapUIs(0);
        GetComponent<PlayerAttackSystem>().ResetStats();
    }

    public void ToggleUI(bool on)
    {
        UICanvas.SetActive(on);
    }

    public void AddPoints(int amt)
    {
        points += amt;
    }

    public void Heal(int heal)
    {
        hp += heal;
    }

    public void TakeDamage(int damage, GameObject attacker)
    {
        if (wasHit)
            return;

        //Debug.Log("Attacked by " + attacker.name + " for " + damage + " damage.");

        wasHit = true;
        Invoke(nameof(NoLongerHit), invinTime);

        if (immuneToDamage)
            return;

        hp -= damage / defense;     

        if (hp <= 0)
        {
            deathSFX.Play();
            Death();
        }
        else
            hitSFX.Play();

        DamageFlashing();
    }

    public void NoLongerHit()
    {
        wasHit = false;
    }

    public void Death()
    {
        lives--;
        alive = false;
        immuneToDamage = true;

        StartCoroutine(DeathPause());

        if (lives <= 0)
            ContinueScreen();
        else
            Invoke(nameof(Revive), reviveTime);
    }


    public void Revive()
    {
        reviveSFX.Play();
        alive = true;
        hp = 6;
        // Give player a bit of time to take damage again
        immuneToDamage = true;
        Invoke(nameof(LoseInvin), invinTime); 
    }

    public void LoseInvin()
    {
        immuneToDamage = false;
    }

    public void ContinueScreen()
    {
        Debug.Log("CONTINUE?");
        // Continue Screen
        onContinueScreen = true;
        SwapUIs(1);
        
        // If no more continues, end player's game
        if(gm.continues <= 0)
            GameOver();   
    }

    public void GameOver()
    {
        SwapUIs(2);
        gameOver = true;
        wasHit = false;
        gm.GameOver();
    }

    // Start is called before the first frame update
    void Start()
    {
        hp = 6;
        originalMaterial = sr.material;
        SwapUIs(0);
    }

    // Update is called once per frame
    void Update()
    {
        DrawUI();

        if (onContinueScreen)
        {  
            if(!gameOver)
            {
                // Player chose to continue
                if (Input.GetKeyDown(shootKey))
                {
                    onContinueScreen = false;
                    continueTimer = 10;
                    gm.continues--;
                    lives = 3;
                    SwapUIs(0);
                    Revive();
                }
                else
                {
                    if (continueTimer > 0)
                    {
                        continueTimer -= Time.deltaTime;
                        continueTimerText.text = Mathf.CeilToInt(continueTimer).ToString();

                        // If the number of continues drop while the countdown is still occuring
                        if (gm.continues <= 0)
                        {
                            onContinueScreen = false;
                            GameOver();
                        }
                           
                    }
                    else
                    {
                        onContinueScreen = false;
                        GameOver();
                    }
                }           
            }     
        }

        // TESTING
        // Take 1 damage
        if (Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage(1, gameObject);
        }
        // Die
        if (Input.GetKeyDown(KeyCode.Q))
        {
            lives = 1;
            TakeDamage(6, gameObject);
        }
        // Game Over
        if (Input.GetKeyDown(KeyCode.R))
            gameOver = true;

        UpdateAnimations();
    }

    void UpdateAnimations()
    {
        anim.SetBool("isDead", gameOver);
        anim.SetBool("isHit", (wasHit || !alive) && !gameOver);
    }

    void DrawUI()
    {
        playerLives.text = lives.ToString();
        playerScore.text = points.ToString();

        for(int i = 0; i < playerHearts.Length; i++)
        {
            // 0 = Full
            // 1 = Half
            // 2 = Empty

            if ((1+i) * 2 <= hp)
                playerHearts[i].sprite = heartSprites[0];
            else if((i*2) + 1 <= hp)
                playerHearts[i].sprite = heartSprites[1];
            else
                playerHearts[i].sprite = heartSprites[2];
        }
    }

    private void SwapUIs(int top)
    {
        for(int i = 0; i < allUIs.Length; i++)
        {
            if (i == top)
                allUIs[i].SetActive(true);
            else
                allUIs[i].SetActive(false);
        }
    }

    private void DamageFlashing()
    {
        // Can cause issues if we have multiple active coroutines at once
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(HitFlashing());
    }

    private IEnumerator HitFlashing()
    {
        sr.material = flashMaterial;
        yield return new WaitForSeconds(hitFlashDuration);
        sr.material = originalMaterial;

        // Signal that the hit flash routine is done
        flashRoutine = null;
    }

    private IEnumerator DeathPause()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.25f);
        Time.timeScale = 1;
    }
}
