using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	// Extends the default Animator controller for 3rd person view to add IK
	[RequireComponent(typeof(AimIK))]
	[RequireComponent(typeof(FullBodyBipedIK))]
	public class AnimatorController3rdPersonIK: AnimatorController3rdPerson {

		// For debugging only
		void OnGUI() {
			GUILayout.Label("Press F to switch Final IK on/off");
			GUILayout.Label("Press C to toggle between 3rd person/1st person camera");
		}

		[SerializeField] bool useIK = true;
		[SerializeField] Camera firstPersonCam; // The FPS camera
		[Range(0f, 1f)] public float headLookWeight = 1f;
		public Vector3 gunHoldOffset;
		public Vector3 leftHandOffset;

		// The IK components
		private AimIK aim;
		private FullBodyBipedIK ik;

		private Vector3 headLookAxis;
		private Quaternion fpsCamDefaultRot;
		private Vector3 leftHandPosRelToRightHand;
		private Quaternion leftHandRotRelToRightHand;
		private Vector3 aimTarget;

		protected override void Start() {
			base.Start();
			
			// Find the IK components
			aim = GetComponent<AimIK>();
			ik = GetComponent<FullBodyBipedIK>();
			
			// Disable the IK components to manage their updating
			aim.Disable();
			ik.Disable();

			// Presuming head is rotated towards character forward at Start
			headLookAxis = ik.references.head.InverseTransformVector(ik.references.root.forward);
			
			fpsCamDefaultRot = firstPersonCam.transform.localRotation;

			// Enable the upper-body aiming pose
			animator.SetLayerWeight(1, 1f);
		}

		public override void Move(Vector3 moveInput, bool isMoving, Vector3 faceDirection, Vector3 aimTarget) {
			base.Move(moveInput, isMoving, faceDirection, aimTarget);

			// Snatch the aim target from the Move call, it will be used by AimIK (Move is called by CharacterController3rdPerson that controls the actual motion of the character)
			this.aimTarget = aimTarget;

			// Toggle FPS/3PS
			if (Input.GetKeyDown(KeyCode.C)) firstPersonCam.enabled = !firstPersonCam.enabled;
			
			// Toggle IK
			if (Input.GetKeyDown(KeyCode.F)) useIK = !useIK;
			if (!useIK) return;

			// IK procedures, make sure this updates AFTER the camera is moved/rotated
			// Sample something from the current pose of the character
			Read();
			
			// AimIK pass
			AimIK();
			
			// FBBIK pass - put the left hand back to where it was relative to the right hand before AimIK solved
			FBBIK();
			
			// Rotate the head to look at the aim target
			HeadLookAt(aimTarget);
			
			// Remove FPS camera banking
			if (firstPersonCam.enabled) StabilizeFPSCamera();
		}

		private void Read() {
			// Remember the position and rotation of the left hand relative to the right hand
			leftHandPosRelToRightHand = ik.references.rightHand.InverseTransformPoint(ik.references.leftHand.position);
			leftHandRotRelToRightHand = Quaternion.Inverse(ik.references.rightHand.rotation) * ik.references.leftHand.rotation;
		}

		private void AimIK() {
			// Set AimIK target position and update
			aim.solver.IKPosition = aimTarget;
			aim.solver.Update(); // Update AimIK
		}

		// Positioning the left hand on the gun after aiming has finished
		private void FBBIK() {
			// Store the current rotation of the right hand
			Quaternion rightHandRotation = ik.references.rightHand.rotation;

			// Put the left hand back to where it was relative to the right hand before AimIK solved
			Vector3 leftHandTarget = ik.references.rightHand.TransformPoint(leftHandPosRelToRightHand);
			ik.solver.leftHandEffector.positionOffset += leftHandTarget - ik.references.leftHand.position;

			// Offsetting hands, you might need that to support multiple weapons with the same aiming pose
			Vector3 rightHandOffset = ik.references.rightHand.rotation * gunHoldOffset;
			ik.solver.rightHandEffector.positionOffset += rightHandOffset;
			ik.solver.leftHandEffector.positionOffset += rightHandOffset + ik.references.rightHand.rotation * leftHandOffset;

			// Update FBBIK
			ik.solver.Update();
			
			// Rotating the hand bones after IK has finished
			ik.references.rightHand.rotation = rightHandRotation;
			ik.references.leftHand.rotation = rightHandRotation * leftHandRotRelToRightHand;
		}

		// Rotating the head to look at the target
		private void HeadLookAt(Vector3 lookAtTarget) {
			Quaternion headRotationTarget = Quaternion.FromToRotation(ik.references.head.rotation * headLookAxis, lookAtTarget - ik.references.head.position);
			ik.references.head.rotation = Quaternion.Lerp(Quaternion.identity, headRotationTarget, headLookWeight) * ik.references.head.rotation;
		}

		// Removes camera banking
		private void StabilizeFPSCamera() {
			// Rotate the FPS camera to its default rotation
			firstPersonCam.transform.localRotation = fpsCamDefaultRot;
			
			// Get the world up ortho-normalized to camera forward
			Vector3 normal = firstPersonCam.transform.forward;
			Vector3 worldUp = Vector3.up;
			Vector3.OrthoNormalize(ref normal, ref worldUp);
			
			// The rotation that rotates camera up to world up.
			Quaternion fromTo = Quaternion.FromToRotation(firstPersonCam.transform.up, worldUp);
			
			// Fade out this effect when looking directly up/down to avoid singularity problems
			float dot = Vector3.Dot(transform.forward, firstPersonCam.transform.forward);
			
			// Twist the camera so that it's up vector will always be pointed towards world up
			firstPersonCam.transform.rotation = Quaternion.Lerp(Quaternion.identity, fromTo, dot) * firstPersonCam.transform.rotation;
		}
	}
}
