using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using SgLib;

public enum GameState
{
    Prepare,
    Playing,
    Paused,
    PreGameOver,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event System.Action<GameState, GameState> GameStateChanged = delegate { };

    public GameState GameState
    {
        get
        {
            return _gameState;
        }
        private set
        {
            if (value != _gameState)
            {
                GameState oldState = _gameState;
                _gameState = value;

                GameStateChanged(_gameState, oldState);
            }
        }
    }

    private GameState _gameState = GameState.Prepare;

    public static int GameCount
    {
        get { return _gameCount; }
        private set { _gameCount = value; }
    }

    private static int _gameCount = 0;

    [Header("Set the target frame rate for this game")]
    public int targetFrameRate = 60;

    // UIマネージャー
    [Header("Gameplay Preferences")]
    public UIManager uIManager;
    public GameObject EnemyPrefab;
    public GameObject parentPlayer;
    public GameObject Ground;
    [Header("Obstacles")]
    public GameObject normalObstacle;

    // ゲーム開始時に生成する障害物の数
    [Header("Gameplay Config")]
    public int initialObstacle = 3;
    // 障害物間のスペース
    public int space = 7;

    // 障害物破棄カウント
    public int obstacleCounter = 4;

    // 障害物の最大揺れ範囲
    public float maxObstacleFluctuationRange = 4;

    // 障害物の最小揺れ範囲
    public float minObstacleFluctuationRange = 3;

    // スコアの値で障害物の速度を減少
    public int scoreToUpdateValue = 10;

    public float decreaseObstacleSpeedValue = 0.05f;

    // 障害物の最小速度係数
    public float minObstacleSpeedFactor = 1f;

    // 障害物の最小速度
    public float maxObstacleSpeedFactor = 1.5f;

    // 障害物の最大速度係数
    public float minimumMinObstacleSpeedFactor = 0.4f;

    // 最小速度の下限
    public float minimumMaxObstacleSpeedFactor = 0.7f;

    private List<GameObject> listObstacle = new List<GameObject>();
    private GameObject obstaclePrefab;
    private GameObject currentObstacle;
    private Vector3 obstaclePosition;
    private Vector3 addedPosition;
    private bool hasCheckedScore = false;
    private int listIndex = 0;
    private int listDestroyIndex = 0;

    // 敵の出現確率パラメータ
    [Range(0f, 1f)]
    public float EnemyFrequecy;

    void OnEnable()
    {
        PlayerController.PlayerDied += PlayerController_PlayerDied;
    }

