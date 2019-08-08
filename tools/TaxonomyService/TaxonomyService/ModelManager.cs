using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using log4net;
using TTI.TTF.Taxonomy.Controllers;
using TTI.TTF.Taxonomy.Model;
using TTI.TTF.Taxonomy.Model.Artifact;
using TTI.TTF.Taxonomy.Model.Core;
using Enum = System.Enum;

namespace TTI.TTF.Taxonomy
{
	public static class ModelManager
	{
		private static readonly ILog _log;
		private static Model.Taxonomy Taxonomy { get; set; }

		static ModelManager()
		{
			Utils.InitLog();
			_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		}

		internal static void Init()
		{
			_log.Info("ModelManager Init");
			Taxonomy = TaxonomyController.Load();
			TaxonomyCache.SaveToCache(Taxonomy.Version.Id, Taxonomy, DateTime.Now.AddDays(1));
		}

		internal static Model.Taxonomy GetFullTaxonomy(TaxonomyVersion version)
		{
			_log.Info("GetFullTaxonomy version " + version.Version);
			return Taxonomy.Version.Version == version.Version ? Taxonomy : TaxonomyCache.GetFromCache(version.Version);
		}
		
		internal static Model.Taxonomy RefreshTaxonomy(TaxonomyVersion version)
		{
			_log.Info("RefreshTaxonomy version " + version.Version);
			Taxonomy = TaxonomyController.Load();
			TaxonomyCache.SaveToCache(Taxonomy.Version.Version, Taxonomy, DateTime.Now.AddDays(1));
			return Taxonomy;
		}

		public static Base GetBaseArtifact(ArtifactSymbol symbol)
		{
			_log.Info("GetBaseArtifact Symbol: " + symbol);
			if (!string.IsNullOrEmpty(symbol.Id))
			{
				return GetArtifactById<Base>(symbol.Id);
			}

			if (!string.IsNullOrEmpty(symbol.Tooling))
			{
				return Taxonomy.BaseTokenTypes
					.SingleOrDefault(e => e.Value.Artifact.ArtifactSymbol.Tooling == symbol.Tooling).Value;
			}

			return new Base();
		}

		public static Behavior GetBehaviorArtifact(ArtifactSymbol symbol)
		{
			_log.Info("GetBehaviorArtifact Symbol: " + symbol);
			if (!string.IsNullOrEmpty(symbol.Id))
			{
				return GetArtifactById<Behavior>(symbol.Id);
			}

			if (!string.IsNullOrEmpty(symbol.Tooling))
			{
				return Taxonomy.Behaviors
					.SingleOrDefault(e => e.Value.Artifact.ArtifactSymbol.Tooling == symbol.Tooling).Value;
			}

			return new Behavior();
		}

		public static BehaviorGroup GetBehaviorGroupArtifact(ArtifactSymbol symbol)
		{
			_log.Info("GetBehaviorGroupArtifact Symbol: " + symbol);
			if (!string.IsNullOrEmpty(symbol.Id))
			{
				return GetArtifactById<BehaviorGroup>(symbol.Id);
			}

			if (!string.IsNullOrEmpty(symbol.Tooling))
			{
				return Taxonomy.BehaviorGroups
					.SingleOrDefault(e => e.Value.Artifact.ArtifactSymbol.Tooling == symbol.Tooling).Value;
			}

			return new BehaviorGroup();
		}

		public static PropertySet GetPropertySetArtifact(ArtifactSymbol symbol)
		{
			_log.Info("GetPropertySetArtifact Symbol: " + symbol);
			if (!string.IsNullOrEmpty(symbol.Id))
			{
				return GetArtifactById<PropertySet>(symbol.Id);
			}

			if (!string.IsNullOrEmpty(symbol.Tooling))
			{
				return Taxonomy.PropertySets
					.SingleOrDefault(e => e.Value.Artifact.ArtifactSymbol.Tooling == symbol.Tooling).Value;
			}

			return new PropertySet();
		}

		public static TemplateFormula GetTemplateFormulaArtifact(ArtifactSymbol formula)
		{
			_log.Info("GetTemplateFormulaArtifact: " + formula);
			if (!string.IsNullOrEmpty(formula.Id))
			{
				return GetArtifactById<TemplateFormula>(formula.Id);
			}

			if (!string.IsNullOrEmpty(formula.Tooling))
			{
				return Taxonomy.TemplateFormulas
					.SingleOrDefault(e => e.Value.Artifact.ArtifactSymbol.Tooling == formula.Tooling).Value;
			}

			return new TemplateFormula();
		}
		
		public static TemplateDefinition GetTemplateDefinitionArtifact(ArtifactSymbol formula)
		{
			_log.Info("GetTemplateDefinitionArtifact: " + formula);
			if (!string.IsNullOrEmpty(formula.Id))
			{
				return GetArtifactById<TemplateDefinition>(formula.Id);
			}

			if (!string.IsNullOrEmpty(formula.Tooling))
			{
				return Taxonomy.TemplateDefinitions
					.SingleOrDefault(e => e.Value.Artifact.ArtifactSymbol.Tooling == formula.Tooling).Value;
			}

			return new TemplateDefinition();
		}

