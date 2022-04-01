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

    private Transition _transition;
    private Portal _otherPortal;
    private List<PortalTraveller> _travellers = new List<PortalTraveller>();

    private static readonly int LeftRenderTexture = Shader.PropertyToID("_LeftEyeTexture");
    private static readonly int RightRenderTexture = Shader.PropertyToID("_RightEyeTexture");

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

        _otherPortal = transition.GetOtherPortal(this);

        _renderPlaneRenderer.material.SetTexture(LeftRenderTexture, _leftPortalCamera.GetRenderTexture());
        _renderPlaneRenderer.material.SetTexture(RightRenderTexture, _rightPortalCamera.GetRenderTexture());
    }

    private bool FrontOfPortal(Vector3 pos)
    {
        return Math.Sign(Vector3.Dot(pos - transform.position, transform.forward)) > 0;
    }

    private void Update()
    {
        HandleTravellers();
    }
    
    private void HandleTravellers()
    {
        for (int i = 0; i < _travellers.Count; i++)
        {
            PortalTraveller traveller = _travellers[i];

            if (FrontOfPortal(traveller.LastPosition) && !FrontOfPortal(traveller.transform.position))
            {
                var m = _otherPortal.transform.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180f,Vector3.up)) * transform.worldToLocalMatrix *
                        traveller.transform.localToWorldMatrix;
                Vector3 targetPosition = m.GetColumn(3);
                Quaternion targetRotation =
                    Quaternion.FromToRotation(transform.forward, _otherPortal.transform.forward) *
                    Quaternion.AngleAxis(180f, Vector3.up);
                _transition.TriggerTransition(traveller,targetPosition,targetRotation);
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