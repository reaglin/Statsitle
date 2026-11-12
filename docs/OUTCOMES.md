# Statistics+ — outcomes

Every module's outcomes with their **stable IDs** (progress, mastery and reports key off them), in the course's own MO
wording from https://reaglin.github.io/EGN3443/; the items that teach each one; the exercises graded by the engine; and
the module-check and practice questions that give evidence for it. **Ron reviews them** (plan 13.6).
Written by `tools/make-outcomes.py`; do not edit by hand. `ContentTests` fails if an outcome has no item or no check question.

## Module 0: Start here

| ID | The learner can | Taught in | Graded exercises | Questions (check / practice) |
|---|---|---|---|---|
| `st.m0.o1` | Navigate Statistics+ to reach the modules, their items and tabs, the Workbench, and your progress. | 0.1 How Statistics+ works | — | 0 / 6 |
| `st.m0.o2` | Run the course's Python or R code and read the results it prints. | 0.2 Python and R for this course | 1 | 0 / 0 |
| `st.m0.o3` | Organize an assignment as a report with the course's sections, from the title to the appendices. | 0.3 The report template | — | 0 / 6 |

## Module 1: Statistics for Engineers

| ID | The learner can | Taught in | Graded exercises | Questions (check / practice) |
|---|---|---|---|---|
| `st.m1.o1` | Calculate descriptive statistics — including measures of central tendency, spread, and shape — for a given engineering dataset. | 1.1 Introduction to statistics in engineering; 1.2 Descriptive statistics; 1.3 Data variability and distribution shape; 1.6 Applications in engineering analysis and quality control | 2 | 2 / 4 |
| `st.m1.o2` | Select the appropriate probability distribution (normal, binomial, or Poisson) to model a described engineering scenario. | 1.4 Probability distributions | — | 2 / 7 |
| `st.m1.o3` | Interpret confidence intervals and hypothesis test results in the context of real-world engineering tolerances and decision-making constraints. | 1.5 Statistical inference and interpretation | — | 2 / 7 |
| `st.m1.o4` | Construct a frequency distribution or visual representation (histogram or box plot) to characterize the variability and distribution shape of an engineering dataset. | 1.3 Data variability and distribution shape; 1.6 Applications in engineering analysis and quality control | 1 | 2 / 5 |
| `st.m1.o5` | Differentiate between sources of data variability and explain their implications for process consistency and product quality in a manufacturing context. | 1.1 Introduction to statistics in engineering; 1.3 Data variability and distribution shape; 1.6 Applications in engineering analysis and quality control | 1 | 2 / 4 |

## Module 2: Descriptive Statistics

| ID | The learner can | Taught in | Graded exercises | Questions (check / practice) |
|---|---|---|---|---|
| `st.m2.o1` | Calculate measures of central tendency (mean, median, and mode) from a given dataset. | 2.1 Introduction to descriptive statistics; 2.2 Measures of central tendency | 1 | 3 / 1 |
| `st.m2.o2` | Calculate measures of variability, including range, variance, and standard deviation, for a given dataset. | 2.1 Introduction to descriptive statistics; 2.3 Measures of variability | 1 | 2 / 1 |
| `st.m2.o3` | Classify the shape of a data distribution as symmetrical, positively skewed, or negatively skewed based on its characteristics. | 2.4 Data distribution (symmetry and skewness) | — | 2 / 8 |
| `st.m2.o4` | Select the appropriate measure of central tendency for a dataset given its data type and distributional context. | 2.2 Measures of central tendency; 2.4 Data distribution (symmetry and skewness); 2.5 Selecting and applying the right measures | 1 | 2 / 4 |
| `st.m2.o5` | Interpret computed descriptive statistics in the context of a real dataset to communicate meaningful insights to a specified audience. | 2.1 Introduction to descriptive statistics; 2.3 Measures of variability; 2.5 Selecting and applying the right measures | 1 | 1 / 8 |

## Module 3: Probability Concepts

| ID | The learner can | Taught in | Graded exercises | Questions (check / practice) |
|---|---|---|---|---|
| `st.m3.o1` | Construct a sample space for a given probability experiment using systematic methods such as lists, tables, or tree diagrams. | 3.2 Sample spaces and events | — | 2 / 7 |
| `st.m3.o2` | Calculate event probabilities using the complementary probability rule, the addition rule, and the multiplication rule. | 3.2 Sample spaces and events; 3.4 Core probability rules | 1 | 3 / 3 |
| `st.m3.o3` | Differentiate between theoretical and experimental probability by comparing their definitions, derivation methods, and the role of the Law of Large Numbers. | 3.1 Introduction to probability; 3.3 Theoretical vs experimental probability | 1 | 2 / 3 |
| `st.m3.o4` | Interpret a calculated probability value in plain language within a real-world engineering or decision-making context. | 3.1 Introduction to probability; 3.3 Theoretical vs experimental probability; 3.5 Interpreting probability in context | 1 | 1 / 5 |
| `st.m3.o5` | Select the appropriate probability rule — complementary, general addition, or general multiplication — for solving a multi-step probability problem. | 3.4 Core probability rules; 3.5 Interpreting probability in context | 1 | 2 / 3 |

