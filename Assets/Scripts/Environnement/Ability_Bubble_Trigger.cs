using UnityEngine;
using UnityEngine.U2D.Animation;
using DG.Tweening;

public class Ability_Bubble_Trigger : MonoBehaviour
{
    private GameObject _bubble;
    private Animator _animator;
    private SpriteResolver _spriteResolver;
    [Tooltip("The bubble will be temporarily disabled after being used if true. If false, the bubble will be destroyed after being used.")]
    public bool isReplenishable = true;
    [Tooltip("The slot in which the bubble will be swapped with the player's current ability.")]
    public int swappingSlot = 0;

    // The bubble is empty (popped)
    private bool isEmpty = false;
    // When the bubble has been popped
    private float _timePopped;

    public void Awake()
    {
        _animator = GetComponent<Animator>();
        _bubble = transform.GetChild(0).gameObject;
        _spriteResolver = _bubble.GetComponent<SpriteResolver>();
        isEmpty = false;
    }

    public void FixedUpdate()
    {
        if(isEmpty)
        {
            if(Time.time - _timePopped > 5)
            {
                isEmpty = false;
                _animator.SetBool(EmptyKey, false);
                _bubble.transform.DOScale(0.43f, 0.1f);
            }
        }
    }


    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Entity Hit");

        // Check if we don't collide with random shit
        if(!other.CompareTag("Player")) return;

        // Don't trigger if the bubble is popped
        if(isEmpty) return;

        Debug.Log("Player Hit");
        AbilityManager.Instance.PutAbilityToSlot(GetBubbleAbilityName(), swappingSlot);

        
        isEmpty = true;

        if(isReplenishable)
        {
            // The bubbble is replenishable, change its state
            _animator.SetBool(EmptyKey, true);
            _timePopped = Time.time;
            _bubble.transform.DOScale(0f, 0.1f);

        }
        else 
        {
            // Delete ?
            transform.DOScale(0f, 0.1f);
        }

        
    }

    private string GetBubbleAbilityName()
    {
        return _spriteResolver.GetLabel();
    }
    
    private static readonly int EmptyKey = Animator.StringToHash("isEmpty");
}
