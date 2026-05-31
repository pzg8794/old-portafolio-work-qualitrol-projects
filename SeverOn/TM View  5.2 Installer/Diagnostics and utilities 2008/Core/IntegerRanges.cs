using System;
using System.Collections;

namespace Serveron.Utility.Core
{
	/// <summary>
	/// A contiguous range of integers.
	/// </summary>
	/// <remarks>
	/// Does not support removal of an integer from a
	/// range because we don't need it for the moment.
	/// Removal is messy because may have to split a
	/// range into two ranges.  We do, however, support
	/// the messy concept of an empty range since that
	/// would be possible if we supported removal.
	/// </remarks>
	public class IntegerRange: IComparable
	{
		#region State, construction and disposal

		private int _min = 0;
		private int _max = 0;
		private bool _empty = true;

		/// <summary>
		/// Create an IntegerRange holding a single value.
		/// </summary>
		/// <param name="n"></param>
		public IntegerRange(int n)
		{
			_min = n;
			_max = n;
			_empty = false;
		}

		/// <summary>
		/// Create an IntegerRange for the given span
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public IntegerRange(int min, int max)
		{
			_min = min;
			_max = max;
			_empty = false;
		}

		#endregion

		#region Public interface 

		/// <summary>
		/// Get the minimim value of the range,
		/// or InvalidOperationException if the range is empty.
		/// </summary>
		public int Min
		{
			get
			{
				return _min;
			} 
		}

		/// <summary>
		/// Get the maximum value of the range,
		/// or InvalidOperationException if the range is empty.
		/// </summary>
		public int Max
		{
			get
			{
				return _max;
			}
		}

		/// <summary>
		/// Answer true if the range contains n.
		/// </summary>
		/// <param name="n">value to check</param>
		/// <returns>true if the range contains n</returns>
		public bool Contains(int n)
		{
			return (!_empty && n >= _min && n <= _max);
		}

		/// <summary>
		/// Answer true if the range can accept the
		/// argument while remaining contiguous.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public bool CanAccept(int n)
		{
			return _empty || (n == _min - 1) || (n == _max + 1);
		}

		/// <summary>
		/// Add the argument to the range, extending the range.
		/// If adding the value would make the range discontiguous,
		/// an ArgumentException is thrown.
		/// </summary>
		/// <param name="n"></param>
		public void Add(int n)
		{
			if (!CanAccept(n)) throw new ArgumentOutOfRangeException();

			if (_empty)
			{
				_min = n;
				_max = n;
				_empty = false;
				return;
			}

			if (n == _min - 1) _min--;
			else _max++;
		}

		public override string ToString()
		{
			if (_empty) return "(empty)";
			return String.Format("{0}=>{1}", _min, _max);
		}


		#endregion

		#region IComparable Members

		/// <summary>
		/// Compare this IntegerRange with another
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int CompareTo(object obj)
		{
			if (_empty) return -1;
			IntegerRange r = obj as IntegerRange;
			if (r == null) throw new ArgumentException("not a range: " + obj.ToString());
			return this._min - r._min;
		}

		#endregion
	}

	/// <summary>
	/// An ordered set of nonoverlapping integer ranges
	/// </summary>
	public class NonoverlappingIntegerRanges
	{
		#region Private state

		private ArrayList _ranges = new ArrayList();

		#endregion

		#region Public interface

		/// <summary>
		/// Answer true if one of the ranges in the set
		/// contains the argument, else false.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public bool Contains(int n)
		{
			// If we expected to have a lot of ranges here,
			// we could sort and then binary search.  For
			// this application, it's not worth it.

			foreach (IntegerRange r in _ranges)
			{
				if (r.Contains(n)) return true;
				if (r.Max > n) break;
			}

			return false;
		}

		/// <summary>
		/// Add the integer n to the set of nonoverlapping
		/// ranges, creating a new range object if required.
		/// </summary>
		/// <param name="n"></param>
		public void Add(int n)
		{
			foreach (IntegerRange r in _ranges)
			{
				if (r.Contains(n)) return;
				if (r.CanAccept(n))
				{
					r.Add(n);
					CheckOverlapAndSort();
					return;
				}
			}

			IntegerRange newRange = new IntegerRange(n);
			InsertSorted(newRange);
		}

		/// <summary>
		/// Return true if the ranges are as specified in the
		/// argument list, else false.  Used by the unit test
		/// suite to automatically verify the merge code.
		/// </summary>
		/// <param name="expectedRanges">ordered array of (min,
		/// max) pairs specifying the bounds of all the ranges
		/// that should exist in the collection.</param>
		public void Verify(int[] expectedRanges)
		{
			if (expectedRanges.Length != 2 * _ranges.Count)
			{
				throw new ApplicationException("Verify: different lengths.");
			}

			for (int i = 0; i < _ranges.Count; ++i)
			{
				int min = expectedRanges[2 * i];
				int max = expectedRanges[2 * i + 1];
				IntegerRange r = (IntegerRange)_ranges[i];
				if (r.Min != min || r.Max != max)
				{
					throw new ApplicationException(String.Format(
						"Verify: expected {0}=>{1}, found {2}", min, max, r));
				}
			}
		}

		/// <summary>
		/// Render a list of ranges as a string.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			System.Text.StringBuilder result = new System.Text.StringBuilder();
			foreach (IntegerRange r in _ranges)
			{
				if (result.Length > 0) result.Append(", ");
				result.Append(r.ToString());
			}

			return result.ToString();
		}


		#endregion

		#region Implementation

		// Check to see if the most recent addition to
		// one of the ranges caused two ranges to merge.
		private void CheckOverlapAndSort()
		{
			for (int i = 0; i < _ranges.Count - 1; ++i)
			{
				if (((IntegerRange)_ranges[i]).Max + 1 >= ((IntegerRange)_ranges[i + 1]).Min)
				{
					int min = ((IntegerRange)_ranges[i]).Min;
					int max = ((IntegerRange)_ranges[i + 1]).Max;

					_ranges.RemoveAt(i);
					_ranges.RemoveAt(i);	// formerly i + 1 ;-)

					IntegerRange newRange = new IntegerRange(min, max);
					if (i >= _ranges.Count)
					{
						_ranges.Add(newRange);
					}
					else
					{
						_ranges.Insert(i, newRange);
					}
				}
			}
		}

		// Insert the new range in the sorted array list.
		private void InsertSorted(IntegerRange newRange)
		{
			for (int i = 0; i < _ranges.Count; ++i)
			{
				if (newRange.Max < ((IntegerRange)_ranges[i]).Min)
				{
					_ranges.Insert(i, newRange);
					return;
				}
			}

			_ranges.Add(newRange);
		}

		#endregion
	}
}
