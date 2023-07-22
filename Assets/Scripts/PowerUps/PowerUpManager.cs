using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum PowerUpType 
{
    RAPID_FIRE,
    SPEED_BOOST
}

public class PowerUpManager : MonoBehaviour
{
    static PowerUpManager instance;
    public static PowerUpManager Instance
    {
        get { return instance; }
    }

    const int POWER_UP_SPAWN_CHANCE = 10;

    Dictionary<PowerUpType, Action> allPowerups = new Dictionary<PowerUpType, Action>();

    List<PowerUp> spawnedPowerUps = new List<PowerUp>();

    [SerializeField] AudioClip powerUpEndClip;

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

        InitPowerUpList();
    }

    public void ApplyPowerUp(PowerUpType ThisPowerUp)
    {
        if(allPowerups.TryGetValue(ThisPowerUp, out Action PowerUpAction))
        {
            PowerUpAction.Invoke();
        }
    }

    public void OnPowerUpEnd()
    {
        AudioManager.Instance.PlayAudioClip(powerUpEndClip);
    }

    void InitPowerUpList() //New powerups get added here, along with their action to run
    {
        allPowerups.Add(PowerUpType.RAPID_FIRE, PlayerShip.Instance.RapidFirePowerUp);
        allPowerups.Add(PowerUpType.SPEED_BOOST, PlayerShip.Instance.SpeedBoostPowerUp);
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
                Destroy(spawnedPowerUps[p]);
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
