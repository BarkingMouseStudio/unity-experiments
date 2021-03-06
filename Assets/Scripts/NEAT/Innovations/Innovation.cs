namespace NEAT {

  public struct Innovation {

    public InnovationType type;
		public int innovationId; // Its unique innovation id
		public int fromNeuronId; // Original sending neuron before split
		public int toNeuronId; // Original receiving neuron before split
    public int oldSynapseInnovationId; // The id of the synapse gene it split

    public Innovation(InnovationType type, int innovationId, int fromNeuronId, int toNeuronId, int oldSynapseInnovationId) {
      this.type = type;
  		this.innovationId = innovationId;
  		this.fromNeuronId = fromNeuronId;
  		this.toNeuronId = toNeuronId;
      this.oldSynapseInnovationId = oldSynapseInnovationId;
    }
  }
}
