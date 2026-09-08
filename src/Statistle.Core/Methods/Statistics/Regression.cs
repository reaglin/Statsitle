using System.Globalization;

namespace Statistle.Core.Methods.Statistics;

/// <summary>
/// The functional forms a two-variable relation is commonly fitted with. Each is a straight line
/// after a transform of x, y or both, so one least-squares routine serves all six.
/// </summary>
public enum RegressionForm
{
    /// <summary>y = a + b·x</summary>
    Linear = 0,

    /// <summary>y = a·e^(b·x), fitted as ln y against x. Needs y &gt; 0.</summary>
    Exponential,

    /// <summary>y = a·x^b, fitted as ln y against ln x. Needs x &gt; 0 and y &gt; 0.</summary>
    Power,

    /// <summary>y = a + b·ln x, fitted as y against ln x. Needs x &gt; 0.</summary>
    Logarithmic,

    /// <summary>y = a + b / x, fitted as y against 1/x. Needs x ≠ 0.</summary>
    Reciprocal,

    /// <summary>y = 1 / (a + b·x), fitted as 1/y against x. Needs y ≠ 0.</summary>
    [System.ComponentModel.Description("Reciprocal-y")]
    ReciprocalY,
}

/// <summary>Ordinary least squares of v on u: v = intercept + slope·u.</summary>
public sealed record LinearFit(double Intercept, double Slope, double RSquared, int Count);

/// <summary>
/// One fitted form in the original variables: the equation, its coefficients, and how well it
/// fits. <see cref="RSquared"/> is the coefficient of determination of the straight line in the
/// transformed variables — the textbook figure for that form. <see cref="StandardError"/> is the
/// standard error of the estimate in the <em>original</em> y units, √(Σ(y − ŷ)² / (n − 2)), which
/// is the number that compares fairly across forms; the analysis picks its best form by it.
/// </summary>
public sealed record RegressionResult(
    RegressionForm Form,
    double A,
    double B,
    double RSquared,
    double StandardError,
    int PointsUsed,
    int PointsSkipped)
{
    /// <summary>The fitted value at x, in original units. NaN where the form is undefined.</summary>
    public double Predict(double x) => Form switch
    {
        RegressionForm.Linear => A + (B * x),
        RegressionForm.Exponential => A * Math.Exp(B * x),
        RegressionForm.Power => x > 0 ? A * Math.Pow(x, B) : double.NaN,
        RegressionForm.Logarithmic => x > 0 ? A + (B * Math.Log(x)) : double.NaN,
        RegressionForm.Reciprocal => x != 0 ? A + (B / x) : double.NaN,
        RegressionForm.ReciprocalY => A + (B * x) != 0 ? 1.0 / (A + (B * x)) : double.NaN,
        _ => double.NaN,
    };

    /// <summary>The equation with its coefficients, e.g. "y = 1.25 + 0.734·x".</summary>
    public string Equation => Regression.Equation(Form, A, B);
}

/// <summary>Least-squares regression on the common transforms.</summary>
public static class Regression
{
    /// <summary>Ordinary least squares, or null when fewer than two points or no spread in u.</summary>
    public static LinearFit? LeastSquares(IReadOnlyList<double> u, IReadOnlyList<double> v)
    {
        ArgumentNullException.ThrowIfNull(u);
        ArgumentNullException.ThrowIfNull(v);

        int n = Math.Min(u.Count, v.Count);
        if (n < 2)
        {
            return null;
        }

        double meanU = 0, meanV = 0;
        for (int i = 0; i < n; i++)
        {
            meanU += u[i];
            meanV += v[i];
        }

        meanU /= n;
        meanV /= n;

        double suu = 0, svv = 0, suv = 0;
        for (int i = 0; i < n; i++)
        {
            double du = u[i] - meanU;
            double dv = v[i] - meanV;
            suu += du * du;
            svv += dv * dv;
            suv += du * dv;
        }

        if (suu <= 0)
        {
            return null;
        }

        double slope = suv / suu;
        double intercept = meanV - (slope * meanU);

        // r² = 1 − SS_res / SS_tot; when v has no spread every line fits perfectly.
        double rSquared = svv <= 0 ? 1.0 : Math.Clamp((suv * suv) / (suu * svv), 0, 1);

        return new LinearFit(intercept, slope, rSquared, n);
    }

