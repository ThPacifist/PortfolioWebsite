using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlyController : MonoBehaviour
{
    //Assets and Public Variables
    PlayerControls PlyCtrl;
    public Transform attackBox;
    public Rigidbody2D rb;
    public Animator plyAnim;
    public SpriteRenderer plySprite;
    public CapsuleCollider2D capCollider; 
    public static Action Interact = delegate { };
    public static Action Embody = delegate { };
    public static Action Special = delegate { };
    public static Action Pause = delegate { };
    public static Action Death = delegate { };
    public ControlMovement cntrlMove;
    public float speed;
    public float jumpHeight;
    public bool canMove = true;
    public bool canJump = true;
    public float angle;

    [SerializeField]
    LayerMask groundLayerMask;

    //Private Variables
    public bool OnWall = false;
    bool batJump = true;
    Vector2 catDir;
    AudioManager audioManager;

    [HideInInspector]
    public bool treadmill = false;

    bool right;
    bool left;

    public bool Right
    { get { return right; } }
    public bool Left
    { get { return left; } }

    [SerializeField]
    bool inWater = false;

    public bool InWater
    { get { return inWater; } }

    [SerializeField]
    SpecialInteractions spcInter;

    private void Awake()
    {
        PlyCtrl = new PlayerControls();
        this.gameObject.transform.position = GameAction.PlaceColOnGround(capCollider);
        TransitionController.slideInAction = UnFreezePlayer;
    }

    private void OnEnable()
    {
        PlyCtrl.Enable();
    }

    void OnDisable()
    {
        PlyCtrl.Disable();
    }

    //Start is called at the start of this script
    private void Start()
    {
        audioManager = GameObject.FindObjectOfType<AudioManager>();
        //Special Interact
        PlyCtrl.Player.Special.performed += _ => SpecialS();

        //Regular interact
        PlyCtrl.Player.Interact.performed += _ => InteractI();

        //Jump
        PlyCtrl.Player.Jump.performed += _ => Jump();

        //Embody
        PlyCtrl.Player.Embody.performed += _ => EmbodyE();

        //Pause
        PlyCtrl.Player.Pause.performed += _ => Pause();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (canMove && !treadmill)
        {
            //Movement for Fish
            if (this.CompareTag("Fish"))
            {
                //Movement when in water
                if (inWater)
                {
                    //Move when the player is pressing buttons
                    if (PlyCtrl.Player.FishInWater.ReadValue<Vector2>() != Vector2.zero)
                    {
                        float x = 0, y = 0;
                        //Checks if either the y or x velocity is exceeding the speed
                        if(Math.Abs(rb.velocity.x) < speed)
                        {
                            x = 1;
                        }
                        if(Math.Abs(rb.velocity.y) < speed)
                        {
                            y = 1;
                        }

                        rb.AddForce(new Vector2(x, y) * PlyCtrl.Player.FishInWater.ReadValue<Vector2>() * 20 * rb.mass);
                    }
                }
                //Movement when on the ground
                else if (isGrounded())
                {
                    //Move when the player is pressing buttons
                    if (PlyCtrl.Player.Movement.ReadValue<float>() != 0)
                    {
                        if (Math.Abs(rb.velocity.x) < speed * 0.3f)
                        {
                            rb.AddForce(Vector2.right * PlyCtrl.Player.Movement.ReadValue<float>() * 20 * rb.mass);
                        }
                    }
                }
                else
                {
                    //Move when the player is pressing the direction
                    if (PlyCtrl.Player.Movement.ReadValue<float>() != 0)
                    {
                        rb.velocity += (Vector2.right * PlyCtrl.Player.Movement.ReadValue<float>() * speed * 0.5f) - new Vector2(rb.velocity.x, 0);
                    }
                }
            }
            else
            {
                //Cat climb movement
                if (this.CompareTag("Cat") && OnWall)
                {
                    rb.gravityScale = 0;
                    rb.AddForce(catDir, ForceMode2D.Impulse);
                    if (PlyCtrl.Player.FishInWater.ReadValue<Vector2>() != Vector2.zero)
                    {
                        //Checks if the player is pushing up or down
                        if (PlyCtrl.Player.FishInWater.ReadValue<Vector2>().y != 0)
                        {
                            rb.velocity += (Vector2.up * PlyCtrl.Player.FishInWater.ReadValue<Vector2>().y * speed * 0.5f) - new Vector2(0, rb.velocity.y);
                            if (audioManager != null)
                            {
                                audioManager.Play("catClimb");
                            }
                        }
                        //Otherwise checks if they push left or right, meaning they can
                        else
                        {
                            if (audioManager != null)
                            {
                                //Debug.Log("Inside if");
                                audioManager.Stop("catClimb");
                            }
                            if (PlyCtrl.Player.FishInWater.ReadValue<Vector2>().x != 0)
                            {
                                rb.velocity += (Vector2.right * PlyCtrl.Player.FishInWater.ReadValue<Vector2>().x * speed * 0.5f) - new Vector2(rb.velocity.x, 0);
                            }
                        }
                    }
                    else if (audioManager != null)
                    {
                        audioManager.Stop("catClimb");
                    }
                }
                //Regular grounded movement
                else if (PlyCtrl.Player.Movement.ReadValue<float>() != 0)
                {
                    //Movement
                    if (Math.Abs(rb.velocity.x) < speed && !spcInter.isAttached)
                    {
                        rb.AddForce(Vector2.right * PlyCtrl.Player.Movement.ReadValue<float>() * 20 * rb.mass);
                    }
                    else if (spcInter.isAttached)
                    {
                        if (PlyCtrl.Player.Movement.ReadValue<float>() != 0)
                        {
                            rb.AddForce(Vector2.right * PlyCtrl.Player.Movement.ReadValue<float>() * 0.6f, ForceMode2D.Impulse);
                        }
                    }

                    //Audio
                    if (audioManager != null)
                    {
                        audioManager.Stop("catClimb");
                        if (tag == "Blob")
                        {
                            audioManager.Play("blobStep");
                        }
                    }
                }
                else
                {
                    if (audioManager != null)
                    {
                        audioManager.Stop("blobStep");
                    }
                }
            }
        }

        //If Cat is not on a wall
        if(!OnWall)
        {
            rb.gravityScale = 1;
            if (audioManager != null)
            {
                audioManager.Stop("catClimb");
            }
        }

        if(!isGrounded())
        {
            if (audioManager != null)
            {
                audioManager.Stop("blobStep");
            }
        }

        //Remove momentum while on ground
        if (PlyCtrl.Player.Movement.ReadValue<float>() == 0 && isGrounded())
        {
            //Reduce the player's speed by half
            rb.velocity *= new Vector2(0.75f, 1);
            //Change any held boxes velocity to match the player
            if (spcInter.heldBox != null)
            {
                spcInter.heldBox.velocity = rb.velocity;
            }
        }

        //Remove momentum while on wall
        if(PlyCtrl.Player.FishInWater.ReadValue<Vector2>().y == 0 && OnWall)
        {
            rb.velocity *= new Vector2(1, 0.5f);
        }

        //int BlobRespawnID = Animator.StringToHash("BlobRespawn");
        //if (plyAnim.GetCurrentAnimatorStateInfo(0).shortNameHash == BlobRespawnID) return;
      
        #region Animation Block
        if (PlyCtrl.Player.Movement.ReadValue<float>() != 0 && canMove)
        {
            plyAnim.SetBool("Walking", true);
        }
        else
        {
            plyAnim.SetBool("Walking", false);
        }

        if (isGrounded())
        {
            plyAnim.SetBool("isJumping", false);
        }
        else
        {
            plyAnim.SetBool("isJumping", true);
        }

        //Check if the blob is attached
        if (spcInter.isAttached)
        {
            plyAnim.SetBool("Swing", true);
        }
        else
        {
            plyAnim.SetBool("Swing", false);
        }

        //Climbing on Wall as cat
        if (OnWall)
        {
            plyAnim.SetBool("Climb", true);

            if(PlyCtrl.Player.FishInWater.ReadValue<Vector2>().y != 0)
            {
                plyAnim.SetBool("Walking", true);
            }
            else
            {
                plyAnim.SetBool("Walking", false);
            }
        }
        else
        {
            plyAnim.SetBool("Climb", false);
        }

        //Pushing and Pulling boxes as Human
        if (!spcInter.HboxHeld)
        {
            if (canMove)
            {
                //Set facing direction
                if (PlyCtrl.Player.Movement.ReadValue<float>() > 0)
                {
                    this.gameObject.transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y),
                        Mathf.Abs(transform.localScale.z));
                    right = true;
                    left = false;
                }
                else if (PlyCtrl.Player.Movement.ReadValue<float>() < 0)
                {
                    this.gameObject.transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y),
                        Mathf.Abs(transform.localScale.z));
                    left = true;
                    right = false;
                }
                else
                {
                    left = false;
                    right = false;
                }
            }
        }
        else
        {
            //Set to Pushing or Pulling
            if (this.transform.localScale.x > 0)//When player is facing left, while holding a heavy box
            {
                if (PlyCtrl.Player.Movement.ReadValue<float>() > 0)//When player is pressing right
                {
                    plyAnim.SetInteger("Facing", -1);
                }
                else if (PlyCtrl.Player.Movement.ReadValue<float>() < 0)//When player is pressing left
                {
                    plyAnim.SetInteger("Facing", 1);
                }
                else
                {
                    plyAnim.SetInteger("Facing", 0);
                }
            }
            else if (this.transform.localScale.x < 0)//When player is facing right, while holding a heavy box
            {
                if (PlyCtrl.Player.Movement.ReadValue<float>() > 0)//When player is pressing right
                {
                    plyAnim.SetInteger("Facing", 1);
                }
                else if (PlyCtrl.Player.Movement.ReadValue<float>() < 0)//When player is pressing left
                {
                    plyAnim.SetInteger("Facing", -1);
                }
                else
                {
                    plyAnim.SetInteger("Facing", 0);
                }
            }
        }

        //Moving in Water as fish
        if(tag == "Fish" && inWater)
        {
            if(PlyCtrl.Player.FishInWater.ReadValue<Vector2>() != Vector2.zero)
            {
                plyAnim.SetBool("Walking", true);
            }
            else
            {
                plyAnim.SetBool("Walking", false);
            }
            angle = Vector2.SignedAngle(Vector2.left, PlyCtrl.Player.FishInWater.ReadValue<Vector2>());

            /*if(angle > 90)
            {
                angle = transform.localScale.x * (90 - (angle - 90));
            }
            else if(angle < -90)
            {
                angle = transform.localScale.x * (90 + (angle + 90));
            }*/

            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            //transform.rotation = rotation;
        }

        //Set direction as cat on wall
        if(OnWall && catDir == Vector2.right)
        {
            this.gameObject.transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y),
                Mathf.Abs(transform.localScale.z));
            right = true;
            left = false;
        }
        else if(OnWall && catDir == Vector2.left)
        {
            this.gameObject.transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y),
                Mathf.Abs(transform.localScale.z));
            left = true;
            right = false;
        }

        //Checking if on ground as bat while carrying box
        if (spcInter.objectHeld && tag == "Bat")
        {
            if (isBoxGrounded(spcInter.heldBox))
            {
                plyAnim.SetBool("isJumping", false);
            }
        }
        #endregion
    }

    //Special check
    private void SpecialS()
    {
        if(Time.timeScale > 0)
        {
            Special();
        }
    }

    //Interact check
    private void InteractI()
    {
        if (Time.timeScale > 0)
        {
            Interact();
        }
    }

    //Jump
    private void Jump()
    {
        if (canJump)
        {
            //If time is moving, do something
            if (Time.timeScale > 0)
            {
                //Fly when bat
                if (CompareTag("Bat") && batJump)
                {
                    batJump = false;
                    rb.AddForce((Vector2.up * jumpHeight) - new Vector2(0, rb.velocity.y), ForceMode2D.Impulse);
                    plyAnim.SetTrigger("Flap");
                    if (audioManager != null)
                    {
                        audioManager.Play("wingFlap");
                    }
                    StartCoroutine(FlyCoolDown());
                }
                //Side jump when climbing
                else if (CompareTag("Cat") && OnWall)
                {
                    rb.AddForce((-catDir * 25) - new Vector2(rb.velocity.x, 0), ForceMode2D.Impulse);
                    catDir = -catDir;
                }
                //Regular jump when appropriate
                else
                {
                    if (isGrounded() || inWater)
                    {
                        rb.AddForce((Vector2.up * jumpHeight) /*- new Vector2(0, rb.velocity.y)*/, ForceMode2D.Impulse);

                    }
                    else if (spcInter.isAttached)
                    {
                        spcInter.ShootTendril();
                        rb.AddForce(rb.velocity.normalized * 5, ForceMode2D.Impulse);
                    }
                }
                plyAnim.SetTrigger("takeOff");
                plyAnim.SetBool("isJumping", true);
            }
        }
    }

    //Embody check
    private void EmbodyE()
    {
        if (Time.timeScale > 0)
        {
            Embody();
        }
    }

    //Check when a trigger is entered
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (CompareTag("Fish"))
        {
            if (other.CompareTag("Water"))
            {
                if (audioManager != null)
                {
                    audioManager.Play("splash");
                }
                inWater = true;
                capCollider.density = 5.5f;
            }
        }
        else if (CompareTag("Blob"))
        {
            if (other.CompareTag("Water"))
            {
                //Makes Blob float
                if (audioManager != null)
                {
                    audioManager.Play("splash");
                }
                inWater = true;
                plyAnim.SetBool("inWater", true);
                capCollider.density = 2;
                jumpHeight = 35;
            }
        }
        else
        {
            if (other.CompareTag("Water"))
            {
                plyAnim.SetTrigger("Death");
            }
        }

        //If the Trigger is Death, call Death delegate
        if(other.CompareTag("Death") || other.CompareTag("Skeleton"))
        {
            plyAnim.SetTrigger("Death");
        }
    }

    //Check for when the player stays in a trigger
    private void OnTriggerStay2D(Collider2D other)
    {
        if (CompareTag("Fish"))
        {
            if (other.CompareTag("Water"))
            {
                inWater = true;
                plyAnim.SetBool("inWater", true);
            }
        }
    }

    //Check for when the trigger is exited
    private void OnTriggerExit2D(Collider2D other)
    {
        if (CompareTag("Fish"))
        {
            if (other.CompareTag("Water"))
            {
                inWater = false;
                plyAnim.SetBool("inWater", false);
                //capCollider.density = cntrlMove.skeloData.density;
            }
        }
        else if (CompareTag("Blob"))
        {
            if (other.CompareTag("Water"))
            {
                //Blob jumps out of water
                inWater = false;
                capCollider.density = cntrlMove.defaultDensity;
                jumpHeight = cntrlMove.defaultJumpHeight;
            }
        }
    }

    //Kill from menu
    public void MenuKill()
    {
        Debug.Log("Menu Kill called");
        plyAnim.SetTrigger("Death");
    }

    public void SetCatOnWall(bool value, Vector2 direction) 
    { 
        OnWall = value;
        catDir = direction;
    }

    //Checks if the player is on the ground
    public bool isGrounded()
    {
        float dist = 0f;
        Vector2 origin = new Vector2(capCollider.bounds.center.x, capCollider.bounds.min.y);
        Vector2 size = new Vector2(capCollider.size.x, 0.05f);
        RaycastHit2D hit = Physics2D.CapsuleCast(origin, size, CapsuleDirection2D.Horizontal, 0f, Vector2.down, 
            dist, groundLayerMask);

        //Debug.Log(hit.collider);
        return hit.collider != null;
    }

    //Checks if the box is on the ground (Bat)
    public bool isBoxGrounded(Rigidbody2D rb)
    {
        BoxCollider2D[] cols = new BoxCollider2D[2];
        rb.GetAttachedColliders(cols); //Gets all the colliders attached to a box
        BoxCollider2D col;

        //Determines which collider is the box collider, versus the trigger collider
        if (cols[0].isTrigger)
        {
            col = cols[1];
        }
        else
        {
            col = cols[0];
        }

        float dist = 0.04f;
        Vector2 size = new Vector2(col.bounds.size.x, dist);
        RaycastHit2D hit = Physics2D.BoxCast(new Vector2(col.bounds.center.x, col.bounds.min.y - size.y), size, 0f, Vector2.down, 
            0, groundLayerMask);

        //Debug.Log("hit is " + hit.collider);
        return hit.collider != null;
    }

    /* Bat */
    //Cooldown for jumping in midair
    IEnumerator FlyCoolDown()
    {
        yield return new WaitForSeconds(0.1f);
        batJump = true;
    }

    public void DisableMovement()
    {
        canMove = false;
        canJump = false;
    }

    public void RenableMovement()
    {
        canMove = true;
        canJump = true;
    }

    public void PlaySoundFromAudioManager(string name)
    {
        audioManager.Play(name);
    }

    //Calls all functions subscribed to death; Used in Death animation
    void TriggerDeath()
    {
        Death();
        //this.gameObject.SetActive(false);
        plySprite.enabled = false;
    }

    void FreezePlayer()
    {
        rb.simulated = false;
        rb.velocity = Vector2.zero;
        DisableMovement();
    }

    void UnFreezePlayer()
    {
        rb.simulated = true;
        RenableMovement();
    }

    //Used for bug testing
    private void OnDrawGizmos()
    {
        float dist = 0f;
        RaycastHit2D hit = Physics2D.CapsuleCast(capCollider.bounds.center, capCollider.size, capCollider.direction, 0f, Vector2.down,
            dist, groundLayerMask);

        Gizmos.DrawLine(hit.centroid + new Vector2(capCollider.bounds.extents.x, 0), 
            hit.centroid - new Vector2(capCollider.bounds.extents.x, 0));
        Gizmos.DrawLine(hit.centroid + new Vector2(0, capCollider.bounds.extents.y), 
            hit.centroid - new Vector2(0, capCollider.bounds.extents.y));

    }
}
