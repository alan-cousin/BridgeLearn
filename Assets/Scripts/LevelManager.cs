using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public static class ConfigValues
{
    public static int[] SPHERE_SCORES = { 1, 10, 20, 30 };
    public static int[] CAPSULE_SCORES = { 2, 12, 22, 32 };
    public static int LEVEL_PASS_SCORE = 100;
    public static int GAME_MAX_SCORE = 400;
    public static int[] PLAYER_APPEARANCE = { 3, 5, 8, 10 };
    public static int CELL_COUNT_X = 20;
    public static int CELL_COUNT_Y = 20;
    public static float CELL_SIZE_X = 10;
    public static float CELL_SIZE_Y = 10;
    public static float PANEL_SIZE_Y = 200;
    public static float PANEL_SIZE_X= 200;
    public static int MAX_LEVEL = 3;
}
public class LevelManager : MonoBehaviour
{
    
    public static LevelManager Instance { get; private set; }

    /// <summary>
    /// UI Objects
    /// </summary>
    public Text m_lblScore;
    public Text m_lblLevel;

    public GameObject m_panelFinish;
    public Text m_lblAttempt;
    public Text m_lblPushed;
    public Text m_lblScored;
    public Text m_title;
    /// <summary>
    /// obstacle prefabs
    /// </summary>
    public List<GameObject> m_listScorePrefabs;


    public bool GameState { get; private set; }

    /// <summary>
    /// map status
    /// </summary>
    private ScoreObject[] m_mapObstacles;

    public int AttemptCount { get; private set; }
    public int PushedCount { get; private set; }
    public int Level { get; private set; }
    public int Score { get; private set; }
    public Obstacle LastObject { get; private set; }

    public void Restart()
    {
        
        Level = 0;
        Score = 0;
        LastObject = Obstacle.NONE;
        PushedCount = 0;
        AttemptCount = 0;
        for (int i = 0; i < 400; i++)
        {
            if (m_mapObstacles[i] != null)
                GameObject.Destroy(m_mapObstacles[i].gameObject);
            
            m_mapObstacles[i] = null;
        }
        m_lblScore.text = "0";
        m_lblLevel.text = "Level 1";
        GameState = true;
        m_panelFinish.SetActive(false);
        SpawnObjects();
    }

    public bool MovableTo(int f_xpos, int f_ypos)
    {
        if (!GameState) return false;
        if (f_xpos < 0 || f_xpos >= ConfigValues.CELL_COUNT_X)
            return false;
        if (f_ypos < 0 || f_ypos >= ConfigValues.CELL_COUNT_Y)
            return false;

        Obstacle obj_type = GetObjectInfo(f_xpos, f_ypos);
        if (obj_type == Obstacle.CUBE)
            return false;
        return true;
    }
    int GetLevelScore(Obstacle f_obj)
    {
        if(f_obj == Obstacle.CAPSULE)
            return ConfigValues.CAPSULE_SCORES[Level];
        if(f_obj == Obstacle.SPHERE)
            return ConfigValues.SPHERE_SCORES[Level];
        return 0;
    }
    void CalcScore(Obstacle f_pushed)
    {
        int _levelScore = GetLevelScore(f_pushed);
        Score = Score + (LastObject == f_pushed ? -2 * _levelScore : _levelScore);
        LastObject = f_pushed;
        PushedCount = PushedCount + 1;
    }
    void StopGame()
    {
        m_lblAttempt.text = AttemptCount.ToString();
        m_lblPushed.text = PushedCount.ToString();
        m_lblScored.text = Score.ToString();
        m_title.text = Score >= ConfigValues.GAME_MAX_SCORE ? "Congratulation!" : "Failed";
        m_panelFinish.SetActive(true);
        GameState = false;
        SaveScore();
    }
    struct SaveData
    {
        public int score;
        public int attempt;
        public int pushed;
    }
    void SaveScore()
    {
        string path = Application.persistentDataPath ;

        string file_name = System.DateTime.Now.ToShortDateString() + "-"+System.DateTime.Now.ToShortTimeString() + ".txt";
        file_name = file_name.Replace(":", "-");
        file_name = file_name.Replace("/", "-");
        string full_path = path +"/"+ file_name;
        Debug.Log(full_path);
        if (!File.Exists(full_path))
        {
            SaveData save_data = new SaveData();
            save_data.score = Score;
            save_data.pushed = PushedCount;
            save_data.attempt = AttemptCount;
            string out_put = JsonUtility.ToJson(save_data);

            //File.WriteAllText(path + "Level01.txt", data);
            StreamWriter sr = new StreamWriter(full_path);
            sr.Write(out_put);
            sr.Close();
        }
    }
    void LevelCheck()
    {
        int _newLevel = (int)(Score / ConfigValues.LEVEL_PASS_SCORE);
        Level = Mathf.Max(Level, Mathf.Min(_newLevel, ConfigValues.MAX_LEVEL));
        if(Score >= ConfigValues.GAME_MAX_SCORE)
            StopGame();
    }

