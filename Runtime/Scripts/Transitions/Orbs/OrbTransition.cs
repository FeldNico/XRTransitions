using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scripts;
using UnityEngine;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

public class OrbTransition : Transition
{
    [SerializeField] private Orb _orb;

    private Camera _camera;

    private Transform _eyeLeftTransform;

    private Transform _eyeRightTransform;

    private TransitionManager _transitionManager;

    private Orb Orb => _orb;

    public override bool IsTransitioning { get; protected set; }

    public override async Task TriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation)
    {
        if (traveller.IsPlayer())
        {
            IsTransitioning = true;
            OnTransition?.Invoke();
        }

        if (traveller.IsPlayer())
        {
            IsTransitioning = false;
            OnTransitionEnd?.Invoke();
        }
    }

    public override async Task Initialization()
    {
        _transitionManager = Object.FindObjectOfType<TransitionManager>();
        _camera = _transitionManager.MainCamera;
        _eyeLeftTransform = _transitionManager.LeftEyeTransform;
        _eyeRightTransform = _transitionManager.RightEyeTransform;
        while (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            await Task.Yield();
        }

        await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime * 300));
        _orb.Initialize(this);
    }
}