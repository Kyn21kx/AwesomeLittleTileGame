using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Runtime.InteropServices;

public static class Utils {

	/// <summary>
	/// Uses the sqr magnitude of the vectors to calculate their distance
	/// </summary>
	/// <param name="a">Vector from point A</param>
	/// <param name="b">Vector from point B</param>
	/// <param name="threshold">Threshold distance</param>
	/// <returns>Whether the vectors are within the threshold</returns>
	public static bool Reached(Vector3 a, Vector3 b, float threshold) {
		Vector3 dir = b - a;
		return dir.sqrMagnitude <= threshold * threshold;
	}

	/// <summary>
	/// Returns a reference to the variable with the highest absolute value to determine 
	/// which one dominates the vector
	/// </summary>
	public static unsafe float* GetMaxAbsPointer(float* a, float* b) {
		if (Mathf.Abs(*a) > Mathf.Abs(*b)) return a;
		return b;
	}

	public static void Randomize<T>(this T[,] values) {
		// Get the dimensions.
		int num_rows = values.GetUpperBound(0) + 1;
		int num_cols = values.GetUpperBound(1) + 1;
		int num_cells = num_rows * num_cols;

		// Randomize the array.
		System.Random rand = new System.Random();
		for (int i = 0; i < num_cells - 1; i++) {
			//if (i == 1) return;
			// Pick a random cell between i and the end of the array.
			int j = rand.Next(i, num_cells);

			// Convert to row/column indexes.
			int row_i = i / num_cols;
			int col_i = i % num_cols;
			int row_j = j / num_cols;
			int col_j = j % num_cols;

			// Swap cells i and j.
			T temp = values[row_i, col_i];
			values[row_i, col_i] = values[row_j, col_j];
			values[row_j, col_j] = temp;
		}
	}

}
