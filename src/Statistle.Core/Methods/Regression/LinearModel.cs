using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;

namespace Statistle.Core.Methods.Regression;

/// <summary>One coefficient of a fitted model with its inference.</summary>
public sealed record Coefficient(string Name, double Estimate, double StandardError, double T, double PValue, double Lower, double Upper, double Vif);

/// <summary>A prediction at one point: the fitted value with the interval on the mean response and on a new observation.</summary>
public sealed record Prediction(double Fitted, double StandardError, double MeanLower, double MeanUpper, double ObservationLower, double ObservationUpper);

/// <summary>
/// Ordinary least squares for y on any number of predictors, with everything EGN3443 Modules
/// 11–13 read off it (Montgomery &amp; Runger Ch. 11–12): the coefficient table, the ANOVA
/// table, R² and adjusted R², the residual standard error, predictions with confidence and
/// prediction intervals, and the diagnostics — standardized residuals, leverage, Cook's
/// distance, Durbin–Watson, variance inflation factors. Written on MathNet's linear algebra:
/// β̂ = (XᵀX)⁻¹Xᵀy.
/// </summary>
public sealed class LinearModel
{
    private readonly Matrix<double> _xtxInverse;

    private LinearModel(IReadOnlyList<string> names, Matrix<double> x, Vector<double> y, double confidence)
    {
        Names = names;
        N = y.Count;
        P = x.ColumnCount;
        Confidence = confidence;
        _xtxInverse = (x.TransposeThisAndMultiply(x)).Inverse();
        Vector<double> beta = _xtxInverse * x.TransposeThisAndMultiply(y);
        Vector<double> fitted = x * beta;
        Vector<double> residuals = y - fitted;
        double mean = y.Average();
        Fitted = [.. fitted];
        Residuals = [.. residuals];
        SumSquaresTotal = y.Sum(v => (v - mean) * (v - mean));
        SumSquaresError = residuals.DotProduct(residuals);
        SumSquaresRegression = SumSquaresTotal - SumSquaresError;
        DegreesOfFreedomError = N - P;
        DegreesOfFreedomRegression = P - 1;
        MeanSquareError = DegreesOfFreedomError > 0 ? SumSquaresError / DegreesOfFreedomError : double.NaN;
        MeanSquareRegression = DegreesOfFreedomRegression > 0 ? SumSquaresRegression / DegreesOfFreedomRegression : double.NaN;
        ResidualStandardError = Math.Sqrt(MeanSquareError);
        RSquared = SumSquaresTotal > 0 ? 1 - SumSquaresError / SumSquaresTotal : double.NaN;
        AdjustedRSquared = DegreesOfFreedomError > 0 && SumSquaresTotal > 0 ? 1 - (1 - RSquared) * (N - 1) / DegreesOfFreedomError : double.NaN;
        F = MeanSquareError > 0 && DegreesOfFreedomRegression > 0 ? MeanSquareRegression / MeanSquareError : double.NaN;
        FPValue = double.IsNaN(F) ? double.NaN : 1 - FisherSnedecor.CDF(DegreesOfFreedomRegression, DegreesOfFreedomError, F);

        // Leverage, standardized residuals, Cook's distance.
        Matrix<double> hat = x * _xtxInverse * x.Transpose();
        Leverage = [.. Enumerable.Range(0, N).Select(i => hat[i, i])];
        StandardizedResiduals = new double[N];
        CooksDistance = new double[N];

        for (int i = 0; i < N; i++)
        {
            double denominator = ResidualStandardError * Math.Sqrt(Math.Max(1e-12, 1 - Leverage[i]));
            StandardizedResiduals[i] = denominator > 0 ? Residuals[i] / denominator : double.NaN;
            CooksDistance[i] = StandardizedResiduals[i] * StandardizedResiduals[i] / P * Leverage[i] / Math.Max(1e-12, 1 - Leverage[i]);
        }

        double dwNumerator = 0;

        for (int i = 1; i < N; i++)
        {
            dwNumerator += (Residuals[i] - Residuals[i - 1]) * (Residuals[i] - Residuals[i - 1]);
        }

        DurbinWatson = SumSquaresError > 0 ? dwNumerator / SumSquaresError : double.NaN;

        // The coefficient table.
        double tCritical = DegreesOfFreedomError > 0 ? StudentT.InvCDF(0, 1, DegreesOfFreedomError, 1 - (1 - confidence) / 2) : double.NaN;
        var coefficients = new List<Coefficient>();

        for (int j = 0; j < P; j++)
        {
            double se = Math.Sqrt(MeanSquareError * _xtxInverse[j, j]);
            double t = se > 0 ? beta[j] / se : double.NaN;
            double p = double.IsNaN(t) || DegreesOfFreedomError <= 0 ? double.NaN : 2 * (1 - StudentT.CDF(0, 1, DegreesOfFreedomError, Math.Abs(t)));
            double vif = j == 0 ? double.NaN : VarianceInflation(x, j);
            coefficients.Add(new Coefficient(names[j], beta[j], se, t, p, beta[j] - tCritical * se, beta[j] + tCritical * se, vif));
        }

        Coefficients = coefficients;
    }

