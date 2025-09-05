using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(PlatformEffector2D))]
public class puzzlestring : MonoBehaviour
{

    void Awake()
    {
        var boxCollider = GetComponent<BoxCollider2D>();
        var effector = GetComponent<PlatformEffector2D>();

        if (boxCollider != null)
            boxCollider.enabled = true;

        if (effector != null)
        {
            effector.useOneWay = true;
            effector.surfaceArc = 170f;
        }
    }

}
