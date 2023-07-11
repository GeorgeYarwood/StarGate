using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    List<EnemyBase> enemiesInScene = new List<EnemyBase>();
    public List<EnemyBase> EnemiesInScene
    {
        get { return enemiesInScene; }
        set {  enemiesInScene = value; }
    }
}
