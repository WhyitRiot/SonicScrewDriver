﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThunderRoad;

namespace SonicScrewDriver
{
    class Sonic : MonoBehaviour
    {
        protected Item item;
        public SonicModule module;
        int sonicType;

        RagdollHand handOne;
        RagdollHand handTwo;
        bool buttonHold;
        bool extended;
        bool doublePress = false;
        bool fadein = true;
        int pressCount = 0;
        float pressCooldown = 0.5f;
        GameObject sonicEnd;
        Vector3 sonicOrigin;
        float sonicRange = 200;

        Animator sonicExtendAnimate;
        int extendAnimPathHash;
        int retractAnimPathHash;
        int extendAnimHash;
        int retractAnimHash;

        Light sonicLight;

        List<AudioSource> audioSources = new List<AudioSource>();
        AudioSource sonic;
        bool sonicPlaying;
        float startPitch;
        float extendPitch;
        float tempPitch;
        float maxPitch;
        AudioSource extend;
        AudioSource click;
        AudioSource unClick;
        AudioSource clasp;
        AudioSource jingle;

        CapsuleCollider collider;

        Transform slide;
        Vector3 slidePos;

        bool grippedRight;
        bool grippedLeft;

        bool coroutineRunning;

        protected void Awake()
        {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<SonicModule>();
            sonicType = module.sonicType;

            //Sounds setup
            if (!String.IsNullOrEmpty(module.sonic))
            {
                sonic = item.GetCustomReference(module.sonic).GetComponent<AudioSource>();
                audioSources.Add(sonic);
            }
            startPitch = sonic.pitch;
            tempPitch = 0f;
            extendPitch = startPitch * 1.5f;
            maxPitch = startPitch * 3;

            if (!String.IsNullOrEmpty(module.extend))
            {
                extend = item.GetCustomReference(module.extend).GetComponent<AudioSource>();
                audioSources.Add(extend);
            }
            if (!String.IsNullOrEmpty(module.unClick))
            {
                unClick = item.GetCustomReference(module.unClick).GetComponent<AudioSource>();
                audioSources.Add(unClick);
            }
            if (!String.IsNullOrEmpty(module.click))
            {
                click = item.GetCustomReference(module.click).GetComponent<AudioSource>();
                audioSources.Add(click);
            }
            if (!String.IsNullOrEmpty(module.clasp))
            {
                clasp = item.GetCustomReference(module.clasp).GetComponent<AudioSource>();
                audioSources.Add(clasp);
            }
            if (!String.IsNullOrEmpty(module.jingle))
            {
                jingle = item.GetCustomReference(module.jingle).GetComponent<AudioSource>();
                audioSources.Add(jingle);
            }

            foreach (AudioSource i in audioSources)
            {
                i.volume = GameManager.options.effectVolume;
            }

            //Raycast setup
            sonicEnd = item.GetCustomReference("sonicEnd").gameObject;
            sonicOrigin = sonicEnd.transform.position;

            //Light setup
            if (!String.IsNullOrEmpty(module.light))
            {
                sonicLight = item.GetCustomReference(module.light).GetComponent<Light>();
                sonicLight.enabled = false;
            }
            if (!String.IsNullOrEmpty(module.lightGroup))
            {
                SonicLights.InitializeLights(item.GetCustomReference(module.lightGroup).gameObject);
                SonicLights.ToggleLights();
            }

            //Animation setup
            if (!String.IsNullOrEmpty(module.sonicExtendAnimate)) sonicExtendAnimate = item.GetCustomReference(module.sonicExtendAnimate).GetComponent<Animator>();
            if (sonicType == 10 && !String.IsNullOrEmpty(module.sonicExtendAnimate))
            {
                sonicExtendAnimate.enabled = false;
            }
            if (!String.IsNullOrEmpty(module.extendHash)) extendAnimHash = Animator.StringToHash(module.extendHash);
            if (!String.IsNullOrEmpty(module.retractHash)) retractAnimHash = Animator.StringToHash(module.retractHash);
            if (!String.IsNullOrEmpty(module.extendAnimPathHash)) extendAnimPathHash = Animator.StringToHash(module.extendAnimPathHash);
            if (!String.IsNullOrEmpty(module.retractAnimPathHash)) retractAnimPathHash = Animator.StringToHash(module.retractAnimPathHash);



            //Collider to track screwdriver movement
            if (!String.IsNullOrEmpty(module.collider)) collider = item.GetCustomReference("collider").GetComponent<CapsuleCollider>();
            if (!String.IsNullOrEmpty(module.slide)) slide = item.GetCustomReference("slide");
            if (!String.IsNullOrEmpty(module.slide)) slidePos = new Vector3(slide.localPosition.x, slide.localPosition.y, slide.localPosition.z);

            //Event subscribers
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnHeldActionEvent += OnHeldAction;
            item.OnUngrabEvent += OnUngrabEvent;
        }

