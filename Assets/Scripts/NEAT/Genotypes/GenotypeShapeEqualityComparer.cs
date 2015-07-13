using System.Collections;
using System.Collections.Generic;

namespace NEAT {

  public class GenotypeShapeEqualityComparer : IEqualityComparer<Genotype> {

    public bool Equals(Genotype a, Genotype b) {
      if (a.NeuronCount != b.NeuronCount) {
        return false;
      }

      if (a.SynapseCount != b.SynapseCount) {
        return false;
      }

      return true;
    }

    public int GetHashCode(Genotype genotype) {
      int hash = 19;
      hash = hash * 31 + genotype.NeuronCount.GetHashCode();
      hash = hash * 31 + genotype.SynapseCount.GetHashCode();
      return hash;
    }
  }
}
