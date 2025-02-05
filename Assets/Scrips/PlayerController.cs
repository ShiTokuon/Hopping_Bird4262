using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using SgLib;

public class PlayerController : MonoBehaviour
{
    public static event System.Action PlayerDied;

    public float jumpForce = 13f; // ジャンプパワー

    public float rotateAngle = 360f; // プレイヤーの回転角

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
    public AnimationClip jump;
    public AnimationClip rotate;

    private Rigidbody rigid;
    private Animator anim;
    private int turn = 1;
    private bool isFinishRotate = false;
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
        cameraController = Camera.main.GetComponent<CameraController>();

        anim = player.GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();

    }
    void Update()
    {
        if (Camera.main.WorldToScreenPoint(transform.position).y < -30 && GameManager.Instance.GameState != GameState.GameOver)
        {
            playerFallDown = true;
            anim.Play(rotate.name);
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
        //SoundManager.Instance.PlaySound(SoundManager.Instance.flap);
        StartCoroutine(AddVelocityForPlayer()); //add velocity for player

        if (!isFinishRotate)
        {
            isFinishRotate = true;
            StartCoroutine(RotateParentPlayer()); //rotate player
        }
    }

    IEnumerator AddVelocityForPlayer()
    {
        yield return new WaitForFixedUpdate();
        rigid.velocity = new Vector3(0, jumpForce, 0);
        anim.SetTrigger(jump.name);
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

                Destroy(other.gameObject);

                //SoundManager.Instance.PlaySound(SoundManager.Instance.hit);
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
            //SoundManager.Instance.PlaySound(SoundManager.Instance.hit);
        }
    }

    IEnumerator RotateParentPlayer()
    {
        turn = turn * (-1);

        float firstCurrentAngle = transform.eulerAngles.y; //Y rotation = 0
        while (firstCurrentAngle < rotateAngle && GameManager.Instance.GameState == GameState.Playing) //Rotate 
        {
            float rotateAmount = 200 * Time.deltaTime;
            firstCurrentAngle += rotateAmount;
            if (turn < 0)
            {
                transform.Rotate(Vector3.up * rotateAmount);
            }
            else
            {
                transform.Rotate(Vector3.down * rotateAmount);
            }
            yield return null;
        }
        if (turn < 0)
        {
            transform.eulerAngles = new Vector3(0, rotateAngle, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, -rotateAngle, 0);
        }


        float secondCurrentAngle = transform.eulerAngles.y; //Y rotation = rotateAngle
        if (turn < 0)
        {
            while (secondCurrentAngle > 0 && GameManager.Instance.GameState == GameState.Playing)
            {
                // 回転スピード
                float rotateAmount = 400 * Time.deltaTime;
                secondCurrentAngle -= rotateAmount;
                transform.Rotate(Vector3.down * rotateAmount);
                yield return null;
            }
        }
        else
        {
            while (secondCurrentAngle < 360 && GameManager.Instance.GameState == GameState.Playing)
            {
                // 回転スピード
                float rotateAmount = 400 * Time.deltaTime;
                secondCurrentAngle += rotateAmount;
                transform.Rotate(Vector3.up * rotateAmount);
                yield return null;
            }
        }

        transform.eulerAngles = new Vector3(0, 0, 0);

        isFinishRotate = false;
    }

    IEnumerator WaitToDisableKinematic()
    {
        yield return new WaitForSeconds(0.5f);
        anim.Play(rotate.name);
        rigid.isKinematic = false;
    }
}
