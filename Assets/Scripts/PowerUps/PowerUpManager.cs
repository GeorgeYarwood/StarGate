using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum PowerUpType 
{
    RAPID_FIRE,
    SPEED_BOOST,
    DOUBLE_LASER
}

public struct PowerUpContainer
{
    public float Time;

    public PowerUpContainer(float SetTime)
    {
        Time = SetTime;
    }
}

//Powerup ideas
//Invincibility
//Double lasers

public class PowerUpManager : MonoBehaviour
{
    TestInterface test;
    static PowerUpManager instance;
    public static PowerUpManager Instance
    {
        get { return instance; }
    }

    const int POWER_UP_SPAWN_CHANCE = 10;

    Dictionary<PowerUpType, PowerUpContainer> allPowerups = new Dictionary<PowerUpType, PowerUpContainer>();

    List<PowerUp> spawnedPowerUps = new List<PowerUp>();

    [SerializeField] AudioClip powerUpEndClip;



    void Start()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        InitPowerUpList();
    }

    public void ApplyPowerUp(PowerUpType ThisPowerUp)
    {
        if(allPowerups.TryGetValue(ThisPowerUp, out PowerUpContainer ThisPowerUpContainer))
        {
            PlayerController.ApplyPowerup(ThisPowerUp, ThisPowerUpContainer);
        }
    }

    public void OnPowerUpEnd()
    {
        AudioManager.Instance.PlayAudioClip(powerUpEndClip);
    }

    void InitPowerUpList() //New powerups get added here, along with their duration
    {
        allPowerups.Add(PowerUpType.RAPID_FIRE, new PowerUpContainer(5.0f));
        allPowerups.Add(PowerUpType.SPEED_BOOST, new PowerUpContainer(8.0f));
        allPowerups.Add(PowerUpType.DOUBLE_LASER, new PowerUpContainer(7.5f));
    }
    
    public void EndAllPowerUps()
    {
        PlayerController.EndAllPowerups();
    }

    public void DropRandomPowerUpAtPosition(Vector2 PositionToSpawn, PowerUp PowerUpToDrop, bool ForceSpawn = false)
    {
        int WillSpawn = Random.Range(0, POWER_UP_SPAWN_CHANCE);
        if(WillSpawn != 0 && !ForceSpawn)
        {
            return;
        }

        PowerUp ThisPowerUp = Instantiate(PowerUpToDrop, PositionToSpawn, Quaternion.identity);
        spawnedPowerUps.Add(ThisPowerUp);
    }

    public void ClearPowerUps()
    {
        for(int p = 0; p < spawnedPowerUps.Count; p++)
        {
            if (spawnedPowerUps[p])
            {
                Destroy(spawnedPowerUps[p].gameObject);
            }
        }

        CleanList();
    }

    void Update()
    {
        CleanList();
    }

    void CleanList()
    {
        for (int p = 0; p < spawnedPowerUps.Count; p++)
        {
            if (!spawnedPowerUps[p])
            {
                spawnedPowerUps.RemoveAt(p);
            }
        }
    }
}
