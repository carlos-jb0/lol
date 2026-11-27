using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ArenaBeastAI : MonoBehaviour
{
    private CharacterController controller;
    private Transform player;
    private int health;
    private int attackDamage;
    private float moveSpeed;
    private float attackRange = 1.9f;
    private float attackCooldown = 1.6f;
    private float nextAttack;
    private ArenaWaveManager owner;

    public bool IsDefeated { get; private set; }

    public void Configure(int hitPoints, int damage, float speed, ArenaWaveManager manager)
    {
        health = hitPoints;
        attackDamage = damage;
        moveSpeed = speed;
        owner = manager;
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (IsDefeated) return;
        if (player == null)
        {
            GameObject found = GameObject.Find("Player");
            if (found != null) player = found.transform;
            if (player == null) return;
        }

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        float distance = offset.magnitude;

        if (distance > attackRange)
        {
            Vector3 direction = offset.normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction), 7f * Time.deltaTime);
            if (controller != null && controller.enabled)
                controller.SimpleMove(direction * moveSpeed);
        }
        else if (Time.time >= nextAttack)
        {
            nextAttack = Time.time + attackCooldown;
            RpgPlayerStats stats = player.GetComponent<RpgPlayerStats>();
            if (stats != null) stats.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(int amount)
    {
        if (IsDefeated || amount <= 0) return;
        health -= amount;
        if (VillageGameManager.Instance != null)
            VillageGameManager.Instance.ShowMessage("Impacto: -" + amount + " HP");
        if (health <= 0)
        {
            IsDefeated = true;
            RpgInventory inventory = GameObject.Find("Player") != null
                ? GameObject.Find("Player").GetComponent<RpgInventory>() : null;
            if (inventory != null && Random.value < .55f)
                inventory.AddResource(RpgResourceKind.BeastFang, 1);

            RpgPlayerStats stats = GameObject.Find("Player") != null
                ? GameObject.Find("Player").GetComponent<RpgPlayerStats>() : null;
            if (stats != null) stats.AddExperience(18);

            if (owner != null) owner.NotifyBeastDefeated(this);
            Destroy(gameObject);
        }
    }
}
