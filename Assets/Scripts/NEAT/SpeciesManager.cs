// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
//
// namespace NEAT {
//
//   // TODO: Builder-pattern
//   // TODO: Species manager
//   // TODO: Single-file configuration
//   public class NEAT {
//
//     public List<Species> spp;
//
//     int nextSpeciesId = 0;
//
//     public int desiredSpeciesCount = 10;
//     public float distanceThreshold = 100.0f;
//     public float distanceThresholdAdjustment = 0.1f;
//
//     public Ecosystem() {
//       this.spp = new List<Species>();
//     }
//
//     // Update existing species or create a new species using the original, if any
//     // TODO: This allocs a ton
//     public List<Species> Speciate(List<Species> spp, List<Phenotype> phenotypes) {
//       // Begin new species
//       var sppNext = spp.Select(sp => sp.Next()).ToList();
//       var totalAverageDistance = 0.0f;
//
//       // Place genotypes
//       foreach (var phenotype in phenotypes) {
//         bool foundSpecies = false;
//
//         float totalDistance = 0.0f;
//         int attempts = 0;
//
//         foreach (var sp in sppNext) {
//           var distance = sp.Distance(phenotype);
//           totalDistance += distance;
//           attempts++;
//
//           if (distance < distanceThreshold) { // Does the phenotype belong to this species
//             sp.phenotypes.Add(phenotype);
//             foundSpecies = true;
//             break;
//           }
//         }
//
//         totalAverageDistance += totalDistance / attempts;
//
//         // Create a new species for the phenotype if necessary
//         if (!foundSpecies) {
//           var spNext = new Species(nextSpeciesId, phenotype.genotype);
//           nextSpeciesId++;
//
//           spNext.phenotypes.Add(phenotype);
//           sppNext.Add(spNext);
//         }
//       }
//
//       if (spp.Count == 1) {
//         distanceThreshold = totalAverageDistance / phenotypes.Count;
//       } else if (spp.Count < desiredSpeciesCount) { // Too few
//         distanceThreshold -= distanceThreshold * distanceThresholdAdjustment; // Decrease threshold
//       } else if (spp.Count > desiredSpeciesCount) { // To many
//         distanceThreshold += distanceThreshold * distanceThresholdAdjustment; // Increase threshold
//       }
//
//       // Prune empty species
//       var dead = sppNext.Where(sp => sp.phenotypes.Count == 0);
//       return sppNext.Except(dead).ToList();
//     }
//
//     // Evaluate population
//     public void Update(List<Phenotype> phenotypes) {
//       // Update species and distance threshold
//       spp = Speciate(spp, phenotypes);
//
//       var populationSize = this.population.Size;
//
//       // Adjust fitnesses
//       foreach (var sp in spp) {
//         var multiplier = 1.0f + ((float)sp.Size / (float)populationSize);
//         foreach (var pt in sp.phenotypes) {
//           pt.adjustedFitness = pt.fitness * multiplier;
//         }
//       }
//
//       var totalAverageFitness = spp.Aggregate(0.0f,
//         (total, sp) => total + sp.AverageFitness);
//       var averageFitness = totalAverageFitness / spp.Count;
//
//       var sortedSpecies = spp.OrderBy(sp => sp.AverageFitness);
//
//       Debug.LogFormat("Species: {0} (Threshold: {1}), Average: {2}",
//         spp.Count, distanceThreshold, averageFitness);
//
//       // TODO: This allocates a bunch
//       var nextPopulation = sortedSpecies.Aggregate(new List<Genotype>(populationSize), (genotypes, sp) => {
//         var offspringCount = Mathf.CeilToInt(
//           (sp.AverageFitness / totalAverageFitness) * (float)populationSize
//         );
//         // TODO: Alloc
//         var offspring = sp.Reproduce(offspringCount, elitism, crossover, mutators, population.innovations);
//         genotypes.AddRange(offspring);
//         return genotypes;
//       }, (offspring) => {
//         offspring = offspring.Take(populationSize).ToList();
//         return new Population(offspring, population.innovations);
//       });
//
//       this.population = nextPopulation;
//       this.generation++;
//     }
//   }
// }
