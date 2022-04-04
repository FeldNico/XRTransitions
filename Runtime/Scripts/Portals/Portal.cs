using System;
using System.Collections.Generic;
using Scripts;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public GameObject RenderPlane;

    private MeshRenderer _renderPlaneRenderer;

    private PortalCamera _leftPortalCamera;
    private PortalCamera _rightPortalCamera;

    private PortalTransition _transition;
    private Transform _destination;
    private List<PortalTraveller> _travellers = new List<PortalTraveller>();

    private void Awake()
    {
        if (RenderPlane == null)
        {
            RenderPlane = transform.Find("RenderPlane").gameObject;
        }

        _renderPlaneRenderer = RenderPlane.GetComponent<MeshRenderer>();
    }

    public void Initialize(PortalTransition transition)
    {
        _transition = transition;
        
        _leftPortalCamera = new GameObject("LeftCamera").AddComponent<PortalCamera>();
        _leftPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Left);

        _rightPortalCamera = new GameObject("RightCamera").AddComponent<PortalCamera>();
        _rightPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Right);

        _destination = transition.Destination;
    }

    public bool FrontOfPortal(Vector3 pos)
    {
        Transform t = transform;
        return Math.Sign(Vector3.Dot(pos - t.position, t.forward)) > 0;
    }

    private void Update()
    {
        HandleTravellers();
    }
    
    private async void HandleTravellers()
    {
        for (int i = 0; i < _travellers.Count; i++)
        {
            PortalTraveller traveller = _travellers[i];

            if (FrontOfPortal(traveller.LastPosition) && !FrontOfPortal(traveller.transform.position))
            {
                var m = _destination.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180f,Vector3.up)) * transform.worldToLocalMatrix *
                        traveller.transform.localToWorldMatrix;
                Vector3 targetPosition = m.GetColumn(3);
                Quaternion targetRotation =
                    Quaternion.FromToRotation(transform.forward, _destination.forward) *
                    Quaternion.AngleAxis(180f, Vector3.up);
                await _transition.TriggerTransition(traveller,targetPosition,targetRotation);
                _travellers.RemoveAt(i);
                i--;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PortalTraveller traveller = other.GetComponent<PortalTraveller>();
        if (traveller && !_travellers.Contains(traveller))
        {
            _travellers.Add(traveller);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PortalTraveller traveller = other.GetComponent<PortalTraveller>();
        if (traveller && _travellers.Contains(traveller))
        {
            _travellers.Remove(traveller);
        }
    }
}