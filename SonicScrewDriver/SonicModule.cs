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
        //2, 3, 4, 10, 11
        public int sonicType;


        //Sounds setup
        public String sonic; 
        public String extend; 
        public String unClick; 
        public String click; 
        public String clasp; 
        public String jingle;

        //Light
        public String light;

        //Collider
        public String collider;

        //Animation setup
        public String sonicExtendAnimate;
        public String sonicRetract;
        public String extendHash;
        public String retractHash;
        public String extendAnimPathHash;
        public String retractAnimPathHash;
        public String slide;


        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<Sonic>();
        }        
    }
}
