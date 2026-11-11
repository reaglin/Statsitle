---
id: st.m5.applying
title: Applying continuous distributions
minutes: 65
objectives: [st.m5.o3, st.m5.o4]
builds-on: st.m5.calculating
example: st.m5.applying.eg
exercise: st.m5.applying.ex
reference: [which-continuous-distribution, weibull-and-fitting, exponential-distribution]
---

# Applying continuous distributions

A real problem does not say "use the normal". It gives a situation, and often a column of measurements. You choose a model that suits the quantity, check it against the data, calculate, and say what the result means for the decision. This item adds one more model, the **Weibull**, and shows how to **fit** a model to data and judge the fit. The course's *Fit to Predict* game, in Module 5 on the course website (reaglin.github.io/EGN3443), walks through the same steps: explore, choose a candidate, validate, predict, recommend.

## Choosing the model

| Model | The quantity | Shape | Clues in the words |
|---|---|---|---|
| **Uniform** U(a, b) | equally likely anywhere in a range | flat | "at random within", "between a and b, no value favoured", a rounding error |
| **Normal** N(μ, σ²) | a measurement scattered around a target by many small effects | symmetric bell | dimensions, fill volumes, strengths, measurement errors |
| **Exponential** Exp(λ) | the waiting time to a random event; the life of a part that fails at random | skewed right, highest at 0 | "at random", "on average λ per hour", a mean time between failures, no wear |
| **Weibull** (β, η) | the life of a part that wears out (or fails early); the strength of a brittle material | flexible; skewed right for β < 3.6 | bearings, belts, fatigue, "wears out", "infant mortality" |

Three questions sort most cases:

1. **Can it be negative, and is it symmetric?** A symmetric measurement around a target: normal. A time or a life, never negative and skewed right: exponential or Weibull.
2. **Do failures depend on age?** No, the part fails at random and an old one is as good as new: exponential. Yes, it wears out: Weibull with β > 1.
3. **Is every value in a range equally likely?** Uniform.

The Workbench also offers the **lognormal** (a quantity whose logarithm is normal, such as particle sizes or repair times). It is one more candidate when the data are skewed.

## The Weibull distribution

The Weibull has a **shape β** and a **scale η** (in the units of X):

P(X ≤ x) = 1 − e^(−(x/η)^β)  R(x) = P(X > x) = e^(−(x/η)^β)

- **R(x), the reliability**, is the chance a part is still working at age x.
- **The scale η is the characteristic life:** R(η) = e⁻¹ = 0.368, so 63.2 % of parts have failed by age η, whatever β is.
- **The shape β says how the failure rate changes with age.** The **hazard rate** h(x) = (β/η)(x/η)^(β−1) is the rate of failure at age x among the parts still running:

| β | Hazard rate | Meaning |
|---|---|---|
| β < 1 | falls with age | early failures, "infant mortality": weak parts fail first |
| β = 1 | constant | failures at random: the Weibull **is the exponential**, with λ = 1/η |
| β > 1 | rises with age | wear-out: bearings, belts, fatigue (often β = 1.1 to 3) |

The **B10 life**, the age by which 10 % have failed, is the percentile for q = 0.10: x = η(−ln 0.90)^(1/β). Bearing makers rate their products by it.

## A worked case by hand

A pump impeller wears out with a Weibull life, β = 2 and η = 10 000 hours.

1. **Reliability at 5000 hours:** R(5000) = e^(−(5000/10 000)²) = e^(−0.25) = **0.7788**. About 78 % of impellers are still in service at 5000 hours.
2. **B10 life:** x = 10 000 × (−ln 0.90)^(1/2) = 10 000 × √0.10536 = **3246 hours**.
3. **Hazard rate at 5000 hours:** h = (2/10 000) × (5000/10 000)¹ = **0.0001 per hour**. At 10 000 hours it is twice that: with β = 2 the rate climbs in a straight line with age.
4. **At the characteristic life:** R(10 000) = e⁻¹ = **0.368**.

## The Example: the life of a fan bearing

A ventilation fan's bearing has a Weibull life with β = 1.5 and η = 8000 hours. The shaded area is the share that **fail** by 2000 hours; the rest are still running:

[[figure: st.m5.applying.eg 1]]

[[results: st.m5.applying.eg]]

R(2000) = e^(−(2000/8000)^1.5) = e^(−0.125) = **0.8825**: 88 % of bearings run past 2000 hours. The B10 life is 1785 hours. The hazard rate at 2000 hours is 0.000094 per hour, and it rises with age, because β > 1. A maintenance planner would replace these bearings on a schedule, before they fail. For an exponential part (β = 1) a schedule would gain nothing: a new part is no better than the old one.

## Fitting a model to data

When there are measurements, the parameters come from the data. The Workbench estimates them by **maximum likelihood**: the values that make the observed data most probable.

| Family | Fitted parameters |
|---|---|
| Normal | μ̂ = x̄, and σ̂ = √(Σ(x − x̄)²/n). σ̂ **divides by n**, as scipy's `stats.norm.fit` does, so it is a little smaller than the sample s of Module 2, which divides by n − 1 |
| Exponential | λ̂ = 1/x̄ |
| Uniform | â = the smallest value, b̂ = the largest |
| Weibull | β̂ and η̂ solve an equation with no closed form; the computer finds them |

