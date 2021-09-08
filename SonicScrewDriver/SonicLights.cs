using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SonicScrewDriver
{
    class SonicLights
    {
        public static Light[] sonicLights;

        public static void InitializeLights(GameObject lightGroup)
        {
            sonicLights = new Light[5];
            sonicLights[0] = lightGroup.transform.Find("Tip").GetComponent<Light>();
            sonicLights[1] = lightGroup.transform.Find("Top1").GetComponent<Light>();
            //sonicLights[2] = lightGroup.transform.Find("Bottom1").GetComponent<Light>();
            sonicLights[2] = lightGroup.transform.Find("Top2").GetComponent<Light>();
            //sonicLights[4] = lightGroup.transform.Find("Bottom2").GetComponent<Light>();
            sonicLights[3] = lightGroup.transform.Find("Top3").GetComponent<Light>();
            //sonicLights[6] = lightGroup.transform.Find("Bottom3").GetComponent<Light>();
            sonicLights[4] = lightGroup.transform.Find("Top4").GetComponent<Light>();
            //sonicLights[8] = lightGroup.transform.Find("Bottom4").GetComponent<Light>();
        }

        public static void ToggleLights()
        {
            foreach (Light i in sonicLights)
            {
                if (i == null)
                {
                    continue;
                }
                i.enabled = !i.enabled;
                Debug.Log("Intensity " + i.intensity);
            }
        }

        public static void TurnOffLights()
        {
            foreach (Light i in sonicLights)
            {
                if (i == null)
                {
                    continue;
                }
                i.enabled = false;
            }
        }

        public static void TurnOnLights()
        {
            foreach(Light i in sonicLights)
            {
                if (i == null)
                {
                    continue;
                }
                i.enabled = true;
            }
        }

        public static void ToggleSingleLight(int i1)
        {
            if (sonicLights[i1] != null)
            {
                sonicLights[i1].enabled = !sonicLights[i1].enabled;
            }
        }

        public static void DecreaseIntensity()
        {
            foreach (Light i in sonicLights)
            {
                i.intensity -= 0.0001f;
            }
        }

        public static void IncreaseIntensity()
        {
            foreach(Light i in sonicLights)
            {
                i.intensity += 0.0001f;
            }
        }

        public static void SetIntensity(float intensity)
        {
            foreach(Light i in sonicLights)
            {
                if (i == null)
                {
                    continue;
                }
                i.intensity = intensity;
            }
        }

        public static void SetMode(int mode)
        {
            Color green = new Color(0f, 128f, 22f);
            Color blue = new Color(0f, 42f, 128f);
            switch (mode)
            {
                case 0:
                    foreach (Light i in sonicLights)
                    {
                        if (i == null)
                        {
                            continue;
                        }
                        i.color = blue;
                    }
                    break;
                case 1:
                    foreach (Light i in sonicLights)
                    {
                        if (i == null)
                        {
                            continue;
                        }
                        i.color = green;
                    }
                    break;
            }
        }
    }
}