    public IReadOnlyList<string> Names { get; }

    public int N { get; }

    /// <summary>The number of coefficients including the intercept.</summary>
    public int P { get; }

    public double Confidence { get; }

    public IReadOnlyList<Coefficient> Coefficients { get; }

    public double Intercept => Coefficients[0].Estimate;

    public double[] Fitted { get; }

    public double[] Residuals { get; }

    public double[] StandardizedResiduals { get; }

    public double[] Leverage { get; }

    public double[] CooksDistance { get; }

    public double SumSquaresTotal { get; }

    public double SumSquaresRegression { get; }

    public double SumSquaresError { get; }

    public double DegreesOfFreedomRegression { get; }

    public double DegreesOfFreedomError { get; }

    public double MeanSquareRegression { get; }

    public double MeanSquareError { get; }

    public double ResidualStandardError { get; }

    public double RSquared { get; }

    public double AdjustedRSquared { get; }

    public double F { get; }

    public double FPValue { get; }

    public double DurbinWatson { get; }

    /// <summary>Fits y = β₀ + β₁x₁ + … with an intercept; null when there are fewer than p + 1 points or the design is singular.</summary>
    public static LinearModel? Fit(IReadOnlyList<string> predictorNames, IReadOnlyList<IReadOnlyList<double>> predictors, IReadOnlyList<double> y, double confidence = 0.95)
    {
        ArgumentNullException.ThrowIfNull(predictors);
        ArgumentNullException.ThrowIfNull(y);
        int n = y.Count, k = predictors.Count;

        if (n < k + 2 || predictors.Any(c => c.Count != n) || y.Any(double.IsNaN) || predictors.Any(c => c.Any(double.IsNaN)))
        {
            return null;
        }

        var x = Matrix<double>.Build.Dense(n, k + 1, (i, j) => j == 0 ? 1 : predictors[j - 1][i]);

        // Rank by SVD is scale-free: a constant column duplicates the intercept, and a column
        // that is a combination of the others makes XᵀX singular whatever the units.
        if (x.Rank() < k + 1)
        {
            return null;
        }

        return new LinearModel(["(Intercept)", .. predictorNames], x, Vector<double>.Build.DenseOfEnumerable(y), confidence);
    }

    /// <summary>The fitted value at new predictor values, with the confidence interval on the mean response and the prediction interval on one observation.</summary>
    public Prediction Predict(IReadOnlyList<double> at)
    {
        Vector<double> x0 = Vector<double>.Build.DenseOfEnumerable([1.0, .. at]);
        double fitted = 0;

        for (int j = 0; j < P; j++)
        {
            fitted += Coefficients[j].Estimate * x0[j];
        }

        double leverage = x0.DotProduct(_xtxInverse * x0);
        double seMean = Math.Sqrt(MeanSquareError * leverage), seNew = Math.Sqrt(MeanSquareError * (1 + leverage));
        double t = StudentT.InvCDF(0, 1, DegreesOfFreedomError, 1 - (1 - Confidence) / 2);

        return new Prediction(fitted, seMean, fitted - t * seMean, fitted + t * seMean, fitted - t * seNew, fitted + t * seNew);
    }

    /// <summary>The expected normal quantiles for the sorted standardized residuals — the Q–Q plot's x axis (Blom's positions).</summary>
    public (double[] Theoretical, double[] Ordered) NormalQuantiles()
    {
        double[] ordered = [.. StandardizedResiduals.Order()];
        double[] theoretical = [.. Enumerable.Range(1, N).Select(i => Normal.InvCDF(0, 1, (i - 0.375) / (N + 0.25)))];
        return (theoretical, ordered);
    }

