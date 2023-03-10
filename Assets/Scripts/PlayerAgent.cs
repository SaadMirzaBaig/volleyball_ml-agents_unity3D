using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class PlayerAgent : Agent
{
    public GameObject area;
    Rigidbody playerRb;
    
    BehaviorParameters behaviorParameters;
    
    
    public Team teamId;

    
    public GameObject ball;
    Rigidbody ballRb;

    GameSettings gameSettings;
    EnvironmentController environmentController;

   
    float jumpingTime;
    Vector3 jumpTargetPos;
    Vector3 jumpStartingPos;
    float agentRot;

    public Collider[] hitGroundColliders = new Collider[3];

    //EnvironmentParameters resetParams;

    void Start()
    {
        environmentController = area.GetComponent<EnvironmentController>();
    }

    public override void Initialize()
    {
        gameSettings = FindObjectOfType<GameSettings>();
        behaviorParameters = gameObject.GetComponent<BehaviorParameters>();

        playerRb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();
        
        // for symmetry between player side
        if (teamId == Team.Blue)
        {
            agentRot = -1;
        }
        else
        {
            agentRot = 1;
        }
    }


    void MoveTowards(Vector3 targetPos, Rigidbody rb, float targetVel, float maxVel)
    {
        var moveToPos = targetPos - rb.worldCenterOfMass;
        var velocityTarget = Time.fixedDeltaTime * targetVel * moveToPos;
        if (float.IsNaN(velocityTarget.x) == false)
        {
            rb.velocity = Vector3.MoveTowards(
                rb.velocity, velocityTarget, maxVel);
        }
    }

 
    public bool CheckIfGrounded()
    {
        hitGroundColliders = new Collider[3];
        var o = gameObject;
        Physics.OverlapBoxNonAlloc(
            o.transform.localPosition + new Vector3(0, -0.05f, 0),
            new Vector3(0.95f / 2f, 0.5f, 0.95f / 2f),
            hitGroundColliders,
            o.transform.rotation);
        var grounded = false;
        foreach (var col in hitGroundColliders)
        {
            if (col != null && col.transform != transform &&
                (col.CompareTag("walkableSurface") ||
                 col.CompareTag("redGoal") ||
                 col.CompareTag("blueGoal")))
            {
                grounded = true; //then we're grounded
                break;
            }
        }
        return grounded;
    }


    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("ball"))
        {
            environmentController.UpdateLastHitter(teamId); //which players had the ball
        }
    }

    public void Jump()
    {
        jumpingTime = 0.2f;
        jumpStartingPos = playerRb.position;
    }


    public void PlayerMovement(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var grounded = CheckIfGrounded();

        var dirToGoForwardAction = act[0];
        var rotateDirAction = act[1];
        var dirToGoSideAction = act[2];
        var jumpAction = act[3];

        if (dirToGoForwardAction == 1)
            dirToGo = (grounded ? 1f : 0.5f) * transform.forward * 1f;
        else if (dirToGoForwardAction == 2)
            dirToGo = (grounded ? 1f : 0.5f) * transform.forward * gameSettings.speedReductionFactor * -1f;

        if (rotateDirAction == 1)
            rotateDir = transform.up * -1f;
        else if (rotateDirAction == 2)
            rotateDir = transform.up * 1f;

        if (dirToGoSideAction == 1)
            dirToGo = (grounded ? 1f : 0.5f) * transform.right * gameSettings.speedReductionFactor * -1f;
        else if (dirToGoSideAction == 2)
            dirToGo = (grounded ? 1f : 0.5f) * transform.right * gameSettings.speedReductionFactor;

        if (jumpAction == 1)
        {
            if (((jumpingTime <= 0f) && grounded))
            {
                Jump();
            }
        }


        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        playerRb.AddForce(agentRot * dirToGo * gameSettings.playerRunSpeed,
            ForceMode.VelocityChange);


        // makes the agent  "jump"
        if (jumpingTime > 0f)
        {
            jumpTargetPos =
                new Vector3(playerRb.position.x,
                    jumpStartingPos.y + gameSettings.playerJumpHeight,
                    playerRb.position.z) + agentRot * dirToGo;

            MoveTowards(jumpTargetPos, playerRb, gameSettings.playerJumpVelocity,
                gameSettings.playerJumpVelocityMaxChange);
        }

        //downward force to end the jump
        if (!(jumpingTime > 0f) && !grounded)
        {
            playerRb.AddForce(
                Vector3.down * gameSettings.fallingForce, ForceMode.Acceleration);
        }

        // controls the jump sequence
        if (jumpingTime > 0f)
        {
            jumpingTime -= Time.fixedDeltaTime;
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        PlayerMovement(actionBuffers.DiscreteActions);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent rotation (1 float)
        sensor.AddObservation(this.transform.rotation.y);

        // Vector from agent to ball (direction to ball) (3 floats)
        Vector3 toBall = new Vector3((ballRb.transform.position.x - this.transform.position.x) * agentRot,
        (ballRb.transform.position.y - this.transform.position.y),
        (ballRb.transform.position.z - this.transform.position.z) * agentRot);

        sensor.AddObservation(toBall.normalized);

        // Distance from the ball (1 float)
        sensor.AddObservation(toBall.magnitude);

        // Agent velocity (3 floats)
        sensor.AddObservation(playerRb.velocity);

        // Ball velocity (3 floats)
        sensor.AddObservation(ballRb.velocity.y);
        sensor.AddObservation(ballRb.velocity.z * agentRot);
        sensor.AddObservation(ballRb.velocity.x * agentRot);
    
    }

    // For human controll
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            // rotate right
            discreteActionsOut[1] = 2;
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            // forward
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // rotate left
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            // backward
            discreteActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // move left
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            // move right
            discreteActionsOut[2] = 2;
        }
        discreteActionsOut[3] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
}
