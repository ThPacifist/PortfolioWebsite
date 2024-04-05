using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishController : Controller
{
    //Variables
    [Header("Fish Settings")]
    public Switch lever;
    public float waterDensity;
    public float angle;

    Quaternion maintainedAngle;
    int frames = 3;
    int elapsedFrames = 0;
    bool isFishFlipping = false;
    float prevDir = 0;
    float temp;

    public override void FixedUpdate()
    {
        if(PlayerBrain.PB.inWater)
            prevDir = transform.localScale.x;
        base.FixedUpdate();
        if (PlayerBrain.PB.inWater)
        {
            if (prevDir != transform.localScale.x && !isFishFlipping)
            {
                transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z * -1);

                /*isFishFlipping = true;
                temp = prevDir;*/
            }

            /*if(isFishFlipping)
            {
                float ratio = (float)elapsedFrames / frames;
                transform.localScale = Vector3.Lerp(new Vector3(temp, transform.localScale.y, transform.localScale.z), 
                    new Vector3(temp * -1, transform.localScale.y, transform.localScale.z), 
                    ratio);
                elapsedFrames = (elapsedFrames + 1) % (frames + 1);
                if (ratio == 1)
                {
                    elapsedFrames = 0;
                    isFishFlipping = false;
                }
            }*/
        }

        //Movement when in water
        //Handles movemnt when on the land
        if (PlayerBrain.PB.canMove)
        {
            if (PlayerBrain.PB.inWater)
            //if (PlyCtrl.Player.Movement.ReadValue<float>() != 0 && PlayerBrain.PB.inWater)
            {
                //Move when the player is pressing buttons
                if (PlyCtrl.Player.FishInWater.ReadValue<Vector2>() != Vector2.zero)
                {
                    float x = 0, y = 0;
                    //Checks if either the y or x velocity is exceeding the speed
                    if (Mathf.Abs(PlayerBrain.PB.rb.velocity.x) < speed)
                    {
                        x = 1;
                    }
                    if (Mathf.Abs(PlayerBrain.PB.rb.velocity.y) < speed)
                    {
                        y = 1;
                    }

                    PlayerBrain.PB.rb.AddForce(new Vector2(x, y) * PlyCtrl.Player.FishInWater.ReadValue<Vector2>() * 20 * PlayerBrain.PB.rb.mass);
                }
            }
            //Movement when on the ground
            else if (isGrounded())
            {
                //Move when the player is pressing buttons
                if (PlyCtrl.Player.Movement.ReadValue<float>() != 0)
                {
                    if (Mathf.Abs(PlayerBrain.PB.rb.velocity.x) < speed * 0.3f)
                    {
                        PlayerBrain.PB.rb.AddForce(Vector2.right * PlyCtrl.Player.Movement.ReadValue<float>() * 20 * PlayerBrain.PB.rb.mass);
                    }
                }
            }
            else
            {
                //Move when the player is pressing the direction
                if (PlyCtrl.Player.Movement.ReadValue<float>() != 0)
                {
                    PlayerBrain.PB.rb.velocity += (Vector2.right * PlyCtrl.Player.Movement.ReadValue<float>() * speed * 0.5f) - new Vector2(PlayerBrain.PB.rb.velocity.x, 0);
                }
            }
        }

        #region Animation Block
        //Moving in Water as fish
        //Check if the blob is attached
        if (PlayerBrain.PB.inWater)
        {
            if (PlyCtrl.Player.FishInWater.ReadValue<Vector2>() != Vector2.zero)
            {
                PlayerBrain.PB.plyAnim.SetBool("Walking", true);
            }
            else
            {
                PlayerBrain.PB.plyAnim.SetBool("Walking", false);
            }

            //Fish Rotation Block
            Vector2 playerInput = new Vector2(Mathf.Round(PlyCtrl.Player.FishInWater.ReadValue<Vector2>().x), 
                Mathf.Round(PlyCtrl.Player.FishInWater.ReadValue<Vector2>().y));

            if (playerInput == Vector2.left || playerInput == Vector2.right)// Player is pressing left or right
            {
                maintainedAngle = Quaternion.Euler(0, 0, 0);
            }
            else if(playerInput == Vector2.up)// Player is pressing up
            {
                maintainedAngle = Quaternion.Euler(0, 0, -90 * Mathf.Round(transform.localScale.x));
            }
            else if (playerInput == Vector2.down)// Player is pressing down
            {
                maintainedAngle = Quaternion.Euler(0, 0, 90 * Mathf.Round(transform.localScale.x));
            }
            else if(playerInput == new Vector2(1, 1))// Player is press right and up
            {
                maintainedAngle = Quaternion.Euler(0, 0, 45);
            }
            else if (playerInput == new Vector2(1, -1))// Player is pressing right and down
            {
                maintainedAngle = Quaternion.Euler(0, 0, -45);
            }
            else if (playerInput == new Vector2(-1, 1))// Player is pressing left and up
            {
                maintainedAngle = Quaternion.Euler(0, 0, -45);
            }
            else if (playerInput == new Vector2(-1, -1))// Player is pressing left and down
            {
                maintainedAngle = Quaternion.Euler(0, 0, 45);
            }

            if (transform.rotation != maintainedAngle)
            {
                //float ratio = (float)elapsedFrames / frames;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, maintainedAngle, 5);
                //elapsedFrames = (elapsedFrames + 1) % (frames + 1);
            }

        }
        else
        {
            maintainedAngle = Quaternion.Euler(0, 0, 0);
            if (transform.rotation != maintainedAngle)
            {
                //float ratio = (float)elapsedFrames / frames;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, maintainedAngle, 5);
                //elapsedFrames = (elapsedFrames + 1) % (frames + 1);
            }
        }
        #endregion
    }

    //Set to default
    public override void SetToDefault()
    {
        PlayerBrain.PB.inWater = false;
        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        this.transform.rotation = rotation;
    }

    //Jump
    public override void Jump()
    {
        if (PlayerBrain.PB.canJump)
        {
            if (isGrounded() || PlayerBrain.PB.inWater)
            {
                PlayerBrain.PB.rb.AddForce((Vector2.up * jumpHeight) /*- new Vector2(0, rb.velocity.y)*/, ForceMode2D.Impulse);
            }

            base.Jump();// this goes last in function
        }
    }

    //Special
    public override void Special()
    {
        if (PlayerBrain.PB.inWater)
            PlayerBrain.PB.plyAnim.SetTrigger("Spin");

        if (lever != null)
        {
            //Activate the lever
            lever.Interact();
        }
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if (PlayerBrain.PB.currentController == this)
        {
            //If the collision is water, swim
            if (other.CompareTag("Water"))
            {
                if (audioManager != null)
                {
                    audioManager.Play("splash");
                }
                PlayerBrain.PB.inWater = true;
                PlayerBrain.PB.plyCol.density = waterDensity;
                PlayerBrain.PB.canJump = false;
            }
        }
    }

    public override void OnTriggerStay2D(Collider2D other)
    {
        if (PlayerBrain.PB.currentController == this)
        {
            if (other.CompareTag("Water"))
            {
                PlayerBrain.PB.inWater = true;
                PlayerBrain.PB.plyAnim.SetBool("inWater", true);
            }
        }
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        if (PlayerBrain.PB.currentController == this)
        {
            //If the collision is water, stop swimming
            if (other.CompareTag("Water"))
            {
                PlayerBrain.PB.inWater = false;
                PlayerBrain.PB.canJump = true;
                PlayerBrain.PB.plyCol.density = density;
                PlayerBrain.PB.plyAnim.SetBool("inWater", false);
            }
        }
    }

    IEnumerator SpinFish(Quaternion angle)
    {
        while(transform.rotation != angle)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, angle, 8 * Time.deltaTime);
            yield return null;
        }
    }

    //Sets lever value
    public void SetLever(Switch l)
    {
        lever = l;
    }

    public override void ToggleBody(bool value)
    {
        Embodiment.canDisembody = value;
    }
}
