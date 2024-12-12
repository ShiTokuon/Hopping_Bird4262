using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public static event System.Action PlayerDied;

    public float jumpForce = 13f; // ジャンプパワー


    [HideInInspector]
    public bool hasStarted = false;
    [HideInInspector]
    public bool hitObstacle;
    [HideInInspector]
    public bool playerFallDown = false;
    [HideInInspector]
    public bool hasHitGround = false;

    [Header("Gameplay Preferences")]
    public GameManager gameManager;
    public GameObject player;

    private Rigidbody rigid;
    private int turn = 1;
    private CameraController cameraController;

    void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
    }

    void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
    }

    void GameManager_GameStateChanged(GameState newState, GameState oldState)
    {
        if (newState == GameState.Playing && oldState == GameState.Prepare)
        {
            if (!hasStarted)
            {
                hasStarted = true;
            }

            Flap();
        }
    }

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        cameraController = Camera.main.GetComponent<CameraController>();
    }
    void Update()
    {
        if (Camera.main.WorldToScreenPoint(transform.position).y < -30 && GameManager.Instance.GameState != GameState.GameOver)
        {
            playerFallDown = true;
            Die();
            Debug.Log("Player Fall Ground");
        }

        if (Input.GetMouseButtonDown(0) && GameManager.Instance.GameState == GameState.Playing)
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

    void Die()
    {
        // Fire event
        if (PlayerDied != null)
        {
            PlayerDied();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 障害物の当たり判定

        if (other.tag == "Enemy") //Hit gold
        {

            //Destroy(other.gameObject);
        }
        else
        {
            // Hit obstacles
            //if (!hitObstacle)
            //cameraController.ShakeCamera();
        }

        if (other.tag == "NormalObstacle") //hit obstacle
        {
            if (!hitObstacle)
            {
                hitObstacle = true;
                Die();
                cameraController.ShakeCamera();
                rigid.velocity = new Vector3(0, 0, 0);
                transform.position = new Vector3(0, transform.position.y, 0);

                //Create particle base on obstacle
                rigid.isKinematic = true;
                StartCoroutine(WaitToDisableKinematic());
            }
        }
    }
    void OnCollisionEnter(Collision col)
    {
        if (GameManager.Instance.GameState == GameState.GameOver && col.collider.tag.Equals("Ground") && !hasHitGround)
        {
            hasHitGround = true;
        }
    }

    IEnumerator WaitToDisableKinematic()
    {
        yield return new WaitForSeconds(0.5f);
        rigid.isKinematic = false;
    }
}
