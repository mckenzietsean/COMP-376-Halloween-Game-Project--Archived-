using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Scene Data")]
    public static GameManager manager;
    public GameObject mainMenu;

    public int maxPlayers = 0;
    public GameObject[] players;
    public int sceneNum = 0;
    public Animator fadeCanvas;

    [Header("Game Overs")]
    public GameObject continueCanvas;
    public GameObject gameOverTexts;
    public TextMeshProUGUI continueText;
    public int continues = 2;
    public int deadPlayers = 0;
    bool gameOver = false;


    [Header("Levels")]
    public LevelManager lm;
    public bool cameraReset = false;
   

    private void Awake()
    {
        if(manager == null)
        {
            manager = this;
            DontDestroyOnLoad(this);
        }
        else if (manager != null)
        {
            Destroy(gameObject);
        }   
    }

    // Start is called before the first frame update
    void Start()
    {
        sceneNum = 0;
        gameOverTexts.SetActive(false);
        gameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            NextScene();

        if (Input.GetKeyDown(KeyCode.Space) && gameOver)
        {
            gameOver = false;
            sceneNum = 0;
            StartCoroutine(SceneFadeOut());
        }

        if (sceneNum != 0 && continues >= 0)
            continueText.text = "Continues: " + continues;
    }

    public void NextScene()
    {
        sceneNum++;

        if (sceneNum >= SceneManager.sceneCountInBuildSettings)
            sceneNum = 0;

        StartCoroutine(SceneFadeOut());  
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneLoaded; //You add your method to the delegate
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
    }

    //After adding this method to the delegate, this method will be called every time
    //that a new scene is loaded. You can then compare the scene loaded to your desired
    //scenes and do actions according to the scene loaded.
    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (sceneNum == 1 || sceneNum == 2) //use your desired check here to compare your scene
        {
            continueCanvas.SetActive(true);
            mainMenu.SetActive(false);
            lm = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
            StartCoroutine(EnablingPlayers());
            StartCoroutine(SceneFadeIn());
        }
        else if (sceneNum == 3)
        {
            for (int i = 0; i < maxPlayers; i++)
            {
                players[i].GetComponent<Player>().DisablePlayer();
                players[i].transform.localPosition = Vector3.zero;
            }

            // Instant fade in
            fadeCanvas.SetBool("FadeIn", true);
        }
        else
        {
            gameOver = false;
            gameOverTexts.SetActive(false);
            continues = 2;
            continueCanvas.SetActive(false);
            mainMenu.SetActive(true);
            players[1].SetActive(true);
            players[0].transform.localPosition = new Vector3(-7, -1.15f, 3);
            players[1].transform.localPosition = new Vector3(-2, -1.07f, 3);
            players[0].GetComponent<PlayerController>().ForceFlip(true);
            players[1].GetComponent<PlayerController>().ForceFlip(false);

            for (int i = 0; i < maxPlayers; i++)
            {
                players[i].GetComponent<Player>().ResetPlayerStats();
                players[i].GetComponent<Player>().DisablePlayer();
            }

            // Instant fade in
            fadeCanvas.SetBool("FadeIn", true);
            mainMenu.GetComponent<Canvas>().sortingOrder = 10;
        }       
    }

    public IEnumerator SceneFadeIn()
    {
        yield return new WaitForSeconds(2.1f);

        // FADE BACK IN HERE
        fadeCanvas.SetBool("FadeIn", true);
        Debug.Log("FADE IN");

        mainMenu.GetComponent<Canvas>().sortingOrder = 10;
    }

    public IEnumerator SceneFadeOut()
    {
        for (int i = 0; i < maxPlayers; i++)
        {
            players[i].GetComponent<Player>().DisablePlayer();
        }

        // FADE TO BLACK HERE
        Debug.Log("FADE OUT");
        fadeCanvas.SetBool("FadeIn", false);
        mainMenu.GetComponent<Canvas>().sortingOrder = 0;

        yield return new WaitForSeconds(1.2f);

        if (maxPlayers >= 2)
            players[1].SetActive(true);
        else
            players[1].SetActive(false);

        SceneManager.LoadScene(sceneNum);
    }


    public IEnumerator EnablingPlayers()
    {
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < maxPlayers; i++)
        {

            players[i].transform.position = Vector3.zero;
            players[i].transform.localPosition = Vector3.zero;
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < maxPlayers; i++)
        {
            players[i].GetComponent<Player>().EnablePlayer();
        }

        cameraReset = true;
    }

    public void GameOver()
    { 
        deadPlayers++;

        // Game over
        if (deadPlayers >= maxPlayers)
        {
            gameOver = true;
            gameOverTexts.SetActive(true);
            // Play GAME OVER theme
        }
    }
}
