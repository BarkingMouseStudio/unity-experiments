using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {
	
	/// <summary>
	/// Contols animation for a third person person controller.
	/// </summary>
	[RequireComponent(typeof(Animator))]
	public class CharacterAnimationThirdPerson: CharacterAnimationBase {
		
		[SerializeField] CharacterThirdPerson characterController;
		[SerializeField] float turnSensitivity = 0.2f; // Animator turning sensitivity
		[SerializeField]  float turnSpeed = 5f; // Animator turning interpolation speed
		[SerializeField]  float runCycleLegOffset = 0.2f; // The offset of leg positions in the running cycle
		[Range(0.1f,3f)] [SerializeField] float animSpeedMultiplier = 1; // How much the animation of the character will be multiplied by
		
		private Animator animator;
		private FullBodyBipedIK ik;
		private Vector3 lastForward;
		private const string groundedDirectional = "Grounded Directional", groundedStrafe = "Grounded Strafe";
		
		protected override void Start() {
			base.Start();

			ik = GetComponent<FullBodyBipedIK>();
			animator = GetComponent<Animator>();

			lastForward = transform.forward;
		}
		
		public override Vector3 GetPivotPoint() {
			return animator.pivotPosition;
		}
		
		// Is the Animator playing the grounded animations?
		public override bool animationGrounded {
			get {
				return animator.GetCurrentAnimatorStateInfo(0).IsName(groundedDirectional) || animator.GetCurrentAnimatorStateInfo(0).IsName(groundedStrafe);
			}
		}
		
		// Update the Animator with the current state of the character controller
		void Update() {
			if (Time.deltaTime == 0f) return;
			
			// Jumping
			if (characterController.animState.jump) {
				float runCycle = Mathf.Repeat (animator.GetCurrentAnimatorStateInfo (0).normalizedTime + runCycleLegOffset, 1);
				float jumpLeg = (runCycle < 0 ? 1 : -1) * characterController.animState.moveDirection.z;
				
				animator.SetFloat ("JumpLeg", jumpLeg);
			}
			
			// Calculate the angular delta in character rotation
			float angle = -GetAngleFromForward(lastForward);
			lastForward = transform.forward;
			angle *= turnSensitivity * 0.01f;
			angle = Mathf.Clamp(angle / Time.deltaTime, -1f, 1f);
			
			// Update Animator params
			animator.SetFloat("Turn", Mathf.Lerp(animator.GetFloat("Turn"), angle, Time.deltaTime * turnSpeed));
			animator.SetFloat("Forward", characterController.animState.moveDirection.z);
			animator.SetFloat("Right", characterController.animState.moveDirection.x);
			animator.SetBool("Crouch", characterController.animState.crouch);
			animator.SetBool("OnGround", characterController.animState.onGround);
			animator.SetBool("IsStrafing", characterController.animState.isStrafing);
			
			if (!characterController.animState.onGround) {
				animator.SetFloat ("Jump", characterController.animState.yVelocity);
			}
			
			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector
			if (characterController.animState.onGround && characterController.animState.moveDirection.z > 0f) {
				animator.speed = animSpeedMultiplier;
			} else {
				// but we don't want to use that while airborne
				animator.speed = 1;
			}
		}

		protected override void LateUpdate() {
			base.LateUpdate();

			if (ik == null) return;

			// Rotate the upper body a little bit to world up vector if the character is rotated (for wall-running)
			if (Vector3.Angle(transform.up, Vector3.up) <= 0.01f) return;
			
			Quaternion r = Quaternion.FromToRotation(transform.up, Vector3.up);
			
			RotateEffector(ik.solver.bodyEffector, r, 0.1f);
			RotateEffector(ik.solver.leftShoulderEffector, r, 0.2f);
			RotateEffector(ik.solver.rightShoulderEffector, r, 0.2f);
			RotateEffector(ik.solver.leftHandEffector, r, 0.1f);
			RotateEffector(ik.solver.rightHandEffector, r, 0.1f);
		}
		
		// Rotate an effector from the root of the character
		private void RotateEffector(IKEffector effector, Quaternion rotation, float mlp) {
			Vector3 d1 = effector.bone.position - transform.position;
			Vector3 d2 = rotation * d1;
			Vector3 offset = d2 - d1;
			effector.positionOffset += offset * mlp;
		}
		
		// Call OnAnimatorMove manually on the character controller because it doesn't have the Animator component
		void OnAnimatorMove() {
			characterController.Move(animator.deltaPosition);
		}
	}
}
