using System;
using System.Threading.Tasks;
using UnityEngine;
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
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                await Task.Yield();
            }
            await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime * 10));
            _portal.Initialize(this);
        }
        
        internal override Task OnTriggerTransition()
        {
            _transitionManager.XROrigin.MoveCameraToWorldLocation(Destination.position);
            var rotDiff = Destination.rotation * Quaternion.Inverse(_transitionManager.MainCamera.transform.rotation);
            rotDiff.ToAngleAxis(out var angle, out var axis);
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