using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndCredits : MonoBehaviour
{
    public GameManager manager;
    public TextMeshProUGUI p1Score;
    public TextMeshProUGUI p2Score;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            manager.NextScene();

        p1Score.text = "Player 1 got <color=red>" + manager.players[0].GetComponent<Player>().points + "</color> points.";

        if (manager.maxPlayers > 1)
            p2Score.text = "Player 2 got <color=blue>" + manager.players[1].GetComponent<Player>().points + "</color> points.";
        else
            p2Score.text = "";
    }
}
