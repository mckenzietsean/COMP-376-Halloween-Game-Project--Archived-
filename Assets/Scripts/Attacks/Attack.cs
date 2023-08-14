using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("References")]
    public GameObject owner;
    public LayerMask ownerLayer;
    public string ownerTag;
    public Rigidbody rb;

    [Header("Properties")]
    public int damage;
    public float lifeTime;
    public float speed;
    public int multiplier = 0; 

    [Header("Layer")]
    public LayerMask hitLayer;
    public LayerMask ignoreLayer;

    private void OnEnable()
    {
        Destroy(gameObject, lifeTime);
        multiplier = 0;
    }

    protected bool InIgnoreLayer(int hitLayer)
    {
        return ((1 << hitLayer) & ignoreLayer) != 0;
    }

    private void OnTriggerEnter(Collider collider)
    {
        // Ignore specified layers (if any)
        if (InIgnoreLayer(collider.gameObject.layer))
            return;

        // Ignore if collision is also the same layer as the owner
        if (collider.gameObject.layer == ownerLayer)
            return;

        // Ignore if tag is also the same tag as the owner
        if (collider.gameObject.CompareTag(ownerTag))
            return;

        IDamageable damageable;
        if (collider.TryGetComponent<IDamageable>(out damageable))
        {
            // If the owner died before landing this hit
            if (owner == null)
            {
                damageable.TakeDamage(damage, null);
                return;
            }
                
            damageable.TakeDamage(damage, owner);

            if (multiplier > 0)
            {
                // Add +5 points if this was done by a player
                Player p;
                if(owner.TryGetComponent<Player>(out p))
                {
                    p.AddPoints(5);
                    Debug.Log("BONUS POINTS (+5)");
                }
            }

            multiplier++;
        }

        // Give it a small delay so attacks can pierce
        Destroy(gameObject);
    }

    private void OnTriggerStay(Collider collider)
    {
        // Ignore if collision is also the same layer as the owner
        if (collider.gameObject.layer == ownerLayer)
            return;
    }
}
