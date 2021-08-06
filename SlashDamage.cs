using System.Collections;
using UnityEngine;

namespace Hornet
{
    public class SlashDamage : MonoBehaviour
    {

        void OnCollisionEnter2D(Collision2D collision) => OnCollisionStay2D(collision);
        void OnCollisionStay2D(Collision2D collision)
        {
            GameObject go = collision.gameObject;
            FSMUtility.SendEventToGameObject(go, "TAKE DAMAGE");
            FSMUtility.SendEventToGameObject(go, "HIT");
        }
    }
}