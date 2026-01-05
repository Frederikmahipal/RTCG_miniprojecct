using UnityEngine;

public class StopPoint : MonoBehaviour
{
    public enum Direction { NorthSouth, EastWest }
    public Direction direction = Direction.NorthSouth;

    [Tooltip("Stop distance")]
    public float stopDistance = 1.5f;
}
