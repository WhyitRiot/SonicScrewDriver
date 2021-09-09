using System;
using ThunderRoad;

namespace SonicScrewDriver
{
    public class SonicModule : ItemModule
    {
        //2, 3, 4, 10, 11, 12
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
        public int lightNumber;
        public String lightGroup;
        public String[] lightObjects;

        //VFX
        public String vfx;

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
