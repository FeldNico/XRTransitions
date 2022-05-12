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

        internal override async Task OnTriggerTransition(Traveller traveller, Vector3 targetPosition,
            Quaternion targetRotation)
        {
            traveller.Origin.position =
                (traveller.Origin.position - traveller.transform.position) + targetPosition;
            targetRotation.ToAngleAxis(out var angle, out var axis);
            traveller.Origin.RotateAround(traveller.transform.position, axis, angle);
        }

        public override async Task Initialization()
        {
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
            transition.TriggerTransition(Traveller.GetPlayerTraveller(), transition.Destination.position,
                Quaternion.identity* Quaternion.AngleAxis(180f,Vector3.up));
        }
    }
}