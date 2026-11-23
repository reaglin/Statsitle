---
id: st.m21
number: 21
part: Part III · Additional Applications of Statistics
title: "Environmental and Earth Sciences"
status: draft
minutes: 300
prerequisites: [st.m11]
outcomes:
  - st.m21.o1: Estimate a design flood or rainfall from annual maxima with a probability distribution, and relate the return period to the exceedance probability
  - st.m21.o2: Detect and size a trend in a climate or environmental record with regression and the Mann–Kendall test
  - st.m21.o3: Model skewed monitoring data with a lognormal distribution, judge compliance with a standard, and handle values below a detection limit
  - st.m21.o4: Interpolate a quantity between measured places, and read a variogram's nugget, slope, sill and range
  - st.m21.o5: Describe earthquake occurrence with the Gutenberg–Richter law and the chance of at least one event with a Poisson model
  - st.m21.o6: Estimate an animal population by mark–recapture and describe a community with a diversity index
topics:
  - "Frequency analysis of floods and rainfall"
  - "Extreme values and climate trends"
  - "Water and air quality monitoring"
  - "Spatial statistics"
  - "Earthquakes and natural hazards"
  - "Ecological sampling"
---

How big a flood should a culvert be built for? Is the climate at a station warming, and how fast? How often does a river break its water-quality standard? How many fish are in a lake? Environmental engineers and earth scientists answer such questions with statistics, and they bring two problems of their own: the events that matter most are **rare** (the 100-year flood), and the data are tied to **places and times**.

This module shows **how statistics is used in the environmental and earth sciences**. It does not teach the methods again. Where a method was taught in Part I, the page says where. Where it belongs to Part II (extreme-value theory, spatial models), the page explains only as much as the application needs, and Part II will cover it in full. The flood-frequency terms follow the hydrology text this program accompanies, *Hydrology: Water Quantity and Quality Control* (Wanielista, Kersten and Eaglin), and SMADA, its software.

**The data in this module are illustrative**: made up for this book, with realistic sizes and patterns, and labelled as such. They are not records of a real river, station, county or region. Each page names where real data of the same kind can be found.

This module has six topics and a module check. Each topic is an item with four lesson tabs (and My notes, for your own notes):

- **Learn** explains how the field uses the method, works a small case, and shows the Python and R.
- **Example** has an example to try, with a figure that moves as you change the data, and the worked example with every result and the code.
- **Exercise** gives you a new data set from the field, or questions. Work out the answers, type them in, and press **Check my answers**.
- **Reference** has the formulas, the field's vocabulary, the Python and R functions, and where to read more.

You need Part I's continuous distributions (Module 5, especially the lognormal and the Weibull in item 5.6), the Poisson and hypergeometric distributions (Module 4) and regression (Module 11). The module check, the last item, asks ten questions over all six outcomes. You need 80 % to pass, and you can try again.
