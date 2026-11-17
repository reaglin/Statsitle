# What an example to try can change

Made by `ExampleTests.Write_the_settings_each_analysis_can_change` (set STATISTLE_TRY_SETTINGS to a file). For each analysis: the settings a `[[try: …]]` can name (with the kind of control), and the results it can show after the `|`. A column of the task's own table is named `@Table/Column` (in double quotes when it has a space), whatever the analysis.

## ChiSquareTest

- Settings: `Test` (choice: GoodnessOfFit, Independence), `Alpha` (number), `Observed` (values), `Proportions` (choice: Equal, Entered), `Expected` (values), `EstimatedParameters` (whole), `Correction` (choice: None, Yates)
- Results: `Categories`, `Rows`, `Columns`, `Total`, `Hypotheses`, `ExpectedTable`, `SmallestExpected`, `CellsBelowFive`, `ExpectedCheck`, `Statistic`, `DegreesOfFreedom`, `Critical`, `P`, `Decision`, `CramersV`, `Notes`

## ConfidenceInterval

- Settings: `Target` (choice: MeanSigmaKnown, MeanSigmaUnknown, Proportion, Variance, TwoMeansPooled, TwoMeansWelch, PairedMeans, TwoProportions), `Source` (choice: Summary, Columns), `Confidence` (number), `SampleMean` (number), `SampleSd` (number), `Sigma` (number), `SampleSize` (number), `Successes` (number), `Sample2Mean` (number), `Sample2Sd` (number), `Sample2Size` (number), `Successes2` (number)
- Results: `Estimate`, `StandardError`, `Critical`, `DegreesOfFreedom`, `MarginOfError`, `Lower`, `Upper`, `SdLower`, `SdUpper`, `WilsonLower`, `WilsonUpper`, `Statement`

## ContinuousDistribution

- Settings: `Family` (choice: Uniform, Normal, Exponential, LogNormal, StudentT, ChiSquared, F, Weibull), `Source` (choice: Entered, FittedToData), `Lower` (number), `Upper` (number), `Mu` (number), `Sigma` (number), `Rate` (number), `LogMu` (number), `LogSigma` (number), `Shape` (number), `Scale` (number), `DegreesOfFreedom` (number), `NumeratorDf` (number), `DenominatorDf` (number), `X` (number), `X2` (number), `Probability` (number)
- Results: `ModelDescription`, `Density`, `AtMost`, `MoreThan`, `Between`, `ZScore`, `Quantile`, `Mean`, `Variance`, `StandardDeviation`, `Median`, `EmpiricalRule`, `HazardRate`, `FitCount`, `FitLower`, `FitUpper`, `FitMu`, `FitSigma`, `FitRate`, `FitLogMu`, `FitLogSigma`, `FitShape`, `FitScale`, `QqCorrelation`, `KsStatistic`

## DescriptiveStatistics

- Settings: `Sample` (values), `Classes` (number)
- Results: `Count`, `Mean`, `Median`, `Mode`, `ModeValue`, `Minimum`, `Maximum`, `Range`, `StandardDeviation`, `Variance`, `PopulationStandardDeviation`, `PopulationVariance`, `StandardError`, `CoefficientOfVariation`, `FirstQuartile`, `ThirdQuartile`, `InterquartileRange`, `LowerHinge`, `UpperHinge`, `LowerFence`, `UpperFence`, `Outliers`, `OutlierCount`, `Skewness`, `Kurtosis`, `LowerControlLimit`, `UpperControlLimit`, `PointsOutsideLimits`, `ClassesUsed`

## DiscreteDistribution

- Settings: `Family` (choice: Binomial, Poisson, Geometric, NegativeBinomial, Hypergeometric, Table), `Trials` (number), `SuccessProbability` (number), `Rate` (number), `SuccessesWanted` (number), `PopulationSize` (number), `PopulationSuccesses` (number), `Draws` (number), `K` (number), `K2` (number)
- Results: `ModelDescription`, `Exactly`, `AtMost`, `LessThan`, `AtLeast`, `MoreThan`, `Between`, `Mean`, `Variance`, `StandardDeviation`, `Support`

## MultipleRegression

- Settings: `Selection` (choice: AllEntered, Forward, Backward), `Confidence` (number), `Folds` (number), `HoldOutShare` (number), `HoldOutSeed` (number)
- Results: `Count`, `PredictorsUsed`, `Equation`, `CoefficientTable`, `ChosenEstimate`, `ChosenSe`, `ChosenT`, `ChosenP`, `RSquared`, `AdjustedRSquared`, `ResidualSe`, `FStatistic`, `FP`, `DfRegression`, `DfError`, `MaxVif`, `MaxVifPredictor`, `DurbinWatson`, `ShapiroW`, `ShapiroP`, `MaxCooksD`, `Influential`, `MaxLeverage`, `InSampleRmse`, `CvRmse`, `HoldOutCount`, `HoldOutRowList`, `HoldOutRmse`, `SelectionNote`

## OneSampleTest

- Settings: `TestKind` (choice: MeanSigmaKnown, MeanSigmaUnknown, Proportion, Variance), `Source` (choice: Summary, Columns), `AlternativeHypothesis` (choice: TwoSided, Less, Greater), `Alpha` (number), `NullValue` (number), `SampleMean` (number), `SampleSd` (number), `Sigma` (number), `SampleSize` (number), `Successes` (number), `AlternativeValue` (number), `TargetPower` (number)
- Results: `Hypotheses`, `Statistic`, `DegreesOfFreedom`, `RejectionRegion`, `PValue`, `Decision`, `CiLower`, `CiUpper`, `Power`, `SampleSizeForPower`

