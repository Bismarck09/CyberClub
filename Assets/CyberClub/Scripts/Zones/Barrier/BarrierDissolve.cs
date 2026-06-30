using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class BarrierDissolve : MonoBehaviour
{
    [SerializeField] private float _dissolveDuration;
    [SerializeField] private GameObject _lock;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private ZonePurchase _zonePurchase;
    
    private Renderer _renderer;
    private Material _dissolveMaterial;

    void Start()
    {
        _renderer = GetComponent<Renderer>();

        _dissolveMaterial = _renderer.materials[0];
    }

    public void Init()
    {
        _zonePurchase.OnZonePurchased += Dissolve;
    }

    private void OnDestroy()
    {
        _zonePurchase.OnZonePurchased -= Dissolve;
    }


    private void Dissolve()
    {
        _lock.SetActive(false);
        _canvas.enabled = false;

        _dissolveMaterial.DOFloat(1f, "_Value", _dissolveDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
    
}
