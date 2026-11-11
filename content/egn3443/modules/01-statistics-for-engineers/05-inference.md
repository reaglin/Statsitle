---
id: st.m1.inference
title: Statistical inference and interpretation
minutes: 45
objectives: [st.m1.o3]
builds-on: st.m1.distributions
example: st.m1.inference.eg
quiz: quizzes/m1-inference.json
reference: [inference-preview]
---

# Statistical inference and interpretation

*A preview. Module 7 teaches confidence intervals and Modules 8 to 10 hypothesis tests, with how to calculate them. This item is about what they are for and how to read them, so that you can interpret the results an engineering report gives you.*

## From a sample to the process

A machine shop bores bearing housings to a diameter of 25.000 mm. Sixteen housings from the day's production were measured: their mean bore is x̄ = 25.004 mm, with a standard deviation s = 0.008 mm.

The engineer does not really care about these sixteen. She wants to know about **all** the housings made that day: is the machine centered on 25.000 mm? The sample mean, 25.004 mm, is a **point estimate** of the process mean μ. But another sixteen housings would give a slightly different x̄. **Statistical inference** reaches a conclusion about μ from the sample and says how sure it is. It has two main tools.

## The confidence interval

A **confidence interval** gives a range of plausible values for the parameter:

estimate ± critical value × standard error

For a mean, the standard error is s / √n, the typical size of the difference between x̄ and μ. The critical value comes from the t distribution (Module 7), using n − 1 = 15 "degrees of freedom", a number that depends on the sample size: for 95 % confidence it is 2.131.

**Worked case.** Standard error = 0.008 / √16 = 0.008 / 4 = 0.002 mm. Margin of error = 2.131 × 0.002 = 0.0043 mm. The 95 % confidence interval is 25.004 ± 0.0043, from **24.9997 to 25.0083 mm**.

[[figure: st.m1.inference.eg 1]]

[[results: st.m1.inference.eg]]

**How to read it.** "We are 95 % confident that the mean bore of the day's production is between 24.9997 and 25.0083 mm." The 95 % describes the method: of many intervals built this way from many samples, about 95 % would contain the true mean. It does **not** say that 95 % of the housings have bores in this range. The interval is for the **mean**, and individual bores vary much more (s = 0.008 mm).

## The hypothesis test

A **hypothesis test** checks a claim about the parameter.

- The **null hypothesis** H₀ is the claim of "no change" or "on target": μ = 25.000 mm.
- The **alternative hypothesis** H₁ is what you would conclude if the data argue strongly against H₀: μ ≠ 25.000 mm.
- The **test statistic** measures how far the estimate is from H₀, in standard errors: t = (x̄ − μ₀) / (s / √n) = (25.004 − 25.000) / 0.002 = **2.00**.
- The **p-value** is the probability of a result at least this far from H₀ if H₀ were true. Here **p = 0.0639**.
- The **significance level** α, chosen before the test, is the risk of wrongly rejecting a true H₀ that you accept: usually 0.05.

**The decision.** If p < α, reject H₀. Here p = 0.0639 is more than α = 0.05, so we **do not reject** H₀: the data do not show that the machine is off center. The sample mean is 0.004 mm above 25.000 mm, but a difference that size could easily come from sampling alone.

[[figure: st.m1.inference.test 1]]

[[results: st.m1.inference.test]]

The test and the interval agree: 25.000 mm is inside the 95 % interval, and the two-sided test at α = 0.05 does not reject μ = 25.000 mm. That is always so for this pair.

## Interpreting results practically

A statistical result is only the start of an engineering decision.

- **"Do not reject H₀" does not prove H₀.** It means the data are not strong enough to show a difference. With more housings, a real 0.004 mm offset might be detected.
- **Statistical significance is not practical significance.** If the bore tolerance is 25.000 ± 0.020 mm, an offset of 0.004 mm hardly matters, whatever the p-value says. With a very large sample, even a 0.001 mm offset becomes "significant" while still being irrelevant to the part.
- **Compare the interval with the tolerance.** The whole interval, 24.9997 to 25.0083 mm, is well inside 24.980 to 25.020 mm. The decision does not depend on where in that interval μ really is: the process is centered well enough.
- **Mind the individuals.** Conformance is decided part by part. A mean inside the tolerance with a large standard deviation can still make many parts out of tolerance (item 1.3).

## Sampling matters

Inference is only as good as the sample. The sixteen housings must represent the day's production: taken at random, or spread over the whole shift. Sixteen housings taken in the first ten minutes would say nothing about the afternoon, when the tool has worn. A **biased** sample cannot be fixed by calculation.

## Connecting inference to decisions

| Engineering question | Tool |
|---|---|
| What is the mean strength of this batch, and how precisely do we know it? | confidence interval |
| Has the process mean moved from its target? | hypothesis test |
| Does the new supplier's material differ from the old one's? | two-sample test (Module 10) |
| How many specimens do we need to know the mean within ±2 MPa? | sample size (Module 7) |

## In Python and R

```python
import numpy as np
from scipy import stats
xbar, s, n = 25.004, 0.008, 16
se = s / np.sqrt(n)                          # 0.002
t_crit = stats.t.ppf(0.975, n - 1)           # 2.1314
print(xbar - t_crit * se, xbar + t_crit * se)   # 24.9997 25.0083
t = (xbar - 25.000) / se                     # 2.00
print(2 * stats.t.sf(abs(t), n - 1))         # p-value 0.0639
```

```r
xbar <- 25.004; s <- 0.008; n <- 16
se <- s / sqrt(n)
t_crit <- qt(0.975, n - 1)                   # 2.1314
c(xbar - t_crit * se, xbar + t_crit * se)    # 24.9997 25.0083
t <- (xbar - 25.000) / se                    # 2.00
2 * pt(abs(t), n - 1, lower.tail = FALSE)    # p-value 0.0639
```

With the measurements themselves, rather than x̄ and s, `stats.ttest_1samp(x, 25.0)` in Python and `t.test(x, mu = 25)` in R give the test and the interval at once.

## Common mistakes

- Saying "there is a 95 % probability that μ is in this interval" as if μ moved. μ is fixed; the interval is what varies from sample to sample.
- Reading the confidence interval for the mean as the range of individual parts.
- Saying "we accept H₀" or "the test proves the mean is 25.000 mm".
- Reporting only "p < 0.05" without the size of the effect and its units.
- Drawing conclusions from a convenience sample that does not represent the process.

## What to do next

Open the **Example** tab and press **Open in the Workbench**. Change the sample size from 16 to 64 and watch the interval narrow. Then answer the interpretation questions on the **Exercise** tab.
