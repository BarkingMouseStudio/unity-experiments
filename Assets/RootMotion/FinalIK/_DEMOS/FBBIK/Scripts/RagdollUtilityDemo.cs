using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

public class RagdollUtilityDemo : MonoBehaviour {

	public RagdollUtility ragdollUtility;

	void OnGUI() {
		GUILayout.Label(" Press R to switch to ragdoll. " +
		                "\n Weigh in one of the FBBIK effectors to make kinematic changes to the ragdoll pose." +
		                "\n A to blend back to animation");
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.R)) ragdollUtility.EnableRagdoll();
		if (Input.GetKeyDown(KeyCode.A)) ragdollUtility.DisableRagdoll();
	}

}
