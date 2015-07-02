namespace NEAT {

  public interface IMutator {

    Genotype Mutate(Genotype genotype, Innovations innovations);
  }
}
