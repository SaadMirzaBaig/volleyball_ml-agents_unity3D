using UnityEngine;

public class BallController : MonoBehaviour
{
    [HideInInspector]
    public EnvironmentController environmentController;

   

    void Start()
    {

        environmentController = GetComponentInParent<EnvironmentController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("boundary"))
        {
            environmentController.CheckEvent(Event.HitOutOfBounds);
        }
        else if (other.gameObject.CompareTag("blueBound"))
        {
            environmentController.CheckEvent(Event.HitIntoBlueArea);
        }
        else if (other.gameObject.CompareTag("redBound"))
        {
            environmentController.CheckEvent(Event.HitIntoRedArea);
        }
        else if (other.gameObject.CompareTag("redGoal"))
        {
            environmentController.CheckEvent(Event.HitRedGoal);
        }
        else if (other.gameObject.CompareTag("blueGoal"))
        {
            environmentController.CheckEvent(Event.HitBlueGoal);
        }
    }


}