---
id: st.m20.abtest
title: Market research and A/B testing
minutes: 45
objectives: [st.m20.o6]
example: st.m20.abtest.eg
exercise: st.m20.abtest.ex
reference: [market-research-ab-tests]
---

# Market research and A/B testing

Businesses ask customers what they think, and try changes on some customers before giving them to all. Both are sampling and testing, as in Part I, with the field's own words: a **survey**, a **margin of error**, a **conversion rate**, an **A/B test**.

## Surveys and the margin of error

A brand surveys 600 customers chosen at random; 222 say they would recommend it. The share is p̂ = 222/600 = **37.0 %**, and the 95 % interval for the share of *all* customers (item 7.4) is

p̂ ± 1.96 √(p̂(1 − p̂)/n) = 0.370 ± 0.039, **33.1 % to 40.9 %**.

The **margin of error**, ±3.9 points, is what market research reports print as "accurate to within ±4 points". Three things it does not cover:

- **Who was asked.** The interval assumes a random sample. Customers who answer an email survey may be the happiest ones (**selection bias**); no formula fixes that.
- **How the question was worded.** "Would you recommend us?" and "Would you recommend us to a friend this week?" measure different things.
- **Sample size.** To halve the margin of error, quadruple the sample (item 7.6): about 2,400 customers for ±2 points.

[[see-example: 1]]

## A/B tests: an experiment on customers

An online shop shows its old checkout page (**A**, the control) to some visitors and a new one (**B**) to others, **chosen at random**. It counts how many buy: the **conversion rate**. Because the visitors are randomly assigned, a difference in conversion can be credited to the page (Module 10's comparison of two proportions, item 10.5).

The Example: 2,400 visitors saw A and 288 bought (12.0 %); 2,350 saw B and 341 bought (14.5 %).

[[results: st.m20.abtest.eg]]

[[figure: st.m20.abtest.eg 1]]

- z = −2.55, p = 0.011: at α = 0.05 the conversion rates differ.
- The 95 % interval for p_A − p_B is −4.4 to −0.6 points: B converts **between 0.6 and 4.4 points better**.
- In business terms: if the shop has 100,000 checkout visitors a month, B brings somewhere between 600 and 4,400 more sales. Whether that pays for building the new page is a business decision the interval informs.

## What business adds to the test

- **Decide the sample size first.** A test stopped "when it looks significant" finds false winners (item 23.3 shows why). Plan n from the smallest lift worth having.
- **One primary metric.** Conversion, say. Testing twenty metrics and reporting the one that moved is a multiple-testing trap (item 14.1).
- **Practical significance.** With huge samples, a 0.1-point lift can be "significant" and worthless. Report the interval for the lift, not only the p-value.
- **Randomise the unit you care about.** Assign customers, not page views, if one customer can see both pages.

## In Python and R

```python
from statsmodels.stats.proportion import proportions_ztest
z, p = proportions_ztest([288, 341], [2400, 2350])
```

```r
prop.test(c(288, 341), c(2400, 2350), correct = FALSE)   # chi-square = z^2
```

[[code: st.m20.abtest.eg]]

## Common mistakes

- **Reading the margin of error as the whole error**, ignoring who answered.
- **Peeking**: checking the A/B test every day and stopping at the first p < 0.05.
- **A significant but tiny lift** reported as a win.
- **Comparing groups that were not randomised** (this month on B against last month on A).

## What to do next

On the **Exercise** tab, two subject lines for a marketing email, 5,000 customers each: test whether their click-through rates differ, and give the interval for the difference.

[[example: A customer survey: the margin of error]]

The brand's survey: 222 of 600 customers would recommend it, **37.0 %**, margin of error **±3.86 points** (33.1 % to 40.9 %).

1. **Survey four times as many:** set the sample size to 2,400 and the count to 888 (still 37 %). The margin of error halves, to about ±1.9 points.
2. **Back to 600 and 222**, and raise the confidence to 0.99: the interval widens. More confidence costs width.
3. **Set the count to 300 of 600** (50 %). The margin of error is at its largest for a given n when p̂ is 0.5, which is why polls plan for 50 %.

[[try: st.m20.survey.try Successes SampleSize Confidence | Estimate MarginOfError Lower Upper figure=1]]
