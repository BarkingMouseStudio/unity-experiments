using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public abstract class Mutator : IMutator {

    public abstract void MutateGenotype(Genotype genotype, MutationResults results);

    public virtual void Mutate(Genotype[] genotypes, MutationResults results) {
      for (int i = 0; i < genotypes.Length; i++) {
        MutateGenotype(genotypes[i], results);
      }
    }
  }
}
