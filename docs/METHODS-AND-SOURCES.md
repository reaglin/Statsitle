# Statistle — methods and sources

The register of every statistic the app computes: the formula as the course states it, where it
comes from, the Python and R that reproduce it, and the worked case that holds it in the tests.
An analysis is not done until its row here is complete. Help text in the code points the
student at the same sources.

**Sources used throughout**

- **EGN3443** — Ron Eaglin, *Probability and Statistics for Engineers*, course modules and sample
  problems, `github.com/reaglin/egn3443` (cited as *EGN3443 Module n*).
- **M&R** — Montgomery, D. C. and Runger, G. C., *Applied Statistics and Probability for
  Engineers*, 7th ed., Wiley, 2018 (cited by chapter and section).
- **NIST** — NIST/SEMATECH *e-Handbook of Statistical Methods*,
  `https://www.itl.nist.gov/div898/handbook/` (cited by section number).
- **SciPy** — `scipy.stats` reference; **statsmodels** reference; **R** — base `stats` package
  documentation (`?t.test` etc.).

Conventions the code and the help state explicitly, because Python and R differ:
- Sample variance divides by n − 1 (`np.var(x, ddof=1)`, `statistics.stdev`, R `var`). Population
  variance is shown separately.
- Quartiles: the course method (Q1 and Q3 as medians of the lower and upper halves, excluding the
  median for odd n — Tukey's hinges) *and* R's default type 7 (`np.percentile` default). Both are
  shown; the help says which is which.
- Skewness and kurtosis: the adjusted (sample) estimators, `scipy.stats.skew(x, bias=False)`,
  `kurtosis(x, bias=False)` (excess); R via `e1071::skewness(x, type = 2)`.
- Two-sample t: the app asks *equal variances?*; Python `ttest_ind(a, b, equal_var=...)`, R
  `t.test(a, b, var.equal = ...)`. Welch is the default in R and the app; pooled is what the
  course teaches first, so both are one click.
- Proportion tests use the normal approximation without continuity correction (the course's
  hand formula); R's `prop.test(..., correct = FALSE)` is generated to match.

Status: `[ ]` not built · `[x]` built, tested against the case named. The Python column is also run against the app by `CodeGenerationTests` on a machine with numpy and scipy.

---

## Descriptive statistics (Module 2; M&R Ch. 6; NIST 1.3.5)

| Statistic | Formula | Python | R | Held by | Status |
|---|---|---|---|---|---|
| Mean | x̄ = Σxᵢ / n | `np.mean(x)` | `mean(x)` | `SummaryStatisticsTests` (hand set 2,4,4,4,5,5,7,9) | [x] |
| Median | middle value (mean of the two middle for even n) | `np.median(x)` | `median(x)` | `SummaryStatisticsTests` | [x] |
| Mode | most frequent value(s); none if all distinct | `stats.mode(x, keepdims=False)` | `names(which.max(table(x)))` | `SummaryStatisticsTests` (ties, none) | [x] |
| Range | max − min | `np.ptp(x)` | `diff(range(x))` | `SummaryStatisticsTests` | [x] |
| Sample variance | s² = Σ(xᵢ − x̄)² / (n − 1) | `np.var(x, ddof=1)` | `var(x)` | `SummaryStatisticsTests` (32/7) | [x] |
| Sample SD | s = √s² | `np.std(x, ddof=1)` | `sd(x)` | `SummaryStatisticsTests` | [x] |
| Population variance / SD | σ² = Σ(xᵢ − μ)² / n | `np.var(x)`, `np.std(x)` | `var(x)*(n-1)/n` | `SummaryStatisticsTests` (σ = 2) | [x] |
| Coefficient of variation | CV = s / x̄ (as %) | `np.std(x, ddof=1)/np.mean(x)` | `sd(x)/mean(x)` | `SummaryStatisticsTests`; NaN at mean 0 | [x] |
| Standard error of the mean | s / √n | `stats.sem(x)` | `sd(x)/sqrt(length(x))` | `SummaryStatisticsTests` | [x] |
| Quartiles, IQR | Tukey hinges and type-7; IQR = Q3 − Q1 | `np.percentile(x, [25, 75])` | `quantile(x)`; `fivenum(x)` | `SummaryStatisticsTests` — R `fivenum` 2 4 4.5 6 9 and `quantile` 4 4.5 5.5 by hand | [x] |
| Percentile p | type 7: position 1 + (n − 1)p | `np.percentile(x, p)` | `quantile(x, p/100)` | `SummaryStatisticsTests` | [x] |
| Outlier fences | Q1 − 1.5·IQR, Q3 + 1.5·IQR | — | `boxplot.stats(x)$out` | `SummaryStatisticsTests` (40 beyond the fence) | [x] |
| Skewness | g₁ adjusted: [n/((n−1)(n−2))] Σ((xᵢ−x̄)/s)³ | `stats.skew(x, bias=False)` | `e1071::skewness(x, type=2)` | Excel's documented SKEW example: 0.359543 | [x] |
| Kurtosis (excess) | g₂ adjusted (as Excel KURT) | `stats.kurtosis(x, bias=False)` | `e1071::kurtosis(x, type=2)` | Excel's documented KURT example: −0.151799 | [x] |
| Mode as a number (plan 10.16) | the one value more frequent than every other; blank (NaN) for no mode or a tie | `values, counts = np.unique(x, return_counts=True)`; the single top value | `tab <- table(x)`; the single `names(tab)[tab == max(tab)]` | `DescriptiveAdditionsTests` (5 of 3 5 5 7 8; tie and none blank); Python cross-check | [x] |
| Number of outliers (plan 10.16) | count of xᵢ < Q1 − 1.5·IQR or > Q3 + 1.5·IQR, type-7 quartiles (the same fences as the Outliers list and the box plot) | `outliers.size` | `length(outliers)` | `DescriptiveAdditionsTests` (530 in the eleven alloy values; two in a set with −40 and 60); Python cross-check | [x] |
| Control limits, individuals chart (plan 10.11) | LCL = x̄ − 3s, UCL = x̄ + 3s, s the sample SD (n − 1); points outside counted; M&R §15-3 (Shewhart charts) estimate σ by the average moving range / d₂ instead — the help says so | `np.mean(x) - 3 * np.std(x, ddof=1)` | `mean(x) - 3 * sd(x)` | `DescriptiveAdditionsTests` (5 ∓ 3√(32/7); one point beyond the UCL marked on the chart, figure 3); Python cross-check | [x] |
| Frequency table, histogram | Sturges k = ⌈log₂n + 1⌉; √n rule | `np.histogram(x, bins='sturges')` | `hist(x, breaks="Sturges")` | `SummaryStatisticsTests` (Sturges 8→4, 20→6; classes partition n) | [x] |

## Probability rules (Module 3; M&R Ch. 2)

| Rule | Formula | Python | R | Held by | Status |
|---|---|---|---|---|---|
| Complement | P(A′) = 1 − P(A) | arithmetic | arithmetic | `DistributionTests`; M&R 2-3 | [x] |
| Addition | P(A∪B) = P(A) + P(B) − P(A∩B) | | | `DistributionTests`; M&R 2-3 | [x] |
| Multiplication, conditional | P(A∩B) = P(A|B)·P(B); P(A|B) = P(A∩B)/P(B) | `DistributionTests` | [x] | M&R 2-4, 2-5 | [ ] |
| Independence | P(A∩B) = P(A)·P(B) | | | `DistributionTests`; M&R 2-6 | [x] |
| Bayes (two events) | P(A|B) = P(B|A)P(A) / [P(B| `DistributionTests`; A)P(A) + P(B | [x] | | | M&R 2-7 | [ ] |
| Permutations, combinations | nPr = n!/(n−r)!, nCr = n!/(r!(n−r)!) | `math.perm`, `math.comb` | `factorial`, `choose` | `DistributionTests`; M&R 2-1.4 | [x] |

## Discrete distributions (Module 4; M&R Ch. 3)

| Distribution | PMF, mean, variance | Python (`scipy.stats`) | R | Held by | Status |
|---|---|---|---|---|---|
| Binomial(n, p) | C(n,x)pˣ(1−p)ⁿ⁻ˣ; np; np(1−p) | `binom.pmf/cdf/sf(k, n, p)` | `dbinom/pbinom(k, n, p)` | `DistributionTests`; EGN3443 M4 P1: B(20, .05), P(X=2) = 0.1887 | [x] |
| Poisson(λ) | e^−λ λˣ/x!; λ; λ | `poisson.pmf/cdf(k, mu)` | `dpois/ppois(k, lambda)` | `DistributionTests`; M&R Ex. 3-33 | [x] |
| Geometric(p) | (1−p)ˣ⁻¹p; 1/p; (1−p)/p² | `geom.pmf(k, p)` | `dgeom(k-1, p)` (R counts failures) | `DistributionTests`; M&R 3-7.1 | [x] |
| Negative binomial(r, p) | C(x−1,r−1)pʳ(1−p)ˣ⁻ʳ; r/p | `nbinom.pmf(k-r, r, p)` | `dnbinom(k-r, r, p)` | `DistributionTests`; M&R 3-7.2 | [x] |
| Hypergeometric(N, K, n) | C(K,x)C(N−K,n−x)/C(N,n) | `hypergeom.pmf(k, N, K, n)` | `dhyper(k, K, N-K, n)` | `DistributionTests`; M&R 3-8 | [x] |
| Typed PMF | E[X] = Σx·p(x); V = Σ(x−μ)²p(x) | `np.sum(x*p)` | `sum(x*p)` | `DistributionTests`; M&R Ex. 3-6 | [x] |

## Continuous distributions (Module 5; M&R Ch. 4)

| Distribution | PDF / CDF, mean, variance | Python | R | Held by | Status |
|---|---|---|---|---|---|
| Uniform(a, b) | 1/(b−a); (a+b)/2; (b−a)²/12 | `uniform.cdf(x, a, b-a)` | `punif(x, a, b)` | `DistributionTests`; M&R Ex. 4-8 | [x] |
| Normal(μ, σ) | Φ((x−μ)/σ); z-scores; 68–95–99.7 | `norm.cdf(x, mu, sigma)`, `norm.ppf(q)` | `pnorm`, `qnorm` | `DistributionTests`; Φ(1.96) = 0.97500 | [x] |
| Exponential(λ) | 1 − e^−λx; 1/λ; 1/λ²; memoryless | `expon.cdf(x, scale=1/lam)` | `pexp(x, rate=lam)` | `DistributionTests`; M&R Ex. 4-21 | [x] |
| Lognormal | | `lognorm` | `plnorm` | `DistributionTests`; M&R 4-11 | [x] |
| t(ν), χ²(ν), F(ν₁, ν₂) | for inference | `t.ppf`, `chi2.ppf`, `f.ppf` | `qt`, `qchisq`, `qf` | `DistributionTests`; t₀.₀₂₅,₁₀ = 2.228; χ²₀.₀₅,₁₀ = 18.307 | [x] |
| Weibull(β, η) (plan 10.9) | 1 − e^(−(x/η)^β); η·Γ(1 + 1/β); η²[Γ(1 + 2/β) − Γ(1 + 1/β)²]; M&R §4-10. Shape β = R's `shape` = scipy's `c`; scale η = R's and scipy's `scale` | `weibull_min(c=shape, scale=scale)` | `pweibull(x, shape, scale)` | `WeibullAndFitTests`: β 1.5, η 1000, x 500 → F 0.297811, f 0.000744783, q₀.₉ 1743.72, mean 902.745, var 375690.28 (scipy = R) | [x] |
| Reliability and hazard rate | R(x) = P(X > x); h(x) = f(x)/R(x) = (β/η)(x/η)^(β−1) for the Weibull, λ for the exponential | `X.pdf(x) / X.sf(x)` | `dweibull(x, ...) / (1 - pweibull(x, ...))` | `WeibullAndFitTests`: h(500) = 0.00106066 for β 1.5, η 1000; β = 1 gives the exponential | [x] |
| Fit to a data column: normal | μ̂ = x̄, σ̂ = √(Σ(x − x̄)²/n) — maximum likelihood, divides by **n** (s = σ̂√(n/(n−1))); M&R §7-4.2 | `stats.norm.fit(data)` | `mean`, `sqrt(mean((x - mean(x))^2))` | `WeibullAndFitTests`: 12 failure times → 120.6, 47.3056 | [x] |
| Fit: exponential, uniform, lognormal | λ̂ = 1/x̄; â = min, b̂ = max; μ̂, σ̂ the mean and the ÷n SD of ln x | `expon.fit(data, floc=0)` (scale x̄); `np.min`/`np.max` (= `uniform.fit`); `lognorm.fit(data, floc=0)` (s, scale e^μ̂) | `1/mean(x)`; `min`/`max`; `mean(log(x))`, ÷n SD | `WeibullAndFitTests`: λ̂ 0.00829187; 41.2, 210.3; 4.70357, 0.443378 | [x] |
| Fit: Weibull | β̂ solves Σx^β ln x / Σx^β − 1/β − mean(ln x) = 0 (Newton inside a bracket; data ÷ mean first), η̂ = (Σx^β̂/n)^(1/β̂); Lawless (2003) §5.2 | `brentq` on the shape equation; `weibull_min.fit(data, floc=0)` agrees to ~6 figures (general optimizer) | `uniroot` on the shape equation (base R; `MASS::fitdistr` stops sooner: 2.7929) | `WeibullAndFitTests`: β̂ 2.7914457, η̂ 135.727722 (R uniroot = scipy brentq); scipy `fit` 2.7914429, 135.727646 | [x] |
| Q–Q plot and its correlation | sorted data against the fitted model's quantiles at (i − 0.5)/n (M&R §6-6 positions), r = Pearson correlation; reference line y = x | `np.corrcoef(X.ppf(p), np.sort(data))[0, 1]` | `cor(qweibull(p, ...), sort(x))` | `WeibullAndFitTests`: Weibull 0.999419, normal 0.997581, lognormal 0.983049, exponential 0.951434, uniform 0.990755 | [x] |
| Kolmogorov–Smirnov D | D = maxᵢ max(i/n − F(x₍ᵢ₎), F(x₍ᵢ₎) − (i−1)/n). **No p-value**: with parameters fitted to the same data it is too large (Lilliefors) | `stats.kstest(data, X.cdf).statistic` | `ks.test(x, function(v) pweibull(v, ...))$statistic` | `WeibullAndFitTests`: Weibull 0.0677313, normal 0.0828842, lognormal 0.113536, exponential 0.333319, uniform 0.129608 | [x] |

## Sampling distributions (Module 6; M&R Ch. 7)

| Item | Formula | Python | R | Held by | Status |
|---|---|---|---|---|---|
| Mean and SE of x̄ | μ_x̄ = μ; σ_x̄ = σ/√n | `norm.cdf(xbar, mu, sigma/np.sqrt(n))` | `pnorm(xbar, mu, sigma/sqrt(n))` | `DistributionTests`; EGN3443 M6 P1 (4200, 350, n 5 → 30) | [x] |
| Sampling distribution of p̂ | p; √(p(1−p)/n) | | | `DistributionTests`; M&R 7-2 | [x] |
| CLT simulation | k samples of n, histogram of means | `rng.normal(...)` | `replicate(k, mean(rnorm(n, ...)))` | the Simulation analysis, below (`SimulationTests`) | [x] |

## Simulation (Modules 3, 6, 7, 8; plan 10.7)

Random numbers are the program's own: **xoshiro256\*\*** (Blackman and Vigna, *ACM TOMS* 47(4), 2021) seeded by **SplitMix64** (Steele, Lea and Flood, 2014), in `Core/Methods/Chance/RandomStream.cs`, so a seeded result is the same on every computer and every .NET version. Python and R draw their own numbers (`np.random.default_rng(seed)`, `set.seed(seed)`), so simulated counts differ from the program's; the generated code says so, and the cross-check compares only the theoretical values (results marked `[SimulatedResult]` need only their labels).

| Item | Formula | Python | R | Held by | Status |
|---|---|---|---|---|---|
| Random stream | xoshiro256\*\*, SplitMix64 seeding; integers by rejection (no modulo bias); uniforms k·2⁻⁵³; normals by Box–Muller; exponentials −ln(1 − U)/λ | `np.random.default_rng(seed)` | `set.seed(seed)` | `SimulationTests`: the reference outputs from state {1, 2, 3, 4} (11520, 0, 1509978240, …), SplitMix64(0) = 0xE220A8397B1DCDAF, seed 1's first three outputs | [x] |
| Experimental probability | (times the event happened) / trials; difference from theory ≈ 2√(p(1 − p)/trials) | `np.mean(event)` | `mean(event)` | `SimulationTests` (pinned counts; within 0.01 of theory at 100 000 trials, three seeds) | [x] |
| Coins: heads in m flips | Binomial(m, ½): C(m, k)/2ᵐ | `stats.binom.pmf(k, m, 0.5)` | `dbinom(k, m, 0.5)` | `SimulationTests` (3 coins, P(≥ 2) = ½; seed 1: 499 heads in 1000) | [x] |
| Dice: the sum of d dice | ways to roll s (convolution) / 6ᵈ | `itertools.product(range(1, 7), repeat=d)` | `expand.grid(rep(list(1:6), d))` | `SimulationTests` (P(7) = 6/36, P(10 with 3) = 27/216; seed 1: 168 sevens in 1000) | [x] |
| Cards: best total (J, Q, K = 10; one ace 11 if ≤ 21) | every ordered deal of ranks weighted 4·3·… for repeats, over 52·51·…; three-card 21 = 2012/22 100 = 0.0910, two-card = 64/1326 | `itertools.product(range(1, 14), repeat=h)` with weights | `expand.grid(rep(list(1:13), h))` with weights | `SimulationTests` (both by brute force over all hands; seed 1: 93 in 1000) | [x] |
| Cards: hearts, aces in a hand | Hypergeometric(52, 13 or 4, h) | `stats.hypergeom.pmf(k, 52, K, h)` | `dhyper(k, K, 52 - K, h)` | `SimulationTests` (one heart in three = 9633/22 100; ≥ 1 ace in five = 1 − C(48,5)/C(52,5)) | [x] |
| Darts: inside the unit circle | P = π/4; π̂ = 4 × proportion | `x**2 + y**2 <= 1` | `x^2 + y^2 <= 1` | `SimulationTests` (seed 1, 10 000 darts: 7914, π̂ = 3.1656) | [x] |
| Sample means (CLT) | mean of x̄ ≈ μ; SD of x̄ (n − 1) ≈ σ/√n: normal σ, uniform (b − a)/√12, exponential 1/λ | `rng.normal/uniform/exponential(…, size=(k, n)).mean(axis=1)` | `replicate(k, mean(rnorm/runif/rexp(n, …)))` | `SimulationTests` (seed 1, exponential λ = 1, n = 30: 0.9962 and 0.1790 against 1 and 0.1826; uniform 2–8, n = 12: SE 0.5) | [x] |
| Interval coverage | share of t intervals x̄ ± t_{α/2,n−1}·s/√n containing μ → confidence level (M&R §8-2) | `stats.t.ppf(1 - (1 - conf)/2, n - 1)` | `t.test(x, conf.level = conf)$conf.int` | `SimulationTests` (seed 1, n = 10, 95 %: 949 of 1000) | [x] |
| Familywise error | m independent tests of true nulls at α: P(≥ 1 rejection) = 1 − (1 − α)ᵐ; rejections ~ Binomial(m, α) | `np.abs(t) > stats.t.ppf(1 - alpha/2, n - 1)` | `t.test(rnorm(n))$p.value < alpha` | `SimulationTests` (seed 1, m = 10, n = 10: 410 of 1000 against 0.4013) | [x] |

## Confidence intervals and sample size (Module 7; M&R Ch. 8)

| Interval | Formula | Python | R | Held by | Status |
|---|---|---|---|---|---|
| Mean, σ known | x̄ ± z_{α/2} σ/√n | `norm.interval(conf, xbar, sigma/np.sqrt(n))` | `xbar + c(-1,1)*qnorm(1-a/2)*sigma/sqrt(n)` | `InferenceTests`; M&R Ex. 8-1 | [x] |
| Mean, σ unknown | x̄ ± t_{α/2,n−1} s/√n | `t.interval(conf, n-1, xbar, stats.sem(x))` | `t.test(x, conf.level)` | `InferenceTests`; M&R Ex. 8-5 | [x] |
| Proportion (Wald) | p̂ ± z√(p̂(1−p̂)/n) | `proportion_confint(k, n, method='normal')` | `prop.test(k, n, correct=FALSE)` (Wilson) | `InferenceTests`; M&R Ex. 8-7 | [x] |
| Variance / SD | (n−1)s²/χ²_{α/2} … | `chi2.ppf` | `qchisq` | `InferenceTests`; M&R Ex. 8-6 | [x] |
| Sample size, mean | n = (z σ / E)² | arithmetic | arithmetic | `InferenceTests`; M&R Ex. 8-2 | [x] |
| Sample size, proportion | n = (z/E)² p(1−p) (0.25 worst case) | | | `InferenceTests`; M&R Ex. 8-8 | [x] |

## One-sample tests (Modules 8–9; M&R Ch. 9)

| Test | Statistic | Python | R | Held by | Status |
|---|---|---|---|---|---|
| z-test for a mean | z = (x̄ − μ₀)/(σ/√n) | `norm.sf(abs(z))*2` | `2*pnorm(-abs(z))` | `InferenceTests`; EGN3443 M9 P1: z = 2.40, p = 0.0164 | [x] |
| t-test for a mean | t = (x̄ − μ₀)/(s/√n), ν = n−1 | `ttest_1samp(x, mu0, alternative=)` | `t.test(x, mu=mu0, alternative=)` | `InferenceTests`; M&R Ex. 9-6 | [x] |
| z-test for a proportion | z = (p̂ − p₀)/√(p₀(1−p₀)/n) | `proportions_ztest(k, n, p0)` | `prop.test(k, n, p0, correct=FALSE)` | `InferenceTests`; EGN3443 M8 P1 | [x] |
| χ² test for a variance | χ² = (n−1)s²/σ₀² | `chi2.sf` | `pchisq` | `InferenceTests`; M&R Ex. 9-8 | [x] |
| Power and β | β = Φ(z_{α/2} − δ√n/σ) − Φ(−z_{α/2} − δ√n/σ) | `statsmodels.stats.power` | `power.t.test` | `InferenceTests`; M&R Ex. 9-3 | [x] |

## Two-sample tests (Modules 9–10; M&R Ch. 10)

| Test | Statistic | Python | R | Held by | Status |
|---|---|---|---|---|---|
| Independent t, pooled | t = (x̄₁−x̄₂)/(s_p√(1/n₁+1/n₂)), s_p² pooled, ν = n₁+n₂−2 | `ttest_ind(a, b, equal_var=True)` | `t.test(a, b, var.equal=TRUE)` | `InferenceTests`; M&R Ex. 10-5 | [x] |
| Welch t | ν by Welch–Satterthwaite | `ttest_ind(a, b, equal_var=False)` | `t.test(a, b)` | `InferenceTests`; M&R Ex. 10-6 | [x] |
| Paired t | on differences | `ttest_rel(a, b)` | `t.test(a, b, paired=TRUE)` | `InferenceTests`; M&R Ex. 10-9 | [x] |
| Two-sample z | σ₁, σ₂ known | `norm` | `pnorm` | `InferenceTests`; M&R Ex. 10-1 | [x] |
| Two proportions | pooled p̂ = (k₁+k₂)/(n₁+n₂) | `proportions_ztest([k1,k2],[n1,n2])` | `prop.test(c(k1,k2), c(n1,n2), correct=FALSE)` | `InferenceTests`; M&R Ex. 10-13 | [x] |
| F-test, two variances | F = s₁²/s₂² | `f.sf` | `var.test(a, b)` | `InferenceTests`; M&R Ex. 10-11 | [x] |
| Cohen's d | (x̄₁−x̄₂)/s_p | arithmetic | `effectsize::cohens_d` | `InferenceTests`; EGN3443 M10 P1 | [x] |
| Shapiro–Wilk; Levene | | `shapiro(x)`, `levene(a, b)` | `shapiro.test`, `car::leveneTest` | `InferenceTests` | [x] |
| Mann–Whitney U; Wilcoxon signed-rank | | `mannwhitneyu`, `wilcoxon` | `wilcox.test` | `InferenceTests`; M&R 10-3 (rank-sum) and 9-9 (signed-rank) | [x] |

## Chi-square tests (Modules 8–10; M&R §9-7, §9-8; plan 10.8)

Pearson's χ² without a continuity correction (the course's hand formula), so the Python is `chi2_contingency(..., correction=False)` and the R `chisq.test(..., correct = FALSE)`; Yates' correction is a choice for a 2 × 2 table only. Entered goodness-of-fit proportions are always rescaled to add to 1 (R `rescale.p = TRUE`); when they add to something more than 0.01 from 1 they are read as weights and the Notes say so.

| Item | Formula | Python | R | Held by | Status |
|---|---|---|---|---|---|
| Goodness of fit | Eᵢ = n·pᵢ (pᵢ = 1/k when equal); χ² = Σ(Oᵢ − Eᵢ)²/Eᵢ; df = k − 1 − m (m parameters estimated from the data) | `stats.chisquare(observed, f_exp=n*p, ddof=m)` | `chisq.test(observed, p = p)`; `pchisq(stat, k - 1 - m, lower.tail = FALSE)` for m > 0 | `ChiSquareTests`: M&R Ex. 9-12 (Poisson defects, m = 1: χ² = 2.94, df 1, p = 0.0865); fair die χ² = 4.2 by hand; cross-check | [x] |
| Independence / homogeneity | Eᵢⱼ = (row i total)(column j total)/n; χ² = ΣΣ(Oᵢⱼ − Eᵢⱼ)²/Eᵢⱼ; df = (r − 1)(c − 1) | `stats.chi2_contingency(table, correction=False)` | `chisq.test(table, correct = FALSE)` | `ChiSquareTests`: M&R Ex. 9-14 (pension plans: E = 136, 136, 68 / 64, 64, 32; χ² = 49.63, df 2); proportional rows → χ² = 0; cross-check (2 × 3, 2 × 2, 3 × 4) | [x] |
| p-value, critical value | p = P(χ²_df ≥ χ²) = Q(df/2, χ²/2) (upper regularized gamma, accurate far out); χ²_{α,df} = F⁻¹(1 − α) | `stats.chi2.sf`, `stats.chi2.ppf(1 - alpha, df)` | `pchisq(..., lower.tail = FALSE)`, `qchisq(1 - alpha, df)` | `ChiSquareTests` (χ²(2): p = e^(−x/2); χ²_{0.05,1} = 3.8415) | [x] |
| Yates' correction (2 × 2) | each \|O − E\| reduced by min(½, \|O − E\|) before squaring = n(\|ad − bc\| − n/2)²/(row and column totals' product) | `chi2_contingency(table, correction=True)` | `chisq.test(table, correct = TRUE)` | `ChiSquareTests` (shortcut formula; ignored on a 2 × 3); cross-check | [x] |
| Expected-count check | smallest Eᵢ and the number below 5; warning when any is below 5 (M&R §9-7) | `expected.min()`, `(expected < 5).sum()` | `min(expected)`, `sum(expected < 5)` | `ChiSquareTests` (2, 3, 1 equal: three below 5) | [x] |
| Cramér's V | √(χ²/(n(min(r, c) − 1))) from the uncorrected χ² | `stats.contingency.association(table, method='cramer')` (the code writes the formula) | formula | `ChiSquareTests` (= √(χ²/n) for 2 × c); cross-check | [x] |

## Simple linear regression (Module 11; M&R Ch. 11)

| Item | Formula | Python | R | Held by | Status |
|---|---|---|---|---|---|
| Slope, intercept | b₁ = S_xy/S_xx; b₀ = ȳ − b₁x̄ | `sm.OLS(y, sm.add_constant(x)).fit()` | `lm(y ~ x)` | `RegressionTests` hand example; M&R 11-2 | [x] |
| σ̂², SEs, t, CIs | σ̂² = SS_E/(n−2); se(b₁) = σ̂/√S_xx | `fit.bse`, `fit.pvalues`, `fit.conf_int()` | `summary(fit)`, `confint(fit)` | `RegressionTests`; M&R 11-4, 11-5 | [x] |
| R², r, ANOVA F | R² = SS_R/SS_T; r = sign(b₁)√R² | `fit.rsquared`, `fit.fvalue`, `fit.ess`, `fit.ssr` | `summary(fit)$r.squared`, `anova(fit)` | `RegressionTests` (R² = 0.6, F = 4.5 = t²) | [x] |
| Prediction at x₀ | CI on mean response; prediction interval | `fit.get_prediction(...).summary_frame(alpha)` | `predict(fit, newdata, interval=)` | `RegressionTests` (se = σ̂/√n at x̄); cross-check | [x] |

## Multiple regression and validation (Modules 12–13; M&R Ch. 12)

| Item | Formula | Python | R | Held by | Status |
|---|---|---|---|---|---|
| Coefficients | β̂ = (XᵀX)⁻¹Xᵀy; rank check by SVD | `sm.OLS(...).fit()` | `lm(y ~ x1 + x2)` | `RegressionTests` exact plane; cross-check | [x] |
| R², adjusted R², F | R²_adj = 1 − (1−R²)(n−1)/(n−p) | `fit.rsquared_adj` | `summary(fit)$adj.r.squared` | cross-check | [x] |
| VIF | 1/(1−R_j²), regressing xⱼ on the others with an intercept | `variance_inflation_factor(Xc, j)` | hand loop over `lm` (or `car::vif`) | VIF = 1 for orthogonal columns; cross-check | [x] |
| Residual diagnostics | rᵢ = eᵢ/(σ̂√(1−hᵢᵢ)); hᵢᵢ = diag(X(XᵀX)⁻¹Xᵀ); Dᵢ = rᵢ²hᵢᵢ/(p(1−hᵢᵢ)); DW = Σ(eᵢ−eᵢ₋₁)²/Σeᵢ²; Shapiro–Wilk on the residuals | `fit.get_influence()`, `durbin_watson` | `hatvalues`, `cooks.distance`, `shapiro.test`, DW by the formula | M&R 12-5; cross-check | [x] |
| Cross-validation | k contiguous folds (as `KFold(shuffle=False)`), RMSE on the held-out rows; k = n is leave-one-out | hand loop with `np.linalg.lstsq` (no sklearn dependency) | hand loop with `qr.solve` | cross-check (5, 10, 4 folds and LOO) | [x] |
| Variable selection | forward: add the smallest-p candidate while p < 0.05; backward: drop the largest-p term while p ≥ 0.05 | not in statsmodels; the chosen set is written into the script | `step` uses AIC, so the chosen set is written | M&R 12-6; `RegressionTests` | [x] |
| Transformations | ln, √, 1/· on x or y; fit linear in the transformed variable, prediction undone (exp, square, reciprocal) with the interval ends re-ordered | `np.log`, `np.sqrt`, `1 / x` | `log`, `sqrt`, `1 / x` | `RegressionTests` (ln y on 3e^{0.4x}); cross-check | [x] |

## ANOVA (Module 14; M&R Ch. 13–14)

| Item | Formula | Python | R | Held by | Status |
|---|---|---|---|---|---|
| One-way table | SS_T = SS_Treatments + SS_E; F = MS_Tr/MS_E on k − 1, N − k | `f_oneway`; `anova_lm(ols('y ~ C(g)'))` | `anova(lm(y ~ g))`, `summary(aov())` | `AnovaTests` M14 problem 1 (F = 48.12 by scipy; the page's 53.7 does not match its own data); cross-check | [x] |
| Effect size, power | η² = SS_Tr/SS_T; ω² = (SS_Tr − (k−1)MS_E)/(SS_T + MS_E); f = √(η²/(1−η²)); power = 1 − F_nc(F_crit; k−1, N−k, f²N) | `stats.ncf.sf`; `FTestAnovaPower` | `pf(…, ncp = f²N)`; `pwr.anova.test` | `AnovaTests` (λ = 0 → central F; power = α at f = 0); cross-check | [x] |
| Tukey HSD | q = |ȳᵢ − ȳⱼ|/√(MS_E/2 (1/nᵢ + 1/nⱼ)) vs q(k, N−k); CI ± q_{α}·se (Tukey–Kramer) | `tukey_hsd`, `studentized_range.ppf`; `pairwise_tukeyhsd` | `TukeyHSD`, `qtukey`, `ptukey` | `StudentizedRange` = R's ptukey (Copenhaver & Holland 1988, AS 190); q table values; cross-check | [x] |
| Bartlett; Levene | T = [(N−k) ln s_p² − Σ(nᵢ−1) ln sᵢ²]/c vs χ²(k−1); Levene median-centred | `bartlett`, `levene` | `bartlett.test`, `car::leveneTest` | equal spreads → T = 0; cross-check | [x] |
| Kruskal–Wallis | H = 12/(N(N+1)) ΣRᵢ²/nᵢ − 3(N+1), tie-corrected, vs χ²(k−1) | `kruskal` | `kruskal.test` | identical groups → H = 0; cross-check | [x] |
| Two-way table (balanced) | SS_A = bnΣ(ȳᵢ.. − ȳ)², SS_B, SS_AB = nΣΣ(ȳᵢⱼ. − ȳᵢ.. − ȳ.ⱼ. + ȳ)², SS_E; F against MS_E; n = 1 → additive model | `anova_lm(ols('y ~ C(a) * C(b)'), typ=2)` | `anova(lm(y ~ a * b))` | `AnovaTests` hand-worked 2×2×2; blocks (n = 1) cross-check | [x] |
