using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

public class OrbTransition : Transition
{
    public Context StartContext => _startContext;
    
    [SerializeField]
    private Context _startContext;
    [SerializeField]
    private GameObject OrbPrefab;
    [SerializeField]
    private Transform ControllerTransform;
    [SerializeField]
    private InputActionProperty _initiateAction;
    private Orb _orb;

    private TransitionManager _transitionManager;

    internal override Task OnTriggerTransition(Traveller traveller, Vector3 targetPosition, Quaternion targetRotation)
    {
        traveller.Origin.position = (traveller.Origin.position - traveller.transform.position) + targetPosition;
        targetRotation.ToAngleAxis(out var angle, out var axis);
        traveller.Origin.RotateAround(traveller.transform.position,axis,angle);
        Deiniate();
        return Task.CompletedTask;
    }

    public override async Task Initialization()
    {
        _transitionManager = Object.FindObjectOfType<TransitionManager>();
        while (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            await Task.Yield();
        }

        await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime * 300));
        _initiateAction.EnableDirectAction();
        _initiateAction.action.started += _ =>
        {
            Initiate();
        };
        _initiateAction.action.performed += _ =>
        {
            Deiniate();
        };
    }

    public override Context GetStartContext()
    {
        return _startContext;
    }

    private void Initiate()
    {
        if (_orb != null)
        {
            Object.Destroy(_orb.gameObject);
        }
        _orb = new GameObject("Orb").AddComponent<Orb>();
        _orb.transform.parent = ControllerTransform;
        _orb.transform.localPosition = Vector3.zero;
        _orb.transform.localRotation = Quaternion.identity;
        _orb.Initialize(this);
    }

    private void Deiniate()
    {
        if (_orb == null)
        {
            return;
        }
        
        Object.Destroy(_orb.gameObject);
        _orb = null;
    }
}