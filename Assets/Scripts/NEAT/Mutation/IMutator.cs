namespace NEAT {

  public interface IMutator {

    void MutateGenotype(Genotype genotype, MutationResults results);
    void Mutate(Genotype[] genotypes, MutationResults results);
  }
}
