using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scripts;
using UnityEngine;
using UnityEngine.XR.Management;

public class OrbTransition : Transition
{
    [SerializeField] private Orb _orb;

    [SerializeField] private Camera _camera;

    [SerializeField] private Transform _eyeLeftTransform;

    [SerializeField] private Transform _eyeRightTransform;

    private TransitionManager _transitionManager;

    private Orb Orb => _orb;
    public Camera Camera => _camera;
    public Transform EyeLeftTransform => _eyeLeftTransform;
    public Transform EyeRightTransform => _eyeRightTransform;


    private void Awake()
    {
        _transitionManager = FindObjectOfType<TransitionManager>();
    }

    public override bool IsTransitioning { get; protected set; }

    public override async Task TriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation)
    {
        if (traveller.IsPlayer())
        {
            IsTransitioning = true;
            OnTransition?.Invoke();
        }

        traveller.Origin.position = (traveller.Origin.position - traveller.transform.position) + targetPosition;
        targetRotation.ToAngleAxis(out var angle, out var axis);
        traveller.Origin.RotateAround(traveller.transform.position, axis, angle);
        Physics.SyncTransforms();

        if (traveller.IsPlayer())
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
        _orb.Initialize(this);
    }
}