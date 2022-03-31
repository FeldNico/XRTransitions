using System;
using Scripts;
using Unity.XR.CoreUtils;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public GameObject RenderPlane;

    private MeshRenderer _renderPlaneRenderer;
    private RenderTexture _renderTexture;
    private PlaneMeshDeformer _deformer;
    
    private PortalCamera _leftPortalCamera;
    private PortalCamera _rightPortalCamera;

    private Portal _otherPortal;
    private PortalTraveller _traveller;

    private static readonly int LeftRenderTexture = Shader.PropertyToID("_LeftEyeTexture");
    private static readonly int RightRenderTexture = Shader.PropertyToID("_RightEyeTexture");

    private void Awake()
    {
        _traveller = FindObjectOfType<PortalTraveller>();
        if (_traveller == null)
        {
            _traveller = FindObjectOfType<XROrigin>().gameObject.AddComponent<PortalTraveller>();
        }

        if (RenderPlane == null)
        {
            RenderPlane = transform.Find("RenderPlane").gameObject;
        }

        _renderPlaneRenderer = RenderPlane.GetComponent<MeshRenderer>();
        _deformer = RenderPlane.GetComponent<PlaneMeshDeformer>();
    }

    public void Initialize(PortalTransition transition)
    {
        _leftPortalCamera = new GameObject("LeftCamera").AddComponent<PortalCamera>();
        _leftPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Left);

        _rightPortalCamera = new GameObject("RightCamera").AddComponent<PortalCamera>();
        _rightPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Right);

        _otherPortal = transition.GetOtherPortal(this);

        _renderPlaneRenderer.material.SetTexture(LeftRenderTexture, _leftPortalCamera.GetRenderTexture());
        _renderPlaneRenderer.material.SetTexture(RightRenderTexture, _rightPortalCamera.GetRenderTexture());
    }

    private int SideOfPortal(Vector3 pos)
    {
        return Math.Sign(Vector3.Dot(pos - transform.position, transform.forward));
    }

    private bool FrontOfPortal(Vector3 pos)
    {
        return SideOfPortal(pos) > 0;
    }

    private void LateUpdate()
    {
        HandleTravellers();
    }

    private void OnTriggerEnter(Collider other)
    {
        PortalTraveller traveller = other.GetComponent<PortalTraveller>();
        if (traveller && !traveller.InPortal)
        {
            OnTravellerEnterPortal(traveller);
            traveller.InPortal = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PortalTraveller traveller = other.GetComponent<PortalTraveller>();
        if (traveller)
        {
            traveller.InPortal = false;
        }

        _deformer.ClearDeformingForce();
    }

    private void HandleTravellers()
    {
        if (!gameObject.activeSelf || _traveller == null) return;
        
        if (FrontOfPortal(_traveller.LastPosition) && !FrontOfPortal(_traveller.Target.position))
        {
            var m = _otherPortal.transform.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180f,Vector3.up)) * transform.worldToLocalMatrix *
                    _traveller.Target.localToWorldMatrix;
            _traveller.Teleport(m.GetColumn(3),Quaternion.FromToRotation(transform.forward,_otherPortal.transform.forward)* Quaternion.AngleAxis(180f,Vector3.up));
            _otherPortal.OnTravellerEnterPortal(_traveller);
        }
        else
        {
            _traveller.LastPosition = _traveller.Target.position;
        }

        _deformer.AddDeformingForce(_traveller.Target.position, !FrontOfPortal(_traveller.Target.position));
    }

    private void OnTravellerEnterPortal(PortalTraveller traveller)
    {
        traveller.LastPosition = traveller.Target.position;
        _deformer.AddDeformingForce(traveller.Target.position, !FrontOfPortal(traveller.Target.position));
    }
}