---
id: st.m22.polling
title: Survey design and polling
minutes: 50
objectives: [st.m22.o1]
example: st.m22.polling.eg
exercise: st.m22.polling.ex
reference: [survey-sampling]
---

# Survey design and polling

A poll of a thousand people describes a country of millions. That works only because of **random sampling** (Module 6), and only as well as the sample is random. The **margin of error** a poll prints measures one kind of error; the others are often larger.

## The Example: a poll of likely voters

A random sample of 1,000 likely voters; 520 support a ballot measure (illustrative, made up for this book).

[[results: st.m22.polling.eg]]

[[figure: st.m22.polling.eg 1]]

- p̂ = **52.0 %**, margin of error **±3.1 points**, 95 % interval **48.9 % to 55.1 %** (item 7.4).
- "52 % support it" is not "a majority support it": the interval includes values below 50 %. A newspaper that calls the vote "decided" has ignored the margin of error.

[[see-example: 1]]

## Planning the sample size

The margin of error at 95 % is about 1.96 √(p(1 − p)/n), largest when p = 0.5. For ±3 points: n = 1.96² × 0.25 / 0.03² = **1,068** (item 7.6). That is why national polls ask about a thousand people: the margin depends on the **sample size**, not on the size of the population.

## Stratified samples and weighting

A simple random sample can by chance have too many young people or too few rural ones. Pollsters **stratify**: divide the population into groups (age, region) and sample each. If a group ends up under-represented in the replies, its answers are **weighted** up.

Suppose 30 % of voters are under 40 but only 20 % of the poll's respondents are. Each young respondent gets a weight of 0.30/0.20 = 1.5, each older one 0.70/0.80 = 0.875. If 60 % of the young and 45 % of the older respondents support the measure, the unweighted share is 0.2 × 60 + 0.8 × 45 = **48 %**, and the weighted share 0.3 × 60 + 0.7 × 45 = **49.5 %**. Weighting corrects the mix, but it widens the margin of error, because some answers count more than others.

## The errors the margin does not cover

- **Coverage**: the people who could be sampled are not the population (no landline, not on the voter roll).
- **Nonresponse**: the people who answer differ from those who do not. Response rates to phone polls are now often below 10 %.
- **Wording and order**: "Do you support the measure to protect jobs?" and "Do you support the tax increase?" can describe the same measure and get different answers.
- **Likely voters**: who will actually vote is itself an estimate.

These errors are not reduced by a bigger sample. A huge online survey of volunteers can be far more wrong than a small random one.

## In Python and R

```python
from statsmodels.stats.proportion import proportion_confint
low, high = proportion_confint(520, 1000, method="normal")
n = 1.96**2 * 0.25 / 0.03**2          # sample size for +/- 3 points
```

```r
prop.test(520, 1000, correct = FALSE)$conf.int
1.96^2 * 0.25 / 0.03^2
# weighted surveys: library(survey); svydesign(...); svymean(...)
```

[[code: st.m22.polling.eg]]

## Common mistakes

- **Reading a lead inside the margin of error as decided.**
- **The margin of error as the whole error.**
- **Thinking a big population needs a big sample.**
- **Comparing two polls' numbers** without the margin of error of the *difference*, which is larger.

## What to do next

On the **Exercise** tab, a city survey of 1,200 residents: the share who used public transport, with its margin of error.

[[example: How many people to ask?]]

The sample size a poll needs, at 95 % confidence and planning for 50 %. It starts at a margin of **±3 points**: **1,068** people.

1. **Halve the margin to ±1.5 points (0.015).** The sample size quadruples, to about 4,270: precision is expensive.
2. **Back to 0.03, and set the expected share to 0.2.** Fewer people are needed (about 683): a share far from 50 % is easier to pin down.
3. **Raise the confidence to 0.99** with the margin at 0.03 and the share at 0.5: about 1,844.

[[try: st.m22.poll-size.try Margin=0.005..0.1 ProportionGuess=0.05..0.5 Confidence | SampleSizeNeeded Critical figure=none]]
