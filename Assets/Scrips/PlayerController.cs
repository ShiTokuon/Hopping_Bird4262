using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public float jumpForce = 13f; // ジャンプパワー

    private Rigidbody rigid;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            Flap();
        }

        // Fix position
        transform.position = new Vector3(0, transform.position.y, 0);
    }

    void Flap()
    {
        StartCoroutine(AddVelocityForPlayer()); //add velocity for player

    }

    IEnumerator AddVelocityForPlayer()
    {
        yield return new WaitForFixedUpdate();
        rigid.velocity = new Vector3(0, jumpForce, 0);
    }

}
