using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

// [RequireComponent(typeof(CharacterController))]
// [RequireComponent(typeof(PlayerInput))]
public class ThirdPersonController : MonoBehaviour, weaponhandler
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;
    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;
    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    [Header("other")]
    [SerializeField]
    private GameObject rightHandObject;
    [SerializeField]
    private GameObject healthbar;
    public float AttackTimeout = 0.8f;
    public float AttackMultiplier = 1.0f;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private float _AttackTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDMotionSpeed;

    private Animator _animator;
    private CharacterController _controller;
    private InputEventReader _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private playerstats playerstats;
    private bool isAttacking;
    private ActionCounter actionCounter = new ActionCounter();

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<InputEventReader>();
        playerstats = new playerstats();

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
        _AttackTimeoutDelta = AttackTimeout;
        _animator.SetFloat("attackSpeed", AttackMultiplier);
    }

    public void damage(float damage)
    {
        playerstats.damage(damage);
        healthbar.GetComponent<healthbar>().UpdateHealthBar(playerstats.hp, playerstats.maxhp);

    }

    internal void stagger()
    {
        _animator.SetTrigger("stagger");
    }

    private void Update()
    {
        JumpAndGravity();
        GroundedCheck();
        Attack();
        Move();
        // printParameters();
        actionCounter.updateTime(Time.deltaTime);
    }

    private void Attack()
    {
        if (_AttackTimeoutDelta >= 0.0f) _AttackTimeoutDelta -= Time.deltaTime;
        if (!Grounded) return;
        if (isAttacking && _AttackTimeoutDelta <= 0.0f)
        {
            print("reset attack cooldown");
            // _animator.SetBool("LightAttack", false);
            isAttacking = false;
        }
        if (_input.lightAttack && _AttackTimeoutDelta <= 0.0f && !isAttacking)
        {
            isAttacking = true;
            print("light attack");
            // _animator.SetBool("LightAttack", true);
            _AttackTimeoutDelta = AttackTimeout;
        }
        _animator.SetBool("LightAttack", isAttacking);
    }


    private void LateUpdate()
    {
        CameraRotation();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        // update animator if using character
        _animator.SetBool("grounded", Grounded);
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            _cinemachineTargetYaw += _input.look.x * Time.deltaTime;
            _cinemachineTargetPitch += _input.look.y * Time.deltaTime;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    public void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        //brake if in attack state
        if (isAttacking) targetSpeed = 0f;

        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        // float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
        float inputMagnitude = 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }
        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }


        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        _animator.SetFloat(_animIDSpeed, _animationBlend);
    }

    public void Roll(){
        bool success = actionCounter.addAction("roll", 2f);
        if(!success) return;
        print("start roll");
        _animator.SetTrigger("roll");
    }

    private void printParameters()
    {
        AnimatorControllerParameter[] paramlist = _animator.parameters;
        for (int i = 0; i < paramlist.Length; i++)
        {
            var p = paramlist[i];
            AnimatorControllerParameterType type = p.type;
            string name = p.name;
            var val = "trigger";
            if (type == AnimatorControllerParameterType.Float)
            {
                // val = _animator.GetFloat(name).ToString();
            };
            if (type == AnimatorControllerParameterType.Bool)
            {
                val = _animator.GetBool(name).ToString();
            };
            if (type == AnimatorControllerParameterType.Int)
            {
                val = _animator.GetInteger(name).ToString();
            };
            print(name + " " + type + " " + val);
        }
    }
    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            _animator.SetBool("grounded", false);

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // update animator if using character
                _animator.SetBool("grounded", true);
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _animator.SetBool("grounded", true);
            }

            // if we are not grounded, do not jump
            _input.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }

    public void onWeaponCollision(Collider other, float damage)
    {
        bool isStatic = other.gameObject.isStatic;
        if (isStatic)
        {
            stagger();
        }
        bool hit = other.gameObject.TryGetComponent<enemy>(out enemy e);
        if (hit)
        {
            e.damage(damage);
        }
    }
}