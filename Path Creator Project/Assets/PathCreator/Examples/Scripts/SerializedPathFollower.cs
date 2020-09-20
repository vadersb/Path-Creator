using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

public class SerializedPathFollower : MonoBehaviour
{
    public SerializedVertexPath path;
    public EndOfPathInstruction endOfPathInstruction;
    public float speed = 5f;
    private float distanceTravelled;
    
    void Start()
    {
        if (path != null)
        {
            path.pathUpdated += OnPathChanged;
        }
    }

    void Update()
    {
        if (path != null)
        {
            distanceTravelled += speed * Time.deltaTime;
            transform.position = path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
            transform.rotation = path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
        }
    }
    
    
    // If the path changes during the game, update the distance travelled so that the follower's position on the new path
    // is as close as possible to its position on the old path
    void OnPathChanged() {
        distanceTravelled = path.GetClosestDistanceAlongPath(transform.position);
    }
}
