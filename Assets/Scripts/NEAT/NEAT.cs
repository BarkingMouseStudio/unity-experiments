using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  // TODO: Builder-pattern
  // TODO: Species manager
  // TODO: Single-file configuration
  public class NEAT {

    public Population population;
    public List<Species> spp;

    private int generation;
    public int Generation {
      get {
        return generation;
      }
    }

    int nextSpeciesId = 0;

    public int desiredSpeciesCount = 10;
    public float distanceThreshold = 100.0f;
    public float distanceThresholdAdjustment = 0.1f;

    const float elitism = 0.25f;

    MultipointCrossover crossover = new MultipointCrossover();

    IMutator[] mutators = new IMutator[]{
      new AddNeuronMutator(0.2f),
      new AddSynapseMutator(0.2f),
      new PerturbNeuronMutator(0.25f),
      new PerturbSynapseMutator(0.25f),
      new ReplaceNeuronMutator(0.15f),
      new ReplaceSynapseMutator(0.15f),
    };

    // Update existing species or create a new species using the original, if any
    // TODO: This allocs a ton
    public List<Species> Speciate(List<Species> spp, List<Phenotype> phenotypes) {
      // Begin new species
      var sppNext = spp.Select(sp => sp.Next()).ToList();

      // Place genotypes
      foreach (var phenotype in phenotypes) {
        bool foundSpecies = false;

        foreach (var sp in sppNext) {
          var distance = sp.Distance(phenotype);
          if (distance < distanceThreshold) { // Does the phenotype belong to this species
            sp.phenotypes.Add(phenotype);
            foundSpecies = true;
            break;
          }
        }

        // Create a new species for the phenotype if necessary
        if (!foundSpecies) {
          var spNext = new Species(nextSpeciesId, phenotype.genotype);
          nextSpeciesId++;

          spNext.phenotypes.Add(phenotype);
          sppNext.Add(spNext);
        }
      }

      // Prune empty species
      var dead = sppNext.Where(sp => sp.phenotypes.Count == 0);
      return sppNext.Except(dead).ToList();
    }

    public NEAT(int populationSize) {
      var innovations = new Innovations();
      var protoGenotype = GetProtoGenotype(innovations);

      this.spp = new List<Species>();
      this.population = new Population(populationSize, protoGenotype, innovations);
    }

    public static Genotype GetProtoGenotype(Innovations innovations) {
      var neuronGenes = Enumerable.Range(0, NetworkIO.inNeuronCount + NetworkIO.outNeuronCount)
        .Select(i => NeuronGene.Random(innovations.GetAddInitialNeuronInnovationId(i), i))
        .ToList();

      var synapseGenes = new List<SynapseGene>();
      // foreach (var inNeuronId in NetworkIO.inNeuronIds) {
      //   foreach (var outNeuronId in NetworkIO.outNeuronIds) {
      //     var fromNeuron = neuronGenes[(int)inNeuronId];
      //     var toNeuron = neuronGenes[(int)outNeuronId];
      //     var innovationId = innovations.GetAddInitialSynapseInnovationId(fromNeuron.InnovationId, toNeuron.InnovationId);
      //     synapseGenes.Add(SynapseGene.Random(innovationId, fromNeuron.InnovationId, toNeuron.InnovationId, true));
      //   }
      // }

      return new Genotype(neuronGenes, synapseGenes);
    }

    public List<Genotype> Sample() {
      return population.genotypes;
    }

    // Evaluate population
    public void Update(List<Phenotype> phenotypes) {
      // Update species and distance threshold
      spp = Speciate(spp, phenotypes);

      if (spp.Count < desiredSpeciesCount) { // Too few
        distanceThreshold -= distanceThreshold * distanceThresholdAdjustment; // Decrease threshold
      } else if (spp.Count > desiredSpeciesCount) { // To many
        distanceThreshold += distanceThreshold * distanceThresholdAdjustment; // Increase threshold
      }

      var populationSize = this.population.Size;

      // Adjust fitnesses
      foreach (var sp in spp) {
        var multiplier = 1.0f + ((float)sp.Size / (float)populationSize);
        foreach (var pt in sp.phenotypes) {
          pt.adjustedFitness = pt.fitness * multiplier;
        }
      }

      var totalAverageFitness = spp.Aggregate(0.0f,
        (total, sp) => total + sp.AverageFitness);
      var averageFitness = totalAverageFitness / spp.Count;

      var sortedSpecies = spp.OrderBy(sp => sp.AverageFitness);

      var bestSpecies = sortedSpecies.First();
      var bestPhenotype = bestSpecies.phenotypes.OrderBy(pt => pt.adjustedFitness).First();

      Debug.LogFormat("Species: {0} (Threshold: {1}), Average: {2}",
        spp.Count, distanceThreshold, averageFitness);

      // TODO: This allocates a bunch
      var nextPopulation = sortedSpecies.Aggregate(new List<Genotype>(populationSize), (genotypes, sp) => {
        var offspringCount = Mathf.CeilToInt(
          (sp.AverageFitness / totalAverageFitness) * (float)populationSize
        );
        // TODO: Alloc
        var offspring = sp.Reproduce(offspringCount, elitism, crossover, mutators, population.innovations);
        genotypes.AddRange(offspring);
        return genotypes;
      }, (offspring) => {
        offspring = offspring.Take(populationSize).ToList();
        return new Population(offspring, population.innovations);
      });

      this.population = nextPopulation;
      this.generation++;
    }
  }
}
