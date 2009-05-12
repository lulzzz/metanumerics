using System;
using System.Collections.Generic;
using System.Diagnostics;
using Meta.Numerics;

namespace Meta.Numerics.Matrices {

    /// <summary>
    /// Represents a square matrix.
    /// </summary>
    public sealed class SquareMatrix : ISquareMatrix {

        private int dimension;
        private double[,] values;

        /// <summary>
        /// Initializes a new square matrix.
        /// </summary>
        /// <param name="dimension">The dimension of the matrix, which must be positive.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dimension"/> &lt; 1.</exception>
        public SquareMatrix (int dimension) {
            if (dimension < 1) throw new ArgumentOutOfRangeException("dimension");
            this.dimension = dimension;
            this.values = new double[dimension, dimension];
        }

        /*
        internal SquareMatrix (double[,] values) {
            if (values.GetLength(0) != values.GetLength(1)) throw new ArgumentException();
            this.dimension = values.GetLength(0);
            this.values = values;
        }
        */

        /// <summary>
        /// Gets the dimension of the matrix.
        /// </summary>
        public int Dimension {
            get {
                return (dimension);
            }
        }

        int IMatrix.RowCount {
            get {
                return (dimension);
            }
        }

        int IMatrix.ColumnCount {
            get {
                return (dimension);
            }
        }

        /// <summary>
        /// Gets or sets an entry of the matrix.
        /// </summary>
        /// <param name="r">The (zero-based) row number.</param>
        /// <param name="c">The (zero-based) column number.</param>
        /// <returns>The value of the specified matrix entry M<sub>r c</sub>.</returns>
        public double this[int r, int c] {
            get {
                if ((r < 0) || (r >= dimension)) throw new ArgumentOutOfRangeException("r");
                if ((c < 0) || (c >= dimension)) throw new ArgumentOutOfRangeException("c");
                return (values[r, c]);
            }
            set {
                if ((r < 0) || (r >= dimension)) throw new ArgumentOutOfRangeException("r");
                if ((c < 0) || (c >= dimension)) throw new ArgumentOutOfRangeException("c");
                values[r, c] = value;
            }
        }


        /// <summary>
        /// Returns a vector representing a given row of the matrix.
        /// </summary>
        /// <param name="r">The (zero-based) row number to return.</param>
        /// <returns></returns>
        /// <remarks>The returned vector is not linked to the matrix. If an entry in the matrix is updated after this method
        /// is called, the returned object will continue to represent a row of the original, not the updated, matrix. Similiarly,
        /// updates to the elements of the returned vector will not update the original matrix.</remarks>
        public RowVector Row (int r) {
            if ((r < 0) || (r >= dimension)) throw new ArgumentOutOfRangeException("r");
            RowVector row = new RowVector(dimension);
            for (int c = 0; c < dimension; c++) {
                row[c] = values[r, c];
            }
            return (row);
        }

        /// <summary>
        /// Gets a copy of one column of the the matrix.
        /// </summary>
        /// <param name="c">The (zero-based) column number to return.</param>
        /// <returns></returns>
        /// <remarks>The returned vector is not linked to the matrix. If an entry in the matrix is updated after this method
        /// is called, the returned object will continue to represent a row of the original, not the updated, matrix. Similiarly,
        /// updates to the elements of the returned vector will not update the original matrix.</remarks>
        public ColumnVector Column (int c) {
            if ((c < 0) || (c >= dimension)) throw new ArgumentOutOfRangeException("c");
            ColumnVector column = new ColumnVector(dimension);
            for (int r = 0; r < dimension; r++) {
                column[r] = values[r, c];
            }
            return (column);
        }

        /// <summary>
        /// Clones the matrix.
        /// </summary>
        /// <returns>An independent clone of the matrix.</returns>
        public SquareMatrix Clone () {
            SquareMatrix clone = new SquareMatrix(dimension);
            for (int r = 0; r < dimension; r++) {
                for (int c = 0; c < dimension; c++) {
                    clone[r, c] = values[r, c];
                }
            }
            return (clone);
        }

        IMatrix IMatrix.Clone () {
            return (Clone());
        }

        /// <summary>
        /// Creates a transpose of the matrix.
        /// </summary>
        /// <returns>The matrix transpose M<sup>T</sup>.</returns>
        public SquareMatrix Transpose () {
            SquareMatrix transpose = new SquareMatrix(dimension);
            for (int r = 0; r < dimension; r++) {
                for (int c = 0; c < dimension; c++) {
                    transpose[c, r] = values[r, c];
                }
            }
            return (transpose);
        }

        IMatrix IMatrix.Transpose () {
            return (Transpose());
        }

        /// <summary>
        /// Computes the inverse of the matrix.
        /// </summary>
        /// <returns>The matrix inverse M<sup>-1</sup>.</returns>
        /// <remarks>
        /// <para>The inverse of a matrix is a matrix M<sup>-1</sup> such that M<sup>-1</sup>M = I, the identify matrix.</para>
        /// <para>The inversion of a matrix is an O(N<sup>3</sup>) operation.</para>
        /// </remarks>
        public SquareMatrix Inverse () {
            SquareMatrix inverse = GaussJordanInvert(this.Clone());
            return (inverse);
        }

        /*
        /// <summary>
        /// Inverts the matrix, in place.
        /// </summary>
        /// <remarks>
        /// <para>This method replaces the elements of M with the elements of M<sup>-1</sup>.</para>
        /// <para>In place matrix inversion saves memory, since seperate storage of M and M<sup>-1</sup> is not required.</para></remarks>
        private void InvertInPlace () {
        }
        */

        /// <summary>
        /// Computes the trace of the matrix.
        /// </summary>
        /// <returns>The trace of the matrix tr(M).</returns>
        /// <remarks><para>The trace of a matrix is the sum of its diagonal elements.</para></remarks>
        public double Trace () {
				double trace = 0.0;
				for (int i=0; i<dimension; i++) {
					trace+= values[i,i];
				}
				return(trace);
        }

        // routines for algorithms

        private static SquareMatrix GaussJordanInvert (SquareMatrix M) {

            // the dimension of the problem
            int d = M.Dimension;

            // initialize a unit matrix
            SquareMatrix N = new SquareMatrix(d);
            for (int r = 0; r < d; r++) {
                for (int c = 0; c < d; c++) {
                    N[r, c] = 0.0;
                }
                N[r, r] = 1.0;
            }

            // keep track of the pivots
            int[] pivots = new int[d];
            for (int i = 0; i < d; i++) {
                pivots[i] = -1;
            }

            // interate over columns
            for (int c = 0; c < d; c++) {

                //Console.WriteLine("c = {0}", c);
                //PrintMatrix(M);
                //PrintMatrix(N);

                // choose a pivot in the current column
                int p = 0;
                double pe = 0.0;
                for (int r = c; r < d; r++) {
                    if ((pivots[r] < 0) && (Math.Abs(M[r, c]) > Math.Abs(pe))) {
                        p = r;
                        pe = M[r, c];
                    }
                }

                //Console.WriteLine("p = {0}, pe = {1}", p, pe);

                // if no non-zero pivot element was found, matrix is singular
                if (pe == 0.0) throw new DivideByZeroException();

                // record the pivot
                //pivots[p] = c;

                // swap rows to move the pivot to the diagonal
                //Console.WriteLine("swap");
                if (p != c) {
                    for (int cp = 0; cp < d; cp++) {
                        double t = M[p, cp];
                        M[p, cp] = M[c, cp];
                        M[c, cp] = t;

                        double u = N[p, cp];
                        N[p, cp] = N[c, cp];
                        N[c, cp] = u;
                    }
                }
                //PrintMatrix(M);
                //PrintMatrix(N);

                // normalize the pivot row
                //Console.WriteLine("normalize");
                for (int cp = 0; cp < d; cp++) {
                    M[c, cp] /= pe;

                    N[c, cp] /= pe;
                }
                //PrintMatrix(M);
                //PrintMatrix(N);

                // for each non-pivot row, add a multiple of the pivot to zero
                // the entries in the pivot's column
                //Console.WriteLine("zero");
                for (int r = 0; r < d; r++) {
                    if (r != c) {
                        double t = -M[r, c];
                        for (int cp = 0; cp < d; cp++) {
                            M[r, cp] += t * M[c, cp];
                            N[r, cp] += t * N[c, cp];
                        }
                    }
                }
                //PrintMatrix(M);
                //PrintMatrix(N);

            }

            // return the inverse
            return (N);

        }