## Module 4: Discrete Probability Distributions

| ID | The learner can | Taught in | Graded exercises | Questions (check / practice) |
|---|---|---|---|---|
| `st.m4.o1` | Construct a valid discrete probability distribution by assigning probabilities to all possible outcomes that satisfy both the non-negativity and summation-to-one properties. | 4.1 Introduction to discrete distributions; 4.2 Probability mass functions | — | 2 / 9 |
| `st.m4.o2` | Calculate the expected value and variance of a discrete random variable using its probability mass function. | 4.3 Expected value and variance | 1 | 2 / 0 |
| `st.m4.o3` | Differentiate between the binomial and Poisson distributions by evaluating whether a given scenario satisfies the defining conditions of each distribution. | 4.4 The binomial distribution; 4.5 The Poisson distribution; 4.6 Applying discrete distributions | 2 | 2 / 8 |
| `st.m4.o4` | Compute exact probabilities for real-world scenarios using the binomial and Poisson probability formulas with correctly identified parameters. | 4.4 The binomial distribution; 4.5 The Poisson distribution; 4.6 Applying discrete distributions | 2 | 2 / 1 |
| `st.m4.o5` | Interpret computed probabilities, expected values, and variances within the context of an applied engineering or scientific problem to support a data-driven conclusion. | 4.1 Introduction to discrete distributions; 4.2 Probability mass functions; 4.3 Expected value and variance; 4.4 The binomial distribution; 4.5 The Poisson distribution; 4.6 Applying discrete distributions | 3 | 2 / 4 |

## Module 5: Continuous Probability Distributions

| ID | The learner can | Taught in | Graded exercises | Questions (check / practice) |
|---|---|---|---|---|
| `st.m5.o1` | Distinguish between discrete and continuous random variables by identifying the role of the probability density function and the cumulative distribution function in describing continuous probability distributions. | 5.1 Introduction to continuous distributions; 5.5 Calculating and interpreting probabilities | — | 2 / 7 |
| `st.m5.o2` | Calculate probabilities for the uniform, normal, and exponential distributions using their respective formulas, z-score conversion, and cumulative distribution functions. | 5.2 The uniform distribution; 5.3 The normal distribution; 5.4 The exponential distribution; 5.5 Calculating and interpreting probabilities | 3 | 5 / 5 |
| `st.m5.o3` | Select the appropriate continuous probability distribution to model a given real-world scenario by evaluating the characteristics of the uniform, normal, and exponential distributions against the properties of the data. | 5.2 The uniform distribution; 5.3 The normal distribution; 5.4 The exponential distribution; 5.6 Applying continuous distributions | 4 | 2 / 0 |
| `st.m5.o4` | Interpret calculated probabilities from continuous distributions within the context of a real-world engineering or professional problem to support data-driven decision-making. | 5.5 Calculating and interpreting probabilities; 5.6 Applying continuous distributions | 1 | 1 / 2 |

## Module 6: Sampling Distributions

| ID | The learner can | Taught in | Graded exercises | Questions (check / practice) |
|---|---|---|---|---|
| `st.m6.o1` | Distinguish between population parameters and sample statistics in the context of a given data analysis scenario. | 6.1 Introduction to sampling distributions; 6.2 Population parameters vs sample statistics | — | 2 / 7 |
| `st.m6.o2` | Calculate the mean and standard error of a sampling distribution of the sample mean using the Central Limit Theorem formulas. | 6.1 Introduction to sampling distributions; 6.3 The Central Limit Theorem; 6.5 Interpreting and applying sampling distributions | 2 | 2 / 2 |
| `st.m6.o3` | Predict how changes in sample size affect the spread of a sampling distribution by applying the inverse relationship between sample size and standard error. | 6.4 The effect of sample size on variability | 1 | 3 / 0 |
| `st.m6.o4` | Identify the conditions under which the Central Limit Theorem justifies using a normal distribution approximation for the sampling distribution of the mean. | 6.3 The Central Limit Theorem | 1 | 2 / 0 |
| `st.m6.o5` | Interpret a sampling distribution to distinguish natural sampling variability from meaningful differences in a real-world engineering data scenario. | 6.5 Interpreting and applying sampling distributions | 1 | 1 / 0 |

