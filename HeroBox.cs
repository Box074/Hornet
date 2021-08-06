using System.Collections;
using UnityEngine;
using TranCore;

namespace Hornet
{
    public class HeroBox : MonoBehaviour
    {
        public TranAttach TranAttach => gameObject.GetTranAttach();
        void HitEnemy(GameObject go)
        {
            HealthManager hm = go.GetComponent<HealthManager>();
            FSMUtility.SendEventToGameObject(go, "TOOK DAMAGE");
            FSMUtility.SendEventToGameObject(go, "HIT");
            FSMUtility.SendEventToGameObject(go, "TAKE DAMAGE");
            if (hm != null)
            {
                hm.Hit(new HitInstance()
                {
                    AttackType = AttackTypes.Nail,
                    Source = gameObject,
                    DamageDealt = PlayerData.instance.nailDamage,
                    Multiplier = 1,
                    MagnitudeMultiplier = 1,
                    CircleDirection = true,
                    IgnoreInvulnerable = false
                });
            }

        }
        void OnCollisionEnter2D(Collision2D collision) => OnCollisionStay2D(collision);
        void OnCollisionStay2D(Collision2D collision)
        {
            HealthManager hm = collision.collider.gameObject.GetComponent<HealthManager>() ??
                collision.otherCollider.GetComponent<HealthManager>();
            if (hm != null)
            {
                if (TranAttach.IsActionInvoking("DASH"))
                {
                    HitEnemy(hm.gameObject);
                }
            }
        }
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}