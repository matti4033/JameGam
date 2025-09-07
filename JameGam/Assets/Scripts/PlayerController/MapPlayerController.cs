
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
    [SerializeField] GameObject goLevel1;
    [SerializeField] GameObject goLevel2;


    [SerializeField] GameObject spriteIsak;
    [SerializeField] Sprite spriteTHIS;

    [SerializeField] private Animator animator;
    [SerializeField] private Animator animatorIsak;

    [SerializeField] private float moveSpeed;
    [SerializeField] private Rigidbody2D rb;


    float moveX, moveY;

    private void Start()
    {
        animatorIsak = spriteIsak.GetComponent<Animator>();
    }
    private void Update()
    {
        spriteTHIS = spriteIsak.GetComponent<SpriteRenderer>().sprite;
    }

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

        //PLAYER PREFAB (SHADOW)
        if (movement.x != 0 || movement.y != 0)
        {
            animator.SetBool("IsMoving", true);

            if (movement.x > 0)
            {
                animator.SetTrigger("MovingRight");
            }
            if (movement.x < 0)
            {
                animator.SetTrigger("MovingLeft");
            }
        }
        else
            animator.SetBool("IsMoving", false);

        //SPRITE PLAYER MODEL
        if (movement.x != 0 || movement.y != 0)
        {
            animatorIsak.SetBool("IsMoving", true);

            if (movement.x > 0)
            {
                animatorIsak.SetTrigger("MovingRight");
            }
            if (movement.x < 0)
            {
                animatorIsak.SetTrigger("MovingLeft");
            }
        }
        else
            animatorIsak.SetBool("IsMoving", false);
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
        //If correct boss is killed for previous lvl you're allowed to enter
        if (collision.tag == "LevelOne" && GameManager.Instance.bossesdead == 0)
        {
            Level1Entrence = true;
            enterLevelText.SetActive(true);
        }
        //If correct boss is killed for previous lvl you're allowed to enter
        if (collision.tag == "LevelTwo" && GameManager.Instance.bossesdead == 1)
        {
            Level2Entrence = true;
            enterLevelText.SetActive(true);
        }
        //If not prompt to go to another level
        else if (collision.tag == "LevelTwo" && GameManager.Instance.bossesdead < 1)
            goLevel1.SetActive(true);

        //If correct boss is killed for previous lvl you're allowed to enter
        if (collision.tag == "LevelThree" && GameManager.Instance.bossesdead == 2)
        {
            Level2Entrence = true;
            enterLevelText.SetActive(true);
        }
        //If not prompt to go to another level
        else if (collision.tag == "LevelThree" && GameManager.Instance.bossesdead < 1)
            goLevel1.SetActive(true);
        else if (collision.tag == "LevelThree" && GameManager.Instance.bossesdead < 2)
            goLevel2.SetActive(true);

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        enterLevelText.SetActive(false);
        goLevel1.SetActive(false);
        goLevel2.SetActive(false);

        Level1Entrence = false;
        Level2Entrence = false;
        Level3Entrence = false;
    }

}

