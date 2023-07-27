using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialoguePanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] GameObject panelGo;    //What we hide and show
    public bool PanelActive
    {
        get { return panelGo.activeInHierarchy; }
    }
    [SerializeField] AudioClip characterBlipSfx;

    const float PRINTING_SPEED = 0.1f;
    const float PUNCTUATION_SPEED = 0.3f;
    const float AUTO_CLOSE_TIME = 2.5f;

    const char COMMA = ',';
    const char FULL_STOP = '.';

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

        if(!isPrinting && panelGo.activeInHierarchy)    //Close the panel after time
        {
            StartCoroutine(AutoClosePanel());
        }
    }

    IEnumerator AutoClosePanel()
    {
        float ThisTime = AUTO_CLOSE_TIME;
        while (ThisTime > 0)
        {
            ThisTime -= 1.0f * Time.deltaTime;
            yield return null;
        }
        if(!isPrinting)
        {
            panelGo.SetActive(false);
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
            bool LongPause = false;
            if (Text[Iterator] == COMMA || Text[Iterator] == FULL_STOP)
            {
                LongPause = true;
            }
            dialogueText.text += Text[Iterator];
            Iterator++;
            AudioManager.Instance.PlayAudioClip(characterBlipSfx);
            if (LongPause)
            {
                yield return new WaitForSeconds(PUNCTUATION_SPEED);
            }
            else
            {
                yield return new WaitForSeconds(PRINTING_SPEED);
            }
        }
        isPrinting = false;
    }
}
