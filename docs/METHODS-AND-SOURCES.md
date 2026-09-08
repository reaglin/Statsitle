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

## Sampling distributions (Module 6; M&R Ch. 7)

| Item | Formula | Python | R | Held by | Status |
|---|---|---|---|---|---|
| Mean and SE of x̄ | μ_x̄ = μ; σ_x̄ = σ/√n | `norm.cdf(xbar, mu, sigma/np.sqrt(n))` | `pnorm(xbar, mu, sigma/sqrt(n))` | `DistributionTests`; EGN3443 M6 P1 (4200, 350, n 5 → 30) | [x] |
| Sampling distribution of p̂ | p; √(p(1−p)/n) | | | `DistributionTests`; M&R 7-2 | [x] |
| CLT simulation | k samples of n, histogram of means | `rng.normal(...)` | `replicate(k, mean(rnorm(n, ...)))` | `DistributionTests` | [x] |

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
| Mann–Whitney U; Wilcoxon signed-rank | | `mannwhitneyu`, `wilcoxon` | `wilcox.test` | `InferenceTests`; M&R 10-6 | [x] |

## Simple linear regression (Module 11; M&R Ch. 11)

| Item | Formula | Python | R | Held by | Status |
|---|---|---|---|---|---|
| Slope, intercept | b₁ = S_xy/S_xx; b₀ = ȳ − b₁x̄ | `linregress(x, y)`; `sm.OLS(y, sm.add_constant(x)).fit()` | `lm(y ~ x)` | M&R Ex. 11-1 | [ ] |
| σ̂², SEs, t, CIs | σ̂² = SS_E/(n−2); se(b₁) = σ̂/√S_xx | `fit.bse`, `fit.pvalues`, `fit.conf_int()` | `summary(fit)`, `confint(fit)` | M&R Ex. 11-2, 11-3 | [ ] |
| R², r, ANOVA F | R² = SS_R/SS_T | `fit.rsquared`, `fit.fvalue` | `summary(fit)$r.squared`, `anova(fit)` | M&R 11-4 | [ ] |
| Prediction at x₀ | CI on mean response; prediction interval | `fit.get_prediction(...).summary_frame()` | `predict(fit, newdata, interval=)` | M&R Ex. 11-7 | [ ] |

## Multiple regression and validation (Modules 12–13; M&R Ch. 12)

| Item | Formula | Python | R | Held by | Status |
|---|---|---|---|---|---|
| Coefficients | β̂ = (XᵀX)⁻¹Xᵀy | `sm.OLS(...).fit()` | `lm(y ~ x1 + x2)` | M&R Ex. 12-1 | [ ] |
| R², adjusted R², F | R²_adj = 1 − (1−R²)(n−1)/(n−p) | `fit.rsquared_adj` | `summary(fit)$adj.r.squared` | M&R 12-2 | [ ] |
| VIF | 1/(1−R_j²) | `variance_inflation_factor` | `car::vif` | M&R 12-6 | [ ] |
| Residual diagnostics | standardized residuals, leverage h_ii, Cook's D, Durbin–Watson | `fit.get_influence()`, `durbin_watson` | `plot(fit)`, `cooks.distance`, `car::durbinWatsonTest` | M&R 12-5 | [ ] |
| Cross-validation | hold-out RMSE; k-fold | `sklearn.model_selection.cross_val_score` | `caret::train` or hand loop | EGN3443 M13 | [ ] |
| Transformations | log, √, 1/x on x or y | `np.log` | `log` | EGN3443 M13 P1 | [ ] |

## ANOVA (Module 14; M&R Ch. 13–14)

| Item | Formula | Python | R | Held by | Status |
|---|---|---|---|---|---|
| One-way ANOVA | SS_T = SS_Treatments + SS_E; F = MS_Tr/MS_E | `f_oneway(*groups)`; `anova_lm` | `aov(y ~ g)`; `summary` | EGN3443 M14 P1 alloys; M&R Ex. 13-1 | [ ] |
| η² | SS_Tr/SS_T | arithmetic | arithmetic | | [ ] |
| Tukey HSD | q_{α,a,f}√(MS_E/n) | `pairwise_tukeyhsd` | `TukeyHSD(aov)` | M&R Ex. 13-3 | [ ] |
| Bartlett, Levene | | `bartlett`, `levene` | `bartlett.test`, `car::leveneTest` | | [ ] |
| Kruskal–Wallis | | `kruskal` | `kruskal.test` | M&R 13-4 | [ ] |
| Two-way ANOVA | main effects, interaction | `anova_lm(ols('y ~ C(a)*C(b)'))` | `aov(y ~ a*b)` | M&R Ex. 14-1 | [ ] |
