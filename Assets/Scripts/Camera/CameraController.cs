using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TrackAxis { X_AXIS, Y_AXIS, BOTH_AXIS }

public class CameraController : MonoBehaviour
{
    [SerializeField] Camera thisCamera;

    static CameraController instance;
    public static CameraController Instance
    {
        get { return instance; }
    }

    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void UpdatePosition(Vector2 NewPosition, TrackAxis AxisToTrack)
    {
        Vector2 FinalTrack = new();
        switch (AxisToTrack)
        {
            case TrackAxis.X_AXIS:
                FinalTrack = new(NewPosition.x, transform.position.y);
                break;
            case TrackAxis.Y_AXIS:
                FinalTrack = new(transform.position.x, NewPosition.y);
                break;

            case TrackAxis.BOTH_AXIS:
                FinalTrack = NewPosition;
                break;
        }
        transform.position = new(FinalTrack.x, FinalTrack.y, transform.position.z);
    }
}

