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
        private Renderer _portalPlaneRenderer;
        private Transform _eyeTransform;
        private Transform _portalTransform;
        private Transform _otherPortalTransform;

        public void Initialize(Portal portal, PortalTransition transition,Camera.StereoscopicEye eye)
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = gameObject.AddComponent<Camera>();
            }
            _mainCamera = transition.Camera;
            _camera.CopyFrom(_mainCamera);
            
            _camera.forceIntoRenderTexture = true;
            _camera.targetTexture = new RenderTexture(_mainCamera.pixelWidth, _mainCamera.pixelHeight, 24);
            _camera.aspect = _mainCamera.aspect;
            _camera.fieldOfView = _mainCamera.fieldOfView;
            _camera.projectionMatrix = _mainCamera.GetStereoProjectionMatrix(eye);
            _camera.nonJitteredProjectionMatrix = _mainCamera.GetStereoNonJitteredProjectionMatrix(eye);
            _camera.enabled = false;
            
            _eyeTransform = eye == Camera.StereoscopicEye.Left
                ? transition.EyeLeftTransform
                : transition.EyeRightTransform;
            _portalPlaneRenderer = portal.RenderPlane.GetComponent<Renderer>();
            _otherPortalTransform = transition.GetOtherPortal(portal).transform;
            _portalTransform = portal.transform;

            transform.parent = _portalTransform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            _isInitialized = true;
        }

        public RenderTexture GetRenderTexture()
        {
            return _camera.targetTexture;
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
            if (_isInitialized && _portalPlaneRenderer.isVisible && InputState.currentUpdateType == InputUpdateType.BeforeRender)
            {
                var localToWorldMatrix = _otherPortalTransform.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180f,Vector3.up)) * _portalTransform.worldToLocalMatrix * _eyeTransform.localToWorldMatrix;
                transform.SetPositionAndRotation(localToWorldMatrix.GetColumn(3),localToWorldMatrix.rotation);
                _camera.Render();
            }
        }
    }
}