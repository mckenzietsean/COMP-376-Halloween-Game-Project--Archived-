using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockadeHandler : MonoBehaviour
{
    [SerializeField] private GameObject blockade;
    [SerializeField] private GameObject explosionPrefab;
    GameObject explosion;

    public void Start()
    {
        blockade = GameObject.FindGameObjectWithTag("Blockade");
    }

    public void Explode()
    {
        explosion = Instantiate(explosionPrefab, new Vector3(113, 0, -6), Quaternion.identity);
        Destroy(explosion, 0.5f);
        blockade.SetActive(false);        
    }
}
