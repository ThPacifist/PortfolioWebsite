using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Is the new Control Movement
/// Note: Whenever we want to prevent the player from emboding or disemboding somewhere, set canEmbody or canDisembody to false
///     but make sure you also set it back to true. Otherwise, the player will never be able to leave it's form
/// </summary>
public class Embodiment : MonoBehaviour
{
    public Transform currentSkeleton;
    public SkeletonTrigger targetSkeleton;
    public GameObject cloudPrefab;
    public PlayerBrain PB;
    public static bool canEmbody = true;
    public static bool canDisembody = false;

    private void OnEnable()
    {
        PlayerBrain.Embody += Embody;
        canEmbody = true;
    }

    private void OnDisable()
    {
        PlayerBrain.Embody -= Embody;
        PlayerBrain.Embody -= Disembody;
    }

    //Changes the player's form
    void Embody()
    {
        Debug.Log("Embody");    
        if(targetSkeleton != null && CheckSpace(targetSkeleton) && canEmbody && !PB.plyAnim.GetBool("isJumping"))
        {
            Instantiate(cloudPrefab, transform.position, Quaternion.identity);
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play("embody");
            }

            //Enables the controller of the targeted form which changes the current controller
            PlayerBrain.PB.currentController.enabled = false;
            PlayerBrain.Skeletons[targetSkeleton.type].enabled = true;
            targetSkeleton.isGrabbed = true;
            //Debug.Log("Player set to " + targetSkeleton.type);

            //Attach skeleton to player and disable it
            currentSkeleton = targetSkeleton.transform.parent;
            currentSkeleton.parent = transform;
            currentSkeleton.transform.position = transform.position;
            currentSkeleton.gameObject.SetActive(false);

            PlayerBrain.Embody -= Embody;
            canEmbody = false;
            PlayerBrain.Embody += Disembody;
            canDisembody = true;
        }
    }

    //Reverts the Player back to the blob form
    void Disembody()
    {
        Debug.Log("Disembody");
        if (currentSkeleton != null && canDisembody && !PB.plyAnim.GetBool("isJumping"))
        {
            Instantiate(cloudPrefab, transform.position, Quaternion.identity);
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play("unEmbody");
            }

            //Disables the controller of the old form and renables Blob form
            BlobController temp = (BlobController)PlayerBrain.Skeletons[PlayerBrain.skeleType.Blob];
            PlayerBrain.PB.currentController.enabled = false;
            PlayerBrain.Skeletons[PlayerBrain.skeleType.Blob].enabled = true;
            Debug.Log("Player set to " + targetSkeleton.type);

            //Renables skeleton
            currentSkeleton.gameObject.SetActive(true);
            currentSkeleton.parent = null;
            currentSkeleton = null;

            //Makes the Player Grab the skeleton
            if (targetSkeleton != null)
            {
                temp.heldSkel = targetSkeleton;
                temp.heldSkel.isGrabbed = true;
                Destroy(PlayerBrain.PB.prefabInstance);
                temp.skelHeld = true;
                PlayerBrain.PB.fixedJ.enabled = true;
                PlayerBrain.PB.fixedJ.connectedBody = temp.heldSkel.transform.parent.GetComponent<Rigidbody2D>();
                temp.heldSkel.skelGObject.transform.position = temp.skelHeldPos.transform.position;
                PlayerBrain.PB.plyAnim.SetBool("isGrabbing", true);
                PlayerBrain.PB.plyAnim.SetTrigger("Disembody");
                temp.jumpHeight = 60;
            }
            else
            {
                Debug.LogError("There is no skeleton to grab.");
            }

            PlayerBrain.Embody += Embody;
            canEmbody = true;
            PlayerBrain.Embody -= Disembody;
            canDisembody = false;
        }
    }

    //Used by Player to forcibly change the player to the correct form
    public void EmbodyThis(SkeletonTrigger target)
    {
        //if target is empty, transform into the blob
        if(target == null)
        {
            //Disables the controller of the old form and renables Blob form
            BlobController temp = (BlobController)PlayerBrain.Skeletons[PlayerBrain.skeleType.Blob];
            PlayerBrain.PB.currentController.enabled = false;
            PlayerBrain.Skeletons[PlayerBrain.skeleType.Blob].enabled = true;
                
            currentSkeleton.gameObject.SetActive(true);
            targetSkeleton.skeloScript.RespawnSkeleton();
            targetSkeleton = null;
            currentSkeleton.parent = null;
            currentSkeleton = null;

            PlayerBrain.Embody += Embody;
            canEmbody = true;
            PlayerBrain.Embody -= Disembody;
            canDisembody = false;
        }
        else
        {
            //Enables the controller of the targeted form which changes the current controller
            PlayerBrain.PB.currentController.enabled = false;
            PlayerBrain.Skeletons[target.type].enabled = true;
            target.isGrabbed = true;
            Debug.Log("Player set to " + target.type);

            if (currentSkeleton != null)
            {
                currentSkeleton.gameObject.SetActive(true);
                targetSkeleton.skeloScript.RespawnSkeleton();
                currentSkeleton.parent = null;
                currentSkeleton = null;
            }

            //Attach skeleton to player and disable it
            currentSkeleton = target.transform.parent;
            currentSkeleton.parent = transform;
            currentSkeleton.transform.position = transform.position;
            currentSkeleton.gameObject.SetActive(false);
            targetSkeleton = target;

            PlayerBrain.Embody -= Embody;
            canEmbody = false;
            PlayerBrain.Embody += Disembody;
            canDisembody = true;
        }
    }

    public void SetTargetSkeleton(SkeletonTrigger target)
    {
        targetSkeleton = target;
    }

    //Checks if theres is enough space for the next form
    bool CheckSpace(SkeletonTrigger target)
    {
        //Controller of the skeleton the player is transforming into
        Controller targetSkeleton = PlayerBrain.Skeletons[target.type];
        //Calculate the center of the collider after transforming
        Vector2 targetCenter = new Vector2(PlayerBrain.PB.plyCol.bounds.center.x, PlayerBrain.PB.plyCol.bounds.min.y + targetSkeleton.colliderSize.y);

        //Defines the colliders that will be detected by the cast to see if there is space
        int layer = LayerMask.NameToLayer("CheckSpace");

        //Send a Capsule cast to see if there is enough space for the player
        RaycastHit2D hit = Physics2D.CapsuleCast(targetCenter, targetSkeleton.colliderSize, targetSkeleton.direction,
            0f, Vector2.down, 0f, layer);
        //Debug.Log(hit.collider);
        return hit.collider == null;
    }

    /// <summary>
    /// To be used by the shrieker to remove the skeleton from the blob
    /// </summary>
    public void ShriekerEvent()
    {
        //EmbodyThis(null);
        Debug.Log("The type is: " + PlayerBrain.PB.currentController.GetType());
        //Checks if the current form is the blob, then reacts accordingly
        if (PlayerBrain.PB.currentController.GetType() == typeof(BlobController))
        {
            BlobController control = (BlobController)PlayerBrain.PB.currentController;

            //Detaches the skeleton from the player if it is carrying a skeleton
            if(control.skelHeld)
            {
                control.heldSkel.isGrabbed = false;
                control.heldSkel = null;
                control.skelHeld = false;
                PlayerBrain.PB.fixedJ.enabled = false;
                PlayerBrain.PB.fixedJ.connectedBody = null;
                PlayerBrain.PB.plyAnim.SetBool("isGrabbing", false);
                control.jumpHeight = 18.1f;
            }
        }
        else
        {
            EmbodyThis(null);
        }
    }

    //Check if it enters a no-disembody area
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Embody"))
        {
            canDisembody = false;
        }
    }

    //Check if it exits a no-disembody area
    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Embody"))
        {
            canDisembody = true;
        }
    }
}
