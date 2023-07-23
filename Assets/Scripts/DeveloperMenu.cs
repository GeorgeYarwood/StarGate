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

            if (GUILayout.Button("Level 1"))
            {
                TraverseLevels(1);
            }
            if (GUILayout.Button("Level 2"))
            {
                TraverseLevels(2);
            }
            if (GUILayout.Button("Level 3"))
            {
                TraverseLevels(3);
            }
            if (GUILayout.Button("Level 4"))
            {
                TraverseLevels(4);
            }
            if (GUILayout.Button("Level 5"))
            {
                TraverseLevels(5);
            }
            if (GUILayout.Button("Level 6"))
            {
                TraverseLevels(6);
            }
            if (GUILayout.Button("Level 7"))
            {
                TraverseLevels(7);
            }
            if (GUILayout.Button("Level 8"))
            {
                TraverseLevels(8);
            }
            if (GUILayout.Button("Level 9"))
            {
                TraverseLevels(9);
            }
            if (GUILayout.Button("Level 10"))
            {
                TraverseLevels(10);
            }
        }
    }

    void TraverseLevels(int Level)
    {
        GameController.Instance.ResetAllLevels();
        GameController.CurrentLevel = Level;
        GameController.Instance.OnLevelComplete();
    }

    void Update()
    {
        if (Input.GetButtonDown(InputHolder.DEVELOPER_MENU))
        {
            menuOpen = !menuOpen;
            Cursor.visible = menuOpen;
            if (menuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
