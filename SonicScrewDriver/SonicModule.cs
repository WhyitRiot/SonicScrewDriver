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
            public String sonicType;


            //Sounds setup
            public String sonic; /* = item.GetCustomReference("soundSonic").GetComponent<AudioSource>();*/
            public String extend; /*= item.GetCustomReference("soundExtend").GetComponent<AudioSource>();*/
            public String unClick; /*= item.GetCustomReference("soundUnclick").GetComponent<AudioSource>();*/
            public String click; /*= item.GetCustomReference("soundClick").GetComponent<AudioSource>();*/
            public String clasp; /*= item.GetCustomReference("soundClasp").GetComponent<AudioSource>();*/
            public String jingle; /*= item.GetCustomReference("soundJingle").GetComponent<AudioSource>();*/

            //Animation setup
            public String sonicExtendAnimate; /* = item.GetCustomReference("sonicExtendAnimate").GetComponent<Animator>();*/
            public String sonicRetract;
            public String extendHash;
            public String retractHash;
            public String extendAnimPathHash;
            public String retractAnimPathHash;


            public override void OnItemLoaded(Item item)
            {
                base.OnItemLoaded(item);
                item.gameObject.AddComponent<Sonic>();
            }

            
        }



}
