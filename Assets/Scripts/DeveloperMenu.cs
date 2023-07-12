using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeveloperMenu : MonoBehaviour
{
    bool menuOpen = false;

    void OnGUI()
    {
        if (menuOpen)
        {
            if (GUILayout.Button("Go to next level"))
            {
                GameController.Instance.OnLevelComplete();
            }
        }
    }

    void Update()
    {
        if (Input.GetButtonDown(InputHolder.DEVELOPER_MENU))
        {
            menuOpen = !menuOpen;  
        }
    }
}
