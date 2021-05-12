using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace SonicScrewDriver
{

        public class SonicModule : ItemModule
        {
            public string projectileID = "Arrow1";
            public string sonicWaveSpawn = "ParryPoint";

            public float projectileSpeed = 40f;
            public float timeToDespawn = 1f;



            public override void OnItemLoaded(Item item)
            {
                base.OnItemLoaded(item);
                item.gameObject.AddComponent<Sonic>();
            }

            
        }



}
