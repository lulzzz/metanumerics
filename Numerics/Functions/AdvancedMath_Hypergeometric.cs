﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meta.Numerics.Functions {
    public static partial class AdvancedMath {

        /// <summary>
        /// Computes the Gauss hypergeometric function. 
        /// </summary>
        /// <param name="a">The first upper parameter.</param>
        /// <param name="b">The second upper parameter.</param>
        /// <param name="c">The lower parameter.</param>
        /// <param name="x">The argument, which must be less than or equal to one.</param>
        /// <returns>The value of <sub>2</sub>F<sub>2</sub>(a, b; c; x).</returns>
        /// <remarks>
        /// <para>The Gauss Hypergeometric function is defined by...</para>
        /// <para>For generic values of a, b, and c, the Gauss hypergeometric function becomes complex for x > 1.
        /// There are specific cases, most commonly for negative integer values of a and b, for which the
        /// function remains real in this range.</para>
        /// </remarks>
        /// <seealso href="https://en.wikipedia.org/wiki/Hypergeometric_function"/>
        public static double Hypergeometric2F1 (double a, double b, double c, double x) {

            // The correct order of these limits is debatable.
            if (x == 0.0) return (1.0);
            if ((a == 0.0) || (b == 0.0)) return (1.0);
            if (IsNonPositiveInteger(c)) {
                if (!((IsNonPositiveInteger(a) && (a >= c)) || (IsNonPositiveInteger(b) && (b >= c)))) {
                    return (Double.NaN);
                }
            }
            if (c == a) return (Math.Pow(1.0 - x, -b));
            if (c == b) return (Math.Pow(1.0 - x, -a));

            if (x < -1.0) {

                // x -> 1/(1-x) maps (-inf, -1) -> (0, 1/2)
                double xPrime = 1.0 / (1.0 - x);

                double bma = b - a;
                double m = Math.Round(bma);
                double e = bma - m;

                if (m < 0) {
                    return (Hypergeometric2F1_Series_OneOverOneMinusX(b, -(int) m, -e, c, xPrime));
                } else {
                    return (Hypergeometric2F1_Series_OneOverOneMinusX(a, (int) m, e, c, xPrime));
                }

            } else if (x < -0.25) {

                // x -> x/(x-1) maps (-1, 0) -> (1/2, 0)
                double xPrime = x / (x - 1.0);

                // Use transformed series with smallest a, b values
                if (Math.Max(Math.Abs(a), Math.Abs(c - b)) <= Math.Max(Math.Abs(b), Math.Abs(c - a))) {
                    return (Math.Pow(1.0 - x, -a) * Hypergeometric2F1_Series(a, c - b, c, xPrime));
                } else {
                    return (Math.Pow(1.0 - x, -b) * Hypergeometric2F1_Series(c - a, b, c, xPrime));
                }

            } else if (x <= 0.5) {

                // Use series with smallest a, b values
                if (Math.Max(Math.Abs(a), Math.Abs(b)) <= Math.Max(Math.Abs(c - a), Math.Abs(c - b))) {
                    return (Hypergeometric2F1_Series(a, b, c, x));
                } else {
                    // This transform doesn't work in the polynomial case! (e.g. -1,-3/2,-2,1/2)
                    return (Math.Pow(1.0 - x, c - a - b) * Hypergeometric2F1_Series(c - a, c - b, c, x));
                }

            } else if (x <= 1.0) {

                // x -> 1-x maps (1/2, 1) -> (0, 1/2)
                double xPrime = 1.0 - x;

                // When xPrime is exactly zero, we can have some problems inside the transformed series routine.
                // Really we should deal with this by handling the x = 1 case analytically, but it's actually
                // not trivial to do so. So for now, just use a very very tiny value of xPrime.
                if (x == 1.0) xPrime = 1.0 / Double.MaxValue;

                double cab = c - a - b;
                double m = Math.Round(cab);
                double e = cab - m;

                if (m < 0) {
                    return (Hypergeometric2F1_Series_OneMinusX(c - a, c - b, -(int) m, -e, xPrime) * Math.Pow(xPrime, cab));
                } else {
                    return (Hypergeometric2F1_Series_OneMinusX(a, b, (int) m, e, xPrime));
                }

            } else {

                throw new ArgumentOutOfRangeException(nameof(x));

            }

        }

        private static bool IsNonPositiveInteger (double x) {
            return ((x <= 0.0) && (Math.Round(x) == x));
        }

        private static double Hypergeometric2F1_Series (double a, double b, double c, double x) {

            if ((Math.Abs(a) + 1.0) * (Math.Abs(b) + 1.0) * Math.Abs(x) > 16.0 * (Math.Abs(c) + 1.0)) {
                //throw new InvalidOperationException();
            }

            // recurrence

            double df = a * b / c * x;
            double f = 1.0 + df;

            for (int km1 = 1; km1 < Global.SeriesMax; km1++) {
                double f_old = f;
                df *= (a + km1) * (b + km1) / (c + km1) * x / (km1 + 1);
                f += df;
                if (f == f_old) return (f);
            }

            throw new NonconvergenceException();

        }

        // e G_{e}(z) = \frac{1}{\Gamma(z)} - \frac{1}{\Gamma(z+e)}
        //            = \frac{1}{\Gamma(z + e)} \left[ \frac{\Gamma(z + e)}{\Gamma(z)} - 1\right]
        //            = \frac{e P_{e}(z)}{\Gamma(z + e)}

        // e P_{e}(z) = \frac{\Gamma(z + e)}{\Gamma(z)} - 1

        // e L_{e}(z) = \ln \left( \frac{\Gamma(z + e)}{\Gamma(z)} \right) = \ln \Gamma(z + e) - \ln \Gamma(z)

        // Lanczos allows us to compute this is a way that the leading e-independent terms cancel analytically,
        // leaving us with an analytic expression for proportional to e.

        private static double NewG(double x, double e) {

            Debug.Assert(Math.Abs(e) <= 0.5);

            // This implementation is based on the paper, but it has some definite deficiencies.
            // For example, for e <~ 1.0E-15 and x near a negative integer, it gives totally wrong
            // answers, and the answers loose accuracy for even larger e. This is because the
            // computation relies on a ratio of h to Gamma, both of which blow up in this region.

            // It would be better to compute G outright from Lanczos, rather than via h. Can we do this?

            double y = x + e;
            if ((x < 0.5) || (y < 0.5)) {

                double h = MoreMath.ReducedExpMinusOne(Lanczos.ReducedLogPochhammer(1.0 - y, e), e);

                if (e == 0.0) {
                    double t = MoreMath.SinPi(x) * h / Math.PI - MoreMath.CosPi(x);
                    return (AdvancedMath.Gamma(1.0 - y) * t);
                } else {
                    double s = MoreMath.SinPi(e) / (Math.PI * e);
                    double s2 = MoreMath.Sqr(MoreMath.SinPi(e / 2.0)) / (Math.PI * e / 2.0);
                    double t = MoreMath.SinPi(x) * (h / Math.PI + s2) - MoreMath.CosPi(x) * s;
                    return (AdvancedMath.Gamma(1.0 - y) * t);
                }

                /*
                if (Math.Abs(Math.Round(x) - x) > Math.Abs(Math.Round(y) - y)) {
                    Global.Swap(ref x, ref y);
                    e = -e;
                }

                double h = MoreMath.ReducedExpMinusOne(Lanczos.ReducedLogPochhammer(1.0 - x, -e), -e);
                h = h * (MoreMath.CosPi(e) + MoreMath.SinPi(e) / Math.Tan(Math.PI * x));
                if (e == 0.0) {
                    h = h - 1.0 / Math.Tan(Math.PI * x);
                } else {
                    h = h + 2.0 * MoreMath.Sqr(MoreMath.SinPi(e / 2.0)) / e - MoreMath.SinPi(e) / e / Math.Tan(Math.PI * x);
                }
                h = h / (1.0 - e * h);
                return (h / AdvancedMath.Gamma(y));
                */
            }

            //if (x < 1.0) {
            //    double y = x + e;
            //    if (IsNonPositiveInteger(x)) return (-1.0 / AdvancedMath.Gamma(y));
            //    if (IsNonPositiveInteger(y)) return (1.0 / AdvancedMath.Gamma(x));
            //}

            // if x < 1/2, need to use transformation
            // should pick larger of x, x + e as denominator

            return (MoreMath.ReducedExpMinusOne(Lanczos.ReducedLogPochhammer(x, e), e) / AdvancedMath.Gamma(x + e));

            //return ((1.0 / AdvancedMath.Gamma(x) - 1.0 / AdvancedMath.Gamma(x + e)) / e);
        }

        // Our approach to evaluating the transformed series is taken from Michel & Stoitsov, "Fast computation of the
        // Gauss hypergeometric function with all its parameters complex with application to the Poschl-Teller-Ginocchio
        // potential wave functions" (https://arxiv.org/abs/0708.0116). Michel & Stoitsov had a great idea, but their
        // exposition leaves much to be desired, so I'll put in a lot of detail here.

        // The basic idea is an old one: use the linear transformation formulas (A&S 15.3.3-15.3.9) to map all x into
        // the region [0, 1/2]. The x -> (1-x) transformation, for example, looks like

        // F(a, b, c, x) =
        //   \frac{\Gamma(c) \Gamma(c-a-b)}{\Gamma(c-a) \Gamma(c-b)} F(a, b, a+b-c+1, 1-x) +
        //   \frac{\Gamma(c) \Gamma(a+b-c)}{\Gamma(a) \Gamma(b)} F(c-a, c-b, c-a-b, 1-x) (1-x)^{c-a-b}

        // When c-a-b is close to an integer, though, there is a problem. Write c = a + b + m + e, where m is a positive integer
        // and e is small. The transformed expression becomes:

        // \frac{F(a, b, c, x)}{\Gamma(c)} =
        //   \frac{\Gamma(m+e)}{\Gamma(b + m + e) \Gamma(a + m + e)} F(a, b, 1 - m - e, 1 - x) +
        //   \frac{\Gamma(-m-e)}{\Gamma(a) \Gamma(b)} F(b + m + e, a + m + e, 1 + m + e, 1 - x) (1-x)^{m + e}

        // In the first term, the F-function blows up as e-> 0 (or \Gamma(m+e), if m=0), and in the second term 
        // \Gamma(-m-e) blows up in that limit. By finding the divergent O(1/e) and the sub-leading O(1) terms, its's not too
        // hard to show that the divgences cancel leaving a finite result, and to derive those results for e=0. (A&S give some
        // of them.) But we still have a problem for e small-but-not-zero: the pre-limit expression have large cancelations,
        // and developing a series is e is unworkable (higher derivatives rapidly become complex and unwieldy).

        // A good solution, introduced by Forrey, and refined by Michel & Stoistov, is to use finite differences instead
        // of derivatives. If we can express the difference betwen \Gamma(z) and \Gamma(z + e) as a function of z and e that
        // we can compute, then we can analytically cancel the divergent parts and be left with a finite expression involving
        // our finite difference function instead of an infinite series of Taylor series terms. For e=0, the finite difference
        // is just the first derivative, but for non-zero e, it implicitly sums the contributions of all Taylor series terms.

        // The finite difference function to use is:
        //   e G_{e}(z) = \frac{1}{\Gamma(z)} - \frac{1}{\Gamma(z+e)}
        // I played around with a few others, e.g. the perhaps more obvious choice \frac{\Gamma(z+e)}{\Gamma(z)} = 1 + e P_{e}(z),
        // but the key advantage of G_{e}(z) is that it is perfectly finite even for non-positive-integer values of z and z+e,
        // because it uses the recriprocol \Gamma function. (I actually had a mostly-working algorithm using P_{e}(z), but it
        // broke down because P_{e}(z) itself still diverged for the problematic z values.)

        // In the x -> (1-x), x -> 1/x, x -> x / (1-x), and x -> 1 - 1/x linear transformations, canceling divergences
        // appear when some arguments of the transformed functions are non-positive-integers.

        private static double Hypergeometric2F1_Series_OneOverOneMinusX (double a, int m, double e, double c, double x1) {

            Debug.Assert(m >= 0);
            Debug.Assert(Math.Abs(e) <= 0.5);
            Debug.Assert(Math.Abs(x1) <= 0.75);

            double b = a + m + e;

            double g_c = AdvancedMath.Gamma(c);
            double rg_a = 1.0 / AdvancedMath.Gamma(a);
            double rg_b = 1.0 / AdvancedMath.Gamma(b);
            double rg_cma = 1.0 / AdvancedMath.Gamma(c - a);
            double rg_cmb = 1.0 / AdvancedMath.Gamma(c - b);

            // Pochhammer product, keeps track of (a)_k (c-b)_k (x')^{a + k}
            double p = Math.Pow(x1, a);

            double f0 = 0.0;
            if (m > 0) {

                f0 = p;

                double q = 1.0;
                for (int k = 1; k < m; k++) {
                    int km1 = k - 1;
                    p *= (a + km1) * (c - b + km1) * x1;
                    q *= (k - m - e) * k;
                    f0 += p / q; 
                }

                f0 *= g_c * rg_b * rg_cma * AdvancedMath.Gamma(m + e);
                p *= (a + (m - 1)) * (c - b + (m - 1)) * x1;
            }

            // Now compute the remaining terms with analytically canceled divergent parts.

            double t = rg_b * rg_cma * (NewG(1.0, -e) / AdvancedIntegerMath.Factorial(m) + NewG(m + 1, e)) -
                1.0 / AdvancedMath.Gamma(1 + m + e) * (NewG(a + m, e) / AdvancedMath.Gamma(c - a - e) + NewG(c - a, -e) / AdvancedMath.Gamma(b)) -
                MoreMath.ReducedExpMinusOne(Math.Log(x1), e) / AdvancedMath.Gamma(a + m) / AdvancedMath.Gamma(c - a - e) / AdvancedMath.Gamma(m + 1 + e);
            t *= p;

            double f1 = t;

            double u = p * rg_b * rg_cma / AdvancedMath.Gamma(1.0 - e) / AdvancedIntegerMath.Factorial(m);

            for (int k = 0; k < Global.SeriesMax; k++) {

                double f1_old = f1;

                int k1 = k + 1;
                int mk1 = m + k1;
                double amk = a + m + k;
                double amke = amk + e;
                double cak = c - a + k;
                double cake = cak - e;
                double k1e = k1 - e;
                double mk1e = mk1 + e;

                double r = amk * cake / k1e / mk1;
                double s = amke * cak / mk1e / k1;

                // Compute (r - s) / e analytically because leading terms cancel
                double d = (amk * cake / mk1 - amk - cake - e + amke * cak / k1) / mk1e / k1e;

                t = (s * t + d * u) * x1;

                f1 += t;

                if (f1 == f1_old) {
                    f1 *= ReciprocalSincPi(e) * g_c;
                    if (m % 2 != 0) f1 = -f1;
                    return (f0 + f1);
                }

                u *= r * x1;

            }

            throw new NonconvergenceException();

        }

        private static double Hypergeometric2F1_Series_OneMinusX (double a, double b, int m, double e, double x1) {

            Debug.Assert(m >= 0);
            Debug.Assert(Math.Abs(e) <= 0.5);
            Debug.Assert(Math.Abs(x1) <= 0.75);

            double c = a + b + m + e;

            // Compute all the gammas we will use.
            double g_c = AdvancedMath.Gamma(c);
            double rg_am = 1.0 / AdvancedMath.Gamma(a + m);
            double rg_bm = 1.0 / AdvancedMath.Gamma(b + m);
            double rg_ame = 1.0 / AdvancedMath.Gamma(a + m + e);
            double rg_bme = 1.0 / AdvancedMath.Gamma(b + m + e);
            double rg_m1e = 1.0 / AdvancedMath.Gamma(m + 1 + e);

            // Pochhammer product, keeps track of (a)_m (b)_m (x')^m
            double p = 1.0;

            // First compute the finite sum, which contains no divergent terms even for e = 0.
            double f0 = 0.0;
            if (m > 0) {

                double t0 = 1.0;
                f0 = t0;
                for (int k = 1; k < m; k++) {
                    int km1 = k - 1;
                    p *= (a + km1) * (b + km1) * x1;
                    t0 *= 1.0 / (1.0 - m - e + km1) / k;
                    f0 += t0 * p;
                }

                f0 *= g_c * rg_bme * rg_ame * AdvancedMath.Gamma(m + e);
                p *= (a + (m - 1)) * (b + (m - 1)) * x1;

            }

            // Now compute the remaining terms with analytically canceled divergent parts.

            double t = rg_bme * rg_ame * (NewG(1.0, -e) / AdvancedIntegerMath.Factorial(m) + NewG(m + 1, e)) -
                rg_m1e * (NewG(a + m, e) * rg_bme + NewG(b + m, e) * rg_am) -
                MoreMath.ReducedExpMinusOne(Math.Log(x1), e) * rg_am * rg_bm * rg_m1e;


            t *= p;
            double f1 = t;
            double u = p * rg_bme * rg_ame / AdvancedMath.Gamma(1.0 - e) / AdvancedIntegerMath.Factorial(m);

            for (int k = 0; k < Global.SeriesMax; k++) {

                double f1_old = f1;

                // Compute a bunch of sums we will use.
                int k1 = k + 1;
                int mk = m + k;
                int mk1 = mk + 1;
                double k1e = k1 - e;
                double amk = a + mk;
                double bmk = b + mk;
                double amke = amk + e;
                double bmke = bmk + e;
                double mk1e = mk1 + e;
 
                // Compute the ratios of each term. These are close, but not equal for e != 0.
                double r = amk * bmk / mk1 / k1e;
                double s = amke * bmke / mk1e / k1;

                // Compute (r - s) / e, with O(1) terms of (r - s) analytically canceled.
                double d = (amk * bmk / mk1 - (amk + bmk + e) + amke * bmke / k1) / mk1e / k1e;

                // Advance to the next term, including the correction for s != t.
                t = (s * t + d * u) * x1;

                f1 += t;

                if (f1 == f1_old) {
                    f1 *= ReciprocalSincPi(e) * g_c;
                    if (m % 2 != 0) f1 = -f1;
                    return (f0 + f1);
                }

                // Advance the u term, which we will need for the next iteration.
                u *= r * x1;

            }

            throw new NonconvergenceException();

        }

        private static double ReciprocalSincPi (double e) {
            Debug.Assert(Math.Abs(e) <= 1.0);
            if (e == 0.0) {
                return (1.0);
            } else {
                double x = Math.PI * e;
                return (x / Math.Sin(x));
            }
        }

    }
}