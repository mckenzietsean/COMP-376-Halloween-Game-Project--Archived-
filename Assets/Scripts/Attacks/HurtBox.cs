using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log(collider.tag + " - " + collider.name);
        if (collider.CompareTag("Player"))
        {
            Debug.Log("Colliding");

            IDamageable damageable;
            if (collider.TryGetComponent<IDamageable>(out damageable))
                damageable.TakeDamage(2, null);
        }

    }
}
