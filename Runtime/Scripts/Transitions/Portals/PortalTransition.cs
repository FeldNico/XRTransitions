﻿using System;
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

        public override async Task TriggerTransition(TransitionTraveller transitionTraveller, Vector3 targetPosition, Quaternion targetRotation)
        {
            if (transitionTraveller.IsPlayer())
            {
                IsTransitioning = true;
                OnTransition?.Invoke();
            }

            transitionTraveller.Origin.position = (transitionTraveller.Origin.position - transitionTraveller.transform.position) + targetPosition;
            targetRotation.ToAngleAxis(out var angle, out var axis);
            transitionTraveller.Origin.RotateAround(transitionTraveller.transform.position,axis,angle);
            Physics.SyncTransforms();

            if (transitionTraveller.IsPlayer())
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