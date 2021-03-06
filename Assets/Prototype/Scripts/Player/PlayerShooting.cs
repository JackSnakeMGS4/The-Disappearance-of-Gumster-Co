using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField, Header("Aiming Fields")]
    private GameObject reticle;
    [SerializeField, Range(1f, 10f)]
    private float range = 5f;
    [SerializeField]
    private Transform fire_point;

    private Vector3 aim_pos;

    private bool is_aiming = false;
    public bool Is_Aiming
    {
        get { return is_aiming; }
        set { is_aiming = value; }
    }
    private LineRenderer aim_line;

    [SerializeField, Header("Projectile Fields")]
    private GameObject standard_shot;
    [SerializeField, Range(1f, 30f)]
    private float firing_velocity = 20f;

    [SerializeField, Header("Gun Settings")]
    private int max_ammo = 9;
    private int current_ammo;
    [SerializeField, Range(0f, 5f)]
    private float reload_time = 2.5f;

    [SerializeField]
    private TextMeshProUGUI ammo_count_text;

    private SpriteRenderer player_sprite;
    private SpriteRenderer reticle_sprite;

    private Coroutine current_coroutine;

    private Camera main;

    private void Awake()
    {
        player_sprite = GetComponent<SpriteRenderer>();
        aim_line = GetComponent<LineRenderer>();
        reticle_sprite = reticle.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        main = Camera.main;
        current_ammo = max_ammo;
    }

    private void Update()
    {
        if(current_coroutine == null)
        {
            ammo_count_text.text = "Ammo: " + current_ammo.ToString();
        }
        DrawCursor();
    }

    private void DrawCursor()
    {
        aim_line.sortingLayerName = player_sprite.sortingLayerName;
        reticle_sprite.sortingLayerName = player_sprite.sortingLayerName;

        reticle.transform.position = aim_pos;
        reticle.SetActive(is_aiming);
        aim_line.enabled = is_aiming;
    }

    public void AimCursor(Vector2 vector2)
    {
        //Debug.Log(vector2);

        if (vector2.magnitude > 0.01f)
        {
            // Fix direction angle bug; probably have to use mathf.atan for it?

            aim_line.SetPosition(0, fire_point.position);
            aim_pos = new Vector3(fire_point.position.x + vector2.x * range, fire_point.position.y + vector2.y * range, 0);
            aim_line.SetPosition(1, aim_pos);
            is_aiming = true;
        }
        else
        {
            aim_pos = fire_point.transform.position;
            //fire_point.position = aim_pos;
            is_aiming = false;
        }
    }

    public void AimCursorWithMouse(Vector2 vector2)
    {
        Vector3 pos = main.ScreenToWorldPoint(vector2);
        aim_line.SetPosition(0, fire_point.position);
        aim_pos = new Vector3(pos.x, pos.y, pos.z);
        aim_line.SetPosition(1, aim_pos);
        //is_aiming = true;
    }

    public void Shoot(Card card = null)
    {
        if (is_aiming && current_ammo > 0)
        {
            //shoot projectile in shot direction
            Vector2 dir = aim_pos - fire_point.position;
            dir.Normalize();

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            fire_point.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            GameObject bullet = Instantiate(card == null ? standard_shot : card.projectile_type, 
                fire_point.position, fire_point.rotation);
            //Debug.Log("Shooting " + card.name);
            bullet.layer = gameObject.layer;
            Projectile projectile = bullet.GetComponent<Projectile>();
            projectile.ProjectileSettings(player_sprite.sortingLayerName, 
                card == null ? 100f : card.percentage_vs_health, 
                card == null ? 100f : card.percentage_vs_shields, 
                dir, firing_velocity, gameObject.tag);

            current_ammo--;

            if(current_ammo <= 0)
            {
                Debug.Log("Auto reloading");
                current_coroutine = StartCoroutine(AutoReload());
            } 
        }
    } 
    
    public void Shoot(GameObject salvo)
    {
        if (is_aiming && current_ammo > 0)
        {
            //shoot projectile in shot direction
            Vector2 dir = aim_pos - fire_point.position;
            dir.Normalize();

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            fire_point.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            GameObject bullet = Instantiate(salvo, fire_point.position, fire_point.rotation);
            //Debug.Log("Shooting " + card.name);
            bullet.layer = gameObject.layer;
            Projectile projectile = bullet.GetComponent<Projectile>();
            projectile.ProjectileSettings(player_sprite.sortingLayerName, 800f, 800f, dir, firing_velocity, gameObject.tag);

            current_ammo = 0;

            if(current_ammo <= 0)
            {
                Debug.Log("Auto reloading");
                current_coroutine = StartCoroutine(AutoReload());
            } 
        }
    }

    public void ManualReload()
    {
        if(current_coroutine != null)
        {
            Debug.Log("Reload is already happening");
            return;
        }

        if(current_ammo < max_ammo)
        {
            current_coroutine = StartCoroutine(AutoReload());
        }
    }

    IEnumerator AutoReload()
    {
        //effects/animation
        //Debug.Log("Reloading");
        ammo_count_text.text = "Reloading";

        yield return new WaitForSeconds(reload_time);
        //Debug.Log("Reload done");
        current_ammo = max_ammo;
        current_coroutine = null;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.black;
    //    Gizmos.DrawWireSphere(aim_pos, .2f);

    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(fire_point.position, .2f);
    //}
}
