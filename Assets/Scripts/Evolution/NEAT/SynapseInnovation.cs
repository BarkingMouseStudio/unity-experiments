namespace NEAT {

  public struct SynapseInnovation {

		public int innovationId;
		public int fromNeuronId;
		public int toNeuronId;

    public SynapseInnovation(int innovationId, int fromNeuronId, int toNeuronId) {
      this.innovationId = innovationId;
      this.fromNeuronId = fromNeuronId;
      this.toNeuronId = toNeuronId;
    }
  }
}
