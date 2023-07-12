using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialoguePanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject panelGo;    //What we hide and show
    [SerializeField] AudioClip characterBlipSfx;

    const float PRINTING_SPEED = 0.2f;

    bool isPrinting = false;
    public bool IsPrinting
    {
        get { return isPrinting; }
    }

    string lastText;

    static DialoguePanel instance;
    public static DialoguePanel Instance
    {
        get { return instance; }
    }

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
        panelGo.SetActive(false);
    }

    void Update()
    {
        if(Input.GetButtonDown(InputHolder.SKIP_DIALOGUE_BUTTON))
        {
            if (isPrinting)
            {
                StopAllCoroutines();
                dialogueText.text = lastText;
                isPrinting = false;
            }
            else
            {
                panelGo.SetActive(false);
            }
        }
    }

    public IEnumerator PrintDialogue(string Text)   //Give us the slow typing effect
    {
        panelGo.SetActive(true);
        isPrinting = true;
        lastText = Text;
        dialogueText.text = "";
        int Iterator = 0;
        while (dialogueText.text.Length < Text.Length)
        {
            dialogueText.text += Text[Iterator];
            Iterator++;
            AudioManager.Instance.PlayAudioClip(characterBlipSfx);
            yield return new WaitForSeconds(PRINTING_SPEED);
        }
        isPrinting = false;
    }
}
