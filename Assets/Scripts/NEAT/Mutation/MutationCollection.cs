namespace NEAT {

  public class MutationCollection {

    private readonly IMutator[] mutators;

    public MutationCollection(params IMutator[] mutators) {
      this.mutators = mutators;
    }

    public MutationResults Mutate(Genotype[] genotypes) {
      var results = new MutationResults();
      foreach (var mutator in mutators) {
        mutator.Mutate(genotypes, results);
      }
      return results;
    }
  }
}
