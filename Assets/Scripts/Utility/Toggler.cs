using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Toggler : MonoBehaviour
{
    MonoBehaviour controller;

    // Start is called before the first frame update
    protected void Start()
    {
        var behaviours = transform.parent.GetComponents<MonoBehaviour>();
        controller = behaviours.FirstOrDefault(b => b.GetType().Name.Equals(name[..^7]));
    }

    public void ToggleController(bool enabled)
    {
        controller.enabled = enabled;
    }
}
