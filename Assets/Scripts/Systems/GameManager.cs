using System.Collections.Generic;
using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    public GameObject Player;
    public static GameManager Instance { get; private set; }

    public List<Checkpoint> CheckPoints = new List<Checkpoint>();

    private int currentCheckpointIndex = 0;
    [SerializeField] private bool UpdateText = false;
    [SerializeField] private TextMeshProUGUI WitchKills;
    [SerializeField] private TextMeshProUGUI GolemKills;
    [SerializeField] private TextMeshProUGUI ZombieKills;
    [SerializeField] private TextMeshProUGUI BossWitchKills;
    [SerializeField] private TextMeshProUGUI BossGolemKills;
    [SerializeField] private GameObject Guide;
    [SerializeField] private Transform SpawnPoint;
    private int witchKills = 0;
    private int golemKills = 0;
    private int zombieKills = 0;
    private int bossWitchKills = 0;
    private int bossGolemKills = 0;

    private Checkpoint CurrentCheckPoint;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddToKilledWitch()
    {
        witchKills++;
        SetKillCounts();
        CheckProgress();
    }

    public void AddToKilledZombie()
    {
        zombieKills++;
        SetKillCounts();
        CheckProgress();
    }

    public void AddToKilledBossWitch()
    {
        bossWitchKills++;
        SetKillCounts();
        CheckProgress();
    }

    public void AddToKilledBossGolem()
    {
        bossGolemKills++;
        SetKillCounts();
        CheckProgress();
    }

    public void AddToKilledGolem()
    {
        golemKills++;
        SetKillCounts();
        CheckProgress();

    }
    public void SetKillCounts()
    {
        if (!UpdateText) return;
        WitchKills.text = witchKills.ToString();
        GolemKills.text = golemKills.ToString();
        BossWitchKills.text = bossWitchKills.ToString();
        BossGolemKills.text = bossGolemKills.ToString();
        ZombieKills.text = zombieKills.ToString();
    }
    public void CheckProgress()
    {
        if (currentCheckpointIndex >= CheckPoints.Count) return;

        var checkpoint = CheckPoints[currentCheckpointIndex];

        bool reachedWitchKills = witchKills >= checkpoint.witchKills;
        bool reachedGolemKills = golemKills >= checkpoint.golemKills;
        bool reachedZombieKills = zombieKills >= checkpoint.zombieKills;
        bool reachedBossWitchKills = bossWitchKills >= checkpoint.bossWitchKills;
        bool reachedBossGolemKills = bossGolemKills >= checkpoint.bossGolemKills;

        if (reachedWitchKills && reachedGolemKills && reachedZombieKills && reachedBossWitchKills && reachedBossGolemKills)
        {
            TriggerCheckpointEvent(checkpoint);
        }
    }

    private void TriggerCheckpointEvent(Checkpoint checkpoint)
    {
        if (CurrentCheckPoint == null || CurrentCheckPoint.hasBeenCleared)
        {
            CurrentCheckPoint = checkpoint;
            checkpoint.checkpointObject.SetActive(true);
            GameObject GuideObject = Instantiate(Guide, SpawnPoint.position, Quaternion.identity);
            MagicalCube mg = GuideObject.GetComponent<MagicalCube>();
            mg.player = Player.transform;
            mg.checkpoint = checkpoint.checkpointObject.transform;
            Debug.Log("Checkpoint Triggered: " + checkpoint.checkpointObject.name);
        }
    }

    public void MarkCheckpointAsCleared()
    {
        if (CurrentCheckPoint != null && !CurrentCheckPoint.hasBeenCleared)
        {
            CurrentCheckPoint.hasBeenCleared = true;
            Debug.Log("Checkpoint Cleared: " + CurrentCheckPoint.checkpointObject.name);

            // Move to the next checkpoint
            currentCheckpointIndex++;

            // Check progress again to see if we can move to the next one
            if (currentCheckpointIndex < CheckPoints.Count)
            {
                CheckProgress();
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GameManager gameManager = (GameManager)target;

            GUILayout.Space(10);
            GUILayout.Label("Development Controls", EditorStyles.boldLabel);

            if (GUILayout.Button("Add 1 Witch Kill"))
            {
                gameManager.AddToKilledWitch();
            }

            if (GUILayout.Button("Add 1 Golem Kill"))
            {
                gameManager.AddToKilledGolem();
            }

            if (GUILayout.Button("Add 1 Zombie Kill"))
            {
                gameManager.AddToKilledZombie();
            }

            if (GUILayout.Button("Add 1 Boss Witch Kill"))
            {
                gameManager.AddToKilledBossWitch();
            }

            if (GUILayout.Button("Add 1 Boss Golem Kill"))
            {
                gameManager.AddToKilledBossGolem();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Check Progress"))
            {
                gameManager.CheckProgress();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Trigger Next Checkpoint"))
            {
                if (gameManager.currentCheckpointIndex < gameManager.CheckPoints.Count)
                {
                    var nextCheckpoint = gameManager.CheckPoints[gameManager.currentCheckpointIndex];
                    gameManager.TriggerCheckpointEvent(nextCheckpoint);
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Mark Current Checkpoint as Cleared"))
            {
                gameManager.MarkCheckpointAsCleared();
            }
        }
    }
#endif
}

[System.Serializable]
public class Checkpoint
{
    public GameObject checkpointObject;
    public int witchKills;
    public int golemKills;
    public int zombieKills;
    public int bossWitchKills;
    public int bossGolemKills;
    public bool hasBeenCleared = false;
}