        /*
        // in-place inversion using the Gauss-Jordon alrogithm
        private void GaussJordanInvert () {

            // array to track pivots
            bool[] pv = new bool[dimension];
            for (int i = 0; i < dimension; i++) {
                pv[i] = false;
            }

            // iterate over columns
            for (int i = 0; i < dimension; i++) {

                // choose a pivot
                int p = 0;
                double pe = 0.0;
                for (int j = 0; j < dimension; j++) {
                    if ((!pv[j]) && (Math.Abs(values[j, j]) > Math.Abs(pe))) {
                        p = j;
                        pe = values[p, p];
                    }
                }
                if (pe == 0.0) throw new DivideByZeroException("Singular matrix");

                // normalize the pivot
                values[p, p] = 1.0;
                for (int j = 0; j < dimension; j++) values[p, j] = values[p, j] / pe;

                // zero the other entries in that column
                for (int j = 0; j < dimension; j++) {
                    if (j == p) continue;
                    double t = -values[j, p];
                    values[j, p] = 0;
                    for (int k = 0; k < dimension; k++) values[j, k] += t * values[p, k];
                }

                // done with that pivot
                pv[p] = true;
            }
        }
        */

        /// <summary>
        /// Computes the LU decomposition of the matrix.
        /// </summary>
        /// <returns>The LU decomposition of the matrix.</returns>
        /// <remarks>
        /// <para>An LU decomposition of a matrix M is a set of matrices L, U, and P such that LU = PM, where L
        /// is lower-left triangular, U is upper-right triangular, and P is a permutation matrix (so that PM is
        /// a row-wise permutation of M).</para>
        /// <para>The LU decomposition of a square matrix is an O(N<sup>3</sup>) operation.</para>
        /// </remarks>
        public SquareLUDecomposition LUDecomposition () {
            SquareMatrix clone = this.Clone();

            int[] perm;
            int parity;
            clone.LUDecompose(out perm, out parity);

            return (new SquareLUDecomposition(clone.values, perm, parity));
        }


        // On return, the lower left part of the matrix is the L matrix (whoose diagonal elements are one),
        // and the upper left part of the matrix (including the diagonal) is the U matrix. The perm vector
        // records the row-wise permutations that were undertaken.
        private void LUDecompose (out int[] perm, out int parity) {

        	// Keep track of permutations and parity
			perm = new int[dimension];
			for (int i=0; i<dimension; i++) {
				perm[i] = i;
			}
            parity = 1;

            // Loop over columns
			for (int j=0; j<dimension; j++) {

                // storage for pivot row and pivot element value
                int p = 0;
                double pe = 0.0;

				// do sums over rows
				for (int i=0; i<j; i++) {
					for (int k=0; k<i; k++) {
						values[i,j] -= values[i,k] * values[k,j];
					}
				}

				for (int i=j; i<dimension; i++) {
					for (int k=0; k<j; k++) {
					    values[i,j] -= values[i,k] * values[k,j];
					}

					// decide on a pivot
					if (Math.Abs(values[i,j]) > pe) {
						p = i;
						pe = Math.Abs(values[i,j]);
					}

				}

				// if necessary, interchange rows to obtain the desired pivot
				if (p != j) {
					for (int k=0; k<dimension; k++) {
						double t = values[p,k];
						values[p,k] = values[j,k];
						values[j,k] = t;
					}
					int tp = perm[p];
					perm[p] = perm[j];
					perm[j] = tp;
                    parity = -parity;
				}

				// divide the appropriate elements by the pivot
				if (values[j,j] == 0.0) throw new DivideByZeroException("Singular matrix");
				for (int i=j+1; i<dimension; i++) {
					values[i,j] = values[i,j] / values[j,j];
				}
			}

        }

        /*
        private static int Min (int a, int b) {
            if (a < b) {
                return (a);
            } else {
                return (b);
            }
        }

        private static int Max (int a, int b) {
            if (a > b) {
                return (a);
            } else {
                return (b);
            }
        }
        */

        /// <summary>
        /// Computes the eigenvalues of the matrix.
        /// </summary>
        /// <returns>The eigenvalues of the matrix.</returns>
        /// <seealso cref="Eigensystem"/>
        public Complex[] Eigenvalues () {
            SquareMatrix A = this.Clone();
            ReduceToHessenberg(A, null);
            Complex[] eigenvalues = ExtractEigenvalues(A, null);
            return (eigenvalues);

        }