		public static QueryResult GetArtifactsOfType(QueryOptions options)
		{
			var result = new QueryResult
			{
				ArtifactType = options.ArtifactType,
				FirstItemIndex = options.LastItemIndex - 1
			};
			try
			{
				switch (options.ArtifactType)
				{
					case ArtifactType.Base:
						var bases = new Bases();
						bases.Base.AddRange(Taxonomy.BaseTokenTypes
							.Values); //unlikely to be more than a handful of these.	
						result.ArtifactCollection = Any.Pack(bases);
						result.FirstItemIndex = 0;
						result.LastItemIndex = bases.Base.Count - 1;
						result.TotalItemsInCollection = Taxonomy.BaseTokenTypes.Count;
						break;
					case ArtifactType.Behavior:
						var behaviors = new Behaviors();
						if (Taxonomy.Behaviors.Count <= options.MaxItemReturn
						) //if max return is greater than the total count, just send back all of them.
						{
							behaviors.Behavior.AddRange(Taxonomy.Behaviors.Values);
							result.FirstItemIndex = 0;
							result.LastItemIndex = behaviors.Behavior.Count - 1;
						}
						else
						{
							behaviors.Behavior.AddRange(behaviors.Behavior.ToList()
								.GetRange(options.LastItemIndex, options.MaxItemReturn));
							result.ArtifactCollection = Any.Pack(behaviors);
							result.LastItemIndex = options.LastItemIndex + behaviors.Behavior.Count - 1;
						}

						result.TotalItemsInCollection = behaviors.Behavior.Count;
						result.ArtifactCollection = Any.Pack(behaviors);
						break;
					case ArtifactType.BehaviorGroup:
						var behaviorGroups = new BehaviorGroups();
						if (Taxonomy.BehaviorGroups.Count <= options.MaxItemReturn
						) //if max return is greater than the total count, just send back all of them.
						{
							behaviorGroups.BehaviorGroup.AddRange(Taxonomy.BehaviorGroups.Values);
							result.FirstItemIndex = 0;
							result.LastItemIndex = behaviorGroups.BehaviorGroup.Count - 1;
						}
						else
						{
							behaviorGroups.BehaviorGroup.AddRange(behaviorGroups.BehaviorGroup.ToList()
								.GetRange(options.LastItemIndex, options.MaxItemReturn));
							result.ArtifactCollection = Any.Pack(behaviorGroups);
							result.LastItemIndex =
								options.LastItemIndex + behaviorGroups.BehaviorGroup.Count - 1;
						}

						result.TotalItemsInCollection = behaviorGroups.BehaviorGroup.Count;
						result.ArtifactCollection = Any.Pack(behaviorGroups);
						break;
					case ArtifactType.PropertySet:
						var propertySets = new PropertySets();
						if (Taxonomy.PropertySets.Count <= options.MaxItemReturn
						) //if max return is greater than the total count, just send back all of them.
						{
							propertySets.PropertySet.AddRange(Taxonomy.PropertySets.Values);
							result.FirstItemIndex = 0;
							result.LastItemIndex = propertySets.PropertySet.Count - 1;
						}
						else
						{
							propertySets.PropertySet.AddRange(propertySets.PropertySet.ToList()
								.GetRange(options.LastItemIndex, options.MaxItemReturn));
							result.ArtifactCollection = Any.Pack(propertySets);
							result.LastItemIndex = options.LastItemIndex + propertySets.PropertySet.Count - 1;
						}

						result.TotalItemsInCollection = propertySets.PropertySet.Count;
						result.ArtifactCollection = Any.Pack(propertySets);
						break;
					case ArtifactType.TemplateFormula:
						var formulas = new TemplateFormulas();
						var templateFormulas = Taxonomy.TemplateFormulas;
						if (templateFormulas.Count <= options.MaxItemReturn) //if max return is greater than the total count, just send back all of them.
						{
							formulas.Templates.AddRange(templateFormulas.Values);
							result.FirstItemIndex = 0;
							result.LastItemIndex = formulas.Templates.Count - 1;
						}
						else
						{
							formulas.Templates.AddRange(templateFormulas.Values.ToList()
								.GetRange(options.LastItemIndex, options.MaxItemReturn));
							result.ArtifactCollection = Any.Pack(formulas);
							result.LastItemIndex =
								options.LastItemIndex + formulas.Templates.Count - 1;
						}

						result.TotalItemsInCollection = formulas.Templates.Count;
						result.ArtifactCollection = Any.Pack(formulas);
						break;
					case ArtifactType.TemplateDefinition:
						var definitions = new TemplateDefinitions();
						var templateDefinitions = Taxonomy.TemplateDefinitions;
						if (templateDefinitions.Count <= options.MaxItemReturn) //if max return is greater than the total count, just send back all of them.
						{
							definitions.Definitions.AddRange(templateDefinitions.Values);
							result.FirstItemIndex = 0;
							result.LastItemIndex = definitions.Definitions.Count - 1;
						}
						else
						{
							definitions.Definitions.AddRange(templateDefinitions.Values.ToList()
								.GetRange(options.LastItemIndex, options.MaxItemReturn));
							result.ArtifactCollection = Any.Pack(definitions);
							result.LastItemIndex =
								options.LastItemIndex + definitions.Definitions.Count - 1;
						}

						result.TotalItemsInCollection = definitions.Definitions.Count;
						result.ArtifactCollection = Any.Pack(definitions);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				return result;
			}
			catch (Exception ex)
			{
				_log.Error("Error retrieving artifact collection of type: " + options.ArtifactType);
				_log.Error(ex);
				return result;
			}
		}
		
		public static NewArtifactResponse CreateArtifact(NewArtifactRequest artifactRequest)
		{
			_log.Info("CreateArtifact: " + artifactRequest.Type);
			return TaxonomyController.CreateArtifact(artifactRequest);
		}

		public static UpdateArtifactResponse UpdateArtifact(UpdateArtifactRequest artifactRequest)
		{
			_log.Info("UpdateArtifact: " + artifactRequest.Type);
			return TaxonomyController.UpdateArtifact(artifactRequest);
		}

		public static DeleteArtifactResponse DeleteArtifact(DeleteArtifactRequest artifactRequest)
		{
			_log.Info("DeleteArtifact: " + artifactRequest.ArtifactSymbol.Tooling);
			return TaxonomyController.DeleteArtifact(artifactRequest);
		}

		public static bool AddOrUpdateInMemoryArtifact(ArtifactType type, Any artifact)
		{
			switch (type)
			{
				case ArtifactType.Base:
					var baseType = artifact.Unpack<Base>();
					try
					{
						Taxonomy.BaseTokenTypes.Remove(baseType.Artifact.ArtifactSymbol.Tooling);
						Taxonomy.BaseTokenTypes.Add(baseType.Artifact.ArtifactSymbol.Tooling, baseType);
					}
					catch (Exception)
					{
						_log.Info("AddOrUpdateInMemoryArtifact did not find an existing: " + type + " with a Tooling Symbol of: " + baseType.Artifact.ArtifactSymbol.Tooling);
						_log.Info("Adding artifact to Taxonomy.");
						Taxonomy.BaseTokenTypes.Add(baseType.Artifact.ArtifactSymbol.Tooling, baseType);
					}
					return true;
				case ArtifactType.Behavior:
					var behavior = artifact.Unpack<Behavior>();
					try
					{
						Taxonomy.Behaviors.Remove(behavior.Artifact.ArtifactSymbol.Tooling);
						Taxonomy.Behaviors.Add(behavior.Artifact.ArtifactSymbol.Tooling, behavior);
					}
					catch (Exception)
					{
						_log.Info("AddOrUpdateInMemoryArtifact did not find an existing: " + type + " with a Tooling Symbol of: " + behavior.Artifact.ArtifactSymbol.Tooling);
						_log.Info("Adding artifact to Taxonomy.");
						Taxonomy.Behaviors.Add(behavior.Artifact.ArtifactSymbol.Tooling, behavior);
					}
					return true;
				case ArtifactType.BehaviorGroup:
					var behaviorGroup = artifact.Unpack<BehaviorGroup>();
					try
					{
						Taxonomy.BehaviorGroups.Remove(behaviorGroup.Artifact.ArtifactSymbol.Tooling);
						Taxonomy.BehaviorGroups.Add(behaviorGroup.Artifact.ArtifactSymbol.Tooling, behaviorGroup);
					}
					catch (Exception)
					{
						_log.Info("AddOrUpdateInMemoryArtifact did not find an existing: " + type + " with a Tooling Symbol of: " + behaviorGroup.Artifact.ArtifactSymbol.Tooling);
						_log.Info("Adding artifact to Taxonomy.");
						Taxonomy.BehaviorGroups.Add(behaviorGroup.Artifact.ArtifactSymbol.Tooling, behaviorGroup);
					}
					return true;
				case ArtifactType.PropertySet:
					var propertySet = artifact.Unpack<PropertySet>();
					try
					{
						Taxonomy.PropertySets.Remove(propertySet.Artifact.ArtifactSymbol.Tooling);
						Taxonomy.PropertySets.Add(propertySet.Artifact.ArtifactSymbol.Tooling, propertySet);
					}
					catch (Exception)
					{
						_log.Info("AddOrUpdateInMemoryArtifact did not find an existing: " + type + " with a Tooling Symbol of: " + propertySet.Artifact.ArtifactSymbol.Tooling);
						_log.Info("Adding artifact to Taxonomy.");
						Taxonomy.PropertySets.Add(propertySet.Artifact.ArtifactSymbol.Tooling, propertySet);
					}
					return true;
				case ArtifactType.TemplateFormula:
					var templateFormula = artifact.Unpack<TemplateFormula>();
					try
					{
						Taxonomy.TemplateFormulas.Remove(templateFormula.Artifact.ArtifactSymbol.Tooling);
						Taxonomy.TemplateFormulas.Add(templateFormula.Artifact.ArtifactSymbol.Tooling, templateFormula);
					}
					catch (Exception)
					{
						_log.Info("AddOrUpdateInMemoryArtifact did not find an existing: " + type + " with a Tooling Symbol of: " + templateFormula.Artifact.ArtifactSymbol.Tooling);
						_log.Info("Adding artifact to Taxonomy.");
						Taxonomy.TemplateFormulas.Add(templateFormula.Artifact.ArtifactSymbol.Tooling, templateFormula);
					}
					return true;
				case ArtifactType.TemplateDefinition:
					var templateDefinition = artifact.Unpack<TemplateDefinition>();
					try
					{
						Taxonomy.TemplateDefinitions.Remove(templateDefinition.Artifact.ArtifactSymbol.Tooling);
						Taxonomy.TemplateDefinitions.Add(templateDefinition.Artifact.ArtifactSymbol.Tooling, templateDefinition);
					}
					catch (Exception)
					{
						_log.Info("AddOrUpdateInMemoryArtifact did not find an existing: " + type + " with a Tooling Symbol of: " + templateDefinition.Artifact.ArtifactSymbol.Tooling);
						_log.Info("Adding artifact to Taxonomy.");
						Taxonomy.TemplateDefinitions.Add(templateDefinition.Artifact.ArtifactSymbol.Tooling, templateDefinition);
					}
					return true;

				default:
					return false;
			}
		}

		//todo:finish this
		private static bool AddTemplateToHierarchy(TemplateDefinition definition)
		{
			
			var templateFormula = Taxonomy.TemplateFormulas
				.SingleOrDefault(e => e.Key == definition.FormulaReference.Id).Value;
			var tokenBase = Taxonomy.BaseTokenTypes.SingleOrDefault(e => e.Key == definition.TokenBase.Reference.Id)
				.Value;
			var classification = new Classification
			{
				TemplateType = templateFormula.TemplateType,
				TokenType = tokenBase.TokenType,
				TokenUnit = tokenBase.TokenUnit
			};
			var targetBranch = new BranchRoot
			{
				BranchFormula = templateFormula,
				BranchIdentifier = new BranchIdentifier
				{
					Classification = classification,
					FormulaId = templateFormula.Artifact.ArtifactSymbol.Id
				}
			};
			try
			{
				switch (classification.TokenUnit)
				{
					case TokenUnit.Fractional:
						if (classification.TokenType == TokenType.Fungible &&
						    classification.TemplateType == TemplateType.SingleToken)
						{
				
						}

						if (classification.TokenType == TokenType.Fungible &&
						    classification.TemplateType == TemplateType.Hybrid)
						{
						}
						if (classification.TokenType == TokenType.NonFungible &&
						    classification.TemplateType == TemplateType.SingleToken)
						{
						}

						if (classification.TokenType == TokenType.NonFungible &&
						    classification.TemplateType == TemplateType.Hybrid)
						{
						}
						break;
					case TokenUnit.Whole:
						if (classification.TokenType == TokenType.Fungible &&
						    classification.TemplateType == TemplateType.SingleToken)
						{
						}

						if (classification.TokenType == TokenType.Fungible &&
						    classification.TemplateType == TemplateType.Hybrid)
						{
						}
						if (classification.TokenType == TokenType.NonFungible &&
						    classification.TemplateType == TemplateType.SingleToken)
						{
						}

						if (classification.TokenType == TokenType.NonFungible &&
						    classification.TemplateType == TemplateType.Hybrid)
						{
						}
						break;
					case TokenUnit.Singleton:
						if (classification.TokenType == TokenType.NonFungible &&
						    classification.TemplateType == TemplateType.SingleToken)
						{
						}

						if (classification.TokenType == TokenType.NonFungible &&
						    classification.TemplateType == TemplateType.Hybrid)
						{
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				return true;
			}
			catch (Exception e)
			{
				_log.Error("Failed to add template to taxonomy: " + e);
				return false;
			}

		}

		internal static string GetArtifactFolderNameBySymbol(ArtifactType artifactType, string tooling)
		{
			_log.Info("GetArtifactFolderNameBySymbol: " + artifactType +": " + tooling);
			try
			{
				switch (artifactType)
				{
					case ArtifactType.Base:
						var baseFolder = Taxonomy.BaseTokenTypes.Single(e =>
							e.Value.Artifact.ArtifactSymbol.Tooling == tooling);
						return baseFolder.Value.Artifact.Name;
					case ArtifactType.Behavior:
						var behaviorFolder = Taxonomy.Behaviors.Single(e =>
							e.Value.Artifact.ArtifactSymbol.Tooling == tooling);
						return behaviorFolder.Value.Artifact.Name;
					case ArtifactType.BehaviorGroup:
						var behaviorGroupFolder = Taxonomy.BehaviorGroups.Single(e =>
							e.Value.Artifact.ArtifactSymbol.Tooling == tooling);
						return behaviorGroupFolder.Value.Artifact.Name;
					case ArtifactType.PropertySet:
						var propertySetFolder = Taxonomy.PropertySets.Single(e =>
							e.Value.Artifact.ArtifactSymbol.Tooling == tooling);
						return propertySetFolder.Value.Artifact.Name;
					case ArtifactType.TemplateFormula:
						var tokenTemplateFolder = Taxonomy.TemplateFormulas.Single(e =>
							e.Value.Artifact.ArtifactSymbol.Tooling == tooling);
						return tokenTemplateFolder.Value.Artifact.Name;
					case ArtifactType.TemplateDefinition:
						var templateDefinitionFolder = Taxonomy.TemplateDefinitions.Single(e =>
							e.Value.Artifact.ArtifactSymbol.Tooling == tooling);
						return templateDefinitionFolder.Value.Artifact.Name;
					case ArtifactType.TokenTemplate:
                              return "";
					default:
						throw new ArgumentOutOfRangeException(nameof(artifactType), artifactType, null);
				}
			}
			catch (Exception)
			{
				_log.Info("No matching artifact folder of type: " + artifactType + " with symbol: " + tooling);
				return "";
			}
		}
		
		internal static bool CheckForUniqueArtifact(ArtifactType artifactType, Artifact artifact)
		{
			var name = artifact.Name;
			_log.Info("CheckForUniqueArtifact: " + artifactType +": " + name);
			try
			{
				if(!string.IsNullOrEmpty(GetArtifactFolderNameBySymbol(artifactType, artifact.ArtifactSymbol.Tooling)))
					throw new Exception("Tooling Symbol Found.");
				switch (artifactType)
				{
					case ArtifactType.Base:
						var baseFolder = Taxonomy.BaseTokenTypes.Single(e =>
							e.Value.Artifact.Name == name);
						break;
					case ArtifactType.Behavior:
						var behaviorFolder = Taxonomy.Behaviors.Single(e =>
							e.Value.Artifact.Name == name);
						break;
					case ArtifactType.BehaviorGroup:
						var behaviorGroupFolder = Taxonomy.BehaviorGroups.Single(e =>
							e.Value.Artifact.Name == name);
						break;
					case ArtifactType.PropertySet:
						var propertySetFolder = Taxonomy.PropertySets.Single(e =>
							e.Value.Artifact.Name == name);
						break;
					case ArtifactType.TokenTemplate:
						break;
					case ArtifactType.TemplateFormula:
						var formulaFolder = Taxonomy.TemplateFormulas.Single(e =>
							e.Value.Artifact.Name == name);
						break;
					case ArtifactType.TemplateDefinition:
						var definitionFormula = Taxonomy.TemplateDefinitions.Single(e =>
							e.Value.Artifact.Name == name);
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(artifactType), artifactType, null);
				}
			}
			catch (Exception)
			{
				return true;
			}
			return false;
		}
		
		internal static bool CheckForUniqueTemplateFormula(string formula, string name)
		{
			_log.Info("CheckForUniqueTemplateFormula: " + name);
			try
			{
				if(string.IsNullOrEmpty(GetArtifactFolderNameBySymbol(ArtifactType.TemplateFormula, formula)))
					throw new Exception("Tooling Symbol Found.");
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}
		
		internal static string MakeUniqueDefinitionName(string name)
		{
			return Utils.GetRandomNameFromName(name);
		}
		internal static Artifact MakeUniqueArtifact(Artifact artifact)
		{
			var newArtifact = artifact.Clone();
			var (name, visual, tooling) = Utils.GetRandomArtifactFromArtifact(artifact.Name, artifact.ArtifactSymbol.Visual, artifact.ArtifactSymbol.Tooling);
			newArtifact.Name = name;
			newArtifact.ArtifactSymbol.Visual = visual;
			newArtifact.ArtifactSymbol.Tooling = tooling;
			newArtifact.ArtifactSymbol.Id = Guid.NewGuid().ToString().ToLower();
			newArtifact.ArtifactSymbol.Version = "1.0";
			return newArtifact;
		}
		
		internal static (string, ArtifactSymbol) MakeUniqueTokenFormula(string name, ArtifactSymbol formula)
		{
			var newArtifact = formula.Clone();
			var (newName, visual, tooling) = Utils.GetRandomTemplate(name, formula.Visual, formula.Tooling);
			newArtifact.Id = Guid.NewGuid().ToString();
			newArtifact.Visual = visual;
			newArtifact.Tooling = tooling;
			newArtifact.Version = "1.0";
			return (newName, newArtifact);
		}
		
		public static TokenSpecification GetTokenSpecification(TokenTemplateId symbol)
		{
			_log.Info("Generating Token Specification from Token Token Template Id: " + symbol);
			var definition = GetTemplateDefinitionArtifact(new ArtifactSymbol
			{
				Id = symbol.DefinitionId
			});
	
			if (definition == null) return null;
			var formula = GetTemplateFormulaArtifact(new ArtifactSymbol
			{
				Id = definition.FormulaReference.Id
			});
			
			var spec = BuildSpecification(definition);
			if (formula.TemplateType != TemplateType.Hybrid) return spec;
			foreach (var c in definition.ChildTokens)
			{
				spec.ChildTokens.Add(BuildSpecification(c));
			}
			return spec;
		}

		private static TokenSpecification BuildSpecification(TemplateDefinition definition)
		{
			var formula = GetTemplateFormulaArtifact(new ArtifactSymbol
			{
				Id = definition.FormulaReference.Id
			});
			
			var validated = ValidateDefinition(formula, definition);
			if (!string.IsNullOrEmpty(validated))
			{
				_log.Error(validated);
				return null;
			}
			
			var retVal = new TokenSpecification
			{
				Artifact = definition.Artifact
			};
			retVal.Artifact.ArtifactSymbol.Type = ArtifactType.TokenTemplate;
			
			retVal.TokenBase = MergeBase(definition);
			
			var behaviorGroups = definition.BehaviorGroups.Select(tb => GetArtifactById<BehaviorGroup>(tb.Reference.Id)).ToList();
			foreach (var bg in definition.BehaviorGroups)
			{
				var behaviorGroup = behaviorGroups.SingleOrDefault(e => e.Artifact.ArtifactSymbol.Id == bg.Reference.Id);
				if (behaviorGroup == null) continue;
				
				var behaviorGroupSpec = new BehaviorGroupSpecification();
				foreach (var b in behaviorGroup.Behaviors)
				{
					var behavior = GetArtifactById<Behavior>(b.Reference.Id);
					if (behavior == null) continue;
					behaviorGroupSpec.Behaviors.Add(behavior.Artifact.ArtifactSymbol);
				}
				retVal.BehaviorGroups.Add(behaviorGroupSpec);
			}
			
			var propertySets = definition.PropertySets.Select(tb => GetArtifactById<PropertySet>(tb.Reference.Id)).ToList();
			var mergedPs = new List<PropertySet>();
			foreach (var ps in definition.PropertySets)
			{
				var propertySet = propertySets.SingleOrDefault(e => e.Artifact.ArtifactSymbol.Id == ps.Reference.Id);
				if (propertySet == null) continue;
				mergedPs.Add(MergePropertySet(propertySet, ps));
			}
			
			var behaviors = definition.Behaviors.Select(tb => GetArtifactById<Behavior>(tb.Reference.Id)).ToList();
			var behaviorReferences = definition.Behaviors;
			foreach (var bgb in definition.BehaviorGroups)
			{
				foreach (var b in bgb.BehaviorArtifacts)
				{
					behaviors.Add(GetArtifactById<Behavior>(b.Reference.Id));
					behaviorReferences.Add(b);
				}
			}

			var (behaviorSpecifications, propSets) = BuildBehaviorSpecs(behaviors, behaviorReferences, mergedPs);
				
			retVal.Behaviors.AddRange(behaviorSpecifications);
			retVal.PropertySets.AddRange(propSets);
			return retVal;
		}

		private static (IEnumerable<BehaviorSpecification>, IEnumerable<PropertySetSpecification>) BuildBehaviorSpecs
			(IEnumerable<Behavior> behaviors, IEnumerable<BehaviorReference> references, IEnumerable<PropertySet> propertySets)
		{
			//check for invocationBindings
			var behaviorReferences = references.ToList();
			var propSets = propertySets.ToList();
			var behaviorsList = behaviors.ToList();

			var influencedBehaviors = 
				new List<(BehaviorReference, Behavior, BehaviorReference)>(); //influencingReference, influencedArtifact, influencedReference
			var influencedPropertySets = new List<(BehaviorReference, PropertySet)>(); //influencingReference, influencedArtifact

			//getting all the influenced behaviors matched with their influencers
			foreach (var b in behaviorReferences)
			{
				if(b.InfluenceBindings.Count == 0)
					continue;
				foreach (var ib in b.InfluenceBindings)
				{
					var inb = behaviorsList.SingleOrDefault(e =>
						e.Artifact.ArtifactSymbol.Id == ib.InfluencedId);
					if (inb != null)
					{
						var influenced =
							behaviorReferences.SingleOrDefault(e => e.Reference.Id == ib.InfluencedId);
						influencedBehaviors.Add((b, inb, influenced));
					}

					var inp = propSets.SingleOrDefault(e => e.Artifact.ArtifactSymbol.Id == ib.InfluencedId);
					if(inp!= null)
						influencedPropertySets.Add((b, inp));
				}
			}

			//removing influenced behaviors and properties from straight merges into specs.
			foreach (var ib in influencedBehaviors)
			{
				behaviorReferences.Remove(ib.Item3);
			}

			foreach (var ibp in influencedPropertySets)
			{
				propSets.Remove(ibp.Item2);
			}
			
			var behaviorList = behaviorsList.ToList();

			var behaviorSpecs = (from b in behaviorReferences let behavior 
				= behaviorList.SingleOrDefault(e => e.Artifact.ArtifactSymbol.Id == b.Reference.Id) 
				select GetBehaviorSpecification(behavior, b)).ToList();
			
			foreach (var (influencingReference, influencedBehavior, influencedReference) in influencedBehaviors)
			{
				var behaviorSpec = new BehaviorSpecification
				{
					Constructor = influencedReference.Constructor,
					ConstructorType = influencedReference.ConstructorType,
					IsExternal = influencedReference.IsExternal,
					Artifact = influencedBehavior.Artifact,
					Properties = {influencedBehavior.Properties}
				};
				foreach (var ib in influencedBehavior.Invocations)
				{
					var invokeBinding = new InvocationBinding
					{
						InvocationStep = new InvocationBinding.Types.InvocationStep
						{
							Invocation = ib
						}
					};
					behaviorSpec.Invocations.Add(invokeBinding);
				}
			
				foreach (var i in influencedReference.InfluenceBindings)
				{
					var invokeBinding = new InvocationBinding
					{
						Influence = new InvocationBinding.Types.Influence
						{
							InfluencedId = i.InfluencedId,
							InfluencedInvocationId = i.InfluencedInvocationId,
							InfluencingId = influencingReference.Reference.Id,
							InfluencingInvocationId = i.InfluencingInvocation.Id,
							InfluenceType = i.InfluenceType
						}
					};
					if (i.InfluenceType == InfluenceType.Intercept)
					{
						invokeBinding.InvocationStep = new InvocationBinding.Types.InvocationStep
						{
							Invocation = i.InfluencingInvocation,
							NextInvocation = new InvocationBinding.Types.InvocationStep
							{
								Invocation = i.InfluencedInvocation
							}
						};
					}
					else
					{
						invokeBinding.InvocationStep = new InvocationBinding.Types.InvocationStep
						{
							Invocation = i.InfluencingInvocation
						};
					}
					behaviorSpec.Invocations.Add(invokeBinding);
				}
				behaviorSpecs.Add(behaviorSpec);
			}

			var propSetSpecs = new List<PropertySetSpecification>();
			foreach (var (behaviorReference, propertySet) in influencedPropertySets)
			{
				var propSetSpec = new PropertySetSpecification
				{
					Artifact = propertySet.Artifact
				};
				foreach (var i in behaviorReference.InfluenceBindings)
				{
					foreach (var p in propertySet.Properties)
					{
						var propSpec = CreatePropertySpec(p);
						foreach (var ip in p.PropertyInvocations)
						{
							if (ip.Id == i.InfluencedInvocationId)
							{
								var invokeBinding = new InvocationBinding
								{
									Influence = new InvocationBinding.Types.Influence
									{
										InfluencedId = i.InfluencedId,
										InfluencedInvocationId = i.InfluencedInvocationId,
										InfluencingId = behaviorReference.Reference.Id,
										InfluencingInvocationId = i.InfluencingInvocation.Id,
										InfluenceType = i.InfluenceType
									}
								};
								if (i.InfluenceType == InfluenceType.Intercept)
								{
									invokeBinding.InvocationStep = new InvocationBinding.Types.InvocationStep
									{
										Invocation = i.InfluencingInvocation,
										NextInvocation = new InvocationBinding.Types.InvocationStep
										{
											Invocation = i.InfluencedInvocation
										}
									};
								}
								else
								{
									invokeBinding.InvocationStep = new InvocationBinding.Types.InvocationStep
									{
										Invocation = i.InfluencingInvocation
									};
								}
								propSpec.PropertyInvocations.Add(invokeBinding);
							}
							else //property in the set that is not influenced
							{
								var invokeBinding = new InvocationBinding
								{
									InvocationStep = new InvocationBinding.Types.InvocationStep
									{
										Invocation = ip
									}
								};
								propSpec.PropertyInvocations.Add(invokeBinding);
							}
							propSetSpec.Properties.Add(propSpec);
						}
					}
					propSetSpecs.Add(propSetSpec);
				}
			}
			return (behaviorSpecs, propSetSpecs);
		}

		private static PropertySpecification CreatePropertySpec(Property p)
		{
			var propSpec = new PropertySpecification
			{
				Name = p.Name,
				TemplateValue = p.TemplateValue,
				ValueDescription = p.ValueDescription
			};
			if (p.Properties.Count == 0)
				return propSpec;
			foreach (var pp in p.Properties)
			{
				propSpec = CreatePropertySpec(pp);
			}

			return propSpec;
		}

		private static PropertySet MergePropertySet(PropertySet ps, PropertySetReference psr)
		{
			foreach (var i in ps.Properties)
			{
				foreach (var m in psr.Properties)
				{
					if (i.Name == m.Name)
						i.MergeFrom(m);
				}
			}
			
			return ps;
		}
		
		private static PropertySetSpecification GetPropertySetSpecification(PropertySet propertySet)
		{
			var propertySetSpec = new PropertySetSpecification
			{
				Artifact = propertySet.Artifact
			};
			
			foreach (var i in propertySet.Properties)
			{
				var propertySpec = new PropertySpecification
				{
					Name = i.Name,
					TemplateValue = i.TemplateValue,
					ValueDescription = i.ValueDescription
				};
				foreach (var pi in i.PropertyInvocations)
				{
					var bp = new InvocationBinding
					{
						InvocationStep = new InvocationBinding.Types.InvocationStep
						{
							Invocation = pi
						}
					};
					propertySpec.PropertyInvocations.Add(bp);
				}
				propertySetSpec.Properties.Add(propertySpec);
			}

			return propertySetSpec;
		}

		private static BehaviorSpecification GetBehaviorSpecification(Behavior behavior, BehaviorReference behaviorReference)
		{
			foreach (var i in behavior.Invocations)
			{
				foreach (var m in behaviorReference.Invocations)
				{
					if (i.Id == m.Id)
						i.MergeFrom(m);
				}
			}

			foreach (var p in behavior.Properties)
			{
				foreach (var m in behaviorReference.Properties)
				{
					if (p.Name == m.Name)
						p.MergeFrom(m);
				}
			}

			var behaviorSpec = new BehaviorSpecification
			{
				Constructor = behaviorReference.Constructor,
				ConstructorType = behaviorReference.ConstructorType,
				IsExternal = behaviorReference.IsExternal,
				Artifact = behavior.Artifact,
				Properties = {behavior.Properties}
			};
			foreach (var ib in behaviorReference.Invocations)
			{
				var invokeBinding = new InvocationBinding
				{
					InvocationStep = new InvocationBinding.Types.InvocationStep
					{
						Invocation = ib
					}
				};
				behaviorSpec.Invocations.Add(invokeBinding);
			}
			
			return behaviorSpec;
		}

		private static Base MergeBase(TemplateDefinition validated)
		{
			var baseToken = GetArtifactById<Base>(validated.TokenBase.Reference.Id);
			baseToken.Artifact.Maps.MergeFrom(validated.Artifact.Maps);
			baseToken.Decimals = validated.TokenBase.Decimals;
			baseToken.Name = validated.TokenBase.Name;
			baseToken.Owner = validated.TokenBase.Owner;
			baseToken.Quantity = validated.TokenBase.Quantity;
			baseToken.Symbol = validated.TokenBase.Symbol;
			baseToken.ConstructorName = validated.TokenBase.ConstructorName;
			return baseToken;
		}

		private static string ValidateDefinition(TemplateFormula formula, TemplateDefinition definition)
		{
			if (definition.TokenBase.Reference.Id != formula.TokenBase.Base.Id)
			{
				return "Error validating definition id: " + definition.Artifact.ArtifactSymbol.Id
				                                          + " against its template id: " +
				                                          formula.Artifact.ArtifactSymbol.Id
				                                          + " the base token does not match";
			}

			foreach (var b in definition.Behaviors)
			{
				var tb = formula.Behaviors.SingleOrDefault(e => e.Behavior.Id == b.Reference.Id);
				if (tb == null)
				{
					return "Error validating definition id: " + definition.Artifact.ArtifactSymbol.Id
					                                          + " against its template id: " +
					                                          formula.Artifact.ArtifactSymbol.Id
					                                          + " the behavior " + b.Reference.Id
					                                          + " is not found.";
				}
			}
			
			foreach (var bg in definition.BehaviorGroups)
			{
				var tbg = formula.BehaviorGroups.SingleOrDefault(e => e.BehaviorGroup.Id == bg.Reference.Id);
				if (tbg == null)
				{
					return "Error validating definition id: " + definition.Artifact.ArtifactSymbol.Id
					                                          + " against its template id: " +
					                                          formula.Artifact.ArtifactSymbol.Id
					                                          + " the behaviorGroup " + bg.Reference.Id
					                                          + " is not found.";
				}
			}
			
			foreach (var ps in definition.PropertySets)
			{
				var tps = formula.PropertySets.SingleOrDefault(e => e.PropertySet.Id == ps.Reference.Id);
				if (tps == null)
				{
					return "Error validating definition id: " + definition.Artifact.ArtifactSymbol.Id
					                                          + " against its template id: " +
					                                          formula.Artifact.ArtifactSymbol.Id
					                                          + " the propertySet " + ps.Reference.Id
					                                          + " is not found.";
				}
			}

			return "";
		}

		public static Model.Taxonomy GetLiteTaxonomy(object version)
		{
			var taxonomyLite = new Model.Taxonomy
			{
				FormulaGrammar = Taxonomy.FormulaGrammar,
				Version = Taxonomy.Version
			};
			taxonomyLite.BaseTokenTypes.Add(Taxonomy.BaseTokenTypes);
			taxonomyLite.Behaviors.Add(Taxonomy.Behaviors);
			taxonomyLite.BehaviorGroups.Add(Taxonomy.BehaviorGroups);
			taxonomyLite.PropertySets.Add(Taxonomy.PropertySets);
			return taxonomyLite;
		}

		public static TokenTemplate GetTokenTemplate(TokenTemplateId request)
		{
			if (TokenTemplateCache.IsInCache(request.DefinitionId))
			{
				return TokenTemplateCache.GetFromCache(request.DefinitionId);
			}

			var definition = Taxonomy.TemplateDefinitions.SingleOrDefault(e => e.Key == request.DefinitionId)
				.Value;
			if (definition == null) return null;
			var formula = Taxonomy.TemplateFormulas
				.SingleOrDefault(e => e.Key == definition.FormulaReference.Id).Value;
			var t = new TokenTemplate
			{
				Definition = definition,
				Formula = formula
			};
			TokenTemplateCache.SaveToCache(t.Definition.Artifact.ArtifactSymbol.Id, t, DateTime.Now.AddDays(1));
			return t;
		}


		public static TemplateDefinition CreateTemplateDefinition(NewTemplateDefinition newTemplateDefinition)
		{
			TemplateFormula templateFormula;
			try
			{
				//get formula, fetch artifacts, create/copy definition references, send to controller to save, return to caller.
				templateFormula = GetArtifactById<TemplateFormula>(newTemplateDefinition.TemplateFormulaId);
			}
			catch (Exception e)
			{
				_log.Error("Error attempting to create a new TemplateDefinition from TemplateFormula with Id: " + newTemplateDefinition.TemplateFormulaId);
				_log.Error(e);
				return new TemplateDefinition();
			}
			
	
			var definition = BuildTemplateDefinition(newTemplateDefinition.TokenName, templateFormula);
			if (templateFormula.TemplateType == TemplateType.Hybrid)
			{
				foreach (var c in definition.ChildTokens)
				{
					var childTemplateFormula =
						GetArtifactById<TemplateFormula>(newTemplateDefinition.TemplateFormulaId);
					definition.ChildTokens.Add(BuildTemplateDefinition(newTemplateDefinition.TokenName + ".child", childTemplateFormula));
				}
			}

			TaxonomyController.CreateArtifact(new NewArtifactRequest
			{
				Artifact = Any.Pack(definition),
				Type = ArtifactType.TemplateDefinition
			});
			return definition;
		}
		
		private static TemplateDefinition BuildTemplateDefinition(string name, TemplateFormula formula)
		{
				//get formula, fetch artifacts, create/copy definition references, send to controller to save, return to caller.
			var retVal = new TemplateDefinition
			{
				Artifact = formula.Artifact
			};
			retVal.Artifact.Name = name;
			retVal.Artifact.ArtifactSymbol.Id = Guid.NewGuid().ToString();
			retVal.Artifact.ArtifactSymbol.Type = ArtifactType.TemplateDefinition;
			retVal.FormulaReference = new ArtifactReference
			{
				Id = formula.Artifact.ArtifactSymbol.Id,
				Type = ArtifactType.TemplateFormula
			};

			var baseToken = GetArtifactById<Base>(formula.TokenBase.Base.Id);
		
			retVal.TokenBase = new BaseReference
			{
				Reference = new ArtifactReference
				{
					Id = baseToken.Artifact.ArtifactSymbol.Id,
					Type = ArtifactType.Base,
					Values = new ArtifactReferenceValues
					{
						ControlUri = "",
						Maps = baseToken.Artifact.Maps
					}
				},
				ConstructorName = "Constructor"
			};

			var behaviors = formula.Behaviors.Select(tb => GetArtifactById<Behavior>(tb.Behavior.Id)).ToList();
			foreach (var b in behaviors)
			{
				retVal.Behaviors.Add(new BehaviorReference
				{
					Reference =  new ArtifactReference
					{
						Id = b.Artifact.ArtifactSymbol.Id,
						Type = ArtifactType.Behavior,
						Values = new ArtifactReferenceValues
						{
							ControlUri = "",
							Maps = b.Artifact.Maps
						}
					},
					ConstructorType = "",
					Properties = { b.Properties },
					Invocations = { b.Invocations},
					IsExternal = true
				});
			}
			
			var behaviorGroups = formula.BehaviorGroups.Select(tb => GetArtifactById<BehaviorGroup>(tb.BehaviorGroup.Id)).ToList();
			foreach (var b in behaviorGroups)
			{
				var behaviorGroup = new BehaviorGroupReference
				{
					Reference = new ArtifactReference
					{
						Id = b.Artifact.ArtifactSymbol.Id,
						Type = ArtifactType.BehaviorGroup,
						Values = new ArtifactReferenceValues
						{
							ControlUri = "",
							Maps = b.Artifact.Maps
						}
					}
				};
		
				var bgb = b.Behaviors.Select(tb => GetArtifactById<Behavior>(tb.Reference.Id)).ToList();
				foreach (var bRef in bgb.Select(br => new BehaviorReference
				{
					Reference = new ArtifactReference
					{
						Id = b.Artifact.ArtifactSymbol.Id,
						Type = ArtifactType.Behavior,
						Values = new ArtifactReferenceValues
						{
							ControlUri = "",
							Maps = b.Artifact.Maps
						}
					},
					ConstructorType = "",
					Properties = {br.Properties},
					Invocations = {br.Invocations},
					IsExternal = true
				}))
				{
					behaviorGroup.BehaviorArtifacts.Add(bRef);
				}

			}
			
			var propertySets = formula.PropertySets.Select(tb => GetArtifactById<PropertySet>(tb.PropertySet.Id)).ToList();
			foreach (var ps in propertySets)
			{
				retVal.PropertySets.Add(new PropertySetReference
				{
					Reference =  new ArtifactReference
					{
						Id = ps.Artifact.ArtifactSymbol.Id,
						Type = ArtifactType.PropertySet,
						Values = new ArtifactReferenceValues
						{
							ControlUri = "",
							Maps = ps.Artifact.Maps
						}
					},
					Properties = { ps.Properties }
				});
			}

			return retVal;
		}

		private static T GetArtifactById<T>(string id)
		{
			if (typeof(T) == typeof(Base))
			{
				Taxonomy.BaseTokenTypes.TryGetValue(id, out var tokenBase);
				return (T) Convert.ChangeType(tokenBase, typeof(T));
			}

			if (typeof(T) == typeof(Behavior))
			{
				Taxonomy.Behaviors.TryGetValue(id, out var behavior);
				return (T) Convert.ChangeType(behavior, typeof(T));
			}

			if (typeof(T) == typeof(BehaviorGroup))
			{
				Taxonomy.BehaviorGroups.TryGetValue(id, out var behaviorGroup);
				return (T) Convert.ChangeType(behaviorGroup, typeof(T));
			}

			if (typeof(T) == typeof(PropertySet))
			{
				Taxonomy.PropertySets.TryGetValue(id, out var propSet);
				return (T) Convert.ChangeType(propSet, typeof(T));
			}

			if (typeof(T) == typeof(TemplateFormula))
			{
				Taxonomy.TemplateFormulas.TryGetValue(id, out var formula);
				return (T) Convert.ChangeType(formula, typeof(T));
			}

			if (typeof(T) != typeof(TemplateDefinition)) return (T) Convert.ChangeType(new object(), typeof(T));
			Taxonomy.TemplateDefinitions.TryGetValue(id, out var definition);
			return (T) Convert.ChangeType(definition, typeof(T));
		}
	}
}