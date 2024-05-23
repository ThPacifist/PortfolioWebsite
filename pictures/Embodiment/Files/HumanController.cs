using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HumanController : Controller
{
    [Header("Human Settings")]
    public Rigidbody2D heldBox;
    [SerializeField]
    Transform heldPos;
    public bool boxHeld;
    public bool heavyBoxHeld;
    float defaultSpeed;
    float defaultJumpHeight;
    //[HideInInspector]
    public Rigidbody2D box;
    string boxTag;

    /*bool greenBox;
    public Vector2 greenOffset;
    public Vector2 greenSize;
    bool redBox;
    public Vector2 redOffset;
    public Vector2 redSize;
    float startingGrav;*/


    public override void Start()
    {
        base.Start();

        defaultSpeed = speed;
        defaultJumpHeight = jumpHeight;
    }

    public override void FixedUpdate()
    {
        if (boxHeld)
        {
            heldBox.transform.position = heldPos.transform.position;
        }

        if (PlayerBrain.PB.canMove)
        {
            //Ground Movement
            if (Mathf.Abs(PlayerBrain.PB.rb.velocity.x) < speed)
            {
                PlayerBrain.PB.rb.AddForce(Vector2.right * PlyCtrl.Player.Movement.ReadValue<float>() * 20 * PlayerBrain.PB.rb.mass);
            }
        }

        //Remove momentum while on ground
        if (PlyCtrl.Player.Movement.ReadValue<float>() == 0 && isGrounded())
        {
            //Reduce the player's speed by half
            PlayerBrain.PB.rb.velocity *= new Vector2(0.75f, 1);
            //Change any held boxes velocity to match the player
            if (heldBox != null)
            {
                heldBox.velocity = PlayerBrain.PB.rb.velocity;
            }
        }

        /*int layer = LayerMask.GetMask("Jumpables");

        greenBox = Physics2D.OverlapBox(new Vector2(transform.position.x + (greenOffset.x * transform.localScale.x), transform.position.y + (greenOffset.y)),
            greenSize, 0f, layer);
        redBox = Physics2D.OverlapBox(new Vector2(transform.position.x + (redOffset.x * transform.localScale.x), transform.position.y + (redOffset.y)),
            redSize, 0f, layer);

        if(greenBox && !redBox && !isGrounded() && !boxHeld && PlayerBrain.PB.canMove)
        {
            //activate ledge grab
        }*/

        #region Animation Block
        if (PlyCtrl.Player.Movement.ReadValue<float>() != 0 && PlayerBrain.PB.canMove)
        {
            PlayerBrain.PB.plyAnim.SetBool("Walking", true);
        }
        else
        {
            PlayerBrain.PB.plyAnim.SetBool("Walking", false);
        }

        if (isGrounded())
        {
            PlayerBrain.PB.plyAnim.SetBool("isJumping", false);
        }
        else
        {
            PlayerBrain.PB.plyAnim.SetBool("isJumping", true);
        }

        //Pushing and Pulling boxes as Human
        if (!heavyBoxHeld)
        {
            if (PlayerBrain.PB.canMove)
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
                    PlayerBrain.PB.plyAnim.SetInteger("Facing", -1);
                }
                else if (PlyCtrl.Player.Movement.ReadValue<float>() < 0)//When player is pressing left
                {
                    PlayerBrain.PB.plyAnim.SetInteger("Facing", 1);
                }
                else
                {
                    PlayerBrain.PB.plyAnim.SetInteger("Facing", 0);
                }
            }
            else if (this.transform.localScale.x < 0)//When player is facing right, while holding a heavy box
            {
                if (PlyCtrl.Player.Movement.ReadValue<float>() > 0)//When player is pressing right
                {
                    PlayerBrain.PB.plyAnim.SetInteger("Facing", 1);
                }
                else if (PlyCtrl.Player.Movement.ReadValue<float>() < 0)//When player is pressing left
                {
                    PlayerBrain.PB.plyAnim.SetInteger("Facing", -1);
                }
                else
                {
                    PlayerBrain.PB.plyAnim.SetInteger("Facing", 0);
                }
            }
        }
        #endregion
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);

        if (PlayerBrain.PB.currentController == this)
        {
            if (other.CompareTag("Water"))
            {
                PlayerBrain.PB.plyAnim.SetTrigger("Death");
            }
        }
    }

    public override void Special()
    {
        //Check if there is a box to hold or a box being held
        if (box != null && !boxHeld)
        {
            if (boxTag == "LBox" || boxTag == "MBox")
            {
                //f (!CheckSpaceForBox(box))
                //{
                    if (!Left && !Right)
                    {
                        PlayerBrain.PB.plyAnim.SetBool("isGrabbing", true);
                    }
                    else
                    {
                        PickUpBoxHuman(box);
                    }
                //}
                /*else
                {
                    Debug.LogError("Not Enough space for box");
                }*/
            }
            else if (boxTag == "HBox")
            {
                if (audioManager != null)
                {
                    audioManager.Play("boxGrab");
                }
                PlayerBrain.PB.plyAnim.SetBool("isPushing", true);

                //Attach Box
                heavyBoxHeld = true;
                heldBox = box;
                PlayerBrain.PB.canJump = false;
                PlayerBrain.PB.fixedJ.enabled = true;
                PlayerBrain.PB.fixedJ.connectedBody = heldBox;
                PlayerBrain.PB.fixedJ.connectedBody.mass = 6;
                speed = 3;
                ToggleBody(false);
            }
        }
        else if (boxHeld || heavyBoxHeld)
        {
            if (!Left && !Right)
            {
                PlayerBrain.PB.plyAnim.SetBool("isGrabbing", false);
                PickUpBoxHuman(null);
            }
            else
            {
                PickUpBoxHuman(null);
            }

            if (heavyBoxHeld)
            {
                PickUpBoxHuman(null);
            }

            PlayerBrain.Interact -= ThrowBox;
        }
    }

    public override void Jump()
    {
        if (PlayerBrain.PB.canJump)
        {
            if (isGrounded() || PlayerBrain.PB.inWater)
            {
                PlayerBrain.PB.rb.AddForce((Vector2.up * jumpHeight) /*- new Vector2(0, rb.velocity.y)*/, ForceMode2D.Impulse);
            }

            base.Jump();
        }
    }

    public void ThrowBox()
    {
        if(heldBox != null && boxHeld && boxTag != "HBox")
        {
            PlayerBrain.PB.plyAnim.SetBool("isGrabbing", false);
            heldBox.gravityScale = 1;
            jumpHeight = defaultJumpHeight;
            PlayerBrain.PB.fixedJ.enabled = false;
            PlayerBrain.PB.fixedJ.connectedBody = null;
            if (tempString == "LBox")
            {
                heldBox.gravityScale = tempValue;
            }
            PlayerBrain.PB.canJump = true;

            float facing = -1 * transform.localScale.x;

            Vector2 direction = new Vector2(facing + PlayerBrain.PB.rb.velocity.x, 
                0.45f + Mathf.Abs(PlayerBrain.PB.rb.velocity.y));
            Debug.Log("Direction: " + direction.normalized);
            heldBox.AddForce(direction.normalized * 300 * heldBox.mass);

            boxHeld = false;
            heldBox = null;
            tempString = "";
        }
        PlayerBrain.Interact -= ThrowBox;
        ToggleBody(true);
    }

    public override void SetToDefault()
    {
        PlayerBrain.PB.plyAnim.SetBool("isGrabbing", false);
        boxHeld = false;
        heavyBoxHeld = false;
        boxTag = null;
        PlayerBrain.PB.fixedJ.enabled = false;
        if (boxTag == "HBox")
            PlayerBrain.PB.fixedJ.connectedBody.mass = 20;
        speed = defaultSpeed;
        jumpHeight = defaultJumpHeight;
        PlayerBrain.PB.fixedJ.connectedBody = null;
        heldBox.gravityScale = 1;
        heldBox = null;
        box = null;
        PlayerBrain.Interact -= ThrowBox;
        ToggleBody(true);
    }

    //Sets the value of the held box
    public void SetHeldBox(Rigidbody2D rb, string inputTag)
    {
        if(rb != null && inputTag != "")
        {
            box = rb;
            boxTag = inputTag;
        }
        else
        {
            StartCoroutine(DelaySetting());
        }

        if (PlayerBrain.PB.prefabInstance == null)
        {
            PlayerBrain.PB.prefabInstance = Instantiate(PlayerBrain.PB.IndicatorPrefab, box.transform);
        }
        else if (rb == null)
        {
            Destroy(PlayerBrain.PB.prefabInstance);
        }
        else if (PlayerBrain.PB.prefabInstance != null)
        {
            Destroy(PlayerBrain.PB.prefabInstance);
            PlayerBrain.PB.prefabInstance = Instantiate(PlayerBrain.PB.IndicatorPrefab, box.transform);
        }
    }

    //Used to hold the mass of a light box
    float tempValue = 1;
    string tempString = "";
    public void PickUpBoxHuman(Rigidbody2D box)
    {
        if (box != null)
        {
            if (audioManager != null)
            {
                audioManager.Play("boxGrab");
            }
            tempString = boxTag;
            PlayerBrain.PB.plyAnim.SetBool("isGrabbing", true);
            //Attach box
            heldBox = box;
            boxHeld = true;
            PlayerBrain.PB.fixedJ.enabled = true;
            PlayerBrain.PB.fixedJ.connectedBody = heldBox;
            heldBox.transform.position = heldPos.transform.position;

            if(tempString == "LBox")
            {
                tempValue = heldBox.gravityScale;
                heldBox.gravityScale = 0;
            }
            else if(tempString == "MBox")
            {
                PlayerBrain.PB.canJump = false;
            }

            PlayerBrain.Interact += ThrowBox;

            ToggleBody(false);
        }
        else
        {
            if (audioManager != null)
            {
                audioManager.Play("boxGrab");
            }
            PlayerBrain.PB.plyAnim.SetBool("isPushing", false);
            PlayerBrain.PB.plyAnim.SetBool("isGrabbing", false);
            boxHeld = false;
            heavyBoxHeld = false;
            PlayerBrain.PB.fixedJ.enabled = false;
            if (tempString == "HBox")
            {
                PlayerBrain.PB.fixedJ.connectedBody.mass = 20;
            }
            else if (tempString == "LBox")
            {
                heldBox.gravityScale = tempValue;
            }
            PlayerBrain.PB.canJump = true;
            speed = defaultSpeed;
            PlayerBrain.PB.fixedJ.connectedBody = null;
            heldBox = null;
            tempString = "";

            PlayerBrain.Interact -= ThrowBox;

            ToggleBody(true);
        }
    }

    //Checks to see if there is enough space for the box, so that it does not get put through a wall
    bool CheckSpaceForBox(Rigidbody2D rb)
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

        float dist = 0;
        int layer = LayerMask.NameToLayer("CheckSpace");
        //Casts a box (much like raycast) into the scene
        RaycastHit2D hit = Physics2D.BoxCast(heldPos.position, col.size, 0f, Vector2.down, dist, layer);

        return hit.collider != null; //If collider exists, sends true. Otherwise false
    }

    public override void CallFromAnimation(int value)
    {
        if(PlayerBrain.PB.currentController == this)
        {
            if (value == 0)
            {
                PickUpBoxHuman(box);
            }
            else
            {
                PickUpBoxHuman(null);
            }
        }
    }

    public override void ToggleBody(bool value)
    {
        Embodiment.canDisembody = value;
    }

    IEnumerator DelaySetting()
    {
        yield return new WaitForSeconds(0.5f);
        box = null;
        boxTag = "";
    }

    private void OnDrawGizmos()
    {
        //Human Box Pos
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(heldPos.position, 0.3f);

        /*Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (redOffset.x * transform.localScale.x), transform.position.y + (redOffset.y)), redSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector2(transform.position.x + (greenOffset.x * transform.localScale.x), transform.position.y + (greenOffset.y)), greenSize);*/
    }
}
