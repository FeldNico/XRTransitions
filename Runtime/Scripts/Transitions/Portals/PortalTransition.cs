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

        public override async Task Initialization()
        {
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                await Task.Yield();
            }
            await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime * 10));
            _portal.Initialize(this);
        }
        
        internal override Task OnTriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation)
        {
            traveller.Origin.position = (traveller.Origin.position - traveller.transform.position) + targetPosition;
            targetRotation.ToAngleAxis(out var angle, out var axis);
            traveller.Origin.RotateAround(traveller.transform.position,axis,angle);
            Physics.SyncTransforms();
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