        /// <summary>
        /// Computes the eigenvalues and eigenvectors of the matrix.
        /// </summary>
        /// <returns>A representation of the eigenvalues and eigenvectors of the matrix.</returns>
        /// <remarks>
        /// <para>For a generic vector v and matrix M, Mv = u will point in some direction with no particular relationship to v.
        /// The eigenvectors of a matrix M are vectors z that satisfy Mz = &#x3BB;z, i.e. multiplying an eigenvector by a
        /// matrix reproduces the same vector, up to a prortionality constant &#x3BB; called the eigenvalue.</para>
        /// <para>For v to be an eigenvector of M with eigenvalue &#x3BB;, (M - &#x3BB;I)z = 0. But for a matrix to
        /// anihilate any non-zero vector, that matrix must have determinant, so det(M - &#x3BB;I)=0. For a matrix of
        /// order N, this is an equation for the roots of a polynomial of order N. Since an order-N polynomial always has exactly
        /// N roots, an order-N matrix always has exactly N eigenvalues.</para>
        /// <para>Since a polynomial with real coefficients can still have complex roots, a real square matrix can nonetheless
        /// have complex eigenvalues (and correspondly complex eigenvectors). However, again like the complex roots of a real
        /// polynomial, such eigenvalues will always occurs in complex-conjugate pairs.</para>
        /// <para>Although the eigenvalue polynomial ensures that an order-N matrix has N eigenvalues, it can occur that there
        /// are not N corresponding independent eigenvectors. A matrix with fewer eigenvectors than eigenvalues is called
        /// defective. Like singularity, defectiveness represents a delecate balance between the elements of a matrix that can
        /// typically be disturbed by just an infinitesimal perturbation of elements. Because of round-off-error, then, floating-point
        /// algorithms cannot reliably identify defective matrices. Instead, this method will return a full set of eigenvectors,
        /// but some eigenvectors, corresponding to very nearly equal eigenvalues, will be very nearly parallel.</para>
        /// <para>While a generic square matrix can be defective, many subspecies of square matrices are guaranteed not to be.
        /// This includes Markov matrices, orthogonal matrices, and symmetric matrices.</para>
        /// <para>Determining the eigenvalues and eigenvectors of a matrix is an O(N<sup>3</sup>) operation. If you need only the
        /// eigenvalues of a matrix, the <see cref="Eigenvalues"/> method is more efficient.</para>
        /// </remarks>
        public ComplexEigensystem Eigensystem () {

            SquareMatrix A = this.Clone();
            Debug.WriteLine("Start A=");
            PrintMatrix(A);

            // start with an identity matrix to track the transforms
            SquareMatrix Q = new SquareMatrix(dimension);
            for (int i = 0; i < dimension; i++) Q[i, i] = 1.0;

            // balance the matrix
            /*
            Balance(A);
            Debug.WriteLine("Balanced A=");
            PrintMatrix(A);
            Debug.WriteLine("Transform Q=");
            PrintMatrix(Q);
            Debug.WriteLine("Test Q^T M Q = A");
            PrintMatrix(Q.Transpose() * this * Q);
            */

            // reduce to Hessenberg form
            ReduceToHessenberg(A, Q);
            Debug.WriteLine("Hessenberg A=");
            PrintMatrix(A);
            Debug.WriteLine("Transform Q=");
            PrintMatrix(Q);
            Debug.WriteLine("Test Q^T M Q = A");
            PrintMatrix(Q.Transpose() * this * Q);
            
            // reduce to Schur form, extracting eigenvalues as we go
            Complex[] eigenvalues = ExtractEigenvalues(A, Q);
            Debug.WriteLine("Schur A=");
            PrintMatrix(A);
            Debug.WriteLine("Transform Q=");
            PrintMatrix(Q);
            Debug.WriteLine("Test Q^T M Q = A");
            PrintMatrix(Q.Transpose() * this * Q);

            // get eigenvectors
            Complex[,] eigenvectors = ExtractEigenvectors(A, Q, eigenvalues);
            NormalizeEigenvectors(eigenvectors);

            ComplexEigensystem eigensystem = new ComplexEigensystem(dimension, eigenvalues, eigenvectors);
            return (eigensystem);

        }

        private static void ReduceToHessenberg (SquareMatrix A, SquareMatrix Q) {

            // note the dimension of the problem
            int d = A.Dimension;

            // iterate over columns
            for (int k=0; k<(d-2); k++) {

                // determine the Householder transform P that will zero the elements of the column below the sub-diagonal
                double[] u = new double[d-1-k];
                for (int rp=0; rp < u.Length; rp++) {
                    int r = k + 1 + rp;
                    u[rp] = A[r,k]; 
                }
                double x = ComputeHouseholderVector(ref u);

                // compute P * A
                // use P = ( 1 - u u^T) so P * A = A - u (u^T * A) = A - u v^T where v^T = u^T A
                for (int c = k + 1; c < d; c++) {
                    double v = 0.0;
                    for (int rp = 0; rp < u.Length; rp++) {
                        int r = k + 1 + rp;
                        v += u[rp] * A[r, c];
                    }
                    for (int rp = 0; rp < u.Length; rp++) {
                        int r = k + 1 + rp;
                        A[r, c] -= u[rp] * v;
                    }
                }

                // note that in the outer loop above c would naively go from 0 to dimension, but for c < k, we know that
                // A[r,c] = 0 for r > k + 1, because those columns are already in Hessenberg form, so we will get v = 0 for them.

                // for c = k, we know that the transform will make the subdiagonal element x and the elements below it zero;
                // we can save computational effort and avoid round-off error by just setting them to that result.
                A[k + 1, k] = x;
                for (int r = k + 2; r < d; r++) {
                    A[r, k] = 0.0;
                }

                // compute A * P
                // use P = ( 1 - u u^T) so A * P = A - (A u) u^T = A - v u^T where v = A u
                for (int r = 0; r < d; r++) {
                    double v = 0.0;
                    for (int cp = 0; cp < u.Length; cp++) {
                        int c = k + 1 + cp;
                        v += A[r, c] * u[cp];
                        //if (A[r, c] == 0.0) Console.WriteLine("{0},{1} => 0", r, c);
                    }
                    for (int cp = 0; cp < u.Length; cp++) {
                        int c = k + 1 + cp;
                        A[r, c] -= v * u[cp];
                    }
                }

                //PrintMatrix(A);
                //Console.ReadLine();

                // if we we are keeping track of transformations, record the transform that got us here
                if (Q != null) {
                    // compute Q * P
                    for (int r = 0; r < d; r++) {
                        double v = 0.0;
                        for (int cp = 0; cp < u.Length; cp++) {
                            int c = k + 1 + cp;
                            v += Q[r, c] * u[cp];
                        }
                        for (int cp = 0; cp < u.Length; cp++) {
                            int c = k + 1 + cp;
                            Q[r, c] -= v * u[cp];
                        }
                    }
                }

                // done with that column

            }

            // done with all columns

        }


        // updates a transform matrix Q with a (right) Householder transform
        // this is used to keep track of accumulated transforms

        /*
        private void HouseholderTransform (ref double[,] Q, int offset, double[] u) {
            // compute Q * P
            for (int r = 0; r < dimension; r++) {
                double v = 0.0;
                for (int cp = 0; cp < u.Length; cp++) {
                    int c = offset + cp;
                    v += Q[r, c] * u[cp];
                }
                for (int cp = 0; cp < u.Length; cp++) {
                    int c = offset + cp;
                    Q[r, c] -= v * u[cp];
                }
            }
        }
        */

        // on return, upper Hessenberg part of the matrix (i.e. the upper right triangle and the first subdiagonal)
        // contains H and the returned matrix contains a unitary transform Q such that Q^(-1) A Q = H.
        /*
        public double[,] HessenbergReduce () {

            // start with an identity matrix to track the transforms
            double[,] Q = new double[dimension, dimension];
            for (int i = 0; i < dimension; i++) Q[i, i] = 1.0;

            // iterate over columns,
            // doing a Householder transform for each column to eliminate the entires in rows below the sub-diagonal
            for (int c = 0; c < (dimension - 1); c++) {

                // determine the sub-diagonal norm
                double x = 0.0;
                for (int r = c + 1; r < dimension; r++) {
                    double v = values[r, c];
                    x += v * v;
                }
                x = Math.Sqrt(x);

                // if the norm is zero, the column in already Hessenberg
                if (x == 0.0) continue;

                // choose the first component of the Householder transform vector to minimize roundoff error
                double s, d;
                if (values[c + 1, c] < 0) {
                    values[c + 1, c] -= x;
                    d = x;
                    s = -x * values[c + 1, c];
                } else {
                    values[c + 1, c] += x;
                    d = -x;
                    s = x * values[c + 1, c];
                }

                // if s=0, we have a problem

                // set A=AH
                for (int i = 0; i < dimension; i++) {
                    double sum = 0.0;
                    for (int k = c + 1; k < dimension; k++) {
                        sum += values[i, k] * values[k, c];
                    }
                    sum = sum / s;
                    for (int j = c + 1; j < dimension; j++) {
                        values[i, j] -= sum * values[j, c];
                    }
                }
                // set A=HA (= HAH)
                for (int j = c + 1; j < dimension; j++) {
                    double sum = 0.0;
                    for (int k = c + 1; k < dimension; k++) {
                        sum += values[k, c] * values[k, j];
                    }
                    sum = sum / s;
                    for (int i = c + 1; i < dimension; i++) {
                        values[i, j] -= values[i, c] * sum;
                    }
                }

                // update Q=QH to reflect the transform
                for (int i = 0; i < dimension; i++) {
                    double sum = 0.0;
                    for (int k = c + 1; k < dimension; k++) {
                        sum += Q[i, k] * values[k, c];
                    }
                    sum = sum / s;
                    for (int j = c + 1; j < dimension; j++) {
                        Q[i, j] -= sum * values[j, c];
                    }
                }

                // update subdiagonal elements in column c
                values[c + 1, c] = d;
                for (int i = c + 2; i < dimension; i++) {
                    values[i, c] = 0.0;
                }
            }

            return (Q);

        }
        */

