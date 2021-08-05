using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modding;
using UnityEngine;
using ModCommon.Util;

namespace Hornet
{
    public class HornetMod : Mod
    {
        static GameObject hornet = null;
        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>()
            {
                ("GG_Hornet_2",@"Boss Holder/Hornet Boss 2")
            };
        }
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            hornet = UnityEngine.Object.Instantiate(
                preloadedObjects["GG_Hornet_2"][@"Boss Holder/Hornet Boss 2"]
                );
            
            hornet.transform.parent = null;
            UnityEngine.Object.DontDestroyOnLoad(hornet);
            hornet.AddComponent<ControlScript>();

            ModHooks.HeroUpdateHook += ModHooks_HeroUpdateHook;
        }

        private void ModHooks_HeroUpdateHook()
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (hornet.activeSelf)
                {
                    hornet.SetActive(false);
                }
                else
                {
                    hornet.SetActive(true);
                }
            }
        }
    }
}
