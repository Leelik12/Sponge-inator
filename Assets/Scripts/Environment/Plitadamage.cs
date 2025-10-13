using System.Threading;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Plitadamage : MonoBehaviour
{
    public float damage;
    public float cooldown;
    private float timer;
    public GubkaWater player;

    private void OnTriggerStay(Collider collision)
    {
        timer -= Time.deltaTime;
        if (collision.gameObject.CompareTag("Player"))
        {
            player = collision.gameObject.GetComponent<GubkaWater>();
            if (timer <= 0f)
            {
                Debug.Log("Игрок подсох");
                timer = cooldown;
                player.currentWater -= damage;
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        timer = cooldown;
    }
    void Start()
    {
        timer = cooldown;
    }
}
