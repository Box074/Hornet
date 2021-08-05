
using System.Collections;
using UnityEngine;
using TranCore;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon.Util;

namespace Hornet
{
    public class ControlScript : MonoBehaviour
    {
        public TranAttach TranAttach => gameObject.GetTranAttach();
        tk2dSpriteAnimator animator = null;
        Rigidbody2D rig = null;
        GameObject SphereG = null;
        GameObject HC1 = null;
        GameObject HC2 = null;
        GameObject needle = null;
        DefaultActions defaultActions = null;
        void OnCollisionEnter2D(Collision2D collision) => OnCollisionStay2D(collision);
        void OnCollisionStay2D(Collision2D collision)
        {
            HealthManager hm = collision.gameObject.GetComponent<HealthManager>() ??
                collision.otherCollider.GetComponent<HealthManager>();
            if (hm != null)
            {
                if (TranAttach.IsActionInvoking("DASH") || TranAttach.IsActionInvoking("DSTAB"))
                {

                    hm.Hit(new HitInstance()
                    {
                        AttackType = AttackTypes.SharpShadow,
                        Source = gameObject,
                        DamageDealt = PlayerData.instance.nailDamage,
                        Multiplier = 1,
                        MagnitudeMultiplier = 1,
                        CircleDirection = true,
                        IgnoreInvulnerable = false
                    });
                    FSMUtility.SendEventToGameObject(hm.gameObject, "TOOK DAMAGE");
                    FSMUtility.SendEventToGameObject(hm.gameObject, "TAKE DAMAGE");
                }
            }
        }
        void Awake()
        {
            PlayMakerFSM control = gameObject.LocateMyFSM("Control");
            SphereG = control.FsmVariables.FindFsmGameObject("Sphere Ball").Value;
            SphereG.TranHeroAttack(AttackTypes.Spell, 1);
            HC1 = control.FsmVariables.FindFsmGameObject("Hit Counter 1").Value.TranHeroAttack(
                AttackTypes.Nail, 15);
            HC2 = control.FsmVariables.FindFsmGameObject("Hit Counter 2").Value.TranHeroAttack(
                AttackTypes.Nail, 15);

            needle = transform.Find("Needle").gameObject.Clone().TranHeroAttack(AttackTypes.Nail, 4)
                .SetParent(null);
            DontDestroyOnLoad(needle);
            needle.LocateMyFSM("Control").InsertMethod("Notify", 0, () =>
            {
                CancelThrow();
            });
            needle.LocateMyFSM("Control").GetState("Destroy").Actions = new FsmStateAction[0];
            needle.name = "HornetAttackN";
            needle.SetActive(false);
            foreach (var v in GetComponents<PlayMakerFSM>()) Destroy(v);


            animator = GetComponent<tk2dSpriteAnimator>();
            rig = GetComponent<Rigidbody2D>();
            defaultActions = new DefaultActions(animator, rig);

            TranAttach.AutoDis = false;

            TranAttach.RegisterAction("M1", defaultActions.MoveHeroTo);
            TranAttach.InvokeActionOn("M1", defaultActions.MoveHeroToTest);
            TranAttach.RegisterAction("M2", defaultActions.MoveTranToHero);
            TranAttach.InvokeActionOn("M2", defaultActions.MoveTranToHeroTest);
            TranAttach.RegisterAction("M3", defaultActions.SetTranScale);
            TranAttach.InvokeActionOn("M3", DefaultActions.AlwaysTrue);
            TranAttach.RegisterAction("TURN", defaultActions.Turn);
            TranAttach.InvokeActionOn("TURN", DefaultActions.TurnTest);

            TranAttach.RegisterAction("JUMP", Jump,
                TranAttach.InvokeWithout("DSTAB"),
                TranAttach.InvokeWithout("JUMP"),
                TranAttach.InvokeWithout("DASH"),
                TranAttach.InvokeWithout("SPHERE")
                );
            TranAttach.InvokeActionOn("FALL", DefaultActions.JumpTest);

            TranAttach.RegisterAction("FALL", defaultActions.Fall,
                TranAttach.InvokeWithout("DSTAB"),
                TranAttach.InvokeWithout("FALL"),
                TranAttach.InvokeWithout("SPHERE"),
                TranAttach.InvokeWithout("DASH"),
                TranAttach.InvokeWithout("THROW")
                );
            TranAttach.InvokeActionOn("FALL", defaultActions.FallTest);

            TranAttach.RegisterAction("STOP", defaultActions.Stop,
                TranAttach.InvokeWithout("STOP"),
                TranAttach.InvokeWithout("RUN"),
                TranAttach.InvokeWithout("DASH")
                );
            TranAttach.InvokeActionOn("STOP", DefaultActions.AlwaysTrue);

            TranAttach.RegisterAction("RUN", Run,
                TranAttach.InvokeWithout("DSTAB"),
                TranAttach.InvokeWithout("DASH"),
                TranAttach.InvokeWithout("SPHERE"),
                TranAttach.InvokeWithout("THROW"),
                TranAttach.InvokeWithout("RUN"));
            TranAttach.InvokeActionOn("RUN", DefaultActions.RunTest);

            TranAttach.RegisterAction("DASH", Dash,
                TranAttach.InvokeWithout("DSTAB"),
                TranAttach.InvokeWithout("DASH"),
                TranAttach.InvokeWithout("SPHERE")
                );
            TranAttach.InvokeActionOn("DASH", DefaultActions.DashTest);

            TranAttach.RegisterAction("SPHERE", Sphere,
                DefaultActions.CanCastAuto,
                TranAttach.InvokeWithout("DSTAB"),
                TranAttach.InvokeWithout("SPHERE"),
                TranAttach.InvokeWithout("DASH")
                );
            TranAttach.InvokeActionOn("SPHERE", TranAttach.And(
                DefaultActions.CastDownTest,
                DefaultActions.DownTest
                ));
            TranAttach.RegisterAction("THROW", Throw,
                TranAttach.InvokeWithout("THROW"),
                TranAttach.InvokeWithout("DSTAB"),
                TranAttach.InvokeWithout("SPHERE"),
                TranAttach.InvokeWithout("DASH")
                //TranAttach.InvokeWithout("JUMP"),
                //TranAttach.InvokeWithout("FALL"),
                //TranAttach.InvokeWithout("RUN")
                );
            TranAttach.InvokeActionOn("THROW", TranAttach.And(
                DefaultActions.CastDownTest,
                TranAttach.Not(
                    TranAttach.Or(
                        DefaultActions.DownTest
                        )
                    )
                ));

            TranAttach.RegisterAction("C", Counter,
                TranAttach.InvokeWithout("DSTAB"),
                TranAttach.InvokeWithout("SPHERE"),
                TranAttach.InvokeWithout("DASH"),
                TranAttach.InvokeWithout("JUMP"),
                TranAttach.InvokeWithout("FALL"),
                TranAttach.InvokeWithout("RUN"),
                TranAttach.InvokeWithout("THROW"),
                TranAttach.InvokeWithout("C"),
                TranAttach.InvokeWithout("ATTACK")
                );
            TranAttach.InvokeActionOn("C", DefaultActions.AttackTest);

            TranAttach.RegisterAction("ATTACK", Attack,
                TranAttach.InvokeWithout("DSTAB"),
                TranAttach.InvokeWithout("SPHERE"),
                TranAttach.InvokeWithout("DASH"),
                TranAttach.InvokeWithout("JUMP"),
                TranAttach.InvokeWithout("FALL"),
                TranAttach.InvokeWithout("RUN"),
                TranAttach.InvokeWithout("THROW"),
                TranAttach.InvokeWith("C"),
                TranAttach.InvokeWithout("ATTACK"));

            TranAttach.RegisterAction("IDLE", defaultActions.Idle,
                TranAttach.InvokeWithout("IDLE"),
                TranAttach.Or(
                    TranAttach.And(
                        () => TranAttach.InvokeCount == 3,
                        TranAttach.InvokeWith("STOP")
                        ),
                    () => TranAttach.InvokeCount == 2
                ));
            TranAttach.InvokeActionOn("IDLE", DefaultActions.AlwaysTrue);

            Destroy(GetComponent<EnemyDeathEffects>());
        }
        void OnEnable()
        {
            if (GetComponent<MeshRenderer>() != null)
            {
                GetComponent<MeshRenderer>().enabled = true;
            }
            rig.isKinematic = false;
            rig.gravityScale = 1;
            foreach (var v in GetComponents<Collider2D>())
            {
                v.enabled = true;
                v.isTrigger = false;
            }

            On.HeroController.CanTalk += HeroController_CanTalk;
            On.HeroController.CanFocus += HeroController_CanFocus;
            On.HeroController.CanQuickMap += HeroController_CanQuickMap;
            On.HeroController.CanNailCharge += HeroController_CanNailCharge;
        }