        private static Complex[] ExtractEigenvalues (SquareMatrix A, SquareMatrix Q) {

            // note the dimension
            int dim = A.Dimension;

            // keep track of eigenvalues found
            Complex[] values = new Complex[dim]; // storage for the eigenvalues
            int c = 0; // count of eigenvalues found

            // keep track of the active area
            int a = 0; // the upper-left index of the active area of the  matrix
            int n = dim - 1; // the lower-right index of the active area of the matrix
            // the effective dimension de = n - a

            // maximum number of iterations
            int countMax = dim * 30;

			// keep track of iterations
			int count = 0;

            while (true) {

                // check whether we have all the eigenvalues
                if (c >= dim) break;

                Debug.WriteLine(String.Format("iteration count = {0}", count));
                //Debug.WriteLine("A = ");
                //PrintMatrix(A);

                // zero negligible sub-diagonal elements
                for (int r = a+1; r <= n; r++) {
                    double x = Math.Abs(A[r, r]) + Math.Abs(A[r - 1, r - 1]);
                    if ((x + A[r, r - 1]) == x) {
                        A[r, r - 1] = 0.0;
                        if (a == (r - 1)) {
                            // we have uncovered an eigenvalue at the top
                            values[a] = A[a,a]; // record the eigenvalue
                            c++; // one more eigenvalue
                            Debug.WriteLine(String.Format("Got eigenvalue {0} from top: {1}", c, values[a]));
                            a++; // active area shrinks by one from the top
                            //count = 0; // reset the iteration count
                        } else if (a == (r - 2)) {
                            // we have isolated a 2x2 matrix at the top
                            // compute its eigenvalues (move this to a subroutine)
                            double tr = A[a, a] + A[a + 1, a + 1];
                            double det = A[a, a] * A[a + 1, a + 1] - A[a, a + 1] * A[a + 1, a];
                            ExtractTwoByTwoEigenvalues(tr, det, out values[a], out values[a + 1]);
                            c += 2; // eigenvalue count increases by two
                            Debug.WriteLine(String.Format("Got eigenvalues up to {0} from the top: {1}, {2}", c, values[a], values[a+1]));
                            a += 2; // active area shrinks by two from the top
                            //count = 0; // reset the iteration count
                        }
                    }
                }
                //Debug.WriteLine(String.Format("a={0}", a));

                // check again
                if (c >= dim) break;

                if ((n == 0) || (A[n, n - 1] == 0.0)) {
                    // we have isolated a single eigenvalue in the lower-right corner
                    values[n] = A[n, n]; // record the eigenvalue
                    c++; // one more eigenvalue
                    Debug.WriteLine(String.Format("Got eigenvalue {0} from bottom: {1}", c, values[n]));
                    n--; // active area decreases by one from bottom
                    //count = 0; // reset the iteration count
                } else {
                    // look at the lower 2X2 matrix
                    int m = n - 1;
                    double tr = A[m, m] + A[n, n];
                    double det = A[m, m] * A[n, n] - A[m, n] * A[n, m];
                    // construct the eigenvalues of the 2 X 2 lower-right matrix
                    if ((m == 0) || (A[m, m - 1] == 0.0)) {
                        // we have isolated a 2 X 2 matrix

                        // compute its eigenvalues
                        ExtractTwoByTwoEigenvalues(tr, det, out values[m], out values[n]);
                        c += 2; // two more eigenvalues
                        Debug.WriteLine(String.Format("Got eigenvalues up to {0} from bottom: {1}, {2}", c, values[m], values[n]));
                        n -= 2; // the active area decreases by two from the bottom
                        //count = 0; // reset the iteration count

                    } else {

                        // an ad hoc shift if we are not converging
                        // i see no evidence that this acomplishes anything!
                        /*
                        if ((count % 8) == 7) {
                            //Console.WriteLine("was tr={0} det={1}", tr, det);
                            tr = A[m, m-1] + A[n, n-1];
                            det = 0.0;
                            //Console.WriteLine("now tr={0} det={1}", tr, det);
                        }
                        */

                        // do a Francis implicit QR step to reduce sub-diagonal elements
                        FrancisTwoStep(A, Q, a, n, tr, det);

                        // up the iteration count
                        count++;
                        if (count > (countMax-5)) {
                            PrintMatrix(A);
                        }
                        if (count > countMax) {
                            throw new NonconvergenceException();
                        }

                    }

                }
            }

            // we have reduced the dimension to zero, so we have all the eigenvalues

            return (values);
        }

        // get the eigenvalues of a real 2x2 matrix

        private static void ExtractTwoByTwoEigenvalues (double tr, double det, out Complex e1, out Complex e2) {
            double p = tr / 2.0;
            double q2 = p * p - det;
            if (q2 < 0.0) {
                // if the descriminant is negative, the eigenvalues are complex
                double q = Math.Sqrt(-q2);
                e1 = new Complex(p, q);
                e2 = new Complex(p, -q);
            } else {
                // otherwise they are real
                double q = Math.Sqrt(q2);
                e1 = p + q;
                e2 = p - q;
            }
        }

        // do a Francis implicit QR step on A
        // the active area is between indices a and n
        // the sum of the eigenvalues is tr, the product is det
        // keep track of the transformations in Q, which may be null

