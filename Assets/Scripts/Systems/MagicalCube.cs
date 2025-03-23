using UnityEngine;
using UnityEngine.AI;

public class MagicalCube : MonoBehaviour
{
    public Transform player;             // Reference to the player
    public Transform checkpoint;         // Reference to the checkpoint
    public float activationRadius = 10f; // Radius in which the cube activates
    public float maxSpeed = 10f;         // Maximum speed of the cube
    public float minSpeed = 2f;          // Minimum speed of the cube
    public float speedIncreaseDistance = 5f; // Distance at which speed increases
    public float DeleteDistance = 2f;    // Distance to delete the cube
    public float DestroyTime = 4f;       // Time before destroying cube after being near checkpoint
    public float maxDistanceToFollow = 20f; // Max distance at which the cube should follow the player

    private NavMeshAgent agent;          // NavMeshAgent for movement

    void Start()
    {
        // Get the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Calculate the distance between the cube and the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float distanceToCheckpoint = Vector3.Distance(transform.position, checkpoint.position);

        // Check if the checkpoint is within DeleteDistance of the player
        if (distanceToCheckpoint < DeleteDistance)
        {
            Destroy(gameObject, DestroyTime); // Destroy cube after some time if the player is near the checkpoint
        }

        // If the player is too far, move the cube towards the player
        if (distanceToPlayer > maxDistanceToFollow)
        {
            agent.SetDestination(player.position); // Move towards the player
            AdjustCubeSpeed(distanceToPlayer);
        }
        else if (distanceToPlayer <= activationRadius)
        {
            // If player is within range, move the cube towards the checkpoint
            agent.SetDestination(checkpoint.position);
            AdjustCubeSpeed(distanceToCheckpoint);
        }
        else
        {
            // If the player is neither too close nor too far, stop or reset the path
            agent.ResetPath();
        }
    }

    // Adjusts the speed based on the distance
    private void AdjustCubeSpeed(float distance)
    {
        // Adjust the cube's speed based on its proximity to the destination
        float speedFactor = Mathf.Clamp01(1 - (distance / speedIncreaseDistance));
        agent.speed = Mathf.Lerp(minSpeed, maxSpeed, speedFactor);
    }
}
