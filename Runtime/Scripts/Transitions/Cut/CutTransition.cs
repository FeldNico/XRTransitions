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

        private bool _wasPressed = false;
        
        internal override async Task OnTriggerTransition()
        {
            TransitionManager.XROrigin.transform.position = Destination.position;
            await Task.CompletedTask;
        }

        internal override async Task OnActionPressed()
        {
            await TriggerTransition();
        }

        internal override async Task OnActionRelease()
        {
            await Task.CompletedTask;
        }

        internal override async Task OnInitialization()
        {
            await Task.CompletedTask;
        }

        internal override async Task OnDeinitialization()
        {
            await Task.CompletedTask;
        }
        
        public override Context GetStartContext()
        {
            return _startContext;
        }
        
        [MenuItem("Transition/Trigger/Cut")]
        public static async void Trigger()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Transition only available in Playmode");
                return;
            }
            
            var transitionManager = Object.FindObjectOfType<TransitionManager>();
            var transition =
                transitionManager.Transitions.FirstOrDefault(transition => transition.GetType() == typeof(CutTransition) && transitionManager.CurrentContext == transition.GetStartContext());
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