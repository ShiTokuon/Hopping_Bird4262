using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public PlayerController playerController;
    public GameObject Ground;
    public float followUpSpeedFactor = 1;
    //public float smoothTime = 0.3f;

    [HideInInspector]
    public bool isStopMoving = false;

    public float shakeDuration = 0.1f;
    public float shakeAmount = 0.3f;
    public float decreaseFactor = 0.3f;

    private Transform playerTransform;
    private float speedGoUp;
    private float speedGoDown;
    private float followDownspeedFactor = 10;
    private float initialPlayerDistance;
    private float currentShakeDuration;
    private Vector3 originalShakePos;
    private bool isShaking = false;

    void Start()
    {
        playerTransform = playerController.transform;
        initialPlayerDistance = transform.position.y - playerTransform.transform.position.y;
    }

    void Update()
    {
        if (isShaking)
            return;

        if (isStopMoving)
            return;

        // プレイヤーの位置を取得
        Vector3 playerPosition = playerTransform.position;

        // プレイヤーとカメラの距離を計算
        float distance = transform.position.y - playerPosition.y;

        // カメラの上昇と下降の速度を計算
        speedGoUp = (initialPlayerDistance - distance) * followUpSpeedFactor;
        speedGoDown = Mathf.Min(-3f, (initialPlayerDistance - distance) * followDownspeedFactor);

        // プレイヤーの位置に基づいてカメラを移動
        if (distance < initialPlayerDistance - 1f)
        {
            // 上昇
            transform.position += new Vector3(0, speedGoUp * Time.deltaTime, 0);
        }
        else if (distance > initialPlayerDistance)
        {
            // 下降
            transform.position += new Vector3(0, speedGoDown * Time.deltaTime, 0);
        }
    }


    public void ShakeCamera()
    {
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        if (isShaking)
            yield break;

        isShaking = true;
        Vector3 originalShakePos = transform.position;
        float currentShakeDuration = shakeDuration;

        while (currentShakeDuration > 0)
        {
            transform.position = originalShakePos + Random.insideUnitSphere * shakeAmount;
            currentShakeDuration -= Time.deltaTime * decreaseFactor;
            yield return null;
        }

        transform.position = originalShakePos;
        isShaking = false;
    }
}
