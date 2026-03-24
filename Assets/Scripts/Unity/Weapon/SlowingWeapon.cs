using UnityEngine;

public class SlowingWeapon : Weapon
{
    [SerializeField] private DonutRingRenderer donutRing;

    private float _currentSlowAmount;
    private float _lastSlowAmount;
    private float _currentArea;
    private float _lastArea;
    private DonutRingRenderer _instance;
    protected new void Awake()
    {
        base.Awake();
        _instance = Instantiate(donutRing, transform);
        _instance.transform.localPosition = Vector3.zero;
        _instance.SetSlowAmount(data.SlowAmount);
        _instance.SetOuterRadius(data.Area);
        _instance.gameObject.SetActive(false);
        _lastSlowAmount = data.SlowAmount;
        _lastArea = data.Area;
    }
    protected override void Fire() { /* Aura — no fire event */ }

    protected override void Update()
    {
        if (!IsActive) return;

        if(_currentSlowAmount != _lastSlowAmount && _currentSlowAmount < _lastSlowAmount)
        {
            _currentSlowAmount = _lastSlowAmount;
            _instance.SetSlowAmount(_currentSlowAmount);
        }
        if(_currentArea != _lastArea && _currentArea < _lastArea)
        {
            _currentArea = _lastArea;
            _instance.SetOuterRadius(_currentArea);
        }

        if (!_instance.gameObject.activeSelf)
        {
            _instance.gameObject.SetActive(true);
            _instance.SetOuterRadius(data.Area);
            InitializeSlowAmount(_instance);
        }
    }
    public void InitializeSlowAmount(DonutRingRenderer donutRing)
    {
        _instance.SetSlowAmount(data.SlowAmount);
    }
}
