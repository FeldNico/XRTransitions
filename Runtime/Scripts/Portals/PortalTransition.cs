using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace Scripts
{
    public class PortalTransition: Transition
    {
        [SerializeField]
        private Portal _startPortal;
        
        [SerializeField]
        private Portal _targetPortal;
        
        [SerializeField]
        private Context _startContext;
        
        [SerializeField]
        private Context _targetContext;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private Transform _eyeLeftTransform;

        [SerializeField]
        private Transform _eyeRightTransform;
        
        private TransitionManager _transitionManager;

        public override Context StartContext
        {
            get => _startContext;
            protected set => _startContext = value;
        }

        public override Context TargetContext
        {
            get => _targetContext;
            protected set => _targetContext = value;
        }

        public override bool IsTransitioning { get; protected set; }

        public Camera Camera => _camera;
        
        public Transform EyeLeftTransform => _eyeLeftTransform;

        public Transform EyeRightTransform => _eyeRightTransform;

        private void Awake()
        {
            _transitionManager = FindObjectOfType<TransitionManager>();
            OnTransitionEnd += () =>
            {
                _transitionManager.CurrentContext = TargetContext;
                (_targetContext, _startContext) = (_startContext, _targetContext);
                (_startPortal, _targetPortal) = (_targetPortal, _startPortal);
            };
        }

        public override async Task TriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation)
        {
            IsTransitioning = true;
            OnTransition?.Invoke();
            
            traveller.Player.position = (traveller.Player.position - traveller.transform.position) + targetPosition;
            targetRotation.ToAngleAxis(out var angle, out var axis);
            traveller.Player.RotateAround(traveller.transform.position,axis,angle);
            Physics.SyncTransforms();

            OnTransitionEnd?.Invoke();
            IsTransitioning = false;
        }

        public override async Task Initialization()
        {
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                await Task.Yield();
            }
            await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime * 10));
            _startPortal.Initialize(this);
            _targetPortal.Initialize(this);
        }

        public Portal GetOtherPortal(Portal thisPortal)
        {
            if (thisPortal == _startPortal)
            {
                return _targetPortal;
            }

            if (thisPortal == _targetPortal)
            {
                return _startPortal;
            }

            return null;
        }
    }
}