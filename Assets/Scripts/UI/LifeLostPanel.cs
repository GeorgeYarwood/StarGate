using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LifeLostPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI livesRemainingText;

    const string LIVES_REMAINING_TEXT = " LIVES REMAINING";

    public void SetRemainingLives(int Lives)
    {
        livesRemainingText.text = Lives.ToString() + LIVES_REMAINING_TEXT;
    }
}
