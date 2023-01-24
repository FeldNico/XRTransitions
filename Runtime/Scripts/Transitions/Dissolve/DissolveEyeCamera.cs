using System;
using Scripts.Utils;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;

namespace Scripts
{
    public class DissolveEyeCamera : MonoBehaviour
    {
        private bool _isInitialized = false;

        public float nearClipOffset = 0.05f;
        public float nearClipLimit = 0.2f;

        private TransitionManager _transitionManager;
        private Camera _camera;
        private Camera _mainCamera;
        private Camera.StereoscopicEye _eye;
        private Renderer _dissolvePlaneRenderer;
        private Transform _eyeTransform;

        private static readonly int LeftRenderTexture = Shader.PropertyToID("_LeftEyeOriginalTexture");
        private static readonly int RightRenderTexture = Shader.PropertyToID("_RightEyeOriginalTexture");

        private void Awake()
        {
            _transitionManager = FindObjectOfType<TransitionManager>();
        }

        public void Initialize(Dissolve dissolve, DissolveTransition transition, Camera.StereoscopicEye eye)
        {
            _mainCamera = _transitionManager.MainCamera;
            _eye = eye;
            _eyeTransform = _eye == Camera.StereoscopicEye.Left
                ? _transitionManager.LeftEyeTransform
                : _transitionManager.RightEyeTransform;
            transform.parent = _mainCamera.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = gameObject.AddComponent<Camera>();
            }
            
            _camera.CopyFrom(_mainCamera);
            _camera.forceIntoRenderTexture = true;
            _camera.targetTexture = new RenderTexture(_mainCamera.pixelWidth, _mainCamera.pixelHeight, 24);
            _camera.aspect = _mainCamera.aspect;
            _camera.fieldOfView = _mainCamera.fieldOfView;
            _camera.projectionMatrix = _mainCamera.GetStereoProjectionMatrix(eye);
            _camera.nonJitteredProjectionMatrix = _mainCamera.GetStereoNonJitteredProjectionMatrix(eye);
            _camera.enabled = false;
            _camera.cullingMask &= ~LayerMask.GetMask("Dissolve");

            _dissolvePlaneRenderer = dissolve.PlaneRenderer;
            _dissolvePlaneRenderer.material.SetTexture(_eye == Camera.StereoscopicEye.Left ? LeftRenderTexture : RightRenderTexture, _camera.targetTexture);

            _isInitialized = true;
        }

        private void OnEnable()
        {
            InputSystem.onAfterUpdate += Render;
        }

        private void OnDisable()
        {
            InputSystem.onAfterUpdate -= Render;
        }

        private void Render()
        {
            if (_isInitialized && InputState.currentUpdateType == InputUpdateType.BeforeRender )
            {
                _camera.Render();
            }
        }

        private void OnDestroy()
        {
            if (_camera != null && _camera.targetTexture != null)
            {
                _camera.targetTexture.Release();
            }
        }
    }
}