using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapPlayerController : MonoBehaviour
{
    private bool Level1Entrence;
    private bool Level2Entrence;
    private bool Level3Entrence;

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
        if (Level1Entrence)
        {
            SceneManager.LoadScene("Level1");
        }

        if (Level2Entrence && GameManager.Instance.bossesdead == 1)
        {
            SceneManager.LoadScene("Level2");
        }
        if (Level3Entrence && GameManager.Instance.bossesdead == 2)
        {
            SceneManager.LoadScene("Level3");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        enterLevelText.SetActive(true);

        if (collision.tag == "LevelOne")
        {
            Level1Entrence = true;

        }
        if (collision.tag == "LevelTwo")
        {
            Level2Entrence = true;

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        enterLevelText.SetActive(false);

        Level1Entrence = false;
        Level2Entrence = false;
    }


    //ANIMATIONS

    [SerializeField] private Animator animator;

}

