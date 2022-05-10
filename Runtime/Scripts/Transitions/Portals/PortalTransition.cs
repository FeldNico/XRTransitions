using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

namespace Scripts
{
    
    [Serializable]
    public class PortalTransition: Transition
    {
        [SerializeField]
        private Portal _portal;

        private TransitionManager _transitionManager;

        public override bool IsTransitioning { get; protected set; }

        public override async Task TriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation)
        {
            if (traveller.IsPlayer())
            {
                IsTransitioning = true;
                OnTransition?.Invoke();
            }

            traveller.Origin.position = (traveller.Origin.position - traveller.transform.position) + targetPosition;
            targetRotation.ToAngleAxis(out var angle, out var axis);
            traveller.Origin.RotateAround(traveller.transform.position,axis,angle);
            Physics.SyncTransforms();

            if (traveller.IsPlayer())
            {
                IsTransitioning = false;
                OnTransitionEnd?.Invoke();
            }
        }

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
    }
}