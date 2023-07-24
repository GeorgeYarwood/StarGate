using UnityEngine;
//These can be placed on enemies that are in levels, or in the level itself
//Some points, such as on encounter/death, are only applicable to enemies
public enum DialogueQueuePoint 
{ 
    ON_ENCOUNTER,
    ON_DEATH,
    LEVEL_START,
    LEVEL_END,
    AT_HP
}

[CreateAssetMenu(menuName = "Level tools/New dialogue...")]
public class Dialogue : ScriptableObject
{
    [SerializeField] string dialogueString;
    public string DialogueString
    {
        get { return dialogueString; }
    }

    [SerializeField] DialogueQueuePoint whenToPlay;
    public DialogueQueuePoint WhenToPlay
    {
        get { return whenToPlay; }
    }

    [SerializeField] bool isEnemy;
    public bool IsEnemy
    {
        get { return isEnemy; }
    }

    [SerializeField] int hpPoint;
    public int HpPoint
    {
        get { return hpPoint; }
    }

    bool hasBeenPlayed = false;
    public bool HasBeenPlayed
    {
        get { return hasBeenPlayed; }
        set { hasBeenPlayed = value; }
    }

    void Awake()
    {
        hasBeenPlayed = false;
    }
}
