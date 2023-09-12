using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum PowerUpType 
{
    RAPID_FIRE,
    SPEED_BOOST
}

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

    public struct PowerUpContainer
    {
        public Action powerUpStart;
        public Action powerUpEnd;

        public PowerUpContainer(Action StartAction, Action EndAction)
        {
            powerUpStart = StartAction;
            powerUpEnd = EndAction;
        }
    }

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
            ThisPowerUpContainer.powerUpStart.Invoke();
        }
    }

    public void OnPowerUpEnd()
    {
        AudioManager.Instance.PlayAudioClip(powerUpEndClip);
    }

    void InitPowerUpList() //New powerups get added here, along with their action to run
    {
        allPowerups.Add(PowerUpType.RAPID_FIRE, new PowerUpContainer(PlayerShip.Instance.RapidFirePowerUp, PlayerShip.Instance.EndRapidFirePowerUp));
        allPowerups.Add(PowerUpType.SPEED_BOOST, new PowerUpContainer(PlayerShip.Instance.SpeedBoostPowerUp, PlayerShip.Instance.EndSpeedBoostPowerUp));
    }
    
    public void EndAllPowerUps()
    {
        for(int p = 0; p < allPowerups.Count; p++)
        {
            allPowerups.ElementAt(p).Value.powerUpEnd.Invoke();
        }
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
