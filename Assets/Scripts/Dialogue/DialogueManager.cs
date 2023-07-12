using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    static DialogueManager instance;
    public static DialogueManager Instance
    {
        get { return instance; }
    }

    [SerializeField] DialoguePanel dialogueDisplay;

    Queue<Dialogue> pendingDialogue = new Queue<Dialogue>();

    void Start()
    {
        if(instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void PlayDialogue(Dialogue DialogueToPlay)
    {
        pendingDialogue.Enqueue(DialogueToPlay);   
    }

    void ProcessPendingDialogue()
    {
        if (pendingDialogue.Count > 0 && !DialoguePanel.Instance.IsPrinting)
        {
            StartCoroutine(dialogueDisplay.PrintDialogue(
                pendingDialogue.Dequeue().DialogueString));
        }
    }

    void Update()
    {
        ProcessPendingDialogue();
    }
}