    void SpawnObjects()
    {
        int spawn_cnt = 1;// Random.Range(1, 4);
        for(int i = 0; i < spawn_cnt; i++)
        {
            int pos = Random.Range(0, 400);

            if (m_mapObstacles[pos] != null) continue;

            int type = Random.Range(1, 4);
            GameObject new_obstacle = GameObject.Instantiate(m_listScorePrefabs[type - 1]);
            m_mapObstacles[pos] = new_obstacle.GetComponent<ScoreObject>();
            /// initialize position
            Vector3 world_pos = Vector3.zero;
            world_pos.x = ((int)(pos / ConfigValues.CELL_COUNT_X)) * ConfigValues.CELL_SIZE_X + ConfigValues.CELL_SIZE_X / 2;
            world_pos.z = ((int)(pos % ConfigValues.CELL_COUNT_X)) * ConfigValues.CELL_SIZE_Y + ConfigValues.CELL_SIZE_Y / 2;
            world_pos.y = 0;
            new_obstacle.transform.position = world_pos;
        }
        
    }
    void DeleteObject(int f_xpos, int f_ypos)
    {
        int idx = f_xpos * ConfigValues.CELL_COUNT_X + f_ypos;
        if (m_mapObstacles[idx] == null) return;
        GameObject.Destroy(m_mapObstacles[idx].gameObject);
        m_mapObstacles[idx] = null;
    }
    Obstacle GetObjectInfo(int f_xpos, int f_ypos)
    {
        int idx = f_xpos * ConfigValues.CELL_COUNT_X + f_ypos;
        if (m_mapObstacles[idx] != null)
        {
            return m_mapObstacles[idx].m_type;
        }
        
        return Obstacle.NONE;
    }
    public Vector3 MoveTo(int f_xpos, int f_ypos)
    {
        AttemptCount = AttemptCount + 1;
        Obstacle obj_type = GetObjectInfo(f_xpos, f_ypos);
        if (obj_type != (int)Obstacle.NONE)
        {
            CalcScore(obj_type);
            DeleteObject(f_xpos, f_ypos);
            LevelCheck();

            UpdateUI();
        }
        SpawnObjects();
        Vector3 pos = Vector3.zero;
        if (CheckStaus(f_xpos, f_ypos))
        {
            pos.x = f_xpos * ConfigValues.CELL_SIZE_X + 5;
            pos.y = 0;
            pos.z = f_ypos * ConfigValues.CELL_SIZE_Y + 5;
        }
        else
        {
            StopGame();
        }

        return pos;
    }
    void UpdateUI()
    {
        m_lblLevel.text = "Level " + (Level + 1);
        m_lblScore.text = Score.ToString();
    }
    bool CheckStaus(int f_xpos, int f_ypos)
    {
        if (MovableTo(f_xpos - 1, f_ypos))
            return true;
        if (MovableTo(f_xpos + 1, f_ypos))
            return true;
        if (MovableTo(f_xpos, f_ypos - 1))
            return true;
        if (MovableTo(f_xpos, f_ypos + 1))
            return true;
        return false;
    }
    
    private void Awake()
    {
        Instance = this;
        m_mapObstacles = new ScoreObject[400];
        
        int state = System.DateTime.Now.Millisecond;
        Random.InitState(state);

       
    }
    // Start is called before the first frame update
    void Start()
    {
        Restart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
