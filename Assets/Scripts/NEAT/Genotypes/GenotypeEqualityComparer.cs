using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class GenotypeEqualityComparer : IEqualityComparer<Genotype> {

    public bool Equals(Genotype a, Genotype b) {
      if (!a.NeuronGenes.SequenceEqual(b.NeuronGenes)) {
        return false;
      }

      if (!a.SynapseGenes.SequenceEqual(b.SynapseGenes)) {
        return false;
      }

      return true;
    }

    public int GetHashCode(Genotype genotype) {
      int hash = 19;
      foreach (var neuronGene in genotype.NeuronGenes) {
        hash = hash * 31 + neuronGene.GetHashCode();
      }
      foreach (var synapseGene in genotype.SynapseGenes) {
        hash = hash * 31 + synapseGene.GetHashCode();
      }
      return hash;
    }
  }
}
