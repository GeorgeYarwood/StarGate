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

    void KillAllEnemies()
    {
        //This is debug only so performance don't matter
        EnemyBase[] AllEnemies = FindObjectsOfType<EnemyBase>();
        foreach(EnemyBase Enemy in AllEnemies)
        {
            Destroy(Enemy.gameObject);
        }
        LaserProjectile[] AllProjectiles = FindObjectsOfType<LaserProjectile>();
        foreach(LaserProjectile Projectile in AllProjectiles)
        {
            Destroy(Projectile.gameObject);
        }
    }

    void TraverseLevels(int Level)
    {
        KillAllEnemies();
        GameController.Instance.ResetAllLevels();
        GameController.CurrentLevel = Level - 1;
        AudioManager.Instance.StopAllMusicLoops();
        GameController.Instance.GoToState(GameStates.FLYING);
    }

    void Update()
    {
#if UNITY_EDITOR
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
#endif
    }
}
