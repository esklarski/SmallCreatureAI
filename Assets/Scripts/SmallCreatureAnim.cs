using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to gameObject to make it wander about randomly.
/// Includes flee routine and simple animator switch.
/// </summary>
public class SmallCreatureAnim : MonoBehaviour
{
    // MOVEMENT SETTINGS
    [Tooltip("Movement speed.")]
    public float moveSpeed = 6f;

    [Tooltip("Rotation speed.")]
    public float rotateSpeed = 75f;


    // RANDOM NUMBER GENERATION SETTINGS
    [Tooltip("Maximum time to rotate. Value is the upper bound on random number generation. [ # / 10 ]")]
    public int maxRotateTime = 20;

    [Tooltip("Same as maxRotateTime, but applied while fleeing. [ # / 10 ]")]
    public int fleeRotateTime = 10;

    [Tooltip("Maximum time to flee. Value is the upper bound on random number generation. [ # / 10 ]")]
    public int maxFleeTime = 40;

    [Tooltip("How much faster should the creature move when fleeing?")]
    public float fleeMultiplier = 1.5f;

    [Tooltip("How does long it spook for? [# seconds]")]
    public float spookInterval = 3f;

    [Tooltip("Tag string the object should flee from")]
    public string fleeFromTag;


    // COROUTINE SWITCHES
    private bool isWandering = false;
    private bool isRotating = false;
    private bool isFleeing = false;


    // MOVEMENT SWITCHES
    private bool isWalking = false;
    private float rotation = 0;


    // Has the player disturbed the creature?
    private bool undisturbed = true;

    // Timer for spook duration. 
    private float spookTimer;

    // Novelty indicator.
    private GameObject alertIndicator;

    // Bias to promote travel. Could be an AnimationCurve...
    private readonly int[] walkWaitBias = {0,0,0,2,4};

    // Reference for reading clarity.
    private Transform self;

    // Target rotation for turning away from obstacles.
    private Quaternion targetRotation;

    // Animator
    private Animator animator;


    private void Awake()
    {
        // self transform
        self = GetComponent<Transform>();

        // animator
        animator = GetComponent<Animator>();
        animator.SetBool("Forward", false);

        // alert indicator object
        alertIndicator = transform.GetChild(3).gameObject; // TODO - there must be a better way of doing this...
        alertIndicator.SetActive(false);

        // initialize timer
        spookTimer = spookInterval;
    }


    private void OnTriggerEnter(Collider other)
    {
        // ignore ground
        if (other.gameObject.tag != "Ground")
        {
            // hit Player, run away
            if (other.gameObject.tag == fleeFromTag)
            {
                undisturbed = false;
                isFleeing = true;
                alertIndicator.SetActive(true);

                if (spookTimer < spookInterval) { spookTimer += spookInterval * 0.25f; }

                targetRotation = Quaternion.LookRotation( (other.transform.position - self.position) * -1);

                SetRotation();
            }

            // hit boundary, turn away
            if (other.gameObject.tag == "Boundary")
            {
                Vector3 closestPoint = other.ClosestPoint(transform.position);

                targetRotation = Quaternion.LookRotation( (closestPoint - self.position) * -1 );

                StopCoroutine("Rotation");
                SetRotation();
            }
        }
    }


    private void Update()
    {
        if (undisturbed == true)
        {   // keep idle motion going
            if (isWandering == false)
            {
                StartCoroutine(Wander());
            }
            if (isRotating == false)
            {
                StartCoroutine(Rotation(maxRotateTime));
            }
        }
        else
        {   // disturbed
            if (spookTimer > 0)
            {   // count down timer and flee
                if (isFleeing == false)
                {
                    StartCoroutine(Flee());
                }

                if (isRotating == false)
                {
                    StartCoroutine(Rotation(fleeRotateTime));
                }

                spookTimer -= Time.deltaTime;
            }
            else
            {   // reset timer, and flee condition
                spookTimer = spookInterval;
                undisturbed = true;

                alertIndicator.SetActive(false);

                StopCoroutine("Flee");
                isFleeing = false;
            }
        }

        // rotation
        self.Rotate(self.up * Time.deltaTime * rotateSpeed * (rotation/10));

        // movement
        if (isWalking == true)
        {
            self.position += self.forward * Time.deltaTime * moveSpeed;
        }
        if (isFleeing == true)
        {
            self.position += (self.forward * Time.deltaTime * moveSpeed) * fleeMultiplier;
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
    private IEnumerator Wander()
    {
        int walkWait;
        float walkTime;

        isWandering = true;

        while (undisturbed)
        {
            walkWait = walkWaitBias[ Random.Range(0,5) ];

            walkTime = Random.Range(20,60)/10;

            yield return new WaitForSeconds(walkWait);
            animator.SetBool("Forward", true);
            isWalking = true;
            yield return new WaitForSeconds(walkTime);
            animator.SetBool("Forward", false);
            isWalking = false;
        }

        isWandering = false;
    }


    /// <summary>
    /// Controls rotation direction and duration.
    /// </summary>
    /// <param name="maxRotateTime"></param>
    private IEnumerator Rotation(int maxRotateTime)
    {
        float rotTime, newRotation;
        bool temp = undisturbed;

        isRotating = true;

        while (undisturbed == temp)
        {
            rotTime = Random.Range(0,maxRotateTime)/10;
            newRotation = Random.Range(-10,11);

            // deadzone for traveling straight forward
            rotation = (newRotation > -4 && newRotation < 4) ? 0 : newRotation;

            yield return new WaitForSeconds(rotTime);
        }

        isRotating = false;
    }


    /// <summary>
    /// Flees from offending collider.
    /// </summary>
    private IEnumerator Flee()
    {
        float fleeTime = Random.Range(10,maxFleeTime)/10;

        animator.SetBool("Forward", true);
        yield return new WaitForSeconds(fleeTime);
        animator.SetBool("Forward", false);
    }
}
