using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    Blue = 0,
    Red = 1,
    Default = 2
}

public enum Event
{
    HitRedGoal = 0,
    HitBlueGoal = 1,
    HitOutOfBounds = 2,
    HitIntoBlueArea = 3,
    HitIntoRedArea = 4
}

public class EnvironmentController : MonoBehaviour
{
    int ballSpawnSide;
    private int resetTimer;
    public int MaxEnvironmentSteps;


    GameSettings gameSettings;
    public GameObject blueGoal;
    public GameObject redGoal;


    public PlayerAgent blueAgent;
    public PlayerAgent redAgent;
    public GameObject ball;
    public UIScore uiScore;

    public List<PlayerAgent> AgentsList = new List<PlayerAgent>();
    
    public List<Renderer> RenderersList = new List<Renderer>();
    Renderer blueGoalRenderer;
    Renderer redGoalRenderer;

    //Rigidbody bluePlayerRb;
    //Rigidbody redPlayerRb;

    Rigidbody ballRb;

    Team lastHitter;



    void Start()
    {

        //bluePlayerRb = blueAgent.GetComponent<Rigidbody>();
        //redPlayerRb = redAgent.GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();


        // spawning ball randomly on both sides
        var spawnSideList = new List<int> { -1, 1 };
        ballSpawnSide = spawnSideList[Random.Range(0, 2)];

        // Render ground to visualise which agent scored
        blueGoalRenderer = blueGoal.GetComponent<Renderer>();
        redGoalRenderer = redGoal.GetComponent<Renderer>();
        RenderersList.Add(blueGoalRenderer);
        RenderersList.Add(redGoalRenderer);

        gameSettings = FindObjectOfType<GameSettings>();

        ResetGame();
    }

  
    public void UpdateLastHitter(Team team)
    {
        lastHitter = team;
    }

 
    public void CheckEvent(Event triggerEvent)
    {
        switch (triggerEvent)
        {
            case Event.HitOutOfBounds:

                blueAgent.EndEpisode();
                redAgent.EndEpisode();
                ResetGame();
                break;

            case Event.HitBlueGoal:

                blueAgent.AddReward(1f);
                redAgent.AddReward(-1f);

                uiScore.ScoreUpdateBlue(1);

                StartCoroutine(ChangeGroundColor(gameSettings.blueGoalMaterial, RenderersList, .5f));

                blueAgent.EndEpisode();
                redAgent.EndEpisode();
                ResetGame();
                break;

            case Event.HitRedGoal:

                redAgent.AddReward(1f);
                blueAgent.AddReward(-1f);

                uiScore.ScoreUpdateRed(1);

                StartCoroutine(ChangeGroundColor(gameSettings.redGoalMaterial, RenderersList, .5f));

                blueAgent.EndEpisode();
                redAgent.EndEpisode();
                ResetGame();
                break;

            case Event.HitIntoBlueArea:
                if (lastHitter == Team.Red)
                {
                    //redAgent.AddReward(1);
                }
                break;

            case Event.HitIntoRedArea:
                if (lastHitter == Team.Blue)
                {
                    //blueAgent.AddReward(1);
                }
                break;
        }
    }

 
    IEnumerator ChangeGroundColor(Material mat, List<Renderer> rendererList, float time)
    {
        foreach (var renderer in rendererList)
        {
            renderer.material = mat;
        }

        yield return new WaitForSeconds(time); 
        
        foreach (var renderer in rendererList)
        {
            renderer.material = gameSettings.groundMaterial;
        }

    }

    void FixedUpdate()
    {
        resetTimer += 1;
        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            blueAgent.EpisodeInterrupted();
            redAgent.EpisodeInterrupted();
            ResetGame();
        }
    }


    public void ResetGame()
    {
        resetTimer = 0;

        lastHitter = Team.Default; // reset last hitter

        foreach (var agent in AgentsList)
        {
            // randomise starting positions and rotations
            var randomPosX = Random.Range(-2f, 2f);
            var randomPosZ = Random.Range(-2f, 2f);
            var randomPosY = Random.Range(0.5f, 3.75f); // depends on jump height
            var randomRot = Random.Range(-45f, 45f);

            agent.transform.localPosition = new Vector3(randomPosX, randomPosY, randomPosZ);
            agent.transform.eulerAngles = new Vector3(0, randomRot, 0);

            agent.GetComponent<Rigidbody>().velocity = default(Vector3);
        }

        // reset ball to starting conditions
        ResetBall();
    }


    void ResetBall()
    {
        var randomPosX = Random.Range(-2f, 2f);
        var randomPosZ = Random.Range(6f, 10f);
        var randomPosY = Random.Range(6f, 8f);

        // alternate ball spawn side

        ballSpawnSide = -1 * ballSpawnSide;

        if (ballSpawnSide == -1)
        {
            ball.transform.localPosition = new Vector3(randomPosX, randomPosY, randomPosZ);
        }
        else if (ballSpawnSide == 1)
        {
            ball.transform.localPosition = new Vector3(randomPosX, randomPosY, -1 * randomPosZ);
        }

        ballRb.angularVelocity = Vector3.zero;
        ballRb.velocity = Vector3.zero;
    }
}
