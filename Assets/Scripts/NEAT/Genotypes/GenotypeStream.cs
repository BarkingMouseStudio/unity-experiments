using System.Collections;
using System.Collections.Generic;

namespace NEAT {

  public class GenotypeStream : IEnumerable<Genotype> {

    private readonly Genotype protoGenotype;

    public GenotypeStream(Genotype protoGenotype) {
      this.protoGenotype = protoGenotype;
    }

    public IEnumerator<Genotype> GetEnumerator() {
      while (true) {
        yield return Genotype.FromPrototype(protoGenotype);
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}
