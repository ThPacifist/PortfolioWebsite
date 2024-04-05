using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CatController : Controller
{
    public static Action Scratch = delegate { };
    public static Action JumpAction = delegate { };
    [Header("Cat Settings")]
    public bool OnWall;
    public bool treadmill = false;

    Vector2 catDir;
    float defGravScale;

    public override void Start()
    {
        base.Start();

        defGravScale = PlayerBrain.PB.rb.gravityScale;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if(OnWall)
        {
            Embodiment.canDisembody = false;
        }
        else
        {
            Embodiment.canDisembody = true;
        }


        if(OnWall)
        {
            PlayerBrain.PB.rb.AddForce(catDir * 10, ForceMode2D.Impulse);//Applies force in towards the climbing wall
        }

        if (PlayerBrain.PB.canMove && !treadmill)
        {
            //Regular grounded movement
            if (PlyCtrl.Player.Movement.ReadValue<float>() != 0 && !OnWall)
            {
                //Movement
                if (Mathf.Abs(PlayerBrain.PB.rb.velocity.x) < speed)
                {
                    PlayerBrain.PB.rb.AddForce(Vector2.right * PlyCtrl.Player.Movement.ReadValue<float>() * 20 * PlayerBrain.PB.rb.mass);
                }

                //Audio
                if (audioManager != null)
                {
                    audioManager.Stop("catClimb");
                }
            }
            //If Cat is on wall
            else if (OnWall)
            {
                //Checks if the player is pushing up or down
                if (PlyCtrl.Player.FishInWater.ReadValue<Vector2>().y != 0)
                {
                    PlayerBrain.PB.rb.velocity += (Vector2.up * PlyCtrl.Player.FishInWater.ReadValue<Vector2>().y * speed * 0.75f) - new Vector2(0, PlayerBrain.PB.rb.velocity.y);
                    if (audioManager != null)
                    {
                        //audioManager.Play("catClimb");
                    }
                }
                else if (audioManager != null)
                {
                    audioManager.Stop("catClimb");
                }
            }
            
            if (!OnWall)
            {
                if (audioManager != null)
                {
                    audioManager.Stop("catClimb");
                }
            }

            //Remove momentum while on wall
            if (PlyCtrl.Player.FishInWater.ReadValue<Vector2>().y == 0 && OnWall)
            {
                PlayerBrain.PB.rb.velocity *= new Vector2(1, 0.5f);
            }

            #region Animation Block
            //Climbing on Wall as cat
            if (OnWall)
            {

                if (PlyCtrl.Player.FishInWater.ReadValue<Vector2>().y > 0)
                {
                    PlayerBrain.PB.plyAnim.SetInteger("WallState", 1);
                }
                else if(PlyCtrl.Player.FishInWater.ReadValue<Vector2>().y < 0)
                {
                    PlayerBrain.PB.plyAnim.SetInteger("WallState", -1);
                }
                else
                {
                    PlayerBrain.PB.plyAnim.SetInteger("WallState", 0);
                }
            }
            else
            {
                PlayerBrain.PB.plyAnim.SetBool("Climb", false);
                PlayerBrain.PB.plyAnim.SetInteger("WallState", 0);
            }

            //Set direction as cat on wall
            if (OnWall && catDir == Vector2.right)
            {
                this.gameObject.transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y),
                    Mathf.Abs(transform.localScale.z));
                right = true;
                left = false;
            }
            else if (OnWall && catDir == Vector2.left)
            {
                this.gameObject.transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y),
                    Mathf.Abs(transform.localScale.z));
                left = true;
                right = false;
            }
            #endregion
        }
    }

    public override void SetToDefault()
    {
        catDir = Vector2.zero;
        OnWall = false;
        PlayerBrain.PB.plyAnim.SetTrigger(form);
    }

    public void SetCatOnWall(bool value, Vector2 direction)
    {
        OnWall = value;
        catDir = direction;
        PlayerBrain.PB.plyAnim.SetBool("Climb", value);
        if (value)
        {
            PlayerBrain.PB.rb.gravityScale = 0;
        }
        else
        {
            PlayerBrain.PB.rb.gravityScale = defGravScale;
        }
    }

    public override void Jump()
    {
        if (PlayerBrain.PB.canJump)
        {
            if (isGrounded() && !OnWall)
            {
                PlayerBrain.PB.rb.AddForce((Vector2.up * jumpHeight) /*- new Vector2(0, rb.velocity.y)*/, ForceMode2D.Impulse);
            }
            //Side jump when climbing
            else if (OnWall)
            {
                PlayerBrain.PB.rb.AddForce(new Vector2(-catDir.x, 1) * jumpHeight * 0.5f, ForceMode2D.Impulse);
                catDir = -catDir;
            }

            base.Jump();
            JumpAction();
        }
    }

    public override void Special()
    {
        //Spawn hitbox
        Scratch();
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

    public override void ToggleBody(bool value)
    {
        Embodiment.canDisembody = value;
    }
}
