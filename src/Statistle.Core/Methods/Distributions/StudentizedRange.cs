using MathNet.Numerics;
using MathNet.Numerics.Distributions;

namespace Statistle.Core.Methods.Distributions;

/// <summary>
/// The studentized range distribution q(k, ν) behind Tukey's HSD: the range of k normal means
/// divided by the pooled standard error. The distribution function follows Copenhaver and
/// Holland (1988) as implemented in R's <c>ptukey</c> (Applied Statistics algorithm AS 190):
/// Gauss–Legendre quadrature over the range density and over the chi distribution of s.
/// scipy's <c>studentized_range</c> and R's <c>ptukey</c>/<c>qtukey</c> give the same numbers.
/// </summary>
public static class StudentizedRange
{
    private static readonly double[] XLeg =
    [
        0.981560634246719250690549090149, 0.904117256370474856678465866119, 0.769902674194304687036893833213,
        0.587317954286617447296702418941, 0.367831498998180193752691536644, 0.125233408511468915472441369464,
    ];

    private static readonly double[] ALeg =
    [
        0.047175336386511827194615961485, 0.106939325995318430960254718194, 0.160078328543346226334652529543,
        0.203167426723065921749064455810, 0.233492536538354808760849898925, 0.249147045813402785000562436043,
    ];

    private static readonly double[] XLegQ =
    [
        0.989400934991649932596154173450, 0.944575023073232576077988415535, 0.865631202387831743880467897712,
        0.755404408355003033895101194847, 0.617876244402643748446671764049, 0.458016777657227386342419442984,
        0.281603550779258913230460501460, 0.950125098376374401853193354250e-1,
    ];

    private static readonly double[] ALegQ =
    [
        0.271524594117540948517805724560e-1, 0.622535239386478928628438369944e-1, 0.951585116824927848099251076022e-1,
        0.124628971255533872052476282192, 0.149595988816576732081501730547, 0.169156519395002538189312079030,
        0.182603415044923588866763667969, 0.189450610455068496285396723208,
    ];

    /// <summary>P(Q ≤ q) for k groups and ν error degrees of freedom.</summary>
    public static double Cdf(double q, double k, double df)
    {
        if (double.IsNaN(q) || double.IsNaN(k) || double.IsNaN(df) || k < 2 || df < 2)
        {
            return double.NaN;
        }

        if (q <= 0)
        {
            return 0;
        }

        if (double.IsPositiveInfinity(q))
        {
            return 1;
        }

        if (df > 25000)
        {
            return RangeProbability(q, 1, k);
        }

        const int nlegq = 16, ihalfq = 8;
        const double eps1 = -30.0, eps2 = 1.0e-14;
        double f2 = df * 0.5;
        double f2lf = f2 * Math.Log(df) - df * Math.Log(2) - SpecialFunctions.GammaLn(f2);
        double f21 = f2 - 1.0;
        double ff4 = df * 0.25;
        double ulen = df <= 100 ? 1.0 : df <= 800 ? 0.5 : df <= 5000 ? 0.25 : 0.125;
        f2lf += Math.Log(ulen);
        double ans = 0.0;
        double otsum = 0.0;

        for (int i = 1; i <= 50; i++)
        {
            otsum = 0.0;
            double twa1 = (2 * i - 1) * ulen;

            for (int jj = 1; jj <= nlegq; jj++)
            {
                int j;
                double t1;

                if (ihalfq < jj)
                {
                    j = jj - ihalfq - 1;
                    t1 = f2lf + f21 * Math.Log(twa1 + XLegQ[j] * ulen) - (XLegQ[j] * ulen + twa1) * ff4;
                }
                else
                {
                    j = jj - 1;
                    t1 = f2lf + f21 * Math.Log(twa1 - XLegQ[j] * ulen) + (XLegQ[j] * ulen - twa1) * ff4;
                }

                if (t1 >= eps1)
                {
                    double qsqz = ihalfq < jj
                        ? q * Math.Sqrt((XLegQ[j] * ulen + twa1) * 0.5)
                        : q * Math.Sqrt((-(XLegQ[j] * ulen) + twa1) * 0.5);
                    double wprb = RangeProbability(qsqz, 1, k);
                    otsum += wprb * ALegQ[j] * Math.Exp(t1);
                }
            }

            if (i * ulen >= 1.0 && otsum <= eps2)
            {
                break;
            }

            ans += otsum;
        }

        return Math.Clamp(ans, 0, 1);
    }