        public void OnHeldAction(RagdollHand interactor, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                if (pressCooldown > 0 && pressCount == 1)
                {
                    doublePress = true;
                }
                else
                {
                    pressCooldown = 0.5f;
                    pressCount++;
                }
                //doublePress = false;
                //if (firstPress)
                //{
                //    if (Time.time - timeOffPress < 0.5f)
                //    {
                //        doublePress = true;
                //    }
                //}
                //if (!firstPress)
                //{
                //    firstPress = true;
                //    timeOffPress = Time.time;
                //}
                if (sonicType == 2 || sonicType == 10 || sonicType == 11 || sonicType == 12)
                {
                    buttonHold = true;
                    sonic.Play();
                    click.Play();
                    sonicPlaying = true;

                    if (sonicType != 12)
                    {
                        sonicLight.enabled = true;
                    }
                    else
                    {
                        if (doublePress && !extended)
                        {
                            StartCoroutine(RotatingLights());
                        }
                        else
                        {
                            SonicLights.ToggleLights();
                            if (extended == true)
                            {
                                SonicLights.SetMode(1);
                            }
                            else
                            {
                                SonicLights.SetMode(0);
                            }
                        }
                    }
                }
            }
            if (action == Interactable.Action.AlternateUseStop)
            {
                if (sonicType == 2 || sonicType == 10 || sonicType == 11 || sonicType == 12)
                {
                    buttonHold = false;
                    sonic.Stop();
                    sonicPlaying = false;
                    unClick.Play();

                    if (sonicType != 12)
                    {
                        sonicLight.enabled = false;
                    }
                    else
                    {
                        if (!doublePress)
                        {
                            SonicLights.ToggleLights();
                        }
                    }
                    doublePress = false;
                }
            }
            if (action == Interactable.Action.UseStart)
            {
                if (!extended)
                {
                    if (sonicType == 11)
                    {
                        extended = true;
                        extend.Play();
                        StartCoroutine(playAnimation(sonicExtendAnimate, 0));
                        PlayerControl.GetHand(interactor.playerHand.side).HapticShort(2f);
                    }
                    if (sonicType == 12)
                    {
                        extended = true;
                    }
                }
                if (sonicType == 3 || sonicType == 4)
                {
                    buttonHold = true;
                    sonic.Play();
                    sonicPlaying = true;
                }
            }
            if (action == Interactable.Action.UseStop)
            {
                if (sonicType == 3 || sonicType == 4)
                {
                    buttonHold = false;
                    sonic.Stop();
                    sonicPlaying = true;
                }
                if (sonicType == 12)
                {
                    extended = false;
                    SonicLights.SetIntensity(0.003f);
                }
            }
        }
        
        public void Item_OnGrabEvent(Handle handle, RagdollHand interactor)
        {
            if (handle.gameObject.name != "SlideHandle")
            {
                if (interactor == Player.currentCreature.handRight)
                {
                    grippedRight = true;
                }
                else
                {
                    grippedLeft = true;
                }
            }

            if (sonicType != 11 && sonicType != 2)
            {
                if (coroutineRunning == false)
                {
                    StartCoroutine(extendSonicInput());
                    coroutineRunning = true;
                }
            }

            if (handle.gameObject.name == "SlideHandle")
            {
                jingle.Play();
                handTwo = interactor;
                if (extended)
                {
                    extended = false;
                    StartCoroutine(playAnimation(sonicExtendAnimate, 1));
                }
                else if (sonicType != 11)
                {
                    if (extended == false)
                    {
                        extended = true;
                        StartCoroutine(playAnimation(sonicExtendAnimate, 0));
                    }
                    else
                    {
                        extended = false;
                        StartCoroutine(playAnimation(sonicExtendAnimate, 1));
                    }
                }
            }
            else
            {
                jingle.Play();
                handOne = interactor;
            }
        }

