using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameManager manager;
    [SerializeField] private CameraFollow camera;
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private GameObject witchPrefab;
    Witch witch;
    Enemy ghost;
    [SerializeField] private AudioSource bgm;
    [SerializeField] private AudioSource bossBgm;
    [SerializeField] private AudioSource victoryBgm;   
    public int levelMultiplier;
    public int playerMultiplier;
    int witchSpawns = 0;
    int triggersWalkedPast = 0;
    public int fastPlayers = 0;
    public bool canSpawn = true;
    float spawnTimer = -5;
    public float spawnTimeCap = 0;
    float fastTimer = 0;
    float fastTimeCap = 0.1f;


    // Start is called before the first frame update
    void Start()
    {
        witchSpawns = 0;
        triggersWalkedPast = 0;

        manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        levelMultiplier = manager.sceneNum;
        playerMultiplier = manager.maxPlayers;
        fastTimer = 0;
        spawnTimer = -5;    // give players a small grace period
    }

    // Update is called once per frame
    void Update()
    {
        // If a player is going fast, spawn MORE ghosts
        if (fastPlayers > 0)
        {
            fastTimer += Time.deltaTime;

            fastTimeCap += Time.deltaTime * 0.02f;   // Slightly increase the timer cap so the more the player spams it, the less will spawn

            // Spawn ghosts for each fast player
            for (int i = 0; i < fastPlayers; i++)
            {
                if (fastTimer > fastTimeCap)
                {
                    fastTimer = 0;
                    SpawnGhost();
                }
            }
        }

        if (!canSpawn)
            return;

        spawnTimer += Time.deltaTime;

        // Randomly spawn ghosts
        if(spawnTimer > spawnTimeCap)
        {
            spawnTimer = 0;
            spawnTimeCap = Random.Range(8, 12) / playerMultiplier;
            SpawnGhost();
        }  
    }

    private void SpawnGhost()
    {
        int xRand = Random.Range(-8, 8);
        ghost = Instantiate(ghostPrefab, new Vector3(camera.transform.position.x + xRand, 14, 0), Quaternion.identity).GetComponent<Enemy>();
        ghost.levelMultiplier = levelMultiplier;
    }

    public void SpawnWitch()
    {
        triggersWalkedPast++;
        
        // If 5 triggers passed and still no spawn OR 10 triggers pass and only 1 spawned OR 1/8 chance normally
        if((triggersWalkedPast == 5 && witchSpawns == 0) || (triggersWalkedPast == 10 && witchSpawns == 1) || (Random.Range(0, 8) < 1))
        {
            witchSpawns++;
            witch = Instantiate(witchPrefab, new Vector3(camera.transform.position.x - 10, 5, 0), Quaternion.identity).GetComponent<Witch>();
            witch.camera = camera.transform;
            witch.levelMultiplier = levelMultiplier;
        }
    }

    public void StartBoss()
    {
        canSpawn = false;
        bgm.Stop();
        bossBgm.Play();
    }

    public void StageClear()
    {
        bossBgm.Stop();
        victoryBgm.Play();
        
        for(int i = 0; i < manager.maxPlayers; i++)
        {
            manager.players[i].GetComponent<Player>().immuneToDamage = true;
        }

        StartCoroutine(GoToNextScene());
        
    }

    IEnumerator GoToNextScene()
    {
        yield return new WaitForSeconds(7f);
        manager.NextScene();
    }
}
