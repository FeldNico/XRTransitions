using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

namespace Scripts.Transitions.Cut
{
    public class CutTransition : Transition
    {
        [SerializeField]
        private Context _startContext;
        [SerializeField]
        private InputActionProperty _initiateAction;

        private TransitionManager _transitionManager;
        private bool _wasPressed = false;
        
        internal override async Task OnTriggerTransition()
        {
            _transitionManager.XROrigin.MoveCameraToWorldLocation(Destination.position);
            var rotDiff = Destination.rotation * Quaternion.Inverse(_transitionManager.MainCamera.transform.rotation);
            rotDiff.ToAngleAxis(out var angle, out var axis);
            _transitionManager.XROrigin.RotateAroundCameraPosition(axis, angle);
            await Task.CompletedTask;
        }

        internal override async Task OnInitialization()
        {
            _transitionManager = Object.FindObjectOfType<TransitionManager>();
            _initiateAction.EnableDirectAction();
            InputSystem.onAfterUpdate += HandleInput;
            await Task.CompletedTask;
        }
        
        private async void HandleInput()
        {
            if (_initiateAction.action.ReadValue<float>() > 0.7f && !_wasPressed)
            {
                _wasPressed = true;
                TriggerTransition();
            }
            if (_initiateAction.action.ReadValue<float>() < 0.3f && _wasPressed)
            {
                _wasPressed = false;
            }
        }
        
        internal override async Task OnDeinitialization()
        {
            _initiateAction.DisableDirectAction();
            InputSystem.onAfterUpdate -= HandleInput;
            _wasPressed = false;
            await Task.CompletedTask;
        }
        
        public override Context GetStartContext()
        {
            return _startContext;
        }
        
        [MenuItem("Transition/Cut")]
        public static async void Trigger()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Transition only available in Playmode");
                return;
            }
            
            var transitionManager = Object.FindObjectOfType<TransitionManager>();
            var transition =
                transitionManager.Transitions.FirstOrDefault(transition => transition.GetType() == typeof(CutTransition));
            if (transition != null)
            {
                await transition.OnInitialization();
                transition.TriggerTransition();
            }
            else
            {
                Debug.LogError("No CutTransition found");
            }
        }
    }
}