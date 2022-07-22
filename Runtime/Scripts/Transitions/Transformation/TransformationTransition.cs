using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;

namespace Scripts.Transformation
{
    public class TransformationTransition : Transition
    {
        [SerializeField] private Context _startContext;
        [SerializeField] private GameObject _transformationPrefab;
        [SerializeField] private float _duration = 1.3f;

        private Transformation _transformation;
        internal override async Task OnInitialization()
        {
            while (!XRGeneralSettings.Instance.Manager.isInitializationComplete || !TransitionManager.MainCamera.stereoEnabled)
            {
                await Task.Delay(1);
            }
        }

        internal override async Task OnDeinitialization()
        {
            while (TransitionManager.IsTransitioning)
            {
                await Task.Delay(1);
            }
            
            if (_transformation != null)
            {
                Object.Destroy(_transformation);
            }
        }

        internal override async Task OnTriggerTransition()
        {
            _transformation = Object.Instantiate(_transformationPrefab).GetComponent<Transformation>();
            _transformation.Initialize(this);
            
            await _transformation.BlendForSeconds(_duration);

            TransitionManager.XROrigin.transform.position = Destination.position;

            Object.Destroy(_transformation.gameObject);
            _transformation = null;
        }

        internal override async Task OnActionPressed()
        {
            await TriggerTransition();
        }

        internal override async Task OnActionRelease()
        {
            await Task.CompletedTask;
        }

        public override Context GetStartContext()
        {
            return _startContext;
        }
    }
}