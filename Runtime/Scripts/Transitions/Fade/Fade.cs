using System.Linq;
using System.Threading.Tasks;

using UnityEngine;

public class Fade : MonoBehaviour
{
    private static readonly int Color = Shader.PropertyToID("_Color");
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");

    public Renderer PlaneRenderer => _planeRenderer;
    
    [SerializeField]
    private Renderer _planeRenderer;

    private TransitionManager _transitionManager;

    private void Awake()
    {
        if (_planeRenderer == null)
        {
            _planeRenderer = GetComponentInChildren<Renderer>();
        }

        _transitionManager = FindObjectOfType<TransitionManager>();
    }

    public void Initialize(FadeTransition transition)
    {
        transform.parent = _transitionManager.MainCamera.transform;
        transform.localPosition = new Vector3(0f, 0f, _transitionManager.MainCamera.nearClipPlane+0.01f);
        transform.localRotation = Quaternion.AngleAxis(180,Vector3.up);
        
        PlaneRenderer.material.SetFloat(Alpha,0f);
    }

    public async Task FadeOut(float seconds, Color color)
    {
        PlaneRenderer.material.SetColor(Color, color);
        PlaneRenderer.material.SetFloat(Alpha,0);
        var startTime = Time.time;
        while (Time.time <= startTime + seconds)
        {
            await Task.Yield();
            PlaneRenderer.material.SetFloat(Alpha,(Time.time - startTime)/seconds);
        }
        PlaneRenderer.material.SetFloat(Alpha,1);
    }
    
    public async Task FadeIn(float seconds)
    {
        PlaneRenderer.material.SetFloat(Alpha,1);
        var startTime = Time.time;
        while (Time.time <= startTime + seconds)
        {
            await Task.Yield();
            PlaneRenderer.material.SetFloat(Alpha,1-(Time.time - startTime)/seconds);
        }
        PlaneRenderer.material.SetFloat(Alpha,0);
    }
}