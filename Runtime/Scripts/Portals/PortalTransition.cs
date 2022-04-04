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
        
        [SerializeField]
        private Transform _destination;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private Transform _eyeLeftTransform;

        [SerializeField]
        private Transform _eyeRightTransform;
        
        private TransitionManager _transitionManager;

        private Portal Portal => _portal;
        public Transform Destination => _destination;
        public Camera Camera => _camera;
        public Transform EyeLeftTransform => _eyeLeftTransform;
        public Transform EyeRightTransform => _eyeRightTransform;
        

        private void Awake()
        {
            _transitionManager = FindObjectOfType<TransitionManager>();
        }
        
        public override bool IsTransitioning { get; protected set; }

        public override async Task TriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation)
        {
            IsTransitioning = true;
            OnTransition?.Invoke();

            traveller.Player.position = (traveller.Player.position - traveller.transform.position) + targetPosition;
            targetRotation.ToAngleAxis(out var angle, out var axis);
            traveller.Player.RotateAround(traveller.transform.position,axis,angle);
            Physics.SyncTransforms();
            
            IsTransitioning = false;
            OnTransitionEnd?.Invoke();
        }

        public override async Task Initialization()
        {
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                await Task.Yield();
            }
            await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime * 10));
            _portal.Initialize(this);
        }
    }
}