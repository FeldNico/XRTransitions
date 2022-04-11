using System;
using System.Collections;
using System.Collections.Generic;
using Scripts;
using Unity.XR.CoreUtils;
using UnityEngine;

public class PortalTraveller : Traveller
{
    public Vector3 LastPosition { get; set; }

    private void Awake()
    {
        if (Origin == null)
        {
            Origin = transform;
        }

        LastPosition = transform.position;
    }

    private void LateUpdate()
    {
        LastPosition = transform.position;
    }
}
