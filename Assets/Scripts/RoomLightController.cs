using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoomLightController : MonoBehaviour
{
    public Light2D[] lights;

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.gameObject.name == "hurtbox")
        {
            foreach (Light2D light in lights)
                light.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name == "hurtbox")
        {
            foreach (Light2D light in lights)
                light.enabled = false;
        }
    }
}