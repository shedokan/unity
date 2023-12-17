using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-1)]
public class InputManager : Singleton<InputManager>
{
    #region Events
    /// <param name="time">In seconds, relative to Time.<see cref="Time.realtimeSinceStartup"/> when the touch started.</param>
    public delegate void StartTouch(Vector2 position, float time);
    public event StartTouch OnStartTouch;
    
    
    /// <param name="time">In seconds, relative to Time.<see cref="Time.realtimeSinceStartup"/> when the touch ended.</param>
    public delegate void EndTouch(Vector2 position, float time);
    public event EndTouch OnEndTouch;
    #endregion
    
    private PlayerControls _playerControls;
    private Camera _mainCamera;
    
    void Awake()
    {
        _playerControls = new PlayerControls();
        _playerControls.Enable();
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        _playerControls?.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

    private void OnDestroy()
    {
        _playerControls.Dispose();
    }

    void Start()
    {
        _playerControls.Touch.PrimaryContact.started += StartTouchPrimary;
        _playerControls.Touch.PrimaryContact.canceled += EndTouchPrimary;
    }

    private void StartTouchPrimary(InputAction.CallbackContext context)
    {
        OnStartTouch?.Invoke(PrimaryPosition(), (float)context.startTime);
    }

    private void EndTouchPrimary(InputAction.CallbackContext context)
    {
        OnEndTouch?.Invoke(PrimaryPosition(), (float)context.time);
    }

    public Vector3 PrimaryPosition()
    {
        return Utils.ScreenToWorld(_mainCamera, _playerControls.Touch.PrimaryPosition.ReadValue<Vector2>());
    }
}