        private static void FrancisTwoStep (SquareMatrix A, SquareMatrix Q, int a, int n, double tr, double det) {

            int dim = A.Dimension;
            //a = 0; // temporary
            int m = n - 1;

            // compute the first column of A' = A^2 - 2 Re(\lambda) A + |\lambda|^2 I
            // note 2 Re(\lambda) = tr and |\lambda|^2 = det
            // because A is Hessenberg, only the first three elements of this column are non-zero

            double[] u = new double[3];
            u[0] = A[a, a] * (A[a, a] - tr) + A[a, a+1] * A[a+1, a] + det;
            u[1] = A[a+1, a] * (A[a, a] + A[a+1, a+1] - tr);
            u[2] = A[a+1, a] * A[a+2, a+1];
            //Debug.WriteLine("u = ");
            //PrintVector(u);

            // the implicit Q theorem says that the Q required to bring A' back to Hessenberg form
            // is determined by its first column, so we can go ahead and apply

            // compute the corresponding Householder transform P that would zero that column
            ComputeHouseholderVector(ref u);

            // compute P * A
            // (PA)_rc = A_rc - u_r (u_k A_kc)
            for (int c = a; c < dim; c++) {
                double v = 0.0;
                for (int rp = 0; rp < 3; rp++) {
                    int r = a + rp;
                    v += u[rp] * A[r, c];
                }
                for (int rp = 0; rp < 3; rp++) {
                    int r = a + rp;
                    A[r, c] -= u[rp] * v;
                }
            }
            // compute A * P
            // (AP)_rc = A_rc - (A_rk u_k) u_c
            for (int r = 0; r < dim; r++) {
                double v = 0.0;
                for (int cp = 0; cp < 3; cp++) {
                    int c = a + cp;
                    v += A[r, c] * u[cp];
                }
                for (int cp = 0; cp < 3; cp++) {
                    int c = a + cp;
                    A[r, c] -= v * u[cp];
                }
            }
            // actually, there is a maximum r we needn't go beyond; replace r < dim by this limit
            // i think we go to at most row a+3, i.e. r < a + 4
            // this same logic applies to loop below, too

            // if we are keeping track of transformations, compute Q * P
            if (Q != null) {
                for (int r = 0; r < dim; r++) {
                    double v = 0.0;
                    for (int cp = 0; cp < u.Length; cp++) {
                        int c = a + cp;
                        v += Q[r, c] * u[cp];
                    }
                    for (int cp = 0; cp < u.Length; cp++) {
                        int c = a + cp;
                        Q[r, c] -= v * u[cp];
                    }
                }
            }

            // A is now no longer Hessenberg; it has two extra elements in the first column and one extra
            // element in the second. use a series of Householder transforms to fix this

            // the following logic is effectively the same as calling HessenbergReduce(A, Q)
            // however, we reproduce that logic here so we can take advantage of our knowledge that
            // the matrix is already Hessenberg with the exception of three specific elements

            for (int k = a; k < (n - 2); k++) {

                // compute the householder to zero the elements of this column below the sub-diagonal
                u[0] = A[k + 1, k];
                u[1] = A[k + 2, k];
                u[2] = A[k + 3, k];
                //PrintVector(u);
                double x = ComputeHouseholderVector(ref u);

                // compute P * A
                for (int c = k + 1; c < dim; c++) {
                    double v = 0.0;
                    for (int rp = 0; rp < 3; rp++) {
                        int r = k + 1 + rp;
                        v += u[rp] * A[r, c];
                    }
                    for (int rp = 0; rp < 3; rp++) {
                        int r = k + 1 + rp;
                        A[r, c] -= u[rp] * v;
                    }
                }

                // set zeroed elements
                A[k + 1, k] = x;
                for (int r = k + 2; r < dim; r++) {
                    A[r, k] = 0.0;
                }

                // compute A * P
                for (int r = 0; r < dim; r++) {
                    double v = 0.0;
                    for (int cp = 0; cp < 3; cp++) {
                        int c = k + 1 + cp;
                        v += A[r, c] * u[cp];
                        //if (A[r, c] == 0.0) Debug.WriteLine(String.Format("{0},{1} => 0", r, c));
                    }
                    for (int cp = 0; cp < 3; cp++) {
                        int c = k + 1 + cp;
                        A[r, c] -= v * u[cp];
                    }
                }

                // if we are keeping track of transforms, compute Q * P
                if (Q != null) {
                    for (int r = 0; r < dim; r++) {
                        double v = 0.0;
                        for (int cp = 0; cp < u.Length; cp++) {
                            int c = k + 1 + cp;
                            v += Q[r, c] * u[cp];
                        }
                        for (int cp = 0; cp < u.Length; cp++) {
                            int c = k + 1 + cp;
                            Q[r, c] -= v * u[cp];
                        }
                    }
                }

            }

            // now only one non-Hessenberg element remains, in the lower-right corner
            // this can be zerod with a 2D Hessenberg transform or a 2D rotation

            double[] up = new double[2];
            up[0] = A[m, n - 2];
            up[1] = A[n, n - 2];
            double xp = ComputeHouseholderVector(ref up);

            // compute P * A
            for (int c = n - 1; c < dim; c++) {
                double v = 0.0;
                for (int rp = 0; rp < 2; rp++) {
                    int r = n - 1 + rp;
                    v += up[rp] * A[r, c];
                }
                for (int rp = 0; rp < 2; rp++) {
                    int r = n - 1 + rp;
                    A[r, c] -= up[rp] * v;
                }
            }

            // set zeroed elements
            A[m, n - 2] = xp;
            A[n, n - 2] = 0.0;

            // compute A * P
            for (int r = 0; r < dim; r++) {
                double v = 0.0;
                for (int cp = 0; cp < 2; cp++) {
                    int c = n - 1 + cp;
                    v += A[r, c] * up[cp];
                }
                for (int cp = 0; cp < 2; cp++) {
                    int c = n - 1 + cp;
                    A[r, c] -= v * up[cp];
                }
            }

            // if we are keeping track of transforms, compute Q*P
            if (Q != null) {
                for (int r = 0; r < dim; r++) {
                    double v = 0.0;
                    for (int cp = 0; cp < up.Length; cp++) {
                        int c = n - 1 + cp;
                        v += Q[r, c] * up[cp];
                    }
                    for (int cp = 0; cp < up.Length; cp++) {
                        int c = n - 1 + cp;
                        Q[r, c] -= v * up[cp];
                    }
                }
            }

            // finished Francis step; A is now once again Hessenberg,
            // hopefully with smaller sub-diagonal elements



        }

        // given A in real Schur form (i.e. upper triangular except for 2 X 2 blocks along the diagonal), and Q
        // that got us there, extract eigenvectors of A and apply Q to transform them to the eigenvectors of the
        // original matrix

