using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTraveller : MonoBehaviour
{
    public Transform Target;

    public Vector3 LastPosition { get; set; }
    public bool InPortal { get; set; }

    public void Awake()
    {
        if (Target == null)
        {
            Target = GetComponentInChildren<Camera>().transform;
        }
    }

    public virtual void Teleport(Vector3 pos, Quaternion rotation)
    {
        transform.SetPositionAndRotation(transform.position + pos - Target.position,transform.rotation * rotation);
        Physics.SyncTransforms();
    }
}
