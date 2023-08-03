using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldColourController : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] worldStars = new SpriteRenderer[2];
    [SerializeField] SpriteRenderer[] worldBackground = new SpriteRenderer[2];
    [SerializeField] SpriteRenderer[] worldForeground = new SpriteRenderer[5];

    static WorldColourController instance;
    public static WorldColourController Instance
    {
        get {  return instance;}
    }

    void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void SetWorldColours(Color StarsColour,  Color BackgroundColour, Color ForegroundColour)
    {
        for(int i = 0; i < worldStars.Length; i++)
        {
            worldStars[i].color = StarsColour;
        }

        for (int i = 0; i < worldBackground.Length; i++)
        {
            worldBackground[i].color = BackgroundColour;
        }

        for (int i = 0; i < worldForeground.Length; i++)
        {
            worldForeground[i].color = ForegroundColour;
        }
    }
}