        private static Complex[,] ExtractEigenvectors (SquareMatrix A, SquareMatrix Q, Complex[] e) {

            // a store which will be used to store each eigenvalue of A
            int dim = A.Dimension;

            // a store for the eigenvectors of the the original matrix,
            // which are th
            Complex[,] X = new Complex[dim, dim];

            // get eigenvectors of A
            for (int k = 0; k < dim; k++) {

                // find the kth eigenvector of the (nearly) upper triangular matrix A
                Complex[] b;
                int imax;
                //Console.WriteLine("Computing eigenvector {0}", k);

                if ((k > 0) && (A[k, k - 1] != 0.0)) {
                    // an extra element to the left
                    b = new Complex[k + 1];
                    b[k] = 1.0;
                    b[k - 1] = A[k - 1, k] / (e[k] - A[k - 1, k - 1]);
                    //Console.WriteLine("b[{0}] = {1}", k, b[k]);
                    //Console.WriteLine("b[{0}] = {1}", k - 1, b[k - 1]);
                    imax = k - 2;
                } else if (((k + 1) < dim) && (A[k + 1, k] != 0.0)) {
                    // an extra element below
                    b = new Complex[k + 2];
                    b[k + 1] = 1.0;
                    b[k] = A[k, k + 1] / (e[k] - A[k, k]);
                    //b[k + 1] = A[k+1,k] / (e[k] - A[k+1,k+1]);
                    //Console.WriteLine("b[{0}] = {1}", k+1, b[k+1]);
                    //Console.WriteLine("b[{0}] = {1}", k, b[k]);
                    imax = k - 1;
                } else {
                    // the pure upper triangular case
                    b = new Complex[k + 1];
                    b[k] = 1.0;
                    imax = k - 1;
                }

                for (int i = imax; i >= 0; i--) {
                    //Console.WriteLine("Component {0}", i);
                    if ((i == 0) || (A[i, i - 1] == 0.0)) {
                        // system is pure tridiagonal, so solution is straightforward
                        Complex s = 0.0;
                        //Console.WriteLine("Start from A[{0},{1}] = {2}", i, k, s);
                        for (int j = i + 1; j < b.Length; j++) {
                            s += A[i, j] * b[j];
                            //Console.WriteLine("Add A[{0},{1}] * B[{2}] = {3} * {4}", i, j, j, A[i, j], b[j]);
                        }
                        Complex t = e[k] - A[i, i];
                        if (s == 0.0) {
                            // deal with trivial Shur form; this arises e.g. for decomposition of unit matrix
                            // without this, we get zero divided by zero, which is NaN
                            b[i] = 0.0;
                        } else {
                            b[i] = s / t;
                        }
                    } else {
                        // system has a sub-diagonal element, so solution is a little more complex
                        Complex s1 = 0.0;
                        Complex s2 = 0.0;
                        for (int j = i + 1; j < b.Length; j++) {
                            s1 += A[i - 1, j] * b[j];
                            s2 += A[i, j] * b[j];
                        }
                        Complex t1 = e[k] - A[i - 1, i - 1];
                        Complex t2 = e[k] - A[i, i];
                        b[i] = (s2 + A[i,i-1] * s1 / t1) / (t2 - A[i,i-1] * A[i-1,i]/ t1);
                    }
                    //Console.WriteLine("b[{0}] = {1}", i, b[i]);
                }

                // transform it to the original basis
                for (int i = 0; i < dim; i++) {
                    Complex x = 0.0;
                    for (int j = 0; j < b.Length; j++) {
                        x += Q[i, j] * b[j];
                    }
                    X[i,k] = x;
                    //Console.Write("{0}  ", x);
                }
                //Console.WriteLine();
                //Console.ReadLine();

            }

            //

            return (X);

        }

        // renormalize eigenvectors so that their 2-norm is unity

        private static void NormalizeEigenvectors (Complex[,] Z) {

            int d = Z.GetLength(0);

            // loop over eigenvectors
            for (int n = 0; n < d; n++) {

                // find the normalization factor
                double x = 0.0;
                for (int i = 0; i < d; i++) {
                    Complex z = Z[i,n];
                    x += z.Re * z.Re + z.Im * z.Im;
                }
                x = Math.Sqrt(x);

                // divide by it
                for (int i = 0; i < d; i++) {
                    Z[i, n] = Z[i, n] / x;
                }

            }

        }

        private static double[] Balance (SquareMatrix A) {

            int d = A.Dimension;

            // a vector to keep track of scale factors
            double[] rhos = new double[d];
            for (int i = 0; i < d; i++) { rhos[i] = 1.0; }

            // iterate over dimensions
            for (int i = 0; i < d; i++) {

                // for each dimension, we will apply a transform Q = diag(1,...,1,rho,1,...,1) where rho is the i'th diagonal element
                // Q A Q^-1 thus multiplies elements of the i'th row by rho and divides elements of the i'th column by rho
                // we choose rho so that (1) the row and column norms are close and (2) rho is an exact power of 2
                double rSum = 0.0;
                double cSum = 0.0;
                for (int j = 0; j < d; j++) {
                    if (i == j) continue;
                    rSum += Math.Abs(A[i, j]);
                    cSum += Math.Abs(A[j, i]);
                }
                if ((rSum == 0.0) || (cSum == 0.0)) continue;
                double rho = ClosestPowerOfTwo(Math.Sqrt(rSum / cSum));
                if (rho != 1.0) {
                    for (int j = 0; j < d; j++) {
                        if (i == j) continue;
                        A[i, j] = A[i, j] * rho;
                        A[j, i] = A[j, i] / rho;
                    }
                    rhos[i] = rhos[i] * rho;
                }
            }

            return (rhos);

        }

        private static double ClosestPowerOfTwo (double x) {
            double y = Math.Round(Math.Log(x) / Math.Log(2.0));
            return (Math.Pow(2.0, y));
        }

        [Conditional("DEBUG")]
        private static void PrintMatrix (IMatrix M) {
            for (int r = 0; r < M.RowCount; r++) {
                for (int c = 0; c < M.ColumnCount; c++) {
                    Debug.Write(String.Format("{0,12:g8} ", M[r, c]));
                }
                Debug.WriteLine(String.Empty);
            }
            Debug.WriteLine("--");
        }
        

        private static void PrintVector (IList<double> v) {
            for (int i = 0; i < v.Count; i++) {
                Debug.Write(String.Format("{0} ", v[i]));
            }
            Debug.WriteLine(String.Empty);
            Debug.WriteLine("-");
        }



        // given a matrix column, returns the properly normalized vector defining the Householder transform that zeros it
        // if v is the returned vector, T = I - v v^T is the transform, applied to A as T A T
        // the computation requires 2 N flops
        private static double ComputeHouseholderVector (ref double[] v) {

            // compute column norm
            double x = 0.0;
            for (int i = 0; i < v.Length; i++) {
                x += v[i] * v[i];
            }
            x = Math.Sqrt(x);

            // create the un-normalized Householder vector and compute the normalization factor
            double norm;
            if (v[0] < 0.0) {
                norm = Math.Sqrt(x * (x - v[0]));
                v[0] -= x;
            } else {
                norm = Math.Sqrt(x * (x + v[0]));
                v[0] += x;
                x = -x;
            }

            // normalize the transform
            if (norm != 0.0) {
                for (int i = 0; i < v.Length; i++) {
                    v[i] = v[i] / norm;
                }
            }

            // return the value of the first component of P v that remains after the transform 
            return (x);
        }

        // Start of the reduction to Hessenberg form: Given the matrix A partitioned
        //   ( a_11 a_12 )
        //   ( a_21 A_22 )
        // where a_11 is a number, a_12 is a row vector, a_21 is a column vector, and A_22 is a matrix
        // and a Householder transform P a_21 = x e1 then PAP is
        //   ( a_11 a_12^T P )
        //   ( x e1 P A_22 P )
        // Continue the reduction to Hessenberg form: Given a part-Hessenberg matrix A partitioned
        //   ( A_11 a_12 A_13 )
        //   (  0   a_22 a_23 )
        //   (  0   a_32 A_33 )
        // where A_11 is a Hessenberg matrix, a_12 is a column vector, A_13 is a matrix, a_22 is a number,
        // a_23 is a row vector, a_32 is a column vector, and A_33 is a matrix
        // and a Householder transform P a_32 = x e1 than PAP is
        //   ( A_11 a_12 A_13 P   )
        //   (  0   a_22 a_23 P   )
        //   (  0   x e1 P A_33 P )
        // Doing Householder transform: Given P = 1 - u u^T,
        //   A P = A - (A u) u^T = A - v u^T where v = A u
        // This is a O(N^2) operation.

