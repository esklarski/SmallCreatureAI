using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to gameObject to make it wander about randomly.
/// </summary>
public class BasicWander : MonoBehaviour
{
    // MOVEMENT SETTINGS
    [Tooltip("Movement speed.")]
    public float moveSpeed = 4f;

    [Tooltip("Rotation speed.")]
    public float rotateSpeed = 75f;


    // RANDOM NUMBER GENERATION SETTINGS
    [Tooltip("Maximum time to rotate. Value is the upper bound on random number generation. [ # / 10 ]")]
    public int maxRotateTime = 20;


    // COROUTINE SWITCHES
    private bool isWandering = false;
    private bool isRotating = false;


    // MOVEMENT SWITCHES
    private bool isWalking = false;
    private float rotation = 0;


    // Bias to promote travel. Could be an AnimationCurve...
    private readonly int[] walkWaitBias = {0,0,0,1,2};

    // Reference for reading clarity.
    private Transform self;

    // Target rotation for turning away from obstacles.
    private Quaternion targetRotation;


    private void Awake()
    {
        // self transform
        self = GetComponent<Transform>();
    }


    /// <param name="other"></param>
    private void OnCollisionEnter(Collision other)
    {
        // ignore ground
        if (other.gameObject.tag != "Ground")
        {
            // hit another Player, turn away
            if (other.gameObject.tag == "Player")
            {
                targetRotation = Quaternion.LookRotation( (other.transform.position - self.position) * -1);

                SetRotation();
            }

            // hit boundary, turn away
            if (other.gameObject.tag == "Boundary")
            {
                Vector3 closestPoint = other.collider.ClosestPoint(transform.position);

                targetRotation = Quaternion.LookRotation( (closestPoint - self.position) * -1 );
 
                StopCoroutine("Rotation");
                SetRotation();
            }
        }
    }


    void Update()
    {
        if (isWandering == false)
        {
            StartCoroutine(Wander());
        }
        if (isRotating == false)
        {
            StartCoroutine(Rotation(maxRotateTime));
        }

        // rotation
        self.Rotate(self.up * Time.deltaTime * rotateSpeed * (rotation/10));

        // movement
        if (isWalking == true)
        {
            self.position += self.forward * Time.deltaTime * moveSpeed;
        }
    }


    /// <summary>
    /// Sets rotation of creature to new heading.
    /// </summary>
    private void SetRotation()
    {
        targetRotation.Set(0, targetRotation.y, 0, 0);
        self.rotation = targetRotation;
    }


    /// <summary>
    /// Controls forward movement duration.
    /// </summary>
    IEnumerator Wander()
    {
        isWandering = true;

        int walkWait = walkWaitBias[ Random.Range(0,5) ];

        float walkTime = Random.Range(20,60)/10;

        yield return new WaitForSeconds(walkWait);
        isWalking = true;
        yield return new WaitForSeconds(walkTime);
        isWalking = false;

        isWandering = false;
    }


    /// <summary>
    /// Controls rotation direction and duration.
    /// </summary>
    /// <param name="maxRotateTime"></param>
    IEnumerator Rotation(int maxRotateTime)
    {
        isRotating = true;

        // divide by 10 to increase posibility resolution
        float rotTime = Random.Range(0,maxRotateTime)/10;
        float newRotation = Random.Range(-10,11);

        // deadzone for traveling straight forward
        rotation = (newRotation > -4 && newRotation < 4) ? 0 : newRotation;

        yield return new WaitForSeconds(rotTime);

        isRotating = false;
    }
}
