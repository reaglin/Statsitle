---
id: st.m23.bayes
title: Naive Bayes
minutes: 45
objectives: [st.m23.o4]
example: st.m23.bayes.eg
exercise: st.m23.bayes.ex
reference: [naive-bayes]
---

# Naive Bayes

The first spam filters that worked well, around 2002, were **naive Bayes** classifiers, and the method is still used for text: fast, simple and surprisingly good. It is Bayes' rule (item 3.4) applied to each word, with one bold ("naive") simplification.

## One word

40 % of a company's email is spam. The word "prize" appears in 30 % of spam and in 1 % of genuine mail (illustrative numbers, made up for this book). An email contains "prize": how likely is it to be spam?

[[results: st.m23.bayes.eg]]

- P(prize) = 0.30 × 0.40 + 0.01 × 0.60 = **0.126**.
- P(spam | prize) = 0.30 × 0.40 / 0.126 = **0.952**.

The **prior**, 40 % spam, is updated by the **evidence** of the word to a **posterior** of 95 %. The same calculation found the positive predictive value of a medical test in item 19.3.

[[see-example: 1]]

## Many words: the odds form

Bayes' rule is easiest to repeat in **odds**:

posterior odds = prior odds × likelihood ratio, with likelihood ratio LR = P(word | spam) / P(word | genuine).

For "prize": prior odds 0.40/0.60 = 0.667, LR = 0.30/0.01 = 30, posterior odds = 20, probability 20/21 = 0.952 (as above).

Now a second word, "free", in 50 % of spam and 5 % of genuine mail: LR = 10. The **naive** assumption is that, within each class, the words appear independently of each other. Then the likelihood ratios simply multiply:

posterior odds = 0.667 × 30 × 10 = 200, P(spam) = 200/201 = **0.995**.

Words that point to genuine mail have LR < 1 and pull the odds down. In practice the filter adds **log** likelihood ratios over hundreds of words, and estimates each word's rates from a training set of labelled emails (with a small count added to every word so a word never seen in spam does not give LR = ∞).

## Why "naive" works

The independence assumption is false: "prize" and "winner" come together. The probabilities naive Bayes gives are therefore too extreme (too close to 0 and 1). But the **ranking** of emails, which is spammier, is often right, and that is what a classifier needs. It is a good first model and a baseline for more complex ones.

## In Python and R

```python
prior_odds = 0.40 / 0.60
post_odds = prior_odds * (0.30/0.01) * (0.50/0.05)
p_spam = post_odds / (1 + post_odds)
# from labelled texts: sklearn.naive_bayes.MultinomialNB().fit(word_counts, labels)
```

```r
prior_odds <- 0.40 / 0.60
post_odds <- prior_odds * (0.30/0.01) * (0.50/0.05)
post_odds / (1 + post_odds)
# from labelled data: e1071::naiveBayes(class ~ ., data = d)
```

[[code: st.m23.bayes.eg]]

## Common mistakes

- **P(word | spam) read as P(spam | word).**
- **Ignoring the prior**: the same word means more in a mailbox that is mostly spam.
- **Trusting the naive probability** as a calibrated chance.
- **A zero count** for a word never seen in one class, making one word decide everything.

## What to do next

On the **Exercise** tab, the word "invoice", common in phishing and in genuine mail: P(invoice) and P(spam | invoice).

[[example: A spam filter: the prior and the word]]

The word "prize": in 30 % of spam and 1 % of genuine mail, with 40 % of mail spam. P(spam | prize) = **0.952**. (This example has no figure: Probability rules draws none. Watch the numbers.)

1. **A cleaner mailbox: P(A) = 0.05.** P(spam | prize) falls to about 0.61: the same word is weaker evidence where spam is rare.
2. **A word common in both: P(B|not A) = 0.25.** The likelihood ratio is only 1.2, and the posterior barely moves from the prior.

[[try: st.m23.spam.try ProbabilityA=0.01..0.9 LikelihoodGivenA=0.01..0.9 LikelihoodGivenNotA=0.001..0.5 | ProbabilityBResult AGivenB figure=none]]
