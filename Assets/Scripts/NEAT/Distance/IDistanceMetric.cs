namespace NEAT {

  public interface IDistanceMetric {

    bool MeasureDistance(Genotype p1, Genotype p2, float threshold);
    float MeasureDistance(Genotype p1, Genotype p2);
  }
}
