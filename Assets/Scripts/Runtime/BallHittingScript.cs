using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Cinemachine;

public class BallHittingScript : MonoBehaviour
{
    [SerializeField] protected Rigidbody _golfBallRB;
    [SerializeField] protected float _minShotForce = 1f;
    [SerializeField] protected float _maxShotForce = 10f;
    [SerializeField] protected Image _shotPowerBarImage;
    [SerializeField] protected Image _shotPowerBarFillImage;
    [SerializeField] protected Canvas _shotPowerBarCanvas;
    [SerializeField] protected Transform _ballCameraTransform;
    [SerializeField] protected Transform _ballAimTarget;
    [SerializeField] protected Transform _ballAimOrigin;
    [SerializeField] protected CinemachineInputAxisController _cameraAxisController;
    [SerializeField] protected CinemachineCamera _cinemachineCamera;
    [SerializeField] protected float _baseFOV = 60f;
    [SerializeField] protected float _aimingFOV = 40f;
    [SerializeField] protected float _maxShotTime = 2f;
    [SerializeField] protected Vector3 _restVelocity;
    [SerializeField] protected float _timeBeforeRest = 1f;

    private bool _isAiming = false;
    private bool _isShooting = false;
    private bool _isBallShot = false;
    private float _shotPowerNormalized = 0f;
    private float _shotPower = 0f;
    private float _maxShotPower = 100f;
    private float _shotTime = 0f;
    private float _deltaShotPower;
    private float _restTimer;

    //hide and keep mouse centered to screen at all times
    //basic drag and release golf shot mechanic
    //on click, lock camera vertical movement
    //draw line from ball backwards x distance
    //on release, hit ball with a a base power multiplied by the amount pulled back

    private void Awake()
    {
        _shotPowerBarCanvas.enabled = false;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        print("_isAiming = " + _isAiming);
        print("_isBallShot = " + _isBallShot);

        SetAimTargetTransform();

        //print("RigidBody Linear Velocity: " + _golfBallRB.linearVelocity);


        //if the ball is moving slow enough for a period of time, stop it
        if(_isBallShot &&
            _golfBallRB.linearVelocity.x <= _restVelocity.x &&
            _golfBallRB.linearVelocity.y <= _restVelocity.y &&
            _golfBallRB.linearVelocity.z <= _restVelocity.z)
        {
            _restTimer += Time.deltaTime;
        }

        if(_restTimer >= _timeBeforeRest)
        {
            _golfBallRB.linearVelocity = Vector3.zero;
            _isBallShot = false;
            _restTimer = 0f;
        }

    }

    private void Update()
    {



        //for hold-to-charge shooting mode
        //if (_isShooting)
        //{
        //    _shotTime += Time.deltaTime;

        //    _shotPower = Mathf.InverseLerp(0f, _maxShotTime, _shotTime);

        //    _shotPowerBarFillImage.fillAmount = _shotPowerNormalized;
        //}

    }

    public void OnLook(InputValue value)
    {
        

        //for click-and-drag shooting mode

        if (!_isShooting) return;

        _deltaShotPower = value.Get<Vector2>().y;

        _shotPower += _deltaShotPower;

        _shotPowerNormalized = Mathf.InverseLerp(0f, _maxShotPower, _shotPower);
        _shotPowerBarFillImage.fillAmount = _shotPowerNormalized;
    }

    public void OnStartAiming(InputValue value)
    {
        _isAiming = true;
        _isShooting = false;
        //show power bar
        _shotPowerBarCanvas.enabled = true;


        _cinemachineCamera.Lens.FieldOfView = _aimingFOV;
    }

    public void OnStopAiming(InputValue value)
    {
        _isAiming = false;
        _isShooting = false;
        //hide power bar
        _shotPowerBarCanvas.enabled = false;


        _cinemachineCamera.Lens.FieldOfView = _baseFOV;
    }

    public void OnStartShooting(InputValue value)
    {
        if (!_isAiming) return;

        _shotTime = 0f;

        _isShooting = true;
        _cameraAxisController.enabled = false;

    }

    public void OnStopShooting(InputValue value)
    {
        if (!_isAiming) return;
        
        
        //add force to ball based on shot charge time
        float _shotForce = Mathf.Lerp(_minShotForce, _maxShotForce, _shotPowerNormalized);
        Vector3 _direction = (_ballAimTarget.position - _ballAimOrigin.position).normalized;

        print("Shot Force: " + _shotForce);
        

        _golfBallRB.AddForce(_direction * _shotForce,ForceMode.Impulse);

        _isBallShot = true;

        _isShooting = false;
        _cameraAxisController.enabled = true;

        _shotPower = 0f;
        _shotPowerNormalized = 0f;
        _shotPowerBarFillImage.fillAmount = 0f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_ballAimOrigin.position, _ballAimTarget.transform.position);
        
    }

    private void SetAimTargetTransform()
    {
        float y = _ballCameraTransform.rotation.eulerAngles.y;

        _ballAimOrigin.transform.rotation = Quaternion.Euler(0f, y, 0f);
    }
}