    /// <summary>The quantile q with P(Q ≤ q) = p, by bisection on <see cref="Cdf"/>.</summary>
    public static double Quantile(double p, double k, double df)
    {
        if (double.IsNaN(p) || p <= 0 || p >= 1 || k < 2 || df < 2)
        {
            return double.NaN;
        }

        double low = 0, high = 10;

        while (Cdf(high, k, df) < p && high < 1e4)
        {
            high *= 2;
        }

        for (int i = 0; i < 60 && high - low > 1e-9; i++)
        {
            double mid = 0.5 * (low + high);

            if (Cdf(mid, k, df) < p)
            {
                low = mid;
            }
            else
            {
                high = mid;
            }
        }

        return 0.5 * (low + high);
    }

    /// <summary>
    /// The probability that the range of cc standard normals is at most w (rr = 1 range), by
    /// Hartley's form: the integral of φ(x)[Φ(x) − Φ(x − w)]^(cc−1) (R's <c>wprob</c>).
    /// </summary>
    private static double RangeProbability(double w, double rr, double cc)
    {
        const int nleg = 12, ihalf = 6;
        const double C1 = -30.0, C3 = 60.0, bb = 8.0, wlar = 3.0, wincr1 = 2.0, wincr2 = 3.0;

        double qsqz = w * 0.5;

        if (qsqz >= bb)
        {
            return 1.0;
        }

        double prW = 2 * Normal.CDF(0, 1, qsqz) - 1.0;

        if (prW >= 1.0)
        {
            return 1.0;
        }

        prW = Math.Pow(prW, cc);
        double wincr = w > wlar ? wincr1 : wincr2;
        double blb = qsqz;
        double binc = (bb - qsqz) / wincr;
        double bub = blb + binc;
        double einsum = 0.0;
        double cc1 = cc - 1.0;

        for (double wi = 1; wi <= wincr; wi++)
        {
            double elsum = 0.0;
            double a = 0.5 * (bub + blb);
            double b = 0.5 * (bub - blb);

            for (int jj = 1; jj <= nleg; jj++)
            {
                int j;
                double xx;

                if (ihalf < jj)
                {
                    j = nleg - jj + 1;
                    xx = XLeg[j - 1];
                }
                else
                {
                    j = jj;
                    xx = -XLeg[j - 1];
                }

                double c = b * xx;
                double ac = a + c;
                double qexpo = ac * ac;

                if (qexpo > C3)
                {
                    break;
                }

                double pplus = 2 * Normal.CDF(0, 1, ac);
                double pminus = 2 * Normal.CDF(w, 1, ac);
                double rinsum = pplus * 0.5 - pminus * 0.5;

                if (rinsum >= Math.Exp(C1 / cc1))
                {
                    rinsum = ALeg[j - 1] * Math.Exp(-(0.5 * qexpo)) * Math.Pow(rinsum, cc1);
                    elsum += rinsum;
                }
            }

            elsum *= 2.0 * b * cc / Math.Sqrt(2 * Math.PI);
            einsum += elsum;
            blb = bub;
            bub += binc;
        }

        prW += einsum;

        if (prW <= Math.Exp(C1 / rr))
        {
            return 0;
        }

        prW = Math.Pow(prW, rr);
        return prW >= 1 ? 1 : prW;
    }
}

/// <summary>
/// The noncentral F distribution, for the power of an F test: a Poisson(λ/2) mixture of central
/// beta distributions (Johnson, Kotz and Balakrishnan, Continuous Univariate Distributions 2,
/// §30.3). scipy's <c>ncf</c> and R's <c>pf(…, ncp = λ)</c> use the same λ.
/// </summary>
public static class NoncentralF
{
    public static double Cdf(double x, double df1, double df2, double lambda)
    {
        if (double.IsNaN(x) || df1 <= 0 || df2 <= 0 || lambda < 0)
        {
            return double.NaN;
        }

        if (x <= 0)
        {
            return 0;
        }

        double y = df1 * x / (df1 * x + df2);
        double half = lambda / 2;
        double sum = 0;
        int last = (int)Math.Ceiling(half + 10 * Math.Sqrt(half) + 30);

        for (int j = 0; j <= last; j++)
        {
            double logWeight = -half + j * Math.Log(Math.Max(half, double.Epsilon)) - SpecialFunctions.FactorialLn(j);

            if (half == 0)
            {
                logWeight = j == 0 ? 0 : double.NegativeInfinity;
            }

            double weight = Math.Exp(logWeight);

            if (weight < 1e-18 && j > half)
            {
                break;
            }

            sum += weight * SpecialFunctions.BetaRegularized(df1 / 2 + j, df2 / 2, y);
        }

        return Math.Clamp(sum, 0, 1);
    }
}
