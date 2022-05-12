using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;

namespace Scripts.Transitions.Cut
{
    public class CutTransition : Transition
    {
        public override bool IsTransitioning { get; protected set; }

        public InputActionProperty InitiateAction;

        public override async Task TriggerTransition(TransitionTraveller transitionTraveller, Vector3 targetPosition,
            Quaternion targetRotation)
        {
            if (transitionTraveller.IsPlayer())
            {
                IsTransitioning = true;
                OnTransition?.Invoke();
            }

            transitionTraveller.Origin.position =
                (transitionTraveller.Origin.position - transitionTraveller.transform.position) + targetPosition;
            targetRotation.ToAngleAxis(out var angle, out var axis);
            transitionTraveller.Origin.RotateAround(transitionTraveller.transform.position, axis, angle);
            Physics.SyncTransforms();

            if (transitionTraveller.IsPlayer())
            {
                IsTransitioning = false;
                OnTransitionEnd?.Invoke();
            }
        }

        public override async Task Initialization()
        {
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                await Task.Yield();
            }
            await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime * 10));
            InitiateAction.EnableDirectAction();
        }
    }
}