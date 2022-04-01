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
        if (Player == null)
        {
            Player = GetComponentInParent<XROrigin>().transform;
        }

        LastPosition = transform.position;
    }

    private void LateUpdate()
    {
        LastPosition = transform.position;
    }
}