Then **check the fit**. A model can always be fitted; the question is whether it suits the data.

1. **The histogram with the fitted density** (figure 3 in the Workbench). Does the curve follow the bars, especially in the tails?
2. **The Q–Q plot** (figure 4): the sorted data against the values the model predicts at the same positions. If the model fits, the points lie near a **straight line**. A bend at either end says the tails are wrong.
3. **The Q–Q correlation r**, how straight that plot is. Near 1 (0.98 or more for 20 to 50 values) the family suits the data. **Compare families on the same data: the larger r fits better.**
4. The **Kolmogorov–Smirnov D**, the largest gap between the data's cumulative proportion and the fitted CDF. Smaller is better. (The Workbench gives no p-value for it: with fitted parameters the usual one is too large.)

When two families fit almost equally well, take the **simpler** one, and let the physics decide: a part that wears out is not exponential, however the numbers fall.

## A fit worked: the lives of pump seals

A refinery recorded the hours to failure of the mechanical seals on 30 process pumps:

[[data: st.m5.applying.exp]]

**The exponential, fitted** (λ̂ = 1/x̄ = 1/956.3 = 0.001046 per hour):

[[figure: st.m5.applying.exp 3]]

[[figure: st.m5.applying.exp 4]]

The exponential puts its peak at 0. It expects 1 − e^(−0.001046 × 200) = 19 % of seals, about 6 of the 30, to fail within 200 hours, but only 2 did; and it expects some to last well past 2100 hours, where none did. The Q–Q plot bends, and r = **0.9304**.

**The Weibull, fitted:**

[[figure: st.m5.applying.weibull 3]]

[[figure: st.m5.applying.weibull 4]]

[[results: st.m5.applying.weibull]]

β̂ = **1.716** and η̂ = 1067 hours. The curve follows the bars, the Q–Q points lie close to the line, and r = **0.9831**, against 0.9304 for the exponential; D is 0.119 against 0.229. β̂ well above 1 says the seals **wear out**.

**Why it matters.** The two models give different answers to the planner's question, "what share of seals reach 500 hours?": the Weibull says R(500) = 0.761, the exponential 0.593. The wrong model would schedule far too many seal changes, or, for other questions, far too few.

## In Python and R

```python
import numpy as np
from scipy import stats
data = np.array([456, 1645, 1441, 651, 992])            # … all 30 lives
beta, loc, eta = stats.weibull_min.fit(data, floc=0)    # β̂ and η̂ (loc fixed at 0)
X = stats.weibull_min(beta, scale=eta)
print(X.sf(500))                                        # reliability R(500)
p = (np.arange(1, len(data) + 1) - 0.5) / len(data)
print(np.corrcoef(X.ppf(p), np.sort(data))[0, 1])       # Q–Q correlation r
```

```r
data <- c(456, 1645, 1441, 651, 992)                    # … all 30 lives
fit <- MASS::fitdistr(data, "weibull")                  # close to the ML values
beta <- fit$estimate["shape"]; eta <- fit$estimate["scale"]
1 - pweibull(500, beta, eta)                            # R(500)
p <- (seq_along(data) - 0.5) / length(data)
cor(qweibull(p, beta, eta), sort(data))                 # Q–Q correlation r
```

scipy calls the shape `c` and R calls it `shape`; both call η the `scale`. `MASS` comes with R. The full script for the seals' Weibull fit, below, finds β̂ from the maximum-likelihood equation itself, in base R, so it matches the Workbench to more figures than `fitdistr`:

[[code: st.m5.applying.weibull]]

## In the Workbench

Choose **Continuous distribution**. For a typed model, set **Distribution** to *Weibull* and type the **Shape (β)** and **Scale (η)**; **P(X > x)** is the reliability R(x), and the **Hazard rate h(x)** is shown. To fit, click **+ Data table** on the left and paste the data (an Exercise's **Copy the data** button copies it with its header), then set **Parameters from** to *Fitted to a data column* and choose the **Column**. Change **Distribution** to compare families on the same column: the fitted parameters, **Q–Q correlation (r)** and the four figures follow. Ctrl+L brings you back to the Course.

## Communicating the result

A good report names the model and why, shows the evidence for the fit, and answers the question in the problem's terms: "The seal lives fit a Weibull with β̂ = 1.72 and η̂ = 1067 h (Q–Q r = 0.983, against 0.930 for the exponential). The seals wear out. 76 % reach 500 hours; to keep failures in service below 10 %, replace them at about 290 hours, the B10 life."

## Common mistakes

- Choosing the normal for lives or waiting times because "most things are normal". Lives are never negative and usually skewed.
- Choosing the exponential for parts that wear out.
- Accepting a fit without looking at the Q–Q plot. A bend in the tails matters most, because the tails are where failures and out-of-spec parts are.
- Reading r alone: compare families on the same data, and say why the winner makes physical sense.
- Giving R(x) when the question asks the share that **fail** by x, or the reverse.

## What to do next

On the **Exercise** tab, a mine tested 25 conveyor idler bearings to failure. Fit the Weibull, find β̂, η̂, the Q–Q correlation and the reliability at 1500 hours, then fit the exponential to the same data and decide which model to use.