## OneWayAnova

- Settings: `Layout` (choice: ColumnsAreGroups, FactorAndResponse), `Alpha` (number)
- Results: `K`, `N`, `GroupTable`, `GrandMean`, `SsBetween`, `SsWithin`, `SsTotal`, `DfBetween`, `DfWithin`, `MsBetween`, `MsWithin`, `F`, `FCritical`, `P`, `Decision`, `EtaSquared`, `OmegaSquared`, `CohensF`, `Power`, `TukeyQ`, `TukeyTable`, `TukeyMinP`, `TukeySignificant`, `Bartlett`, `BartlettP`, `Levene`, `LeveneP`, `ShapiroW`, `ShapiroP`, `KruskalH`, `KruskalP`, `WelchF`, `WelchDf`, `WelchP`

## ProbabilityRules

- Settings: `Relationship` (choice: Independent, MutuallyExclusive, KnownIntersection, KnownConditional, KnownLikelihoods), `ProbabilityA` (number), `ProbabilityB` (number), `Intersection` (number), `ConditionalAGivenB` (number), `LikelihoodGivenA` (number), `LikelihoodGivenNotA` (number), `Items` (number), `Chosen` (number)
- Results: `ComplementA`, `ComplementB`, `ProbabilityBResult`, `IntersectionResult`, `Union`, `AGivenB`, `BGivenA`, `Independence`, `Permutations`, `Combinations`

## SampleSize

- Settings: `Target` (choice: Mean, Proportion), `Confidence` (number), `Margin` (number), `Sigma` (number), `ProportionGuess` (number)
- Results: `Critical`, `SampleSizeNeeded`, `Note`

## SamplingDistribution

- Settings: `Statistic` (choice: SampleMean, SampleProportion), `PopulationMean` (number), `PopulationSigma` (number), `PopulationProportion` (number), `SampleSize` (number), `Value` (number), `Value2` (number), `Shape` (choice: NotKnown, Normal), `Shade` (choice: Below, Above)
- Results: `Centre`, `StandardError`, `Z`, `AtMost`, `AtLeast`, `Between`, `Lower95`, `Upper95`, `Condition`

## SimpleRegression

- Settings: `XTransformation` (choice: None, Log, SquareRoot, Inverse), `YTransformation` (choice: None, Log, SquareRoot, Inverse), `Confidence` (number), `PredictAt` (number)
- Results: `Count`, `Skipped`, `Equation`, `Intercept`, `Slope`, `SlopeSe`, `SlopeT`, `SlopeP`, `SlopeLower`, `SlopeUpper`, `InterceptSe`, `InterceptP`, `Correlation`, `RSquared`, `AdjustedRSquared`, `ResidualSe`, `SsRegression`, `SsError`, `SsTotal`, `FStatistic`, `FP`, `FittedAt`, `MeanLower`, `MeanUpper`, `NewLower`, `NewUpper`, `DurbinWatson`, `ShapiroW`, `ShapiroP`, `MaxCooksD`, `Influential`, `MaxLeverage`

## Simulation

- Settings: `Experiment` (choice: Coins, Dice, Cards, Darts, SampleMeans, IntervalCoverage, FamilywiseError), `Trials` (number), `Seed` (number), `Coins` (number), `Dice` (number), `Cards` (number), `CardOutcome` (choice: Total, Hearts, Aces), `EventIs` (choice: EqualTo, AtLeast, AtMost), `Heads` (number), `DiceTotal` (number), `CardTarget` (number), `Distribution` (choice: Normal, Uniform, Exponential), `PopulationMean` (number), `PopulationSigma` (number), `Lower` (number), `Upper` (number), `Rate` (number), `SampleSize` (number), `Confidence` (number), `Alpha` (number), `Tests` (number)
- Results: `TrialsRun`, `EventText`, `Occurrences`, `ExperimentalProbability`, `TheoreticalProbability`, `Difference`, `PiEstimate`, `MeanOfMeans`, `SdOfMeans`, `TheoreticalMean`, `TheoreticalStandardError`

## TwoSampleTest

- Settings: `TestKind` (choice: PooledT, WelchT, PairedT, TwoSampleZ, TwoProportions, VarianceRatio, MannWhitney, WilcoxonSignedRank), `Source` (choice: Summary, Columns), `AlternativeHypothesis` (choice: TwoSided, Less, Greater), `Alpha` (number), `Delta0` (number), `SampleMean` (number), `SampleSd` (number), `Sigma` (number), `SampleSize` (number), `Successes` (number), `Sample2Mean` (number), `Sample2Sd` (number), `Sigma2` (number), `Sample2Size` (number), `Successes2` (number)
- Results: `Hypotheses`, `Statistic`, `DegreesOfFreedom`, `RejectionRegion`, `PValue`, `Decision`, `CiLower`, `CiUpper`, `PooledSd`, `EffectSize`, `Z`, `Note`, `Shapiro1`, `Shapiro1P`, `Shapiro2`, `Shapiro2P`, `Levene`, `LeveneP`

## TwoWayAnova

- Settings: `Alpha` (number)
- Results: `LevelsA`, `LevelsB`, `Replicates`, `N`, `CellTable`, `GrandMean`, `SsA`, `SsB`, `SsAB`, `SsError`, `SsTotal`, `DfA`, `DfB`, `DfAB`, `DfError`, `MsA`, `MsB`, `MsAB`, `MsError`, `FA`, `FB`, `FAB`, `PA`, `PB`, `PAB`, `DecisionA`, `DecisionB`, `DecisionAB`, `ShapiroW`, `ShapiroP`

