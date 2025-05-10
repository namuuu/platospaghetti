using System;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Animator _anim;
    [SerializeField] private SpriteRenderer _sprite;
    private PlayerScript _player;
    private PlayerStats _playerStats;
    private AudioSource _audioSource;

    [Header("Particles")]
    [SerializeField] private ParticleSystem _moveParticles;
    [SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private ParticleSystem _landParticles;

    private Color _currentGradient;
    private CapsuleCollider2D _col;
    private float hitboxOffset = 0;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _player = GetComponentInParent<PlayerScript>();
        _playerStats = _player.stats;
        _col = GetComponentInParent<CapsuleCollider2D>();
        hitboxOffset = _col.offset.y;
    }

    private void OnEnable()
    {
        _player.OnGroundChange += OnGroundChange;
        _player.OnJump += OnJumped;

        _moveParticles.Play();
    }

    private void OnDisable()
    {
        _player.OnGroundChange -= OnGroundChange;
        _player.OnJump -= OnJumped;

        _moveParticles.Stop();
    }

    private void FixedUpdate()
    {
        if(_player == null) return;

        HandleSpriteFlip();
        HandleSpeed();
    }

    private void HandleSpriteFlip()
    {
        if(_player._frameVelocity.x > 0.1f)
        {
            // _sprite.flipX = false;
            _player.transform.localScale = new Vector3(1, 1, 1);
            _player.transform.rotation = Quaternion.Euler(0, 0, 0);
        } else if(_player._frameVelocity.x < -0.1f)
        {
            // _sprite.flipX = true;
            _player.transform.localScale = new Vector3(1, -1, 1);
            _player.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
    }

    private void HandleSpeed()
    {
        _anim.SetFloat(SpeedKey, Mathf.Lerp(0.7f, 1.4f, Math.Abs(_player._frameVelocity.x) / _playerStats.maxSpeed));
        if((_player._frameVelocity.x > 0.1f || _player._frameVelocity.x < -0.1f) && _player._grounded)
        {
            if(!_moveParticles.isPlaying) _moveParticles.Play();
        }
        else 
        {
            _moveParticles.Stop();
        }
    }

    private void OnGroundChange(bool grounded, float impact)
    {
        if(grounded)
        {
            _moveParticles.Play();
            _anim.SetTrigger(GroundedKey);
        }
        else 
        {
            _moveParticles.Stop();
        }
    }

    private void OnJumped()
    {
        _anim.SetTrigger(JumpKey);
        _jumpParticles.Play();
    }

    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int SpeedKey = Animator.StringToHash("IdleSpeed");
    private static readonly int JumpKey = Animator.StringToHash("Jump");
}
