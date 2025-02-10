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
            if (GUILayout.Button($"Toggle invincibility ({PlayerController.DebugInvincible})"))
            {
                PlayerController.DebugInvincible = !PlayerController.DebugInvincible;
            }
            if (GUILayout.Button("Go to next level"))
            {
                TraverseLevels(GameController.CurrentLevel + 1);
            }

            if (GUILayout.Button("Level 1"))
            {
                TraverseLevels(0);
            }
            if (GUILayout.Button("Level 2"))
            {
                TraverseLevels(1);
            }
            if (GUILayout.Button("Level 3"))
            {
                TraverseLevels(2);
            }
            if (GUILayout.Button("Level 4"))
            {
                TraverseLevels(3);
            }
            if (GUILayout.Button("Level 5"))
            {
                TraverseLevels(4);
            }
            if (GUILayout.Button("Level 6"))
            {
                TraverseLevels(5);
            }
            if (GUILayout.Button("Level 7"))
            {
                TraverseLevels(6);
            }
            if (GUILayout.Button("Level 8"))
            {
                TraverseLevels(7);
            }
            if (GUILayout.Button("Level 9"))
            {
                TraverseLevels(8);
            }
            if (GUILayout.Button("Level 10"))
            {
                TraverseLevels(9);
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

        PlayState PState = GameController.Instance.GetCurrentGameState as PlayState;
        PState.EndLevel(Debug: true);

        GameController.Instance.ResetAllLevels();
        GameController.CurrentLevel = Level;
        GameController.Instance.LoadCurrentLevel();
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
