using UnityEngine;

public class RoomScript : MonoBehaviour
{
    public GameObject virtualCam;

    public Vector2[] respawnPoints;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerScript player = other.GetComponent<PlayerScript>();

            if(player.respawnPoint != Vector2.zero)
            {
                Time.timeScale = 0.01f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;

                // Resume time in 0.3 seconds
                Invoke("ResumeTime", .005f);
            }

            // Debug.Log("Player entered camera trigger");
            virtualCam.SetActive(true);

            // Time.timeScale = 1f;
            // Time.fixedDeltaTime = 0.02f;

            // Get the player's coordinates
            Vector2 playerPos = other.transform.position;

            // Find the closest respawn point
            Vector2 closestRespawn = respawnPoints[0];
            float closestDistance = Vector2.Distance(playerPos, closestRespawn);

            foreach (Vector2 respawn in respawnPoints)
            {
                float distance = Vector2.Distance(playerPos, respawn);
                if (distance < closestDistance)
                {
                    closestRespawn = respawn;
                    closestDistance = distance;
                }
            }

            player.SetRespawnPoint(closestRespawn);
        }
    }

    void ResumeTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            virtualCam.SetActive(false);
        }
    }
}