        private bool HeroController_CanNailCharge(On.HeroController.orig_CanNailCharge orig, HeroController self)
        {
            return false;
        }

        private bool HeroController_CanQuickMap(On.HeroController.orig_CanQuickMap orig, HeroController self)
        {
            if (!HeroController.instance.cState.dead) return true;
            return false;
        }

        private bool HeroController_CanFocus(On.HeroController.orig_CanFocus orig, HeroController self)
        {
            if (!HeroController.instance.cState.dead) return true;
            return false;
        }

        private bool HeroController_CanTalk(On.HeroController.orig_CanTalk orig, HeroController self)
        {
            if (!HeroController.instance.cState.dead) return true;
            return false;
        }

        void OnDisable()
        {
            On.HeroController.CanTalk -= HeroController_CanTalk;
            On.HeroController.CanFocus -= HeroController_CanFocus;
            On.HeroController.CanQuickMap -= HeroController_CanQuickMap;
            On.HeroController.CanNailCharge -= HeroController_CanNailCharge;

            On.HeroController.TakeDamage -= _NoDamage;
        }
        IEnumerator Sphere()
        {
            CancelThrow();
            defaultActions.CancelFall();
            DefaultActions.TakeCastMPAuto();
            On.HeroController.TakeDamage -= _NoDamage;
            On.HeroController.TakeDamage += _NoDamage;
            rig.gravityScale = 0;
            yield return animator.PlayAnimWait("Sphere Antic A Q");
            animator.Play("Sphere Attack");
            SphereG.SetActive(true);
            yield return new WaitForSeconds(1);
            SphereG.SetActive(false);
            yield return animator.PlayAnimWait("Sphere Recover A");
            rig.gravityScale = 1;
            On.HeroController.TakeDamage -= _NoDamage;
        }
        IEnumerator Dash()
        {
            CancelThrow();
            CancelCounter();
            rig.gravityScale = 0;
            On.HeroController.TakeDamage -= _NoDamage;
            On.HeroController.TakeDamage += _NoDamage;
            animator.Play("G Dash");
            rig.SetVY(0);
            if (TranAttach.IsActionInvoking("THROW"))
            {
                needle.SetActive(false);
                rig.rotation = Vector2.Angle(transform.position, needle.transform.position);
                iTween.MoveTo(gameObject, needle.transform.position + new Vector3(0, 0.5f), 0.75f);
                yield return new WaitForSeconds(0.65f);
                animator.Play("Idle");
            }
            else
            {
                rig.SetVX(HeroController.instance.cState.facingRight ?
                    HeroController.instance.DASH_SPEED :
                    -HeroController.instance.DASH_SPEED);
                yield return new WaitForSeconds(0.5f);
                rig.SetVX(0);


                yield return animator.PlayAnimWait("G Dash Recover1");
                yield return animator.PlayAnimWait("G Dash Recover2");
            }
            rig.rotation = 0;
            rig.gravityScale = 1;
            On.HeroController.TakeDamage -= _NoDamage;
        }

