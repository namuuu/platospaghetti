using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu]
public class PlayerStats: ScriptableObject
{
    [Header("Speed")]
    public float maxSpeed = 8.5f; // Speed at which the player moves
    public float acceleration = 120f; // How fast the player accelerates

    [Header("Deceleration")]
    public float airDeceleration = 30f; // How fast the player stops moving in the air
    public float groundDeceleration = 200f; // How fast the player stops moving
    public float turningDeceleration = 120f; // How fast the player stops moving when turning

    [Header("Jumping")]
    public float jumpForce = 17f;
    public float jumpBufferTime = 0.2f; // Time after jump was pressed that the player can still jump
    public float coyoteTime = .15f; // Time after the player leaves the ground that they can still jump

    [Header("Falling and Gravity")]
    public float maxFallSpeed = 40f; // Maximum speed the player can fall
    public float FallAcceleration = 45f; // Gravity applied to the player when in the air
    public float groundingForce = 1.5f; // Force applied to the player when grounded

    [Header("Misc")]
    public float groundDistance = 0.05f; // Distance from the ground to check for collisions
    public float deadZone = 0.1f; // Deadzone for the input
    public Vector2 wallJumpForce = new(13, 13); // Force applied to the player when wall jumping

    [Header("Dashing")]
    public float dashSpeed = 20f; // Speed at which the player dashes
    public float dashDuration = 0.2f; // Duration of the dash
}