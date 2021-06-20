using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
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
        GameObject sonicEnd;
        Vector3 sonicOrigin;
        float sonicRange = 200;

        Animator sonicExtendAnimate;
        int extendAnimPathHash;
        int retractAnimPathHash;
        int extendAnimHash;
        int retractAnimHash;

        Light sonicLight;

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


        protected void Awake()
        {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<SonicModule>();
            sonicType = module.sonicType;

            //Sounds setup
            if (!String.IsNullOrEmpty(module.sonic)) sonic = item.GetCustomReference(module.sonic).GetComponent<AudioSource>();
            startPitch = sonic.pitch;
            tempPitch = startPitch;
            extendPitch = startPitch * 1.5f;
            maxPitch = startPitch * 2;

            if (!String.IsNullOrEmpty(module.extend)) extend = item.GetCustomReference(module.extend).GetComponent<AudioSource>();
            if (!String.IsNullOrEmpty(module.unClick)) unClick = item.GetCustomReference(module.unClick).GetComponent<AudioSource>();
            if (!String.IsNullOrEmpty(module.click)) click = item.GetCustomReference(module.click).GetComponent<AudioSource>();
            if (!String.IsNullOrEmpty(module.clasp)) clasp = item.GetCustomReference(module.clasp).GetComponent<AudioSource>();
            if (!String.IsNullOrEmpty(module.jingle)) jingle = item.GetCustomReference(module.jingle).GetComponent<AudioSource>();

            //Raycast setup
            sonicEnd = item.GetCustomReference("sonicEnd").gameObject;
            sonicOrigin = sonicEnd.transform.position;

            //Light setup
            sonicLight = item.GetCustomReference("sonicLight").GetComponent<Light>();
            sonicLight.enabled = false;

            //Animation setup
            if (!String.IsNullOrEmpty(module.sonicExtendAnimate)) sonicExtendAnimate = item.GetCustomReference(module.sonicExtendAnimate).GetComponent<Animator>();
            if (!String.IsNullOrEmpty(module.extendHash)) extendAnimHash = Animator.StringToHash(module.extendHash);
            if (!String.IsNullOrEmpty(module.retractHash)) retractAnimHash = Animator.StringToHash(module.retractHash);
            if (!String.IsNullOrEmpty(module.extendAnimPathHash)) extendAnimPathHash = Animator.StringToHash(module.extendAnimPathHash);
            if (!String.IsNullOrEmpty(module.retractAnimPathHash)) retractAnimPathHash = Animator.StringToHash(module.retractAnimPathHash);

            //Collider
            collider = item.GetCustomReference("collider").GetComponent<CapsuleCollider>();


            //Event subscribers
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnHeldActionEvent += OnHeldAction;
            item.OnUngrabEvent += OnUngrabEvent;

        }

        public void OnHeldAction(RagdollHand interactor, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
            {
                AdjustPitch();
                buttonHold = true;
                click.Play();
                sonic.Play();
                sonicPlaying = true;
                sonicLight.enabled = true;
            }
            if (action == Interactable.Action.AlternateUseStop)
            {
                buttonHold = false;
                sonic.Stop();
                sonicPlaying = false;
                unClick.Play();
                sonicLight.enabled = false;
            }
            if (action == Interactable.Action.UseStart)
            {
                if (!extended)
                {
                    extended = true;
                    if (sonicType == 11)
                    {
                        extend.Play();
                        StartCoroutine(playAnimation(sonicExtendAnimate, 0));
                    }
                    else
                    {
                        sonicExtendAnimate.SetBool(retractAnimHash, false);
                        sonicExtendAnimate.SetBool(extendAnimHash, true);
                        AdjustPitch();
                    }

                    PlayerControl.GetHand(interactor.playerHand.side).HapticShort(2f);
                }
            }
            if (action == Interactable.Action.UseStop)
            {
                if (sonicType == 10)
                {
                    extended = false;
                    sonicExtendAnimate.SetBool(extendAnimHash, false);
                    sonicExtendAnimate.SetBool(retractAnimHash, true);
                    AdjustPitch();
                }
            }
        }
        
        public void Item_OnGrabEvent(Handle handle, RagdollHand interactor)
        {
            if (handle.gameObject.name == "SlideHandle")
            {
                jingle.Play();
                handTwo = interactor;
                if (extended)
                {
                    extended = false;
                    StartCoroutine(playAnimation(sonicExtendAnimate, 1));
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
        }

        public void Update()
        {
            if (buttonHold)
            {
                //Activate sonic function
                ToggleSonic(handOne);

                //Sonic wave (raycast)
                sonicEnd = item.GetCustomReference("sonicEnd").gameObject;
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
                                creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                                foreach (RagdollPart ragdoll in creature.gameObject.GetComponentsInChildren<RagdollPart>())
                                {
                                    ragdoll.rb.isKinematic = false;
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
        }

        private void ToggleSonic (RagdollHand interactor = null)
        {
            if (sonicPlaying)
            {
                if (collider.attachedRigidbody.velocity.magnitude > 1 && sonic.pitch < maxPitch)
                {
                    sonic.pitch += ((Time.deltaTime * startPitch));
                }
                else if (collider.attachedRigidbody.velocity.magnitude < 1 && sonic.pitch > tempPitch)
                {
                    sonic.pitch -= ((Time.deltaTime * (startPitch + 1)));
                }
            }
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
        }

        private void AdjustPitch()
        {
            if (extended)
            {
                tempPitch = extendPitch;
                sonic.pitch = extendPitch;
                maxPitch = startPitch * 3;
            }
            else
            {
                tempPitch = startPitch;
                sonic.pitch = startPitch;
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
                AdjustPitch();
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
