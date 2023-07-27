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

    bool InPauseMenu
    {
        get {
            return GameController.Instance.GetCurrentGameState.GetType()
            == typeof(PauseGameState);
        }
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

    bool pauseResume = false;

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

        if(GameController.Instance.GetCurrentGameState.GetType()
            == typeof(PauseGameState) && panelGo.activeInHierarchy)
        {
            panelGo.SetActive(false);
            StopAllCoroutines();
            pauseResume = true;
        }
        else if(GameController.Instance.LastGameState &&
            GameController.Instance.LastGameState.GetType()
            == typeof(PauseGameState) && pauseResume)
        {
            panelGo.SetActive(true);
            StartCoroutine(PrintDialogue(lastText, lastIterator));
            pauseResume = false;
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

    int lastIterator = 0;
    public IEnumerator PrintDialogue(string Text, int Iterator = 0)   //Give us the slow typing effect
    {
        panelGo.SetActive(true);
        isPrinting = true;
        if(Iterator == 0)
        {
            lastText = Text;
            dialogueText.text = "";
        }
        while (dialogueText.text.Length < Text.Length)
        {
            if (InPauseMenu)
            {
                break;
            }
            bool LongPause = false;
            if (Text[Iterator] == COMMA || Text[Iterator] == FULL_STOP)
            {
                LongPause = true;
            }
            dialogueText.text += Text[Iterator];
            Iterator++;
            lastIterator = Iterator;
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
