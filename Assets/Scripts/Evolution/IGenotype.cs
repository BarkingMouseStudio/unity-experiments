using UnityEngine;
using System.Collections;

// Multiple different genotypes can be used but are flattened
// to a common genotype.
interface IGenotype {

	// Handles flattening genotype representation to explicit representation.
	CommonGenotype ToCommonGenotype();
}
