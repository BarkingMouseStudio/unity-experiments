namespace NEAT {

  public class PruneSynapseMutator : IMutator {

    // float p;

    public PruneSynapseMutator(float p) {
      // this.p = p;
    }

    public void Mutate(Genotype genotype, MutationResults results) {
      // var disabledSynapses = genotype.SynapseGenes
      //   .Where(g => !g.isEnabled)
      //   .Where(_ => Random.value < p);
      // var synapseGenes = genotype.SynapseGenes.Except(disabledSynapses);
      // genotype.SynapseGenes = synapseGenes.ToArray();
      //
      // var orphanedNeurons = genotype.NeuronGenes;
      // var neuronGenes = genotype.NeuronGenes.Except(orphanedNeurons);
      // genotype.NeuronGenes = neuronGenes.ToArray();
      //
      // results.orphanedNeurons += orphanedNeurons.Count;
      // results.prunedSynapses += prunedSynapses.Count;
    }
  }
}
