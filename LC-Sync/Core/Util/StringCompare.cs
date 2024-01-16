using System;
using System.Collections.Generic;
using System.Text;

/*
 * Credits: https://github.com/mononobi/string-compare/
 */

namespace System
{
    /// <summary>
    /// Represents operations to compare strings in a relative manner.
    /// </summary>
    public class StringCompare
    {
        private double minSimilarityLong = 85;
        private double minSimilarityShort = 70;

        private double maxToleranceLong = 20;
        private double maxToleranceShort = 10;

        /// <summary>
        /// Initializes an instance of StringCompare with default values.
        /// </summary>
        public StringCompare()
        {
        }

        /// <summary>
        /// Initializes an instance of StringCompare with specified values.
        /// </summary>
        /// <param name="minSimilarityLong">
        /// Minimum acceptable similarity percentage for long strings. Value should be between 0 to 100.</param>
        /// <param name="minSimilarityShort">
        /// Minimum acceptable similarity percentage for short strings. Value should be between 0 to 100.</param>
        /// <param name="maxToleranceLong">
        /// Maximum acceptable tolerance percentage for long strings. Value should be between 0 to 100.</param>
        /// <param name="maxToleranceShort">
        /// Maximum acceptable tolerance percentage for short strings. Value should be between 0 to 100.</param>
        public StringCompare(double minSimilarityLong, double minSimilarityShort,
                             double maxToleranceLong, double maxToleranceShort) : this(minSimilarityLong, maxToleranceLong)
        {
            this.MinSimilarityShort = minSimilarityShort;
            this.MaxToleranceShort = maxToleranceShort;
        }

        /// <summary>
        /// Initializes an instance of StringCompare with specified values.
        /// </summary>
        /// <param name="minSimilarityLong">
        /// Minimum acceptable similarity percentage for long strings. Value should be between 0 to 100.</param>
        /// <param name="maxToleranceLong">
        /// Maximum acceptable tolerance percentage for long strings. Value should be between 0 to 100.</param>
        public StringCompare(double minSimilarityLong, double maxToleranceLong)
        {
            this.MinSimilarityLong = minSimilarityLong;
            this.MaxToleranceLong = maxToleranceLong;
        }