        private void QRDecompose () {
			// loop over columns, doing a Householder transform for each
            for (int c = 0; c < dimension; c++) {

                // determine column norm
                double x = 0.0;
                for (int r = c; r < dimension; r++) {
                    x += values[r, c] * values[r, c];
                }
                x = Math.Sqrt(x);

                // choose the first component of Householder vector in a way that minimizes roundoff error
                double s, d;
                if (values[c, c] < 0) {
                    values[c, c] -= x;
                    d = x;
                    s = -x * values[c, c];
                } else {
                    values[c, c] += x;
                    d = -x;
                    s = x * values[c, c];
                }

                // check for singulatity
                if (s == 0.0) throw new DivideByZeroException();

                // update lower right submatrix
                for (int j = c + 1; j < dimension; j++) {
                    double sum = 0.0;
                    for (int k = c; k < dimension; k++) {
                        sum += values[k, c] * values[k, j];
                    }
                    sum = sum / s;
                    for (int i = c; i < dimension; i++) {
                        values[i, j] -= values[i, c] * sum;
                    }
                }

                // renormalize the Householder vector so that leading component is 1;
                // this is always possible if s=0 exception wasn't triggered above
                for (int r = c + 1; r < dimension; r++) {
                    values[r, c] = values[r, c] / values[c, c];
                }

                // insert the diagonal entry where the leading component was stored
                values[c, c] = d;

            }
        }


        // operators

        // equality

        /// <summary>
        /// Determines whether two square matrices are equal.
        /// </summary>
        /// <param name="M1">The first matrix.</param>
        /// <param name="M2">The second matrix.</param>
        /// <returns>True if <paramref name="M1"/> and <paramref name="M2"/> are equal, otherwise false.</returns>
        public static bool operator == (SquareMatrix M1, SquareMatrix M2) {
            return (Matrix.Equals(M1, M2));
        }

        /// <summary>
        /// Determines whether two square matrices are not equal.
        /// </summary>
        /// <param name="M1">The first matrix.</param>
        /// <param name="M2">The second matrix.</param>
        /// <returns>False if <paramref name="M1"/> and <paramref name="M2"/> are equal, otherwise true.</returns>
        public static bool operator != (SquareMatrix M1, SquareMatrix M2) {
            return (!Matrix.Equals(M1, M2));
        }

        /// <summary>
        /// Determines whether the given object is an equal matrix.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if <paramref name="obj"/> is an equal matrix, otherwise false.</returns>
        public override bool Equals (object obj) {
            return (Matrix.Equals(this, obj as IMatrix));
        }

        // matrix arithmetic

        internal static SquareMatrix Add (ISquareMatrix M1, ISquareMatrix M2) {
            if (M1.Dimension != M2.Dimension) throw new DimensionMismatchException();
            SquareMatrix N = new SquareMatrix(M1.Dimension);
            for (int r = 0; r < N.Dimension; r++) {
                for (int c = 0; c < N.Dimension; c++) {
                    N[r, c] = M1[r, c] + M2[r, c];
                }
            }
            return (N);
        }


        /// <summary>
        /// Computes the sum of two square matrices.
        /// </summary>
        /// <param name="M1">The first matrix.</param>
        /// <param name="M2">The second matrix.</param>
        /// <returns>The sum <paramref name="M1"/> + <paramref name="M2"/>.</returns>
        /// <remarks>
        /// <para>Matrix addition is an O(N<sup>2</sup>) process.</para>
        /// </remarks>
        public static SquareMatrix operator + (SquareMatrix M1, SquareMatrix M2) {
            return (Add(M1, M2));
        }

        internal static SquareMatrix Subtract (ISquareMatrix M1, ISquareMatrix M2) {
            if (M1.Dimension != M2.Dimension) throw new DimensionMismatchException();
            SquareMatrix N = new SquareMatrix(M1.Dimension);
            for (int r = 0; r < N.Dimension; r++) {
                for (int c = 0; c < N.Dimension; c++) {
                    N[r, c] = M1[r, c] - M2[r, c];
                }
            }
            return (N);
        }

        /// <summary>
        /// Computes the difference of two square matrices.
        /// </summary>
        /// <param name="M1">The first matrix.</param>
        /// <param name="M2">The second matrix.</param>
        /// <returns>The difference <paramref name="M1"/> - <paramref name="M2"/>.</returns>
        /// <remarks>
        /// <para>Matrix subtraction is an O(N<sup>2</sup>) process.</para>
        /// </remarks>
        public static SquareMatrix operator - (SquareMatrix M1, SquareMatrix M2) {
            return (Subtract(M1, M2));
        }

        internal static SquareMatrix Multiply (ISquareMatrix M1, ISquareMatrix M2) {
            if (M1.Dimension != M2.Dimension) throw new DimensionMismatchException();
            SquareMatrix N = new SquareMatrix(M1.Dimension);
            for (int r = 0; r < N.Dimension; r++) {
                for (int c = 0; c < N.Dimension; c++) {
                    N[r, c] = 0.0;
                    for (int i = 0; i < N.Dimension; i++) {
                        N[r, c] += M1[r, i] * M2[i, c];
                    }
                }
            }
            return (N);
        }

        /// <summary>
        /// Computes the product of two square matrices.
        /// </summary>
        /// <param name="M1">The first matrix.</param>
        /// <param name="M2">The second matrix.</param>
        /// <returns>The product <paramref name="M1"/> * <paramref name="M2"/>.</returns>
        /// <remarks>
        /// <para>Note that matrix multiplication is not commutative, i.e. M1*M2 is generally not the same as M2*M1.</para>
        /// <para>Matrix multiplication is an O(N<sup>3</sup>) process.</para>
        /// </remarks>
        public static SquareMatrix operator * (SquareMatrix M1, SquareMatrix M2) {
            return (Multiply(M1, M2));
        }

        // mixed arithmetic

        internal static SquareMatrix Multiply (double x, SquareMatrix M) {
            SquareMatrix N = new SquareMatrix(M.Dimension);
            for (int r = 0; r < N.Dimension; r++) {
                for (int c = 0; c < N.Dimension; c++) {
                    N[r, c] = x * M[r, c];
                }
            }
            return (N);
        }

        /// <summary>
        /// Computes the product of a real number and a square matrix.
        /// </summary>
        /// <param name="x">The real number.</param>
        /// <param name="M">The matrix.</param>
        /// <returns>The product of <paramref name="x"/> and <paramref name="M"/>.</returns>
        public static SquareMatrix operator * (double x, SquareMatrix M) {
            return (Multiply(x, M));
        }

        /// <summary>
        /// Computes the the quotient of a square matrix and a real number.
        /// </summary>
        /// <param name="M">The matrix.</param>
        /// <param name="x">The real number.</param>
        /// <returns>The quotient <paramref name="M"/>/<paramref name="x"/>.</returns>
        public static SquareMatrix operator / (SquareMatrix M, double x) {
            return (Multiply(1.0 / x, M));
        }

    }


    /// <summary>
    /// Represents the LU decomposition of a square matrix.
    /// </summary>
    /// <remarks><para>An LU decomposition is a representation of a matrix M as the product of a lower-left-triagular matrix L and
    /// and an upper-right-triangular matrix U. To avoid numerical instability, we allow ourselves to decompose a row-wise
    /// permutation of a matrix, so that we have P M = L U, where P is a permutation matrix.</para>
    /// <para>Given an LU decomposition of M, we can solve equations of the form M x = y in O(N<sup>2</sup>) time. We can also compute
    /// det M in O(N) time.</para></remarks>
    /// <seealso cref="SquareMatrix"/>
    public class SquareLUDecomposition : ISquareDecomposition {

        private double[,] lu;
        private int[] perm;
        private int pi;

