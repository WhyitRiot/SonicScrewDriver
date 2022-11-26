using UnityEngine;

namespace SonicScrewDriver
{
    class SonicLights
    {
        public Light[] sonicLights;
        private Color green = new Color(0f, 128f, 22f);
        private Color blue = new Color(0f, 42f, 128f);

        public SonicLights(int lights, string[] lightObjects, GameObject lightGroup)
        {
            sonicLights = new Light[lights];
            for (int i = 0; i < lights; i++)
            {
                sonicLights[i] = lightGroup.transform.Find(lightObjects[i]).GetComponent<Light>();
            }
        }

        public void ToggleLights()
        {
            foreach (Light i in sonicLights)
            {
                if (i == null)
                {
                    continue;
                }
                i.enabled = !i.enabled;
            }
        }

        public void TurnOffLights()
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

        public void TurnOnLights()
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

        public void ToggleSingleLight(int i1)
        {
            if (sonicLights[i1] != null)
            {
                sonicLights[i1].enabled = !sonicLights[i1].enabled;
            }
        }

        public void AddIntensity(float intensity)
        {
            foreach(Light i in sonicLights)
            {
                i.intensity += intensity;
            }
        }

        public void SetIntensity(float intensity)
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

        public void DecreaseIntensity(float intensity)
        {
            foreach (Light i in sonicLights)
            {
                if (i == null)
                {
                    continue;
                }
                if (i.intensity - intensity < 0f)
                {
                    i.intensity = 0f;
                }
                i.intensity -= intensity;
            }
        }

        public void SetMode(int mode)
        {
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