    void OnDisable()
    {
        PlayerController.PlayerDied -= PlayerController_PlayerDied;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(Instance.gameObject);
            Instance = this;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void PlayerController_PlayerDied()
    {
        GameOver();
    }

    void Start()
    {
        GameState = GameState.Prepare;
        Application.targetFrameRate = targetFrameRate;
        ScoreManager.Instance.Reset();

        // ランダムなオブジェクトを配置
        RandomObstacleType();

        Vector3 firstObstaclePos = Ground.transform.position + new Vector3(0, 13f, 0);
        currentObstacle = Instantiate(obstaclePrefab, firstObstaclePos, Quaternion.identity) as GameObject;
        currentObstacle.GetComponent<ObstacleController>().fluctuationRange = Random.Range(minObstacleFluctuationRange, maxObstacleFluctuationRange);
        currentObstacle.GetComponent<ObstacleController>().movingSpeed = Random.Range(minObstacleSpeedFactor, maxObstacleSpeedFactor);
        currentObstacle.transform.parent = transform;
        listObstacle.Add(currentObstacle);


        addedPosition = new Vector3(0, space, 0);
        // 次の障害物を生成
        obstaclePosition = currentObstacle.transform.position + addedPosition;

        for (int i = 0; i < initialObstacle; i++)
        {
            CreateObstacle();
        }

        StartCoroutine(GenerateObstacle());

        if (SoundManager.Instance.background != null)
            SoundManager.Instance.PlayMusic(SoundManager.Instance.Title);
    }

    public void StartGame()
    {
        GameState = GameState.Playing;

        if (SoundManager.Instance.background != null)
            SoundManager.Instance.StopMusic();
        if (SoundManager.Instance.background != null)
            SoundManager.Instance.PlayMusic(SoundManager.Instance.background);
    }

    public void GameOver()
    {
        GameState = GameState.GameOver;
        GameCount++;

        if (SoundManager.Instance.background != null)
            SoundManager.Instance.StopMusic();
    }

    public void RestartGame(float delay = 0)
    {
        StartCoroutine(CRRestartGame(delay));
    }

    IEnumerator CRRestartGame(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //Create obstacle
    void CreateObstacle()
    {
        RandomObstacleType();

        currentObstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity) as GameObject;
        currentObstacle.GetComponent<ObstacleController>().fluctuationRange = Random.Range(minObstacleFluctuationRange, maxObstacleFluctuationRange);
        currentObstacle.GetComponent<ObstacleController>().movingSpeed = Random.Range(minObstacleSpeedFactor, maxObstacleSpeedFactor);
        currentObstacle.transform.parent = transform;

        CreateEnemy();

        listObstacle.Add(currentObstacle);

        obstaclePosition = currentObstacle.transform.position + addedPosition;

    }

    IEnumerator GenerateObstacle()
    {
        while (GameState != GameState.GameOver)
        {
            // 障害物のスコアの加算
            if (parentPlayer.transform.position.y > listObstacle[listIndex].transform.position.y)
            {
                ScoreManager.Instance.AddScore(1);
                hasCheckedScore = false;
                CreateObstacle();
                listIndex++;
            }

            // 一定の障害物を通過したら障害物を消去
            if (ScoreManager.Instance.Score > obstacleCounter)
            {
                Ground.transform.position = listObstacle[listDestroyIndex].transform.position;
                Destroy(listObstacle[listDestroyIndex]);
                listDestroyIndex++;
                obstacleCounter++;
            }


            if (ScoreManager.Instance.Score != 0 && ScoreManager.Instance.Score % scoreToUpdateValue == 0 && !hasCheckedScore)
            {
                hasCheckedScore = true;

                minObstacleSpeedFactor -= decreaseObstacleSpeedValue;
                maxObstacleSpeedFactor -= decreaseObstacleSpeedValue;

                if (minObstacleSpeedFactor <= minimumMinObstacleSpeedFactor)
                {
                    minObstacleSpeedFactor = minimumMinObstacleSpeedFactor;
                }

                if (maxObstacleSpeedFactor <= minimumMaxObstacleSpeedFactor)
                {
                    maxObstacleSpeedFactor = minimumMaxObstacleSpeedFactor;
                }
            }


            yield return null;
        }
    }

    void CreateEnemy()
    {
        float EnemyProbability = Random.Range(0f, 1f);
        if (EnemyProbability <= EnemyFrequecy)
        {
            Vector3 EnemyPosition = currentObstacle.transform.position + new Vector3(0, space / 2f, 0);
            GameObject currentEnemy = Instantiate(EnemyPrefab, EnemyPosition, Quaternion.Euler(0, 0, 45)) as GameObject;

            float EnemyFluctuationRange = Random.Range(minObstacleFluctuationRange, maxObstacleFluctuationRange);

            currentEnemy.GetComponent<EnemyController>().fluctuationRange = EnemyFluctuationRange;
            currentEnemy.GetComponent<EnemyController>().movingSpeed = Random.Range(minObstacleSpeedFactor * 2, maxObstacleSpeedFactor * 2);

            int indexPosition = Random.Range(0, 2);
            if (indexPosition == 0)
            {
                currentEnemy.transform.position += new Vector3(-EnemyFluctuationRange, 0, 0);
            }
            else
            {
                currentEnemy.transform.position += new Vector3(EnemyFluctuationRange, 0, 0);
            }
            currentEnemy.transform.parent = currentObstacle.transform;
        }
    }

    void RandomObstacleType()
    {

        obstaclePrefab = normalObstacle;

    }
}
