// Strategy pattern to vary algorithm
interface IEvolutionaryAlgorithm {

  // Draws a population to be evaluated from the EA.
  IGenotype[] Sample(int count);

  // Records the evaluated population's fitness.
  void Record(float[] fitness);
}
