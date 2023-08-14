using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] private LevelManager lm;
    [SerializeField] private GameObject bossPrefab;
    Boss boss;
    [SerializeField] private CameraFollow cam;
    bool active = true;
    [SerializeField] private int startDelay = 0;

    private void OnTriggerEnter(Collider other)
    {
        // Player is going to fight the boss
        if (other.tag == "Player" && active)
        {
            cam.freeze = true;
            active = false;
            StartCoroutine(SpawnBoss());
        }
    }

    IEnumerator SpawnBoss()
    {
        yield return new WaitForSeconds(startDelay);
        if(startDelay == 0)
            boss = Instantiate(bossPrefab, new Vector3(cam.transform.position.x, 20, 0), Quaternion.identity).GetComponent<Boss>();
        else
            boss = Instantiate(bossPrefab, new Vector3(165, 0, 0), Quaternion.identity).GetComponent<Boss>();
        boss.camera = cam.transform;
        boss.lm = lm;
        boss.hp *= lm.playerMultiplier;
        lm.StartBoss();
    }
}
