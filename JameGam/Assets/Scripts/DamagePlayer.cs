using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    [SerializeField] GameObject player;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        player.GetComponent<GameObject>().CompareTag("Player");
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("1");
        if (collision.gameObject.CompareTag("Player"))
        {
        Debug.Log("2");
            var hp = collision.gameObject.GetComponent<PlayerHealth>();
            if (hp != null) hp.Damage(1);
        }
    }
}
