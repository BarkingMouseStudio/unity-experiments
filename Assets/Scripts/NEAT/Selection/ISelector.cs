  using System.Collections;
  using System.Collections.Generic;

  namespace NEAT {

    public interface ISelector {

      Genotype[] Select(Specie[] species, int count);
    }
  }
