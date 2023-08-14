using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameManager manager;
    public Transform[] allPlayers;
    public Transform farthestPlayer;
    //public float smoothSpeed = 0.125f;
    public bool freeze;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (manager.cameraReset)
        {
            manager.cameraReset = false;
            freeze = false;
            transform.position = new Vector3(0, transform.position.y, transform.position.z);
        }

        if (freeze)
            return;

        DetermineFarthest();



        // Player has moved forward OR Level reset
        if(farthestPlayer.transform.position.x > transform.position.x)
        {
            transform.position = new Vector3(farthestPlayer.transform.position.x, transform.position.y, transform.position.z);
        }
    }

    void DetermineFarthest()
    {
        farthestPlayer = manager.players[0].transform;
        for (int i = 0; i < manager.maxPlayers-1; i++)
        {
            if (manager.players[i].transform.position.x < manager.players[i + 1].transform.position.x)
                farthestPlayer = manager.players[i + 1].transform;
        }
    }
}
