using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBossAnimation : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject explosionPrefab;
    GameObject explosion;
    bool active = true;

    private void OnTriggerEnter(Collider other)
    {
        // Player is going to fight the boss
        if (other.tag == "Player" && active)
        {
            active = false;
            StartCoroutine(DoAnimation());
        }
    }

    public void Start()
    {
        anim.SetBool("yes", false);
    }

    IEnumerator DoAnimation()
    {
        anim.SetBool("yes", true);
        yield return new WaitForSeconds(4f);
        explosion = Instantiate(explosionPrefab, new Vector3(165, 0, -6), Quaternion.identity);
        Destroy(explosion, 0.5f);
    }
}
