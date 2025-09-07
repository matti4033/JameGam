using UnityEngine;

public class DamagePlayer : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var hp = other.gameObject.GetComponent<PlayerHealth>();
            if (hp != null) hp.Damage(1);
        }
    }
}
