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

        RagdollHand handOne;
        RagdollHand handTwo;
        bool buttonHold;
        bool extended;
        GameObject sonicEnd;
        Vector3 sonicOrigin;
        float sonicRange = 100;

        Animator sonicExtendAnimate;
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

            //Sounds setup
            sonic = item.GetCustomReference("soundSonic").GetComponent<AudioSource>();
            startPitch = sonic.pitch;
            tempPitch = startPitch;
            extendPitch = startPitch * 1.5f;
            maxPitch = startPitch * 2;
            extend = item.GetCustomReference("soundExtend").GetComponent<AudioSource>();
            unClick = item.GetCustomReference("soundUnclick").GetComponent<AudioSource>();
            click = item.GetCustomReference("soundClick").GetComponent<AudioSource>();
            clasp = item.GetCustomReference("soundClasp").GetComponent<AudioSource>();
            jingle = item.GetCustomReference("soundJingle").GetComponent<AudioSource>();

            //Raycast setup
            sonicEnd = item.GetCustomReference("sonicEnd").gameObject;
            sonicOrigin = sonicEnd.transform.position;

            //Light setup
            sonicLight = item.GetCustomReference("sonicLight").GetComponent<Light>();
            sonicLight.enabled = false;

            //Animation setup
            sonicExtendAnimate = item.GetCustomReference("sonicExtendAnimate").GetComponent<Animator>();
            extendAnimHash = Animator.StringToHash("Base Layer.sonicExtendAnimate");
            retractAnimHash = Animator.StringToHash("Base Layer.sonicRetract");

            //Collider
            collider = item.GetCustomReference("collider").GetComponent<CapsuleCollider>();


            //Event subscribers
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnHeldActionEvent += OnHeldAction;
            //item.OnUngrabEvent += OnUngrabEvent;

        }

        public void OnHeldAction(RagdollHand interactor, Handle handle, Interactable.Action action)
        {
                if (action == Interactable.Action.AlternateUseStart)
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
                        extend.Play();
                        extended = true;
                        StartCoroutine(playAnimation(sonicExtendAnimate, "Extending", "sonicExtendAnimate"));
                        PlayerControl.GetHand(interactor.playerHand.side).HapticShort(2f);
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
                    StartCoroutine(playAnimation(sonicExtendAnimate, "Retracting", "sonicRetract"));
                }
            }
            else
            {
                jingle.Play();
                handOne = interactor;
            }
        }

        public void Update()
        {
            if (buttonHold)
            {
                //Activate sonic function
                ToggleSonic(handOne);

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
                            if (!creature.isPlayer)
                            {
                                foreach (RagdollPart ragdoll in creature.gameObject.GetComponentsInChildren<RagdollPart>())
                                {
                                    ragdoll.rb.isKinematic = false;
                                    ragdoll.rb.AddForce(-hit.normal * 200f);
                                }
                            }
                        }


                    }
                    else if (hit.collider.GetComponentInParent<Creature>())
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
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);

        }

        IEnumerator playAnimation(Animator animator, String animation, String animationName)
        {
            int animHash = 0;
            int boolHash = 0;

            if (animationName == "sonicExtendAnimate")
            {
                animHash = extendAnimHash;
            }
            if (animationName == "sonicRetract")
            {
                animHash = retractAnimHash;
            }

            boolHash = Animator.StringToHash(animation);
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

            if (animationName == "sonicRetract")
            {
                clasp.Play();
            }

            animator.SetBool(boolHash, false);
        }
    }
}
