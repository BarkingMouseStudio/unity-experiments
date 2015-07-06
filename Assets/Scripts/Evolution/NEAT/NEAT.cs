using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class NEAT {

    public Population population;
    public List<Species> spp;

    private int generation;
    public int Generation {
      get {
        return generation;
      }
    }

    // TODO: SpeciesManager
    int nextSpeciesId = 0;

    public int desiredSpeciesCount = 10;
    public float distanceThreshold = 30.0f;
    public float distanceThresholdAdjustment = 0.1f;

    const int minimalNeuronCount = 46;
    const float elitism = 0.2f;

    MultipointCrossover crossover = new MultipointCrossover();

    IMutator[] mutators = new IMutator[]{
      new AddNeuronMutator(0.2f),
      new AddSynapseMutator(0.2f),
      new PerturbNeuronMutator(0.2f),
      new PerturbSynapseMutator(0.2f),
      new ReplaceNeuronMutator(0.1f),
      new ReplaceSynapseMutator(0.1f),
    };

    // Update existing species or create a new species using the original, if any
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
          var spNext = new Species(nextSpeciesId++, phenotype.genotype);
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
      var neuronGenes = Enumerable.Range(0, minimalNeuronCount)
        .Select(i => new NeuronGene(innovations.GetAddInitialNeuronInnovationId(i), i))
        .ToList();

      // TODO: Put references somewher else, like NetworkIO
      var synapseGenes = new List<SynapseGene>();
      foreach (var inNeuronId in Reifier.inNeuronIds) {
        foreach (var outNeuronId in Reifier.outNeuronIds) {
          var fromNeuron = neuronGenes[(int)inNeuronId];
          var toNeuron = neuronGenes[(int)outNeuronId];
          var innovationId = innovations.GetAddInitialSynapseInnovationId(fromNeuron.InnovationId, toNeuron.InnovationId);
          synapseGenes.Add(new SynapseGene(innovationId, fromNeuron.InnovationId, toNeuron.InnovationId, true));
        }
      }

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
        distanceThreshold += distanceThreshold * distanceThresholdAdjustment; // Increase threshold
      } else if (spp.Count > desiredSpeciesCount) { // To many
        distanceThreshold -= distanceThreshold * distanceThresholdAdjustment; // Decrease threshold
      }

      // Adjust fitnesses
      foreach (var sp in spp) {
        foreach (var pt in sp.phenotypes) {
          pt.adjustedFitness = pt.fitness / sp.Size;
        }
      }

      var populationSize = this.population.Size;
      var totalAverageFitness = spp.Aggregate(0.0f,
        (total, sp) => total + sp.AverageFitness);

      var sortedSpecies = spp.OrderBy(sp => sp.AverageFitness);

      int totalOffspringCount = 0;

      // TODO: Species manager
      var nextPopulation = sortedSpecies.Aggregate(new List<Genotype>(populationSize), (genotypes, sp) => {
        var offspringCount = Mathf.CeilToInt(
          (sp.AverageFitness / totalAverageFitness) * (float)populationSize
        );
        totalOffspringCount += offspringCount;
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
