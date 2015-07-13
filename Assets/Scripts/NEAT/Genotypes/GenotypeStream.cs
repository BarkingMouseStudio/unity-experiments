using System.Collections;
using System.Collections.Generic;

namespace NEAT {

  public class GenotypeStream : IEnumerable<Genotype> {

    private readonly InnovationCollection innovations;
    private readonly Genotype protoGenotype;

    public GenotypeStream(Genotype protoGenotype, InnovationCollection innovations) {
      this.protoGenotype = protoGenotype;
      this.innovations = innovations;
    }

    public IEnumerator<Genotype> GetEnumerator() {
      while (true) {
        yield return Genotype.FromPrototype(protoGenotype, innovations);
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}
