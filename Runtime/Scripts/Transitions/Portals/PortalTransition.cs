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
        [SerializeField] private Portal _portal;
        [SerializeField] private InputActionProperty _initiateAction;
        private Context _startContext;
        private TransitionManager _transitionManager;

        public override async Task Initialization()
        {
            _transitionManager = Object.FindObjectOfType<TransitionManager>();
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete ||
                   !_transitionManager.MainCamera.stereoEnabled)
            {
                await Task.Delay(1);
            }

            _portal.Initialize(this);
            _portal.gameObject.SetActive(false);
            _initiateAction.EnableDirectAction();
            if (_transitionManager.MainCamera.GetComponent<Collider>() == null)
            {
                _transitionManager.MainCamera.gameObject.AddComponent<SphereCollider>().radius = 0.1f;
            }
            InputSystem.onAfterUpdate += HandleInput;
        }

        private void HandleInput()
        {
            if (_initiateAction.action.WasPressedThisFrame())
            {
                _portal.AnimPortal();
            }
        }

        

        internal override Task OnTriggerTransition()
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
            _portal.AnimPortal();
            if (Destination.GetComponent<Portal>() != null)
            {
                Destination.GetComponent<Portal>().AnimPortal();
            }
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
                transitionManager.Transitions.FirstOrDefault(transition => transition.GetType() == typeof(PortalTransition)) as PortalTransition;
            if (transition != null)
            {
                transition._portal.AnimPortal();
            }
            else
            {
                Debug.LogError("No PortalTransition found");
            }
        }
    }
}