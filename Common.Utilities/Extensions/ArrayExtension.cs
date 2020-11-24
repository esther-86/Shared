using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Utilities
{
    public class SameSequenceInfo<T>
    {
        public int IndexFromWhole { get; private set; }
        public IEnumerable<T> SequenceFromWhole { get; private set; }
        public IEnumerable<T> sequenceFromWholeWithPartCount { get; private set; }

        public SameSequenceInfo(int indexFromWhole, IEnumerable<T> sequenceFromWhole, IEnumerable<T> sequenceFromWholeWithPartCount)
        {
            this.IndexFromWhole = indexFromWhole;
            this.SequenceFromWhole = sequenceFromWhole;
            this.sequenceFromWholeWithPartCount = sequenceFromWholeWithPartCount;
        }
    }

    public static class ArrayExtension
    {
        // http://stackoverflow.com/questions/11207526/best-way-to-split-an-array
        public static T[] Slice<T>(this T[] source, int index, int length)
        {
            T[] slice = new T[length];
            Array.Copy(source, index, slice, 0, length);
            return slice;
        }

        // http://stackoverflow.com/questions/9183892/linq-query-list-contains-a-list-with-same-order
        // http://stackoverflow.com/questions/18037625/check-if-one-list-contains-all-items-from-another-list-in-order
        /// <summary>
        /// HL: TODO: Currently not efficient
        /// </summary>
        public static IEnumerable<SameSequenceInfo<T>> GetSameSequences<T>(this IEnumerable<T> whole, IEnumerable<T> part)
        {
            // Generate a list of different sequences from the whole list, where the count is the count of part
            IEnumerable<SameSequenceInfo<T>> sameSequenceInfos = whole
                    .Where((item, index) => index <= whole.Count() - part.Count())
                    .Select((item, index) => new SameSequenceInfo<T>(index, whole.Skip(index), whole.Skip(index).Take(part.Count())));

            foreach (SameSequenceInfo<T> sameSequenceInfo in sameSequenceInfos)
            {
                if (sameSequenceInfo.sequenceFromWholeWithPartCount.SequenceEqual(part))
                    yield return sameSequenceInfo;
            }
        }

        /// <summary>
        /// HL: TODO: Currently not efficient
        /// </summary>
        public static int GetBeginningIndexWhereRestOfSequenceMatchesWith<T>(this IEnumerable<T> whole, IEnumerable<T> listToExtractPart)
        {
            int maxValueCount = Math.Max(listToExtractPart.Count(), whole.Count());
            for (int i = 0; i < Math.Min(listToExtractPart.Count(), whole.Count()); i++)
            {
                IEnumerable<T> extractedPart = listToExtractPart.Where((item, index) => index >= i).Select((item) => item);
                if (whole.GetSameSequences<T>(extractedPart).Count() != 0)
                    return i;
            }
            return int.MinValue;
        }

        public static decimal GetShiftedDataSameness<T1>(this IEnumerable<T1> whole, IEnumerable<T1> part, Func<T1, T1, bool> funcSpecialFirstMatchCheck)
        {
            if (whole == null || part == null)
                return 0;
            
            // int[] whole = { 0, 0, 0, 1, 2, 3, 0, 0, 0 };
            // int[] part = { 0, 0, 0, 0, 0, 1, 2, 3, 0 };
            // Because there's duplicate, we can't just return after getting the sameness percentage starting at the end of the whole's IEnumerable
            //      We want to look at every possible ending and return the maxSameness
            T1 lastPart = part.Last();
            decimal maxSameness = decimal.MinValue;

            // First find, look at duration only
            // lastPart will match with ending index : 1/length
            // lastPart will match with ending index - 1 : 1/length
            // lastPart will match with ending index - 2 : 7/length: maxSameness
            // lastPart will match with ending index - 6 : 1/length: lower than maxSameness now, so can stop looking
            for (int i = whole.Count() - 1; i >= 0; i--)
            {
                T1 firstMatchWhole = whole.ElementAt(i);
                if (funcSpecialFirstMatchCheck.Invoke(lastPart, firstMatchWhole))
                {
                    decimal sameness = whole.GetShiftedDataSameness<T1>(part, funcSpecialFirstMatchCheck, i);
                    if (sameness > maxSameness)
                        maxSameness = sameness;
                    if (sameness < maxSameness)
                        return maxSameness; // Early termination case
                }
            }

            return maxSameness;
        }

        static decimal GetShiftedDataSameness<T1>(this IEnumerable<T1> whole, IEnumerable<T1> part, 
            Func<T1, T1, bool> funcSpecialFirstMatchCheck, int lastIndexInWholeMatchingLastIndexInExtracted)
        {
            if (whole == null || part == null)
                return 0;

            // Start out with the last index in whole being the start of the match
            // int[] whole = { 1, 2, 3, 4 }
            // int[] part = { 4, 1, 2, 3 }
            decimal partsLength = (decimal)part.Count();
            // When this function is called, we know that we have our first match already
            // {3} : {3, 4}: 3/ Part's length: 25%
            decimal maxSameness = 1/ partsLength;

            IEnumerable<T1> extractedPart = null;
            IEnumerable<T1> extractedWhole = null;
            // {2, 3} : {2, 3, 4}: 2/ Part's length: 50%
            // {1, 2, 3} : { 1, 2, 3, 4}: 3/ Part's length: 75%
            // {4, 1, 2, 3: No match: Return 75%
            for (int i = part.Count() - 2; i >= 0; i--) // - 2 because we already looked at the last index match
            {
                extractedPart = part.Where((item, index) => index >= i).Select((item) => item);
                // {1, 2, 3, 4} : lastIndexInWholeMatchingLastIndexInExtracted = 2
                // {2, 3}: Need to look from whole's index 1 now, which is: 2 - (2-1) = 1
                int offsetFromFirstExtractedIndex = (extractedPart.Count() - 1);
                int offsetFromFirstWholeIndex = (lastIndexInWholeMatchingLastIndexInExtracted - offsetFromFirstExtractedIndex);
                // If we are out of values to look at in whole IEnumerable, don't look any further because it will return 1 sameness
                if (offsetFromFirstWholeIndex < 0)
                    return maxSameness;
                extractedWhole = whole.Where((item, index) => index >= offsetFromFirstWholeIndex).Select((item) => item);

                // Check that the second index has the same value
                // And that the first index has the same duration
                // If this is not satisfied, we can do an early stop because we're not going to find a greater match %; else, we can continue to look backward if the sameness level was increased
                if (!funcSpecialFirstMatchCheck.Invoke(extractedPart.First(), extractedWhole.First()))
                    return maxSameness;
                if (extractedPart.Count() > 1 && extractedWhole.Count() > 1 && 
                    !extractedPart.ElementAt(1).Equals(extractedWhole.ElementAt(1)))
                    return maxSameness;

                decimal sameness = (decimal)extractedPart.Count() / partsLength;
                if (sameness > maxSameness)
                    maxSameness = sameness;
                else
                    return maxSameness;
            }

            return maxSameness;
        }
    }
}
