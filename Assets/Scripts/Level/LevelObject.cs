using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level tools/New level...")]
public class LevelObject : ScriptableObject
{
    [SerializeField] AudioClip levelSong;
    public AudioClip LevelSong
    {
        get { return levelSong; }
    }

    [SerializeField] int enemiesPerLevel;
    public int EnemiesPerLevel
    {
        get { return enemiesPerLevel; }
    }
    [SerializeField] EnemyBase[] enemyTypesToSpawn = new EnemyBase[2];
    public EnemyBase[] EnemyTypesToSpawn
    {
        get { return enemyTypesToSpawn; }
    }
    [SerializeField] LevelObject subLevel;
    public LevelObject SubLevel
    {
        get { return subLevel; }
    }

    [SerializeField] LevelObject parentLevel;
    public LevelObject ParentLevel
    {
        get { return parentLevel; }
    }

    public bool IsSubLevel
    {
        get { return parentLevel; } //If this level has a parent level, it is a sublevel
    }

    bool isInitialised = false;
    public bool IsInitialised
    {
        get { return isInitialised; }
        set { isInitialised = value; }
    }

    [SerializeField] bool hasSublevel = true;
    public bool HasSublevel
    {
        get { return hasSublevel; }
    }

    List<EnemyBase> enemiesInScene = new List<EnemyBase>();
    public List<EnemyBase> EnemiesInScene
    {
        get { return enemiesInScene; }
        set { enemiesInScene = value; }
    }

    [SerializeField] Dialogue[] levelDialogue = new Dialogue[3];
    public Dialogue[] LevelDialogue
    {
        get { return levelDialogue; }
    }

    [SerializeField] Color backgroundColour = Color.black;
    public Color BackgroundColour
    {
        get { return backgroundColour; }
    }
    [SerializeField] Color starsColour = Color.red + Color.yellow;
    public Color StarsColour
    {
        get { return starsColour; }
    }
    [SerializeField] Color foregroundColour = Color.red + Color.yellow;
    public Color ForegroundColour
    {
        get { return foregroundColour; }
    }

    [SerializeField] bool useParentLevelColour = true;
    public bool UseParentLevelColour
    {
        get { return useParentLevelColour; }
    }

    [SerializeField] PlayerControllerType playerType = PlayerControllerType.SHIP;
    public PlayerControllerType PlayerType
    {
        get { return playerType; }
    }

    [SerializeField] GameObject[] setPieces = new GameObject[0];
    public GameObject[] SetPieces
    {
        get { return setPieces; }
    }

    List<GameObject> setPiecesInScene = new List<GameObject>();
    public List<GameObject> SetPiecesInScene
    {
        get { return setPiecesInScene; }
        set {  setPiecesInScene = value; }
    }
}