## Module 7: Point Estimation and Confidence Intervals

| ID | The learner can | Taught in | Graded exercises | Questions (check / practice) |
|---|---|---|---|---|
| `st.m7.o1` | Distinguish between the properties of unbiasedness and efficiency when evaluating point estimators such as the sample mean and sample proportion. | 7.1 Introduction to point estimation | — | 2 / 8 |
| `st.m7.o2` | Apply the Central Limit Theorem to justify the use of normal-based inference methods for the sampling distribution of the sample mean. | 7.2 Sampling distributions and the central limit theorem; 7.3 Confidence intervals for means | 2 | 2 / 0 |
| `st.m7.o3` | Construct confidence intervals for population means and proportions by selecting the appropriate distribution (z or t), computing the margin of error, and forming the interval. | 7.3 Confidence intervals for means; 7.4 Confidence intervals for proportions; 7.6 Precision and margin of error; 7.7 Applying estimation to real data | 4 | 3 / 0 |
| `st.m7.o4` | Evaluate the effect of confidence level, sample size, and population variability on the width and precision of a confidence interval. | 7.5 Interpreting confidence intervals; 7.6 Precision and margin of error; 7.7 Applying estimation to real data | 2 | 2 / 1 |
| `st.m7.o5` | Interpret a confidence interval in context using correct statistical language that accurately reflects the meaning of the confidence level. | 7.5 Interpreting confidence intervals; 7.7 Applying estimation to real data | 1 | 1 / 6 |

## Module 8: Hypothesis Testing

| ID | The learner can | Taught in | Graded exercises | Questions (check / practice) |
|---|---|---|---|---|
| `st.m8.o1` | Construct correctly structured null and alternative hypotheses — including directional and non-directional forms — from a given real-world engineering claim. | 8.1 Foundations of hypothesis testing; 8.2 Formulating the null and alternative hypotheses | — | 2 / 7 |
| `st.m8.o2` | Select the appropriate hypothesis test (one-sample t-test, two-sample t-test, paired t-test, chi-square, or ANOVA) for a given data scenario based on data type, number of groups, and sample characteristics. | 8.5 Applying common hypothesis tests | 1 | 2 / 0 |
| `st.m8.o3` | Calculate a test statistic and corresponding p-value for a given sample dataset using the correct sampling distribution. | 8.3 Test statistics and sampling distributions; 8.4 P-values and significance levels; 8.5 Applying common hypothesis tests | 3 | 3 / 0 |
| `st.m8.o4` | Evaluate a hypothesis test conclusion by comparing the p-value to a pre-specified significance level and distinguishing statistical significance from practical significance. | 8.1 Foundations of hypothesis testing; 8.4 P-values and significance levels; 8.7 Interpreting and communicating results | 2 | 2 / 3 |
| `st.m8.o5` | Differentiate between Type I and Type II errors in a hypothesis testing scenario and identify the consequences of each error type for a given engineering context. | 8.1 Foundations of hypothesis testing; 8.6 Type I and Type II errors; 8.7 Interpreting and communicating results | 2 | 1 / 1 |

## Module 9: Hypothesis Testing II

| ID | The learner can | Taught in | Graded exercises | Questions (check / practice) |
|---|---|---|---|---|
| `st.m9.o1` | Select the appropriate hypothesis test (two-sample z-test, two-sample t-test, or paired-sample t-test) for a given engineering scenario based on data type, sample size, and sample independence | 9.1 Two-sample hypothesis tests; 9.2 Paired sample comparisons; 9.3 Selecting the appropriate test | 2 | 2 / 6 |
| `st.m9.o2` | Compute the test statistic for two-sample and paired-sample hypothesis tests using the correct formula and degrees of freedom | 9.1 Two-sample hypothesis tests; 9.2 Paired sample comparisons | 2 | 2 / 0 |
| `st.m9.o3` | Evaluate whether the assumptions underlying two-sample and paired-sample tests are satisfied for a given dataset | 9.4 Assumptions and conditions; 9.5 Introduction to non-parametric methods | 2 | 2 / 0 |
| `st.m9.o4` | Interpret p-values and confidence intervals from two-sample and paired-sample tests to distinguish between statistical significance and practical significance | 9.6 Interpreting and communicating results | 1 | 2 / 0 |
| `st.m9.o5` | Identify an appropriate non-parametric alternative when the assumptions of a parametric two-sample test cannot be satisfied | 9.3 Selecting the appropriate test; 9.5 Introduction to non-parametric methods | 1 | 2 / 2 |

## Totals

47 outcomes; 59 graded exercises, 90 module-check questions and 161 practice questions
list one of them (an exercise counts once for each outcome its item lists).
