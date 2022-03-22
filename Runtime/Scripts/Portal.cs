using System;
using System.Threading.Tasks;
using Scripts;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class Portal : MonoBehaviour
{
    public GameObject RenderPlane;

    private MeshRenderer _renderPlaneRenderer;
    private RenderTexture _renderTexture;
    
    //private Camera _portalCamera;
    private PortalCamera _leftPortalCamera;
    private PortalCamera _rightPortalCamera;
    
    private static readonly int LeftRenderTexture = Shader.PropertyToID("_LeftEyeTexture");
    private static readonly int RightRenderTexture = Shader.PropertyToID("_RightEyeTexture");

    public void Initialize(PortalTransition transition)
    {
        _leftPortalCamera = new GameObject("LeftCamera").AddComponent<PortalCamera>();
        _leftPortalCamera.Initialize(this,transition,Camera.StereoscopicEye.Left);
        
        _rightPortalCamera = new GameObject("RightCamera").AddComponent<PortalCamera>();
        _rightPortalCamera.Initialize(this,transition,Camera.StereoscopicEye.Right);

        if (RenderPlane == null)
        {
            RenderPlane = transform.Find("RenderPlane").gameObject;
        }
        _renderPlaneRenderer = RenderPlane.GetComponent<MeshRenderer>();

        _renderPlaneRenderer.material.SetTexture(LeftRenderTexture, _leftPortalCamera.GetRenderTexture());
        _renderPlaneRenderer.material.SetTexture(RightRenderTexture, _rightPortalCamera.GetRenderTexture());
    }
}
