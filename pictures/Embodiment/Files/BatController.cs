using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatController : Controller
{
    [Header("Bat Settings")]
    public Rigidbody2D heldBox;
    [SerializeField]
    Transform heldPos;
    [HideInInspector]
    public Rigidbody2D box;
    string boxTag;
    bool batJump = true;
    bool boxHeld;

    private bool flap;

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if(boxHeld)
        {
            heldBox.transform.position = heldPos.transform.position;
        }

        if(PlayerBrain.PB.canMove)
        {
            //Ground Movement
            if (Mathf.Abs(PlayerBrain.PB.rb.velocity.x) < speed)
            {
                PlayerBrain.PB.rb.AddForce(Vector2.right * PlyCtrl.Player.Movement.ReadValue<float>() * 20 * PlayerBrain.PB.rb.mass);
            }
        }

        #region Animation Block
        //Checking if on ground as bat while carrying box
        if (boxHeld && tag == "Bat")
        {
            if (isBoxGrounded(heldBox))
            {
                PlayerBrain.PB.plyAnim.SetBool("isJumping", false);
            }
        }
        #endregion
    }

    public override void Jump()
    {
        if (PlayerBrain.PB.canJump)
        {
            //Fly when bat
            if (batJump)
            {
                batJump = false;
                PlayerBrain.PB.rb.AddForce((Vector2.up * jumpHeight) - new Vector2(0, PlayerBrain.PB.rb.velocity.y), ForceMode2D.Impulse);
                if (audioManager != null)
                {
                    PlayerBrain.PB.plyAnim.SetTrigger("Flap");
                    audioManager.Play("wingFlap");
                }
                StartCoroutine(FlyCoolDown());

                if (flap)
                {
                    base.Jump();
                }
            }
        }
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
        //Attach box
        if (box != null && !boxHeld)
        {
            if (boxTag == "LBox")
            {
                if (!Left && !Right)
                {
                    PlayerBrain.PB.plyAnim.SetBool("isGrabbing", true);
                }
                else
                {
                    PickUpBoxBat(box);
                }
            }
        }
        else if (boxHeld)
        {
            PlayerBrain.PB.plyAnim.SetBool("isGrabbing", false);
            PickUpBoxBat(null);
        }
    }

    public override void SetToDefault()
    {
        PlayerBrain.PB.plyAnim.SetBool("isGrabbing", false);
        boxHeld = false;
        boxTag = null;
        PlayerBrain.PB.fixedJ.enabled = false;
        PlayerBrain.PB.fixedJ.connectedBody = null;
        heldBox = null;
        box = null;
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
        int layer = LayerMask.GetMask("Jumpables", "PickupAbles");
        Vector2 size = new Vector2(col.bounds.size.x, dist);
        RaycastHit2D hit = Physics2D.BoxCast(new Vector2(col.bounds.center.x, col.bounds.min.y - size.y), size, 0f, Vector2.down,
            0, layer);

        //Debug.Log("hit is " + hit.collider);
        return hit.collider != null;
    }

    public void PickUpBoxBat(Rigidbody2D box)
    {
        if (box != null)
        {
            if (audioManager != null)
            {
                audioManager.Play("boxGrab");
            }
            PlayerBrain.PB.plyAnim.SetBool("isGrabbing", true);
            //Attach box
            heldBox = box;
            heldBox.gravityScale = 0;
            boxHeld = true;
            PlayerBrain.PB.fixedJ.enabled = true;
            PlayerBrain.PB.fixedJ.connectedBody = heldBox;
            heldBox.transform.position = heldPos.transform.position;
            ToggleBody(false);
        }
        else
        {
            if (audioManager != null)
            {
                audioManager.Play("boxGrab");
            }
            PlayerBrain.PB.plyAnim.SetBool("isGrabbing", false);
            boxHeld = false;
            heldBox.gravityScale = 1;
            PlayerBrain.PB.fixedJ.enabled = false;
            PlayerBrain.PB.fixedJ.connectedBody = null;
            heldBox = null;
            ToggleBody(true);
        }
    }

    //Sets the value of the held box
    public void SetHeldBox(Rigidbody2D rb, string inputTag)
    {
        box = rb;
        boxTag = inputTag;
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

    //Cooldown for jumping in midair
    IEnumerator FlyCoolDown()
    {
        yield return new WaitForSeconds(0.4f);
        batJump = true;
    }

    public override void CallFromAnimation(int value)
    {
        if (PlayerBrain.PB.currentController == this)
        {
            if (value == 0)
            {
                PickUpBoxBat(box);
            }
            else
            {
                PickUpBoxBat(null);
            }
        }
    }

    public override void ToggleBody(bool value)
    {
        Embodiment.canDisembody = value;
    }

    private void OnDrawGizmos()
    {
        //Bat Box Pos
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(heldPos.position, 0.3f);
    }

    //Flap animation 
    public void PlayFlapAnim()
    {
        flap = true;
    }

    public void StopFlapAnim()
    {
        //flap = false;
    }
}
