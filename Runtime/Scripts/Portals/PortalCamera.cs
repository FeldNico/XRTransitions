using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;

namespace Scripts
{
    public class PortalCamera : MonoBehaviour
    {
        private bool _isInitialized = false;

        private Camera _camera;
        private Camera _mainCamera;
        private Portal _portal;
        private Renderer _portalPlaneRenderer;
        private Transform _eyeTransform;
        private Transform _portalTransform;
        private Transform _destination;
        
        private static readonly int LeftRenderTexture = Shader.PropertyToID("_LeftEyeTexture");
        private static readonly int RightRenderTexture = Shader.PropertyToID("_RightEyeTexture");

        public void Initialize(Portal portal, PortalTransition transition, Camera.StereoscopicEye eye)
        {
            _portal = portal;
            _portalTransform = portal.transform;
            transform.parent = _portalTransform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            
            _destination = transition.Destination;
            
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = gameObject.AddComponent<Camera>();
            }
            _mainCamera = transition.Camera;
            _camera.CopyFrom(_mainCamera);
            _camera.forceIntoRenderTexture = true;
            _camera.targetTexture = new RenderTexture(_mainCamera.pixelWidth*2, _mainCamera.pixelHeight*2, 24);
            _camera.aspect = _mainCamera.aspect;
            _camera.fieldOfView = _mainCamera.fieldOfView;
            _camera.projectionMatrix = _mainCamera.GetStereoProjectionMatrix(eye);
            _camera.nonJitteredProjectionMatrix = _mainCamera.GetStereoNonJitteredProjectionMatrix(eye);
            _camera.enabled = false;
            
            _eyeTransform = eye == Camera.StereoscopicEye.Left
                ? transition.EyeLeftTransform
                : transition.EyeRightTransform;
            _portalPlaneRenderer = portal.RenderPlane.GetComponent<Renderer>();
            _portalPlaneRenderer.material.SetTexture(eye == Camera.StereoscopicEye.Left ? LeftRenderTexture : RightRenderTexture, _camera.targetTexture);

            _isInitialized = true;
        }

        private void OnEnable()
        {
            InputSystem.onAfterUpdate += RenderPortal;
        }

        private void OnDisable()
        {
            InputSystem.onAfterUpdate -= RenderPortal;
        }

        private void RenderPortal()
        {
            if (_isInitialized && InputState.currentUpdateType == InputUpdateType.BeforeRender && _portalPlaneRenderer.isVisible )
            {
                var localToWorldMatrix = _destination.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180f,Vector3.up)) * _portalTransform.worldToLocalMatrix * _eyeTransform.localToWorldMatrix;
                transform.SetPositionAndRotation(localToWorldMatrix.GetColumn(3),localToWorldMatrix.rotation);
                _camera.Render();
            }
        }
    }
}