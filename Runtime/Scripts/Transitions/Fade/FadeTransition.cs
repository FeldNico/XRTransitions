using System;
using System.Linq;
using System.Threading.Tasks;
using Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;
using Object = UnityEngine.Object;

public class FadeTransition : Transition
    {

        [SerializeField] private Context _startContext;
        [SerializeField] private GameObject _fadePrefab;
        [SerializeField] private Color _color = Color.black;
        [SerializeField] private float _duration;

        private Fade _fade;
        
        internal override async Task OnInitialization()
        {
            await Task.CompletedTask;
        }

        internal override async Task OnDeinitialization()
        {
            while (TransitionManager.IsTransitioning)
            {
                await Task.Delay(1);
            }

            if (_fade != null)
            {
                Object.Destroy(_fade.gameObject);
            }
        }

        internal override async Task OnTriggerTransition()
        {
            _fade = Object.Instantiate(_fadePrefab).GetComponent<Fade>();
            _fade.Initialize(this);

            await _fade.FadeOut(_duration / 2f, _color);

            TransitionManager.XROrigin.transform.position = Destination.position;

            await _fade.FadeIn(_duration / 2f);
        }

        internal override async Task OnActionDown(bool isRight)
        {
            await TriggerTransition();
        }

        internal override async Task OnActionUp()
        {
            await Task.CompletedTask;
        }

        public override Context GetStartContext()
        {
            return _startContext;
        }
    }