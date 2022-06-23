using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

namespace Scripts
{
    public class PortalTransition : Transition
    {
        [SerializeField] private Transform _portalPosition;
        [SerializeField] private GameObject _portalPrefab;
        [SerializeField] private GameObject _portalBasePrefab;
        [SerializeField] private InputActionProperty _initiateAction;
        private Context _startContext;
        private TransitionManager _transitionManager;
        private Portal _portal;
        private GameObject _portalBase;
        private bool _wasPressed = false;
        private bool _isAnimating = false;

        internal override async Task OnInitialization()
        {
            _transitionManager = Object.FindObjectOfType<TransitionManager>();
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete ||
                   !_transitionManager.MainCamera.stereoEnabled)
            {
                await Task.Delay(1);
            }

            _portalBase = Object.Instantiate(_portalBasePrefab, _portalPosition);
            _initiateAction.EnableDirectAction();
            if (_transitionManager.MainCamera.GetComponent<Collider>() == null)
            {
                _transitionManager.MainCamera.gameObject.AddComponent<SphereCollider>().radius = 0.1f;
            }

            InputSystem.onAfterUpdate += HandleInput;
        }

        internal override async Task OnDeinitialization()
        {
            _initiateAction.DisableDirectAction();
            InputSystem.onAfterUpdate -= HandleInput;
            while (_isAnimating || IsTransitioning)
            {
                await Task.Delay(1);
            }

            if (_portalBase != null)
            {
                Object.Destroy(_portalBase);
            }

            if (_portal != null)
            {
                await _portal.Destroy();
            }
            
            if (_transitionManager.MainCamera.GetComponent<Collider>() != null)
            {
                Object.Destroy(_transitionManager.MainCamera.GetComponent<Collider>());
            }
            
            _wasPressed = false;
        }

        private async void HandleInput()
        {
            if (_isAnimating || _transitionManager.CurrentContext != GetStartContext())
            {
                return;
            }

            if (_initiateAction.action.ReadValue<float>() > 0.7f && !_wasPressed)
            {
                _wasPressed = true;
                _isAnimating = true;
                if (_portal == null)
                {
                    var portalToCam = _transitionManager.MainCamera.transform.position - _portalPosition.position;
                    portalToCam.y = 0;
                    _portal = Object.Instantiate(_portalPrefab, _portalPosition.position,
                        Quaternion.LookRotation(portalToCam, Vector3.up), _portalPosition).GetComponent<Portal>();
                    Destination.rotation = Quaternion.LookRotation(portalToCam, Vector3.up);
                    await _portal.Initialize(this);
                }
                else
                {
                    await _portal.Destroy();
                }
                
                _isAnimating = false;
            }
            if (_initiateAction.action.ReadValue<float>() < 0.3f && _wasPressed)
            {
                _wasPressed = false;
            }
        }


        internal override async Task OnTriggerTransition()
        {
            var localToWorldMatrix = Destination.localToWorldMatrix *
                                     Matrix4x4.Rotate(Quaternion.AngleAxis(180f, Vector3.up)) *
                                     _portal.transform.worldToLocalMatrix *
                                     _transitionManager.MainCamera.transform.localToWorldMatrix;
            _transitionManager.XROrigin.MoveCameraToWorldLocation(localToWorldMatrix.GetColumn(3));
            Quaternion targetRotation =
                Quaternion.FromToRotation(_portal.transform.forward, Destination.forward) *
                Quaternion.AngleAxis(180f, Vector3.up);
            targetRotation.ToAngleAxis(out var angle, out var axis);
            _transitionManager.XROrigin.RotateAroundCameraPosition(axis, angle);
            await _portal.Destroy();
        }

        public override Context GetStartContext()
        {
            if (_startContext == null)
            {
                _startContext = _portalPosition.GetComponentInParent<Context>();
            }

            return _startContext;
        }

        [MenuItem("Transition/Portal")]
        public static async void Trigger()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Transition only available in Playmode");
                return;
            }

            var transitionManager = Object.FindObjectOfType<TransitionManager>();
            var transition =
                transitionManager.Transitions.FirstOrDefault(transition =>
                    transition.GetType() == typeof(PortalTransition)) as PortalTransition;
            if (transition != null)
            {
                if (transition._portal == null)
                {
                    var portalToCam = transitionManager.MainCamera.transform.position -
                                      transition._portalPosition.position;
                    portalToCam.y = 0;
                    transition._portal = Object.Instantiate(transition._portalPrefab,
                        transition._portalPosition.position, Quaternion.LookRotation(portalToCam, Vector3.up),
                        transition._portalPosition).GetComponent<Portal>();
                    transition._portal.Initialize(transition);
                }
                else
                {
                    transition._portal.Destroy();
                }
            }
            else
            {
                Debug.LogError("No PortalTransition found");
            }
        }
    }
}