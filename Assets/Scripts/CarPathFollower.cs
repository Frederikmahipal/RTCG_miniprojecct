using UnityEngine;

public class CarPathFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 6f;
    public float turnSpeed = 6f;
    public float arriveDistance = 0.5f;

    [Header("Traffic Light Stop")]
    public TrafficLightController trafficLightController;

    int index = 0;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[index];

        StopPoint stopPoint = target.GetComponent<StopPoint>();
        if (stopPoint != null && trafficLightController != null)
        {
            float distToStop = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(target.position.x, 0, target.position.z)
            );

            if (distToStop <= stopPoint.stopDistance && trafficLightController.IsRedFor(stopPoint.direction))
            {
                return; 
            }
        }

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude < arriveDistance)
        {
            index = (index + 1) % waypoints.Length;
            return;
        }

        Vector3 dir = toTarget.normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }
    }
}
