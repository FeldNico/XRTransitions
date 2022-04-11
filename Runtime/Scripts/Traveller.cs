using Unity.XR.CoreUtils;
using UnityEngine;

namespace Scripts
{
    public abstract class Traveller : MonoBehaviour
    {
        public Transform Origin;

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