        /// <summary>
        /// Gets the dimension of the system.
        /// </summary>
        public int Dimension {
            get {
                return (lu.GetLength(0));
            }
        }

        /// <summary>
        /// Computes the determinant of the original matrix.
        /// </summary>
        /// <returns></returns>
        public double Determinant () {
            double lnDet = 0.0;
            int sign = pi;
            for (int i = 0; i < Dimension; i++) {
                if (lu[i, i] < 0.0) {
                    sign = -sign;
                    lnDet += Math.Log(-lu[i, i]);
                } else {
                    lnDet += Math.Log(lu[i, i]);
                }
            }
            return (Math.Exp(lnDet) * sign);
        }


        /// <summary>
        /// Returns the solution vector that, when multiplied by the original matrix, produces the given left-hand side vector.
        /// </summary>
        /// <param name="rhs">The right-hand side vector.</param>
        /// <returns>The left-hand side vector.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="rhs"/> is <c>null</c>.</exception>
        /// <exception cref="DimensionMismatchException">The dimension of <paramref name="rhs"/> is not the same as the
        /// dimension of the matrix.</exception>
        public ColumnVector Solve (IList<double> rhs) {
            if (rhs.Count != Dimension) throw new DimensionMismatchException();

            // solve Ly = x
            double[] y = new double[Dimension];
            for (int i = 0; i < Dimension; i++) {
                y[i] = rhs[perm[i]]; // unscramble rows
                for (int j = 0; j < i; j++) {
                    y[i] -= lu[i, j] * y[j];
                }
            }

            // solve Uz = y
            double[] z = new double[Dimension];
            for (int i = (Dimension - 1); i >= 0; i--) {
                z[i] = y[i];
                for (int j = (Dimension - 1); j > i; j--) {
                    z[i] -= lu[i, j] * z[j];
                }
                z[i] = z[i] / lu[i, i];
            }
            return (new ColumnVector(z));
        }

        /// <summary>
        /// Returns the the inverse of the original matrix.
        /// </summary>
        /// <returns></returns>
        public SquareMatrix Inverse () {
            SquareMatrix MI = new SquareMatrix(Dimension);

            // back-substitute to determine each column
            for (int c = 0; c < Dimension; c++) {
                ColumnVector rhs = new ColumnVector(Dimension);
                for (int r=0; r<Dimension; r++) rhs[r] = 0.0;
                rhs[c] = 1.0;
                ColumnVector lhs = Solve(rhs);
                for (int r=0; r<Dimension; r++) MI[r,c] = lhs[r];
            }

            return (MI);
        }

        ISquareMatrix ISquareDecomposition.Inverse () {
            return (Inverse());
        }

        /// <summary>
        /// Gets the L factor.
        /// </summary>
        /// <returns></returns>
        public SquareMatrix LMatrix () {
            SquareMatrix L = new SquareMatrix(Dimension);
            for (int r = 0; r < Dimension; r++) {
                for (int c = 0; c < r; c++) {
                    L[r, c] = lu[r, c];
                }
                L[r, r] = 1.0;
            }
            return (L);
        }

        /// <summary>
        /// Gets the U factor.
        /// </summary>
        /// <returns></returns>
        public SquareMatrix UMatrix () {
            SquareMatrix U = new SquareMatrix(Dimension);
            for (int r = 0; r < Dimension; r++) {
                for (int c = r; c < Dimension; c++) {
                    U[r, c] = lu[r, c];
                }
            }
            return (U);
        }

        /// <summary>
        /// Gets the permutation matrix.
        /// </summary>
        /// <returns></returns>
        public SquareMatrix PMatrix () {
            SquareMatrix P = new SquareMatrix(Dimension);
            for (int r = 0; r < Dimension; r++) {
                P[r, perm[r]] = 1.0;
            }
            return (P);
        }

        internal SquareLUDecomposition (double[,] lu, int[] perm, int pi) {
            this.lu = lu;
            this.perm = perm;
            this.pi = pi;
        }

    }


    /// <summary>
    /// Represents a collection of complex eigenvalues and eigenvectors.
    /// </summary>
    public class ComplexEigensystem {

        private int dimension;

        private Complex[] eigenvalues;

        private Complex[,] eigenvectors;

        internal ComplexEigensystem (int dimension, Complex[] eigenvalues, Complex[,] eigenvectors) {
            this.dimension = dimension;
            this.eigenvalues = eigenvalues;
            this.eigenvectors = eigenvectors;
        }

        /// <summary>
        /// Gets the dimension of the eigensystem.
        /// </summary>
        public int Dimension {
            get {
                return (dimension);
            }
        }

        /// <summary>
        /// Gets the specified eigenvalue.
        /// </summary>
        /// <param name="n">The number of the eigenvalue.</param>
        /// <returns>The <paramref name="n"/>th eigenvalue.</returns>
        public Complex Eigenvalue (int n) {
            if ((n < 0) || (n >= dimension)) throw new ArgumentOutOfRangeException("n");
            return (eigenvalues[n]);
        }

        /// <summary>
        /// Gets the specified eigenvector.
        /// </summary>
        /// <param name="n">The number of the eigenvector.</param>
        /// <returns>The <paramref name="n"/>th eigenvector.</returns>
        public Vector<Complex> Eigenvector (int n) {
            if ((n < 0) || (n >= dimension)) throw new ArgumentOutOfRangeException("n");
            Vector<Complex> eigenvector = new Vector<Complex>(dimension);
            for (int i = 0; i < dimension; i++) {
                eigenvector[i] = eigenvectors[i, n];
            }
            return (eigenvector);
        }

        /*
        /// <summary>
        /// Gets the matrix transform that diagonalizes the original matrix.
        /// </summary>
        public Complex[,] Transform {
            get {
                return (eigenvectors);
            }
        }
        */

    }

    /*
    public class QRDecomposition {

        private double[,] qr;

        public int Dimension {
            get {
                return (qr.GetLength(0));
            }
        }

        public SquareMatrix QMatrix () {

            // create matrix storage, initialized to the unit matrix
            SquareMatrix Q = new SquareMatrix(Dimension);
            for (int i = 0; i < Dimension; i++) {
                Q[i, i] = 1.0;
            }

            // loop over Householder transforms
            for (int n = Dimension - 1; n >= 0; n--) {

                // determine Householder norm
                double s = 1.0;
                for (int i = n + 1; i < Dimension; i++) {
                    s += qr[i, n] * qr[i,n];
                }
                s = s / 2.0;

                // multiply by previous Q
                for (int j = n; j < Dimension; j++) {
                    double sum = Q[n, j];
                    for (int k = n + 1; k < Dimension; k++) {
                        sum += qr[k, n] * Q[k, j];
                    }
                    sum = sum / s;
                    Q[n, j] -= sum;
                    for (int i = n + 1; i < Dimension; i++) {
                        Q[i, j] -= qr[i, n] * sum;
                    }
                }
            }
            return (Q);

        }

        internal QRDecomposition (double[,] qr) {
            this.qr = qr;
        }

    }
    */

    /*
     * Some test Hessenberg and eigenvalue problems:
     * 
     * (  1  -4   0   3 )
     * ( -1   3   0  -2 )
     * (  3  -7  -2   6 )
     * (  0   4   0  -2 )
     * 
     * has eigenvalues (2,-2,1,-1)
     */

}