using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Was initially going for something re-usable but that went out the window. This just tracks on the X axis to give us the parralax effect
public class TrackObject : MonoBehaviour
{
    [SerializeField] GameObject objectToTrack;
    [SerializeField] TrackAxis axis;
    Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update() => UpdatePosition();

    public void UpdatePosition()
    {
        if (GameController.Instance.GetCurrentGameState != GameController.Instance.FlyingStateInstance)
        {
            return;
        }

        Vector2 FinalTrack = initialPosition;
        switch (axis)
        {
            case TrackAxis.X_AXIS:
                FinalTrack = new(objectToTrack.transform.position.x, initialPosition.y);
                break;
            case TrackAxis.Y_AXIS:
                FinalTrack = new(initialPosition.x, objectToTrack.transform.position.y);
                break;

            case TrackAxis.BOTH_AXIS:
                FinalTrack = objectToTrack.transform.position;
                break;
        }
        Vector3 MoveVector = Vector3.MoveTowards(transform.position, new(initialPosition.x + FinalTrack.x, FinalTrack.y, transform.position.z), 5.0f) * 0.2f;
        transform.position = new(MoveVector.x, initialPosition.y, transform.position.z);
    }
}
