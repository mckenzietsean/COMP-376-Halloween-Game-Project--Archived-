using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformEffector3D : MonoBehaviour
{
    [SerializeField] private Vector3 entryDirection = Vector3.up;
    [SerializeField, Range(1.0f, 2.0f)] private float triggerScaleX = 1.25f;
    [SerializeField, Range(1.0f, 10.0f)] private float triggerScaleY = 8f;
    private BoxCollider collider;
    private BoxCollider collisionCheckTrigger = null;

    float dot = 1;
    bool firstCollision = true;
    Vector3 offset = new Vector3(0, 0.5f, 0);

    void Awake()
    {
        collider = GetComponent<BoxCollider>();
        collider.isTrigger = false;

        collisionCheckTrigger = gameObject.AddComponent<BoxCollider>();
        collisionCheckTrigger.size = new Vector3(collider.size.x * triggerScaleX, collider.size.y * triggerScaleY, collider.size.z);
        collisionCheckTrigger.center = collider.center;
        collisionCheckTrigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (firstCollision)
            Physics.IgnoreCollision(collider, other, true);        

        Debug.Log("ENTERED " + dot + " ... " + Physics.GetIgnoreCollision(collider, other));
    }

    private void OnTriggerStay(Collider other)
    {

        if (Physics.ComputePenetration(collisionCheckTrigger, transform.position, transform.rotation, other, other.transform.position, other.transform.rotation, out Vector3 collisionDirection, out float penetrationDepth))
            dot = Vector3.Dot(entryDirection, collisionDirection);

        if (firstCollision)
        {
            if (dot < 0)
                Physics.IgnoreCollision(collider, other, false);
            else
                Physics.IgnoreCollision(collider, other, true);
        }

        Debug.Log("STAYED = " + dot + " ... " + Physics.GetIgnoreCollision(collider, other));
    }

    private void OnTriggerExit(Collider other)
    {

        // Opposite direction, don't pass through
        if (dot < 0)
            Physics.IgnoreCollision(collider, other, false);
        else
            Physics.IgnoreCollision(collider, other, true);

        Debug.Log("EXITED = " + dot + " ... " + Physics.GetIgnoreCollision(collider, other));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, entryDirection);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, -entryDirection);
    }
}
