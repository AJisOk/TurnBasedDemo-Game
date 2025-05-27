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
    [SerializeField] protected float _maxShotTime = 2f;

    private bool _isAiming = false;
    private bool _isShooting = false;
    private float _shotPower = 0f;
    private float _shotTime = 0f;


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

        SetAimTargetTransform();

    }

    private void Update()
    {
        if (_isShooting)
        {
            _shotTime += Time.deltaTime;

            _shotPower = Mathf.InverseLerp(0f, _maxShotTime, _shotTime);

            _shotPowerBarFillImage.fillAmount = _shotPower;
        }

    }


    public void OnStartAiming(InputValue value)
    {
        _isAiming = true;
        _isShooting = false;
        //show power bar
        _shotPowerBarCanvas.enabled = true;

    }

    public void OnStopAiming(InputValue value)
    {
        _isAiming = false;
        _isShooting = false;
        //hide power bar
        _shotPowerBarCanvas.enabled = false;
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
        float _shotForce = Mathf.Lerp(_minShotForce, _maxShotForce, _shotPower);
        Vector3 _direction = (_ballAimOrigin.position - _ballAimTarget.position).normalized;

        print("Shot Force: " + _shotForce);
        

        _golfBallRB.AddForce(_direction * _shotForce,ForceMode.Impulse);


        _isShooting = false;
        _cameraAxisController.enabled = true;

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
