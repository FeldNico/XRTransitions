using System;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace Scripts
{
    public class Traveller : MonoBehaviour
    {
        [SerializeField]
        private Transform _origin;
        public Transform Origin => _origin;

        private Vector3 _lastPosition;
        public Vector3 LastPosition => _lastPosition;
        
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

        private static Traveller _playerTraveller;
        public static Traveller GetPlayerTraveller()
        {
            if (_playerTraveller == null)
            {
                _playerTraveller = FindObjectsOfType<Traveller>().FirstOrDefault(traveller => traveller.IsPlayer());
            }

            return _playerTraveller;
        }
    }
}