    /// <summary>
    /// Fits one form. Points the transform cannot take (a log of a non-positive value, a division
    /// by zero) are left out and counted, not silently dropped. Null when fewer than three usable
    /// points remain or x has no spread.
    /// </summary>
    public static RegressionResult? Fit(RegressionForm form, IReadOnlyList<double> x, IReadOnlyList<double> y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        int n = Math.Min(x.Count, y.Count);
        List<double> u = [], v = [], xs = [], ys = [];

        for (int i = 0; i < n; i++)
        {
            if (!Accepts(form, x[i], y[i]))
            {
                continue;
            }

            u.Add(TransformX(form, x[i]));
            v.Add(TransformY(form, y[i]));
            xs.Add(x[i]);
            ys.Add(y[i]);
        }

        int skipped = n - xs.Count;

        if (xs.Count < 3)
        {
            return null;
        }

        LinearFit? line = LeastSquares(u, v);
        if (line is null)
        {
            return null;
        }

        (double a, double b) = Coefficients(form, line.Intercept, line.Slope);

        var result = new RegressionResult(form, a, b, line.RSquared, 0, xs.Count, skipped);

        double residual = 0;
        for (int i = 0; i < xs.Count; i++)
        {
            double error = ys[i] - result.Predict(xs[i]);
            residual += error * error;
        }

        double standardError = xs.Count > 2 ? Math.Sqrt(residual / (xs.Count - 2)) : 0;

        return result with { StandardError = double.IsFinite(standardError) ? standardError : double.NaN };
    }

    /// <summary>Whether a point can be transformed under this form.</summary>
    public static bool Accepts(RegressionForm form, double x, double y)
    {
        if (!double.IsFinite(x) || !double.IsFinite(y))
        {
            return false;
        }

        return form switch
        {
            RegressionForm.Linear => true,
            RegressionForm.Exponential => y > 0,
            RegressionForm.Power => x > 0 && y > 0,
            RegressionForm.Logarithmic => x > 0,
            RegressionForm.Reciprocal => x != 0,
            RegressionForm.ReciprocalY => y != 0,
            _ => false,
        };
    }

    /// <summary>The equation of a form in words and symbols, e.g. "y = a·e^(b·x)".</summary>
    public static string Template(RegressionForm form) => form switch
    {
        RegressionForm.Linear => "y = a + b·x",
        RegressionForm.Exponential => "y = a·e^(b·x)",
        RegressionForm.Power => "y = a·x^b",
        RegressionForm.Logarithmic => "y = a + b·ln x",
        RegressionForm.Reciprocal => "y = a + b / x",
        RegressionForm.ReciprocalY => "y = 1 / (a + b·x)",
        _ => string.Empty,
    };

    /// <summary>The equation with numbers in place of a and b.</summary>
    public static string Equation(RegressionForm form, double a, double b)
    {
        string A = Number(a);
        string absB = Number(Math.Abs(b));
        string sign = b < 0 ? "−" : "+";

        return form switch
        {
            RegressionForm.Linear => $"y = {A} {sign} {absB}·x",
            RegressionForm.Exponential => $"y = {A}·e^({Number(b)}·x)",
            RegressionForm.Power => $"y = {A}·x^{Number(b)}",
            RegressionForm.Logarithmic => $"y = {A} {sign} {absB}·ln x",
            RegressionForm.Reciprocal => $"y = {A} {sign} {absB} / x",
            RegressionForm.ReciprocalY => $"y = 1 / ({A} {sign} {absB}·x)",
            _ => string.Empty,
        };
    }

    private static double TransformX(RegressionForm form, double x) => form switch
    {
        RegressionForm.Power or RegressionForm.Logarithmic => Math.Log(x),
        RegressionForm.Reciprocal => 1.0 / x,
        _ => x,
    };

    private static double TransformY(RegressionForm form, double y) => form switch
    {
        RegressionForm.Exponential or RegressionForm.Power => Math.Log(y),
        RegressionForm.ReciprocalY => 1.0 / y,
        _ => y,
    };

    /// <summary>Back from the straight line's intercept and slope to the form's a and b.</summary>
    private static (double A, double B) Coefficients(RegressionForm form, double intercept, double slope) =>
        form switch
        {
            RegressionForm.Exponential or RegressionForm.Power => (Math.Exp(intercept), slope),
            _ => (intercept, slope),
        };

    private static string Number(double value) =>
        value.ToString("G4", CultureInfo.InvariantCulture);
}
