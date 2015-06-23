// Strategy pattern to vary algorithm with an ask/tell inteface
interface IEvolutionaryAlgorithm {

  int Generation { get; }

  // Draws a population to be evaluated from the EA.
  CommonGenotype[] Sample(int count);

  // Update with the evaluated population's fitness.
  Tuple<float, int, int> Update(float[] fitness);
}
