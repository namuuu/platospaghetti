using UnityEngine;

public class Damage_Script : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
    {
        // collider.

        GameObject other = collider.gameObject;
        if(other.CompareTag("Player") && other.TryGetComponent(out PlayerScript player))
        {
            player.TriggerDeath();
        }
    }
}