    /// <summary>
    /// k-fold cross-validation without shuffling — contiguous folds, as scikit-learn's
    /// KFold(shuffle=False) — returning the root-mean-square prediction error over the held-out
    /// observations, beside the in-sample RMSE for comparison.
    /// </summary>
    public static (double InSampleRmse, double CrossValidatedRmse) CrossValidate(IReadOnlyList<IReadOnlyList<double>> predictors, IReadOnlyList<double> y, int folds)
    {
        ArgumentNullException.ThrowIfNull(predictors);
        ArgumentNullException.ThrowIfNull(y);
        int n = y.Count;
        string[] names = [.. Enumerable.Range(1, predictors.Count).Select(i => $"x{i}")];
        LinearModel? whole = Fit(names, predictors, y);

        if (whole is null || folds < 2 || folds > n)
        {
            return (whole is null ? double.NaN : Math.Sqrt(whole.SumSquaresError / n), double.NaN);
        }

        double sumSquares = 0;
        int counted = 0;

        for (int f = 0; f < folds; f++)
        {
            int start = f * n / folds, end = (f + 1) * n / folds;
            int[] train = [.. Enumerable.Range(0, n).Where(i => i < start || i >= end)];
            List<IReadOnlyList<double>> trainX = [.. predictors.Select(c => (IReadOnlyList<double>)[.. train.Select(i => c[i])])];
            LinearModel? model = Fit(names, trainX, [.. train.Select(i => y[i])]);

            if (model is null)
            {
                return (Math.Sqrt(whole.SumSquaresError / n), double.NaN);
            }

            for (int i = start; i < end; i++)
            {
                double predicted = model.Predict([.. predictors.Select(c => c[i])]).Fitted;
                sumSquares += (y[i] - predicted) * (y[i] - predicted);
                counted++;
            }
        }

        return (Math.Sqrt(whole.SumSquaresError / n), Math.Sqrt(sumSquares / counted));
    }

    /// <summary>1/(1 − R²ⱼ), from regressing column j of the design on the other predictors.</summary>
    private static double VarianceInflation(Matrix<double> x, int column)
    {
        if (x.ColumnCount <= 2)
        {
            return 1;
        }

        Vector<double> target = x.Column(column);
        Matrix<double> others = x.RemoveColumn(column);
        Matrix<double> oto = others.TransposeThisAndMultiply(others);

        if (others.Rank() < others.ColumnCount)
        {
            return double.PositiveInfinity;
        }

        Vector<double> beta = oto.Inverse() * others.TransposeThisAndMultiply(target);
        Vector<double> fitted = others * beta;
        double mean = target.Average();
        double sst = target.Sum(v => (v - mean) * (v - mean)), sse = (target - fitted).DotProduct(target - fitted);
        double r2 = sst > 0 ? 1 - sse / sst : 0;
        return r2 >= 1 ? double.PositiveInfinity : 1 / (1 - r2);
    }
}

/// <summary>The transformations of Module 13, applied to x or to y before a linear fit.</summary>
public enum Transformation
{
    [System.ComponentModel.Description("None")]
    None = 0,

    [System.ComponentModel.Description("Natural log, ln")]
    Log,

    [System.ComponentModel.Description("Square root, √")]
    SquareRoot,

    [System.ComponentModel.Description("Reciprocal, 1/")]
    Inverse,
}

public static class Transformations
{
    public static double Apply(Transformation t, double v) => t switch
    {
        Transformation.Log => Math.Log(v),
        Transformation.SquareRoot => Math.Sqrt(v),
        Transformation.Inverse => 1 / v,
        _ => v,
    };

    public static double Undo(Transformation t, double v) => t switch
    {
        Transformation.Log => Math.Exp(v),
        Transformation.SquareRoot => v * v,
        Transformation.Inverse => 1 / v,
        _ => v,
    };

    /// <summary>Whether the value can be transformed: logs and roots need positives, reciprocals need non-zero.</summary>
    public static bool Accepts(Transformation t, double v) => t switch
    {
        Transformation.Log => v > 0,
        Transformation.SquareRoot => v >= 0,
        Transformation.Inverse => v != 0,
        _ => true,
    };

    public static string Symbol(Transformation t, string name) => t switch
    {
        Transformation.Log => $"ln({name})",
        Transformation.SquareRoot => $"√{name}",
        Transformation.Inverse => $"1/{name}",
        _ => name,
    };

    public static string Python(Transformation t, string name) => t switch
    {
        Transformation.Log => $"np.log({name})",
        Transformation.SquareRoot => $"np.sqrt({name})",
        Transformation.Inverse => $"1 / ({name})",
        _ => name,
    };

    public static string R(Transformation t, string name) => t switch
    {
        Transformation.Log => $"log({name})",
        Transformation.SquareRoot => $"sqrt({name})",
        Transformation.Inverse => $"1 / ({name})",
        _ => name,
    };

    /// <summary>The inverse transformation in Python, to turn a fitted value back into original units.</summary>
    public static string PythonUndo(Transformation t, string expression) => t switch
    {
        Transformation.Log => $"np.exp({expression})",
        Transformation.SquareRoot => $"({expression}) ** 2",
        Transformation.Inverse => $"1 / ({expression})",
        _ => expression,
    };

    /// <summary>The inverse transformation in R.</summary>
    public static string RUndo(Transformation t, string expression) => t switch
    {
        Transformation.Log => $"exp({expression})",
        Transformation.SquareRoot => $"({expression})^2",
        Transformation.Inverse => $"1 / ({expression})",
        _ => expression,
    };
}
