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
    private GameObject _orbPrefab;
    [SerializeField]
    private Transform _controllerTransform;
    [SerializeField]
    private InputActionProperty _initiateAction;
    private Orb _orb;

    private TransitionManager _transitionManager;
    private bool _wasPressed;

    
    internal override Task OnTriggerTransition()
    {
        var localToWorldMatrix = Destination.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.AngleAxis(180f,Vector3.up)) * _orb.LocalDummy.transform.worldToLocalMatrix * _transitionManager.MainCamera.transform.localToWorldMatrix;
        _transitionManager.XROrigin.MoveCameraToWorldLocation(localToWorldMatrix.GetColumn(3));
        Quaternion targetRotation = localToWorldMatrix.rotation *
                                    Quaternion.Inverse(_transitionManager.MainCamera.transform.rotation);
        targetRotation.ToAngleAxis(out var angle,out var axis);
        _transitionManager.XROrigin.RotateAroundCameraPosition(axis, angle);
        Deinitiate();
        return Task.CompletedTask;
    }

    internal override async Task OnInitialization()
    {
        _transitionManager = Object.FindObjectOfType<TransitionManager>();
        while (!XRGeneralSettings.Instance.Manager.isInitializationComplete || !_transitionManager.MainCamera.stereoEnabled)
        {
            await Task.Delay(1);
        }
        _initiateAction.EnableDirectAction();
        InputSystem.onAfterUpdate += HandleInput;
    }
    
    internal override async Task OnDeinitialization()
    {
        _initiateAction.EnableDirectAction();
        InputSystem.onAfterUpdate += HandleInput;
        while (IsTransitioning)
        {
            await Task.Delay(1);
        }

        Deinitiate();
        _wasPressed = false;
    }


    public override Context GetStartContext()
    {
        return _startContext;
    }

    private async void HandleInput()
    {
        if (_initiateAction.action.ReadValue<float>() > 0.7f && !_wasPressed)
        {
            _wasPressed = true;
            Initiate();
        }
        if (_initiateAction.action.ReadValue<float>() < 0.3f && _wasPressed)
        {
            _wasPressed = false;
            Deinitiate();
        }
    }
    
    private void Initiate()
    {
        Debug.Log("Initiate");
        if (_orb != null)
        {
            Object.Destroy(_orb.gameObject);
        }

        _orb = Object.Instantiate(_orbPrefab).GetComponent<Orb>();
        _orb.transform.parent = _controllerTransform;
        _orb.transform.localPosition = new Vector3(0,0.15f,0);
        _orb.transform.localRotation = Quaternion.identity;
        _orb.Initialize(this);
    }

    private void Deinitiate()
    {
        Debug.Log("Deinitiate");
        if (_orb == null)
        {
            return;
        }
        
        Object.Destroy(_orb.gameObject);
        _orb = null;
    }
}