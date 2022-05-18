using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

namespace Scripts
{
    public class PortalTransition: Transition
    {
        [SerializeField]
        private Portal _portal;
        private Context _startContext;
        private TransitionManager _transitionManager;
        public override async Task Initialization()
        {
            _transitionManager = Object.FindObjectOfType<TransitionManager>();
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete || !_transitionManager.MainCamera.stereoEnabled)
            {
                await Task.Delay(1);
            }
            _portal.Initialize(this);
        }
        
        internal override Task OnTriggerTransition()
        {
            var localToWorldMatrix = Destination.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180f,Vector3.up)) * _portal.transform.worldToLocalMatrix * _transitionManager.MainCamera.transform.localToWorldMatrix;
            _transitionManager.XROrigin.MoveCameraToWorldLocation(localToWorldMatrix.GetColumn(3));
            Quaternion targetRotation =
                Quaternion.FromToRotation(_portal.transform.forward, Destination.forward) *
                Quaternion.AngleAxis(180f, Vector3.up);
            targetRotation.ToAngleAxis(out var angle,out var axis);
            _transitionManager.XROrigin.RotateAroundCameraPosition(axis, angle);
            return Task.CompletedTask;
        }
        
        public override Context GetStartContext()
        {
            if (_startContext == null)
            {
                _startContext = _portal.GetComponentInParent<Context>();
            }
            return _startContext;
        }
    }
}