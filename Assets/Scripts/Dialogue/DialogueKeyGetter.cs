using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueKeyGetter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI inputPromptText;

    public void Start()
    {
        //This isn't possible :(
        //inputPromptText.text = InputHolder.SKIP_DIALOGUE_BUTTON).ToString();
    }
}
