using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapPlayerController : MonoBehaviour
{
    private bool MattiIsPro;

    [SerializeField] GameObject enterLevelText;

    [SerializeField] private float moveSpeed;
    [SerializeField] private Rigidbody2D rb;


    float moveX, moveY;

    private void FixedUpdate()
    {
        Vector2 movement = new Vector2(moveX, moveY);
        rb.linearVelocity = movement * moveSpeed;
    }

    public void OnMove(InputValue value)
    {
        Vector2 movement = value.Get<Vector2>();

        moveX = movement.x;

        moveY = movement.y;
    }

    public void OnInteract(InputValue value)
    {
        Debug.Log("asdasdad");
        if (MattiIsPro)
        {
            Debug.Log("PRESSED");
            SceneManager.LoadScene("Matti");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("IN THE AREA");
        enterLevelText.SetActive(true);

        if (collision.tag == "LevelMatti")
        {
            MattiIsPro = true;

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        enterLevelText.SetActive(false);

        MattiIsPro = false;
    }
}

