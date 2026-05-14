using UnityEngine;

public class ZonePurchaseConfig : MonoBehaviour
{
    [SerializeField] private GameObject _barrierObject;
    [SerializeField] private int _zonePrice;

     public GameObject BarrierObject => _barrierObject;
     public int ZonePrice => _zonePrice;
}
