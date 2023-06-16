using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Level tools/New level...")]
public class LevelObject : ScriptableObject
{
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

    List<EnemyBase> enemiesInScene = new List<EnemyBase>();
    public List<EnemyBase> EnemiesInScene
    {
        get { return enemiesInScene; }
        set {  enemiesInScene = value; }
    }

    [SerializeField] bool isSubLevel;
    public bool IsSubLevel
    {
        get { return isSubLevel; }
    }
}
