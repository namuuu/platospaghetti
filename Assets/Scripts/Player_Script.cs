using UnityEngine;
using System;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerScript : MonoBehaviour
{
    public PlayerStats stats;

    public Vector2 respawnPoint; // Point where the player respawns    

    // Components and structures
    private Rigidbody2D _rb; // Rigidbody2D component
    private CapsuleCollider2D _col; // Collider2D component
    private PlayerInput _input; // Input component
    [HideInInspector] public Vector2 _frameVelocity; // Velocity of the player this frame
    [HideInInspector] public Vector2 _fixedStick; // Input from the player

    // Actions
    public event Action<bool, float> OnGroundChange; // Event called when the player changes grounded state
    public event Action OnJump; // Event called when the player changes grounded state
    public event Action<bool> OnDash; // Event called when the player dashes

    private float _time; // Time since the game started
    public bool _cinematic; // Whether the player is in a cinematic

    private bool _cachedQueryStartInColliders; // Cached value of Physics2D.queriesStartInColliders

    public static PlayerScript Instance { get; private set; }
    
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        _input = GetComponent<PlayerInput>();

        Instance = this;   

        _cachedQueryStartInColliders = Physics2D.queriesStartInColliders; 
    }

    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;
        GatherInput();
    }

    // Gather input from the player, and put results into variables
    private void GatherInput() 
    {
        // Debug Respawn
        if(Input.GetKeyDown(KeyCode.R)) {
            Respawn();
        }

        _fixedStick = _input.actions["Move"].ReadValue<Vector2>();

        if(_input.actions["Jump"].triggered)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }

        if(_input.actions["Ability 1"].triggered)
        {
            HandleAbilityInput(AbilityManager.Instance.ReadAbility(0), true);
        }
    }

    private void HandleAbilityInput(string abilityName, bool triggered)
    {
        switch(abilityName)
        {
            case "dash":
                _dashToConsume = true;
                _dashToConsumeTime = _time;
                break;
        }
    }

    private void FixedUpdate()
    {
        if(_cinematic) return;

        CheckCollisions();
        // HandleTimers();

        HandleJumping();
        HandleHorizontalMovement(_fixedStick);
        HandleGravity();
        HandleDashing();

        // Debug.Log(_frameVelocity.ToString() + ' ' + _grounded);
        ApplyMovement();
    }

    #region Timers

    // private void HandleTimers()
    // {
    
    // }

    #endregion

    #region Collision

    private float _frameLeftGround = float.MinValue;
    [HideInInspector] public bool _grounded;
    private bool _closeToWall;
    private Vector2 _wallNormal;

    private void CheckCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // Ground and Ceiling
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, stats.groundDistance);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, stats.groundDistance);
        
        // Walls
        bool wallHitRight = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.right, stats.groundDistance);
        bool wallHitLeft = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.left, stats.groundDistance);

        _closeToWall = wallHitRight || wallHitLeft;
        _wallNormal = wallHitRight ? Vector2.right : wallHitLeft ? Vector2.left : Vector2.zero;

        // Hit a Ceiling
        if(ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

        // Landed on Ground
        if(!_grounded && groundHit) {
            _grounded = true;
            _bufferedJumpUsable = true;
            _coyoteUsable = true;

            OnGroundChange?.Invoke(true, _frameVelocity.y);
        } 
        // Left the ground
        else if(_grounded && !groundHit) {
            // Debug.Log("Left the ground. frameLeftGround: " + _frameLeftGround);
            _grounded = false;
            _frameLeftGround = _time;

            OnGroundChange?.Invoke(false, 0);
        }

        Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
    }

    #endregion

    #region Horizontal Movement

    // Handles horizontal movement
    // This will make the _frameVelocity.x move towards _input.move.x
    private void HandleHorizontalMovement(Vector2 move)
    {
        // Debug.Log(move);

        if(_dashing && !_isDashCancelled) return;

        if (move.x != 0)
        {
            var deceleration = _grounded ? Math.Sign(move.x).Equals(Math.Sign(_frameVelocity.x)) ? stats.groundDeceleration: stats.turningDeceleration : stats.airDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, move.x * stats.maxSpeed, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, (_grounded ? stats.groundDeceleration : stats.airDeceleration) * Time.fixedDeltaTime);
        }

        // Check if the player is moving into a wall
    }

    #endregion

    #region Jumping

    private bool _jumpToConsume = false;
    private bool _bufferedJumpUsable = false; // Whether the jump (if buffered) can be consumed
    private float _timeJumpWasPressed = float.MinValue;
    private bool _coyoteUsable = false;

    private bool HasBufferedJump => _bufferedJumpUsable && (_time - _timeJumpWasPressed < stats.jumpBufferTime);
    private bool CanUseCoyote => _coyoteUsable && !_grounded && _time - _frameLeftGround < stats.coyoteTime;

    private void HandleJumping()
    {

        // Check if the user pressed Jump (now or through the buffer), return if not
        if(!_jumpToConsume && !HasBufferedJump) return;

        // Debug.Log("Grounded: " + _grounded + " Coyote: " + CanUseCoyote + " time: " + _time + " frameLeftGround: " + _frameLeftGround);

        // Check if the player can jump. In the future, we could separate a "ExecuteWallJump" function
        if(CanUseCoyote || _grounded || _closeToWall) ExecuteJump();

        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _bufferedJumpUsable = false;
        if(_grounded || CanUseCoyote) {
            // Execute grounded jump
            _frameVelocity.y = stats.jumpForce;
            _coyoteUsable = false;
            OnJump?.Invoke();

            if(_dashing) _isDashCancelled = true;
            return;
        } else if(_closeToWall) {
            // Execute wall jump
            _frameVelocity.y = stats.wallJumpForce.y;
            _frameVelocity.x = -_wallNormal.x * stats.wallJumpForce.x;
            return;
        }        
    }

    #endregion

    #region Dashing

    private bool _dashToConsume = false;
    private float _dashToConsumeTime;
    private bool _dashUsable = false;
    private float _dashUsageTime;
    private bool _dashing;
    private bool _isDashCancelled = false;

    private bool HasBufferedDash => _dashUsable && _dashToConsume && (_time - _dashToConsumeTime < stats.jumpBufferTime);

    private void HandleDashing()
    {
        // Check if the dash has ended
        if(_dashing && _time - _dashUsageTime > stats.dashDuration) {
            _dashing = false;
            OnDash?.Invoke(false);

            if(_isDashCancelled == false)
                _frameVelocity.Scale(new Vector2(0.3f, 0.3f));
            
            return;
        }

        // Reset dash if grounded
        if(!_dashUsable && _grounded) _dashUsable = true;

        if(!AbilityManager.Instance.HasAbility("dash")) return;
        
        // Check if any interaction with the dash has to be made
        if(!HasBufferedDash || _dashing) return;

        // Can't dash if no direction is given
        if(_fixedStick.x == 0 && _fixedStick.y == 0) return;

        // Execute dash
        _dashing = true;
        _dashUsable = false;
        _dashUsageTime = _time;
        _isDashCancelled = false;
        OnDash?.Invoke(true);

        // Apply dash force
        _frameVelocity = _fixedStick.normalized * stats.dashSpeed;
    }

    #endregion

    #region Gravity

    private void HandleGravity() 
    {
        if(_dashing && !_isDashCancelled) return;

        if(_grounded && _frameVelocity.y <= 0) {
            _frameVelocity.y = -stats.groundingForce;
            return;
        }

        var airGravity = stats.FallAcceleration;
        // TODO: Fast air gravity on end jump button
        _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -stats.maxFallSpeed, airGravity * Time.fixedDeltaTime);
    }

    #endregion

    private void ApplyMovement() => _rb.linearVelocity = _frameVelocity;

    #region Respawn System

    public void SetRespawnPoint(Vector2 point) => respawnPoint = point;
    public void Respawn() {
        transform.position = respawnPoint;
        _frameVelocity = Vector2.zero;
    }

    public void TriggerDeath() {
        Respawn();
    }

    #endregion  
}

// References every input made by the player
public struct FrameInput
{
    public bool jumpDown;
    public bool jumpHeld;
    public Vector2 move;
}