        private void _NoDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go,
            GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
        {
            
        }
        public void CancelCounter()
        {
            isCounter = false;
        }
        bool isCounter = false;
        IEnumerator Counter()
        {
            isCounter = true;
            On.HeroController.TakeDamage -= _NoDamage;
            On.HeroController.TakeDamage += _NoDamage;
            yield return animator.PlayAnimWait("Counter Antic");
            animator.Play("Counter Stance");
            while (InputHandler.Instance.inputActions.attack.IsPressed && isCounter) yield return null;
            if (isCounter)
            {
                TranAttach.InvokeAction("ATTACK");
            }
            On.HeroController.TakeDamage -= _NoDamage;
        }

        IEnumerator Attack()
        {
            HC1.GetComponent<DamageEnemies>().damageDealt = PlayerData.instance.nailDamage;
            HC2.GetComponent<DamageEnemies>().damageDealt = PlayerData.instance.nailDamage;

            HC1.SetActive(true);
            yield return animator.PlayAnimWait("Counter Attack 1");
            HC1.SetActive(false);
            HC2.SetActive(true);
            yield return animator.PlayAnimWait("Counter Attack 2");
            HC2.SetActive(false);
            animator.Play("Counter Attack Recover");
        }

        IEnumerator Run()
        {
            CancelThrow();
            CancelCounter();
            animator.Play("Run");
            rig.SetVX(
                HeroController.instance.cState.facingRight?
                HeroController.instance.RUN_SPEED_CH_COMBO:
                -HeroController.instance.RUN_SPEED_CH_COMBO
                );
            yield return null;
        }
        IEnumerator Jump()
        {
            CancelThrow();
            CancelCounter();
            yield return null;
            rig.velocity = new Vector2(0, 25);
            Modding.Logger.Log("V: " + rig.velocity.y.ToString());
            animator.Play("Jump");
            
            yield return new WaitForSeconds(0.25f);
            
        }
        public void CancelThrow()
        {
            isThrow = false;
        }
        bool isThrow = false;
        IEnumerator Throw()
        {
            isThrow = true;
            rig.gravityScale = 0;
            yield return animator.PlayAnimWait("Throw Antic Q");
            rig.gravityScale = 1;
            animator.Play("Throw");
            needle.transform.position = transform.position + new Vector3(0,-0.5f);
            needle.SetActive(true);

            if (DefaultActions.LeftTest())
            {
                needle.GetComponent<Rigidbody2D>().SetVX(-38);
            }else if (DefaultActions.RightTest())
            {
                needle.GetComponent<Rigidbody2D>().SetVX(38);
            }
            if (DefaultActions.UpTest())
            {
                needle.GetComponent<Rigidbody2D>().SetVY(38);
            }
            else if (DefaultActions.DownTest())
            {
                needle.GetComponent<Rigidbody2D>().SetVY(-38);
            }
            else
            {
                needle.GetComponent<Rigidbody2D>().SetVX(
                                HeroController.instance.cState.facingRight ?
                                38 : -38
                                );
            }
            while (isThrow && needle.activeSelf) yield return null;
            if (needle.activeSelf)
            {
                animator.Play("Throw Recover");
            }
            needle.SetActive(false);
        }
    }
}