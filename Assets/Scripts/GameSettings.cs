using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public float playerRunSpeed = 1.5f;
    public float playerJumpHeight = 2.75f;
    public float playerJumpVelocity = 777;
    public float playerJumpVelocityMaxChange = 10;

    // Slows down strafe & backward movement
    public float speedReductionFactor = 0.75f;

    public Material blueGoalMaterial;
    public Material redGoalMaterial;
    public Material groundMaterial;

    // This is a downward force applied when falling to make jumps look less floaty
    public float fallingForce = 150;
}
