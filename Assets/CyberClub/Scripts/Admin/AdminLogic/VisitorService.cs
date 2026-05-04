using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class VisitorService : MonoBehaviour
{
    [SerializeField] private DeviceRegistry _deviceRegistry;
    [SerializeField] private VisitorQueue _visitorQueue;
    [SerializeField] private float _serviceInterval;

    private DeviceEntry _freeDevice;
    private Visitor _visitor;

    private bool _isServicing;

    private void Update()
    {
        if (!_isServicing)
            ServiceVisitor();
    }
    

    private void ServiceVisitor()
    {
        _visitor =  _visitorQueue.GetNextVisitor();
        _freeDevice = _deviceRegistry.GetRandomFreeDevice();

        if (_visitor != null && _freeDevice != null)
        {
            _isServicing = true;
            StartCoroutine(Service());
        }
    }

    private IEnumerator Service()
    {
        yield return new WaitForSeconds(_serviceInterval);

        _visitorQueue.RemoveVisitor(_visitor);
        _visitor.GetComponent<VisitorMovement>().Move(_freeDevice.Device.TargetPoint.position);
        _freeDevice.Device.Reserve(15f);

        _isServicing = false;
    }
}
