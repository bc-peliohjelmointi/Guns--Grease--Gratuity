using UnityEngine;
using UnityEngine.InputSystem;

public class PullOurScript : MonoBehaviour
{
    public GameObject gun;
    public Animator animator;

    public bool gunIsOut = true;

    void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            gunIsOut = !gunIsOut;
            

            if (gunIsOut)
            {
                // Activate the gun instantly before the pull-out animation plays
                Debug.Log("Gun out");
               
            }
            else
            {
               
            }
            animator.SetBool("GunOut", gunIsOut);
            if (animator.GetBool("GunOut"))
                Debug.Log("Animator");
        }
    }

}
