using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    [SerializeField] GameObject player;
    void Start()
    {
        player.GetComponent<GameObject>().CompareTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
        }
    }
}