        /// <summary>
        /// Returns a value indicating that two specified strings are equal under specified conditions by this instance.
        /// </summary>
        /// <param name="text1">
        /// First string to be compared.</param>
        /// <param name="text2">
        /// Second string to be compared.</param>
        /// <param name="similarity">
        /// Gets the similarity percentage between two strings calculated by this function.</param>
        /// <param name="tolerance">
        /// Gets the tolerance percentage between two strings calculated by this function.</param>
        /// <returns>
        /// True for equal and false for not equal.</returns>
        public bool IsEqual(string text1, string text2, out double similarity, out double tolerance)
        {
            double minSimilarity = this.MinSimilarityLong;
            double maxTolerance = this.MaxToleranceLong;

            text1 = text1.Trim();
            text2 = text2.Trim();

            text1 = RemoveDuplicateSpace(text1);
            text2 = RemoveDuplicateSpace(text2);

            text1 = text1.ToLower();
            text2 = text2.ToLower();

            double len1 = text1.Length;
            double len2 = text2.Length;

            string temp1 = text1;
            string temp2 = text2;

            double tempLen = 0;
            double similarityPercent = 0;

            int lastIndex = 0;
            double toleranceCount = 0;
            double tolerancePercent = 0;

            if (len1 >= len2 && len1 > 0)
            {
                if (len1 < 8)
                {
                    minSimilarity = this.MinSimilarityShort;
                    maxTolerance = this.MaxToleranceShort;
                }

                foreach (char c in text2.ToCharArray())
                {
                    int ind = -1;
                    ind = temp1.IndexOf(c);

                    if (ind >= 0)
                    {
                        temp1 = temp1.Remove(ind, 1);

                        if (ind < lastIndex)
                        {
                            toleranceCount++;
                        }

                        lastIndex = ind;
                    }
                }

                tempLen = temp1.Length;
                similarityPercent = 100 - ((tempLen / len1) * 100);

                if (len2 > 0)
                {
                    tolerancePercent = (toleranceCount / len2) * 100;
                }
                else
                {
                    tolerancePercent = 0;
                }
            }
            else if (len2 > len1 && len2 > 0)
            {
                if (len2 < 8)
                {
                    minSimilarity = this.MinSimilarityShort;
                    maxTolerance = this.MaxToleranceShort;
                }

                foreach (char c in text1.ToCharArray())
                {
                    int ind = -1;
                    ind = temp2.IndexOf(c);

                    if (ind >= 0)
                    {
                        temp2 = temp2.Remove(ind, 1);

                        if (ind < lastIndex)
                        {
                            toleranceCount++;
                        }

                        lastIndex = ind;
                    }
                }

                tempLen = temp2.Length;
                similarityPercent = 100 - ((tempLen / len2) * 100);

                if (len1 > 0)
                {
                    tolerancePercent = (toleranceCount / len1) * 100;
                }
                else
                {
                    tolerancePercent = 0;
                }
            }
            else if (len1 == len2 && len1 == 0)
            {
                similarityPercent = 100;
                tolerancePercent = 0;
            }

            similarity = similarityPercent;
            tolerance = tolerancePercent;

            if (similarityPercent >= minSimilarity && tolerancePercent <= maxTolerance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a value indicating that two specified strings are equal under specified conditions by this instance.
        /// </summary>
        /// <param name="text1">
        /// First string to be compared.</param>
        /// <param name="text2">
        /// Second string to be compared.</param>
        /// <returns>
        /// True for equal and false for not equal.</returns>
        public bool IsEqual(string text1, string text2)
        {
            double similarity = 0;
            double tolerance = 0;

            return this.IsEqual(text1, text2, out similarity, out tolerance);
        }

        /// <summary>
        /// Gets or sets the minimum acceptable similarity percentage for long strings. Value should be between 0 to 100.
        /// </summary>
        public double MinSimilarityLong
        {
            set
            {
                if (value >= 0 && value <= 100)
                {
                    this.minSimilarityLong = value;
                }
                else
                {
                    throw new Exception("MinSimilarity for long strings should be between 0 to 100.");
                }
            }
            get
            {
                return this.minSimilarityLong;
            }
        }

        /// <summary>
        /// Gets or sets the minimum acceptable similarity percentage for short strings. Value should be between 0 to 100.
        /// </summary>
        public double MinSimilarityShort
        {
            set
            {
                if (value >= 0 && value <= 100)
                {
                    this.minSimilarityShort = value;
                }
                else
                {
                    throw new Exception("MinSimilarity for short strings should be between 0 to 100.");
                }
            }
            get
            {
                return this.minSimilarityShort;
            }
        }

        /// <summary>
        /// Gets or sets the maximum acceptable tolerance percentage for long strings. Value should be between 0 to 100.
        /// </summary>
        public double MaxToleranceLong
        {
            set
            {
                if (value >= 0 && value <= 100)
                {
                    this.maxToleranceLong = value;
                }
                else
                {
                    throw new Exception("MaxTolerance for long strings should be between 0 to 100.");
                }
            }
            get
            {
                return this.maxToleranceLong;
            }
        }

        /// <summary>
        /// Gets or sets the maximum acceptable tolerance percentage for short strings. Value should be between 0 to 100.
        /// </summary>
        public double MaxToleranceShort
        {
            set
            {
                if (value >= 0 && value <= 100)
                {
                    this.maxToleranceShort = value;
                }
                else
                {
                    throw new Exception("MaxTolerance for short strings should be between 0 to 100.");
                }
            }
            get
            {
                return this.maxToleranceShort;
            }
        }

        /// <summary>
        /// Gets the string that every repetitive spaces replaced by only one space.
        /// </summary>
        /// <param name="text">
        /// String that should be replaced repetitive spaces with one space.</param>
        /// <returns>
        /// String that all repetitive spaces replaced by only one space.</returns>
        public static string RemoveDuplicateSpace(string text)
        {
            text = text.Trim();

            int nameLen = (text.Length / 2) + 1;

            while (nameLen >= 0)
            {
                text = text.Replace("  ", " ");

                nameLen--;
            }

            return text;
        }
    }
}