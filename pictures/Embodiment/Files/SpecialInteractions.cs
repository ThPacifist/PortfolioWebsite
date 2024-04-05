using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpecialInteractions : MonoBehaviour
{
    //Fish Tail spin Interaction done by Benathen 9/7
    //Initial code started by Jason on 9/1
    //Picking up and dropping boxes done on by Jason 9/2
    /*
     * TODO:
     * 
     */

        //IMPORTANT: For any successfully executed special action set a cooldown time, unset special ready, and call the cooldown timer!!!

    //Public variables and assets
    public PlyController plyCntrl;
    public Rigidbody2D plyRb;
    public Rigidbody2D heldBox;
    public SkeletonTrigger heldSkel;
    public Collider2D plyCol;
    public GameObject attackBox;
    public GameObject lamp;
    public GameObject note;
    public SpringJoint2D spring;
    public static Action Climb = delegate { };
    public static Action Scratch = delegate { };
    public LineRenderer lineRender;
    public bool isAttached;
    public bool objectHeld;
    public bool skelHeld;
    public bool HboxHeld;
    [SerializeField]
    GameObject IndicatorPrefab;

    public GameObject prefabInstance;

    //Private variables
    private bool specialReady = true;
    private float cooldownTime;
    [HideInInspector]
    public Rigidbody2D box;
    [HideInInspector]
    public SkeletonTrigger skeleton;
    
    public Switch lever;
    string boxTag;
    AudioManager audioManager;

    [SerializeField]
    Transform HheldPos;
    [SerializeField]
    Transform BheldPos;
    [SerializeField]
    Transform skelHeldPos;

    [SerializeField]
    FixedJoint2D fixedJ;
    [SerializeField]
    Animator plyAnim;
    [SerializeField]
    ControlMovement cntrlMovement;

    //Enable on enable and disable on disable
    private void OnEnable()
    {
        PlyController.Special += WaterSpecial;
        PlyController.Special += LandSpecial;
        PlyController.Special += AirSpecial;
    }

    private void OnDisable()
    {
        PlyController.Special -= WaterSpecial;
        PlyController.Special -= LandSpecial;
        PlyController.Special -= AirSpecial;
    }

    private void Start()
    {
        audioManager = GameObject.FindObjectOfType<AudioManager>();

        spring.enabled = false;
        fixedJ.enabled = false;

        lineRender.positionCount = 2;
        lineRender.SetPosition(0, transform.position);//Starting Position of Tendril Line
        lineRender.SetPosition(1, transform.position);//Ending Position of Tendril Line
        lineRender.enabled = false;
    }

    private void FixedUpdate()
    {
        lineRender.SetPosition(0, transform.position);
        if (!isAttached)
        {
            lineRender.SetPosition(1, transform.position);
        }
        else
        {
            //Gets the vector that starts from the lamp position and goes to the player position
            Vector2 targetDir = transform.position - lamp.transform.position;
            //Gets the angle of the player again, except returns negative angle when the player is to the right of the swing
            float signedAngle = Vector2.SignedAngle(targetDir, Vector2.down);
            //Changes the player's rotation to be relative to the swing
            Quaternion rotation = Quaternion.Euler(0, 0, -signedAngle);
            this.transform.rotation = rotation;
        }

        if(objectHeld && heldBox != null)
        {
            if (tag == "Human")// tag is tag of this gameobject
            {
                heldBox.transform.position = HheldPos.transform.position;
            }
            else if (tag == "Bat")// tag is tag of this gameobject
            {
                heldBox.transform.position = BheldPos.transform.position;
            }
        }
        if(skelHeld)
        {
            if(tag == "Blob")// tag is tag of this gameobject
            {
                heldSkel.skelGObject.transform.position = skelHeldPos.transform.position;
            }
        }
    }

    //Special for fish is going to be written by Benathen
    private void WaterSpecial()
    {
        if (tag == "Fish")
        {
            if (specialReady)
            {
                if(plyCntrl.InWater)
                plyAnim.SetTrigger("Spin");

                if (lever != null)
                {
                    //Activate the lever
                    lever.Interact();
                    //Cooldown
                    cooldownTime = 1;
                    specialReady = false;
                    StartCoroutine(SpecialCoolDown());
                }
            }
        }
    }

    //Sets lever value
    public void SetLever(Switch l)
    {
        lever = l;
    }

    //Special for blob is tendril grab and swing
    //Special for cat is climb or cat scratch
    //Special for human is to pick up boxes up to medium weight
    private void LandSpecial()
    {
        if (specialReady)
        {
            switch (tag) //Tag of gameobject
            {
                case "Blob":
                    //Pick up Skeleton
                    if (skeleton != null && !isAttached && !skelHeld)
                    {
                        if (!CheckSpaceForSkelo())
                        {
                            if (!plyCntrl.Left && !plyCntrl.Right)
                            {
                                plyAnim.SetBool("isGrabbing", true);
                            }
                            else
                            {
                                PickUpSkeleton(skeleton);
                            }
                        }
                        else
                        {
                            Debug.LogError("There is not enough space for skeleton");
                        }
                    }
                    //Tentacle swing
                    else if (lamp != null && !skelHeld)
                    {
                        ShootTendril();
                    }
                    //Drop skeleton
                    else if(skelHeld)
                    {
                        if (!plyCntrl.Left && !plyCntrl.Right)
                        {
                            plyAnim.SetBool("isGrabbing", false);
                        }
                        else
                        {
                            PickUpSkeleton(null);
                        }
                    }

                    break;
                case "Human":
                    //Check if there is a box to hold or a box being held
                    if (box != null && !objectHeld)
                    {
                        if (boxTag == "LBox" || boxTag == "MBox")
                        {
                            if (!CheckSpaceForBox(box))
                            {
                                if (!plyCntrl.Left && !plyCntrl.Right)
                                {
                                    plyAnim.SetBool("isGrabbing", true);
                                }
                                else
                                {
                                    PickUpBoxHuman(box);
                                }
                            }
                            else
                            {
                                Debug.LogError("Not Enough space for box");
                            }
                        }
                        else if(boxTag == "HBox")
                        {
                            if (audioManager != null)
                            {
                                audioManager.Play("boxGrab");
                            }
                            plyAnim.SetBool("isPushing", true);
                            
                            //Attach Box
                            HboxHeld = true;
                            fixedJ.enabled = true;
                            fixedJ.connectedBody = box;
                            fixedJ.connectedBody.mass = 6;
                            plyCntrl.speed = 3;

                            //Cooldown
                            cooldownTime = 1;
                            specialReady = false;
                            StartCoroutine("SpecialCoolDown");
                        }
                    }
                    else if (objectHeld || HboxHeld)
                    {
                        if (!plyCntrl.Left && !plyCntrl.Right)
                        {
                            plyAnim.SetBool("isGrabbing", false);
                            PickUpBoxHuman(null);
                        }
                        else
                        {
                            PickUpBoxHuman(null);
                        }

                        if (HboxHeld)
                        {
                            PickUpBoxHuman(null);
                        }
                    }
                    break;
                case "Cat":
                    //Spawn hitbox
                    Scratch();
                    //Cooldown
                    cooldownTime = 1;
                    specialReady = false;
                    StartCoroutine("SpecialCoolDown");
                    break;
            }
        }
    }

    //Special for bat is to pick up very light boxes
    private void AirSpecial()
    {
        if (tag == "Bat")
        {
            if (specialReady)
            {
                //Attach box
                if (box != null && !objectHeld)
                {
                    if (boxTag == "LBox")
                    {
                        if (!plyCntrl.Left && !plyCntrl.Right)
                        {
                            plyAnim.SetBool("isGrabbing", true);
                        }
                        else
                        {
                            PickUpBoxBat(box);
                        }
                    }
                }
                else if (objectHeld)
                {
                    plyAnim.SetBool("isGrabbing", false);
                    PickUpBoxBat(null);
                }
            }
        }
    }

    void AllAnimationClips()
    {
        int i = 0;
        int clipIndex = 0;
        Debug.Log("Inside all animation clips");
        foreach(AnimationClip clip in plyAnim.runtimeAnimatorController.animationClips)
        {
            if(clip.name == "BlobPickup")
            {
                clipIndex = i;
                Debug.Log("Clip index is " + clipIndex);
            }
            i++;
        }
    }

    //Check for boxes
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Check if it is a switch
        if(other.CompareTag("Lever"))
        {
            lever = other.GetComponent<Switch>();
        }
    }
    //Sets the value of the lamp
    public void SetSwingerGameObject(GameObject value) 
    { 
        lamp = value;
        if (prefabInstance == null)
        {
            prefabInstance = Instantiate(IndicatorPrefab, lamp.transform);
        }
        else if (value == null)
        {
            Destroy(prefabInstance);
        }
        else if (prefabInstance != null)
        {
            Destroy(prefabInstance);
            prefabInstance = Instantiate(IndicatorPrefab, lamp.transform);
        }
    }

    //Sets the value of the held box
    public void SetHeldBox(Rigidbody2D rb, string inputTag) 
    { 
        box = rb;
        boxTag = inputTag;
        if (prefabInstance == null)
        {
            prefabInstance = Instantiate(IndicatorPrefab, box.transform);
        }
        else if (rb == null)
        {
            Destroy(prefabInstance);
        }
        else if (prefabInstance != null)
        {
            Destroy(prefabInstance);
            prefabInstance = Instantiate(IndicatorPrefab, box.transform);
        }
    }

    //Sets the value of the skeleton to be held
    public void SetHeldSkel(SkeletonTrigger skel)
    {
        skeleton = skel;
        if (prefabInstance == null)
        {
            prefabInstance = Instantiate(IndicatorPrefab, skeleton.transform);
        }
        else if (skel == null)
        {
            Destroy(prefabInstance);
        }
        else if (prefabInstance != null)
        {
            Destroy(prefabInstance);
            prefabInstance = Instantiate(IndicatorPrefab, skeleton.transform);
        }
    }

    //Remove boxes from selection
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Lever"))
        {
            lever = null;
        }
    }

    //Special cooldown
    IEnumerator SpecialCoolDown()
    {
        yield return new WaitForSeconds(cooldownTime);
        specialReady = true;
    }

    //Checks to see if there is enough space for the box, so that it does not get put through a wall
    bool CheckSpaceForBox(Rigidbody2D rb)
    {
        BoxCollider2D[] cols = new BoxCollider2D[2];
        rb.GetAttachedColliders(cols); //Gets all the colliders attached to a box
        BoxCollider2D col;

        //Determines which collider is the box collider, versus the trigger collider
        if(cols[0].isTrigger)
        {
            col = cols[1];
        }
        else
        {
            col = cols[0];
        }

        float dist = 0;
        int layer = LayerMask.NameToLayer("CheckSpace");
        //Casts a box (much like raycast) into the scene
        RaycastHit2D hit = Physics2D.BoxCast(HheldPos.position, col.size, 0f, Vector2.down, dist, layer);

        return hit.collider != null; //If collider exists, sends true. Otherwise false
    }

    //Checks to see if there is enough space for the skeleton, so that it does not get put through a wall
    bool CheckSpaceForSkelo()
    {
        Vector2 start = skelHeldPos.position + Vector3.up; //Pos of skelHeldPos up one
        float dist = 0.2f;

        string name =  "Jumpables";
        int layerMask = LayerMask.NameToLayer(name);

        RaycastHit2D hit = Physics2D.Raycast(start, Vector2.down, dist, layerMask);//Sends a ray from start and will only hit colliders in Jumpables layer
        Debug.DrawRay(start, Vector2.down, Color.red);

        return hit.collider != null; //If collider exists, sends true. Otherwise false
    }

    //Creates the tentacle between the lamp and the player
    public void ShootTendril()
    {
        //Debug.Log("Spring on " + spring.isActiveAndEnabled);
        if (audioManager != null)
        {
            audioManager.Play("Swing");
        }
        if (!spring.isActiveAndEnabled)
        {
            lineRender.enabled = true;
            spring.enabled = true;
            spring.connectedAnchor = lamp.transform.position;
            lineRender.SetPosition(1, new Vector3(lamp.transform.position.x, lamp.transform.position.y, transform.position.z));
            isAttached = true;

            //Cooldown
            cooldownTime = 0.3f;
            specialReady = false;
        }
        else
        {
            plyCntrl.canMove = true;
            spring.enabled = false;
            spring.connectedAnchor = Vector2.zero;
            lineRender.SetPosition(1, transform.position);
            lineRender.enabled = false;
            Quaternion rotation = Quaternion.Euler(0, 0, 0);
            this.transform.rotation = rotation;
            isAttached = false;

            //Cooldown
            cooldownTime = 0.5f;
            specialReady = false;
        }
        //init Cooldown
        StartCoroutine("SpecialCoolDown");
    }

    //This function is called when we want the blob to pick up skeleton
    public void PickUpSkeleton(SkeletonTrigger skelo)
    {
        if (skelo != null && !skelHeld)
        {
            heldSkel = skelo;
            heldSkel.isGrabbed = true;
            Destroy(prefabInstance);
            skelHeld = true;
            fixedJ.enabled = true;
            fixedJ.connectedBody = heldSkel.transform.parent.GetComponent<Rigidbody2D>();
            heldSkel.skelGObject.transform.position = skelHeldPos.transform.position;
            plyAnim.SetBool("isGrabbing", true);
            plyCntrl.jumpHeight = 60;

            Debug.Log("Picked up " + skelo.name);
        }
        else if(skelo == null && skelHeld)
        {
            heldSkel.isGrabbed = false;
            heldSkel = null;
            skelHeld = false;
            fixedJ.enabled = false;
            fixedJ.connectedBody = null;
            plyAnim.SetBool("isGrabbing", false);
            plyCntrl.jumpHeight = 18.1f;

            Debug.Log("Put Down my object");
        }

        //Cooldown
        cooldownTime = 1;
        specialReady = false;
        StartCoroutine("SpecialCoolDown");
    }

    public void PickUpBoxHuman(Rigidbody2D box)
    {
        if (box != null)
        {
            if (audioManager != null)
            {
                audioManager.Play("boxGrab");
            }
            plyAnim.SetBool("isGrabbing", true);
            //Attach box
            heldBox = box;
            objectHeld = true;
            fixedJ.enabled = true;
            fixedJ.connectedBody = heldBox;
            heldBox.transform.position = HheldPos.transform.position;
        }
        else
        {
            if (audioManager != null)
            {
                audioManager.Play("boxGrab");
            }
            plyAnim.SetBool("isPushing", false);
            plyAnim.SetBool("isGrabbing", false); 
            objectHeld = false;
            HboxHeld = false;
            fixedJ.enabled = false;
            if(boxTag == "HBox")
                fixedJ.connectedBody.mass = 20;
            //plyCntrl.speed = cntrlMovement.skeloData.speed;
            fixedJ.connectedBody = null;
            heldBox = null;
        }

        //Cooldown
        cooldownTime = 1;
        specialReady = false;
        StartCoroutine("SpecialCoolDown");
    }

    public void PickUpBoxBat(Rigidbody2D box)
    {
        if (box != null)
        {
            if (audioManager != null)
            {
                audioManager.Play("boxGrab");
            }
            plyAnim.SetBool("isGrabbing", true);
            //Attach box
            heldBox = box;
            objectHeld = true;
            fixedJ.enabled = true;
            fixedJ.connectedBody = heldBox;
            heldBox.transform.position = BheldPos.transform.position;
        }
        else
        {
            if (audioManager != null)
            {
                audioManager.Play("boxGrab");
            }
            plyAnim.SetBool("isGrabbing", false);
            //plyAnim.SetTrigger("Bat");
            objectHeld = false;
            fixedJ.enabled = false;
            fixedJ.connectedBody = null;
            heldBox = null;
        }

        //Cooldown
        cooldownTime = 1;
        specialReady = false;
        StartCoroutine("SpecialCoolDown");
    }

    //Used in BlobPickUp Animation to pick up the skeleton on the correct frame
    public void CallPickUpFromAnimation()
    {
        PickUpSkeleton(skeleton);
    }

    //Used in BlobPutDown Animation to put down the skeleton on the correct frame
    public void CallPutDownFromAnimation()
    {
        PickUpSkeleton(null);
    }

    //Used in BlobPickUp Animation to pick up the skeleton on the correct frame
    public void CallPickUpBoxFromHumanAnimation()
    {
        PickUpBoxHuman(box);
    }

    //Used in BlobPutDown Animation to put down the skeleton on the correct frame
    public void CallPutDownBoxFromHumanAnimation()
    {
        PickUpBoxHuman(null);
    }

    //Used in BlobPickUp Animation to pick up the skeleton on the correct frame
    public void CallPickUpBoxFromBatAnimation()
    {
        PickUpBoxBat(box);
    }

    //Used in BlobPutDown Animation to put down the skeleton on the correct frame
    public void CallPutDownBoxFromBatAnimation()
    {
        PickUpBoxBat(null);
    }

    private void OnDrawGizmos()
    {
        //Human Box Pos
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(HheldPos.position, 0.3f);

        //Bat Box Pos
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(BheldPos.position, 0.3f);

        //Blob Box Pos
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(skelHeldPos.position, 0.3f);
    }

    private void Interact()
    {
        Debug.Log("Interact called");

        //Check if there is a note to interact with
        if(note != null)
        {
            Debug.Log(note);
            Debug.Log(note.active);
            if (!note.active)
            {
                note.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                note.SetActive(false);
                Time.timeScale = 1;
            }
        }
    }

    /*
     * Indexes of Animations:
     * Grab animation: 22
     */
}
