using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

namespace Scripts
{
    public class PortalTransition : Transition
    {
        [SerializeField] private Transform _portalPosition;
        [SerializeField] private GameObject _portalPrefab;
        private Context _startContext;
        private Portal _portal;
        private Portal _destinationPortal;
        private bool _isAnimating = false;

        internal override async Task OnInitialization()
        {
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete ||
                   !TransitionManager.MainCamera.stereoEnabled)
            {
                await Task.Delay(1);
            }
            
            if (TransitionManager.MainCamera.GetComponent<Collider>() == null)
            {
                TransitionManager.MainCamera.gameObject.AddComponent<SphereCollider>().radius = 0.1f;
            }
        }

        internal override async Task OnDeinitialization()
        {
            while (_isAnimating || TransitionManager.IsTransitioning)
            {
                await Task.Delay(1);
            }

            if (_portal != null)
            {
                await Task.WhenAll(_portal.Destroy());
            }
            
            if (TransitionManager.MainCamera.GetComponent<Collider>() != null)
            {
                Object.DestroyImmediate(TransitionManager.MainCamera.GetComponent<Collider>());
            }
            
        }

        internal override async Task OnTriggerTransition()
        {
            TransitionManager.XROrigin.transform.position = Destination.transform.position;
            await Task.WhenAll(_portal.Destroy());
        }

        internal override async Task OnActionDown(bool isRight)
        {
            if (_isAnimating)
            {
                return;
            }
            
            _isAnimating = true;
            if (_portal == null)
            {
                var portalToCam = TransitionManager.CenterEyePosition - _portalPosition.position;
                portalToCam.y = 0;
                _portal = Object.Instantiate(_portalPrefab, _portalPosition.position,
                    Quaternion.LookRotation(portalToCam, Vector3.up), _portalPosition).GetComponent<Portal>();
                
                await _portal.Initialize(this);
            }
            else
            {
                await Task.WhenAll(_portal.Destroy());
            }
                
            _isAnimating = false;
        }

        internal override async Task OnActionUp()
        {
            await Task.CompletedTask;
        }

        public override Context GetStartContext()
        {
            if (_startContext == null)
            {
                _startContext = _portalPosition.GetComponentInParent<Context>();
            }

            return _startContext;
        }
    }
}