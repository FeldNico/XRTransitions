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

        public float nearClipOffset = 0.05f;
        public float nearClipLimit = 0.2f;
        
        private Camera _camera;
        private Camera _mainCamera;
        private Camera.StereoscopicEye _eye;
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

            _eye = eye;
            _eyeTransform = _eye == Camera.StereoscopicEye.Left
                ? transition.EyeLeftTransform
                : transition.EyeRightTransform;
            _portalPlaneRenderer = portal.RenderPlane.GetComponent<Renderer>();
            _portalPlaneRenderer.material.SetTexture(_eye == Camera.StereoscopicEye.Left ? LeftRenderTexture : RightRenderTexture, _camera.targetTexture);

            _isInitialized = true;
        }
        
        void SetNearClipPlane () {
            // Learning resource:
            // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
            int dot = Math.Sign (Vector3.Dot (_destination.forward, _destination.position - transform.position));

            Vector3 camSpacePos = _camera.worldToCameraMatrix.MultiplyPoint(_destination.position);
            Vector3 camSpaceNormal = _camera.worldToCameraMatrix.MultiplyVector(_destination.forward) * dot;
            float camSpaceDst = -Vector3.Dot (camSpacePos, camSpaceNormal) + nearClipOffset;

            // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
            if (Mathf.Abs (camSpaceDst) > nearClipLimit) {
                Vector4 clipPlaneCameraSpace = new Vector4 (camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);
                _camera.projectionMatrix = CalculateObliqueMatrix(_mainCamera.GetStereoProjectionMatrix(_eye), clipPlaneCameraSpace);
            } else {
                _camera.projectionMatrix = _mainCamera.GetStereoProjectionMatrix(_eye);
            }
        }
        
        static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
        {
            Matrix4x4 obliqueMatrix = projection;
            Vector4 q = projection.inverse * new Vector4(
                Math.Sign(clipPlane.x),
                Math.Sign(clipPlane.y),
                1.0f,
                1.0f
            );
            Vector4 c = clipPlane * (2.0F / (Vector4.Dot (clipPlane, q)));
            obliqueMatrix[2] = c.x - projection[3];
            obliqueMatrix[6] = c.y - projection[7];
            obliqueMatrix[10] = c.z - projection[11];
            obliqueMatrix[14] = c.w - projection[15];
            return obliqueMatrix;
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
                SetNearClipPlane();
                _camera.Render();
            }
        }
    }
}