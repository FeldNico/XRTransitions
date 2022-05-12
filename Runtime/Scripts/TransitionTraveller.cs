using System;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace Scripts
{
    public class TransitionTraveller : MonoBehaviour
    {
        [SerializeField]
        private Transform _origin;
        public Transform Origin => _origin;

        private Vector3 _lastPosition;
        public Vector3 LastPosition => LastPosition;
        
        private void Awake()
        {
            if (_origin == null)
            {
                _origin = transform;
            }
        }

        private void LateUpdate()
        {
            _lastPosition = transform.position;
        }

        private bool? _isTravellerPlayer = null;

        public bool IsPlayer()
        {
            if (!_isTravellerPlayer.HasValue)
            {
                _isTravellerPlayer = GetComponentInParent<XROrigin>() != null;
            }

            return _isTravellerPlayer.Value;
        }
    }
}