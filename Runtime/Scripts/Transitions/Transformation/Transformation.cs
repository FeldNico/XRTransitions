using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Transformation
{
    public class Transformation : MonoBehaviour
    {
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");
        
        public Renderer Renderer => _renderer;
        public Transform LocalDummy => _localDummy;
        
        [SerializeField]
        private Renderer _renderer;
        private TransformationCamera _leftPortalCamera;
        private TransformationCamera _rightPortalCamera;
        private TransitionManager _transitionManager;
        private Transform _destination;
        private Transform _localDummy;
        
        private void Awake()
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<Renderer>();
            }

            _transitionManager = FindObjectOfType<TransitionManager>();
            _localDummy = new GameObject("DissolveLocalDummy").transform;
            //_localDummy.parent = _transitionManager.XROrigin.transform;
            var camPos = _transitionManager.MainCamera.transform.position;
            camPos.y = _transitionManager.XROrigin.transform.position.y;
            _localDummy.position = camPos;
            _localDummy.rotation = Quaternion.identity;
        }

        public void Initialize(TransformationTransition transition)
        {
            _destination = transition.Destination;
        
            _leftPortalCamera = new GameObject("LeftCamera").AddComponent<TransformationCamera>();
            _leftPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Left);

            _rightPortalCamera = new GameObject("RightCamera").AddComponent<TransformationCamera>();
            _rightPortalCamera.Initialize(this, transition, Camera.StereoscopicEye.Right);
        
            transform.parent = _transitionManager.MainCamera.transform;
            transform.localPosition = new Vector3(0f, 0f, _transitionManager.MainCamera.nearClipPlane+0.01f);
            transform.localRotation = Quaternion.AngleAxis(180,Vector3.up);
        }

        public async Task BlendForSeconds(float seconds)
        {
            var startTime = Time.time;
            Renderer.material.SetFloat(Alpha,0);
            while (Time.time <= startTime + seconds)
            {
                await Task.Yield();
                Renderer.material.SetFloat(Alpha,(Time.time - startTime)/seconds);
            }
            Renderer.material.SetFloat(Alpha,1);
        }
    }
}