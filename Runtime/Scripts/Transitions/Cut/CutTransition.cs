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
        
        internal override async Task OnTriggerTransition()
        {
            _transitionManager.XROrigin.MoveCameraToWorldLocation(Destination.position);
            var rotDiff = Destination.rotation * Quaternion.Inverse(_transitionManager.MainCamera.transform.rotation);
            rotDiff.ToAngleAxis(out var angle, out var axis);
            _transitionManager.XROrigin.RotateAroundCameraPosition(axis, angle);
        }

        public override async Task Initialization()
        {
            _transitionManager = Object.FindObjectOfType<TransitionManager>();
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                await Task.Yield();
            }
            await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime * 10));
            _initiateAction.EnableDirectAction();
        }
        
        public override Context GetStartContext()
        {
            return _startContext;
        }
        
        [MenuItem("Transition/Cut")]
        public static async void Trigger()
        {
            var transitionManager = Object.FindObjectOfType<TransitionManager>();
            var transition =
                transitionManager.Transitions.First(transition => transition.GetType() == typeof(CutTransition));
            await transition.Initialization();
            transition.TriggerTransition();
        }
    }
}