        public void OnUngrabEvent(Handle handle, RagdollHand interactor, bool throwing)
        {
            handTwo = null;
            handOne = null;
            grippedRight = false;
            grippedLeft = false;
            if (handle.gameObject.name != "SlideHandle")
            {
                if (sonicType != 11 && sonicType != 2)
                {
                    StopCoroutine(extendSonicInput());
                    coroutineRunning = false;
                }
            }
        }

        public void Update()
        {
            //if (Player.currentCreature.handRight.playerHand.controlHand.alternateUsePressed)
            //{
            //    Debug.Log("Right AltUse pressed");
            //}
            //if (Player.currentCreature.handLeft.playerHand.controlHand.alternateUsePressed)
            //{
            //    Debug.Log("Left AltUse pressed");
            //}

            //Fix volume
            foreach (AudioSource i in audioSources)
            {
                i.volume = GameManager.options.effectVolume + 1;
            }

            //Double press cooldown
            if (pressCooldown > 0)
            {
                pressCooldown -= Time.deltaTime;
            }
            else
            {
                pressCount = 0;
            }

            //float intensity = 0.001f; /*DEBUG*/
            //if (Input.GetKey(KeyCode.LeftShift))
            //{
            //    intensity = 0.0001f;
            //}
            //if (Input.GetKey(KeyCode.UpArrow))
            //{
            //    foreach (Light i in SonicLights.sonicLights)
            //    {
            //        i.intensity += intensity;
            //    }
            //}
            //if (Input.GetKey(KeyCode.DownArrow))
            //{
            //    foreach (Light i in SonicLights.sonicLights)
            //    {
            //        i.intensity -= intensity;
            //    }
            //}

            if (buttonHold)
            {
                //Activate sonic function
                ToggleSonic(handOne);
                AdjustPitch();

                if (extended && doublePress && sonicType == 12)
                {
                    if (fadein)
                    {
                        if (SonicLights.sonicLights[0].intensity < 0.01)
                        {
                            SonicLights.IncreaseIntensity();
                        }
                        else
                        {
                            fadein = false;
                        }
                    }
                    else
                    {
                        if (SonicLights.sonicLights[0].intensity >= 0.01)
                        {
                            SonicLights.DecreaseIntensity();
                        }
                        else
                        {
                            fadein = true;
                        }
                    }
                }

                //Sonic wave (raycast)
                try
                {
                    sonicOrigin = sonicEnd.transform.position;
                    RaycastHit hit;

                    if (Physics.Raycast(sonicOrigin, sonicEnd.transform.TransformDirection(Vector3.forward), out hit, sonicRange))
                    {
                        if (extended)
                        {
                            if (hit.collider.GetComponentInParent<Creature>())
                            {
                                Creature creature = hit.collider.GetComponentInParent<Creature>();

                                if (creature != Player.currentCreature)
                                {
                                    if (creature.currentHealth != 0)
                                    {
                                        creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                                    }
                                    foreach (RagdollPart ragdoll in creature.gameObject.GetComponentsInChildren<RagdollPart>())
                                    {
                                        ragdoll.rb.AddForce(-hit.normal * 200f);
                                    }
                                }
                            }
                            else
                            {
                                hit.rigidbody.AddForce(-hit.normal * 200f);
                            }
                        }
                        else if (hit.collider.GetComponentInParent<Creature>() && hit.collider.GetComponentInParent<Creature>() != Player.currentCreature)
                        {
                            Creature creature = hit.collider.GetComponentInParent<Creature>();

                            foreach (RagdollHand hand in creature.gameObject.GetComponentsInChildren<RagdollHand>())
                            {
                                hand.UnGrab(true);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }

        private void ToggleSonic (RagdollHand interactor = null)
        {
            if (sonicPlaying)
            {
                if (collider.attachedRigidbody.velocity.magnitude > 1 && sonic.pitch < maxPitch)
                {
                    tempPitch += ((Time.deltaTime * startPitch));
                }
                else if (collider.attachedRigidbody.velocity.magnitude < 1 && sonic.pitch > startPitch)
                {
                    tempPitch -= ((Time.deltaTime * (startPitch + 1)));
                }
            }
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
        }

        private void AdjustPitch()
        {
            if (extended)
            {
                if (sonicType != 10)
                {
                    startPitch = extendPitch;
                }
            }
            else
            {
                startPitch = 1;
            }
            sonic.pitch = tempPitch + startPitch;
        }

        IEnumerator RotatingLights()
        {
            bool pressed = true;
            while (pressed)
            {
                for (int i = 1; i < 5; i++)
                {
                    SonicLights.ToggleSingleLight(i);
                    yield return new WaitForSeconds(0.064f);
                    SonicLights.ToggleSingleLight(i);
                    Debug.Log("Activating " + SonicLights.sonicLights[0].intensity);
                    if (grippedLeft)
                    {
                        pressed = Player.currentCreature.handLeft.playerHand.controlHand.alternateUsePressed;
                        Debug.Log("Left hand AltUse");
                    }
                    if (grippedRight)
                    {
                        pressed = Player.currentCreature.handRight.playerHand.controlHand.alternateUsePressed;
                        Debug.Log("Right hand AltUse");
                    }
                    if (pressed == false)
                    {
                        Debug.Log("Pressed is false");
                        break;
                    }
                }
            }
            SonicLights.TurnOffLights();
        }

        IEnumerator extendSonicInput()
        {
            while (grippedRight | grippedLeft)
            {
                float x; float y; float z;
                float axis = 0f;
                y = slide.localPosition.y; z = slide.localPosition.z;
                if (grippedRight)
                {
                    axis = Player.currentCreature.handRight.playerHand.controlHand.useAxis;
                }
                if (grippedLeft)
                {
                    axis = Player.currentCreature.handLeft.playerHand.controlHand.useAxis;
                }

                if (sonicType != 3 && sonicType != 4)
                {
                    if (axis > 0f)
                    {
                        extended = true;
                        x = 0.0491f * axis + 0.0809f;
                        slide.localPosition = new Vector3(x, y, z);
                        startPitch = 0.5f * axis + 1f;
                    }
                    else
                    {
                        extended = false;
                        slide.localPosition = slidePos;
                    }
                }
                else if (sonicType == 3)
                {
                    if (axis > 0f)
                    {
                        x = -0.007994f * axis + 0.058894f;
                        slide.localPosition = new Vector3(x, y, z);
                    }
                    else
                    {
                        slide.localPosition = slidePos;
                    }
                }
                else
                {
                    if (axis > 0f)
                    {
                        x = -0.029476f * axis + 0.055976f;
                        slide.localPosition = new Vector3(x, y, z);
                    }
                    else
                    {
                        slide.localPosition = slidePos;
                    }
                }
                yield return null;
            }       
        }

        IEnumerator playAnimation(Animator animator, int animation)
        {
            int animHash = 0;
            int boolHash = 0;

            if (animation == 0)
            {
                animHash = extendAnimPathHash;
                boolHash = extendAnimHash;
            }
            if (animation == 1)
            {
                animHash = retractAnimPathHash;
                boolHash = retractAnimHash;
            }

            animator.SetBool(boolHash, true);

            while (animator.GetCurrentAnimatorStateInfo(0).fullPathHash != animHash)
            {
                yield return null;
            }
            float counter = 0;
            float waitTime = animator.GetCurrentAnimatorStateInfo(0).length;

            while (counter < waitTime)
            {
                counter += Time.deltaTime;
                yield return null;
            }

            if (sonicType == 11)
            {
                if (animation == 1)
                {
                    clasp.Play();
                }
            }
            animator.SetBool(boolHash, false);
        }
    }
}
