using System;
using System.Collections.Generic;
using System.Text;

namespace Algorithms
{
    // C# implementation of above approach  
    using System;

    public class JaroWinkler
    {

        // Function to calculate the  
        // Jaro Similarity of two strings  
        public static double jaro_distance(string s1, string s2)
        {
            // If the strings are equal  
            if (s1 == s2)
                return 1.0;

            // Length of two strings  
            int len1 = s1.Length,
                len2 = s2.Length;

            if (len1 == 0 || len2 == 0)
                return 0.0;

            // Maximum distance upto which matching  
            // is allowed  
            int max_dist = (int)Math.Floor((double)
                            Math.Max(len1, len2) / 2) - 1;

            // Count of matches  
            int match = 0;

            // Hash for matches  
            int[] hash_s1 = new int[s1.Length];
            int[] hash_s2 = new int[s2.Length];

            // Traverse through the first string  
            for (int i = 0; i < len1; i++)
            {

                // Check if there is any matches  
                for (int j = Math.Max(0, i - max_dist);
                    j < Math.Min(len2, i + max_dist + 1); j++)

                    // If there is a match  
                    if (s1[i] == s2[j] &&
                        hash_s2[j] == 0)
                    {
                        hash_s1[i] = 1;
                        hash_s2[j] = 1;
                        match++;
                        break;
                    }
            }

            // If there is no match  
            if (match == 0)
                return 0.0;

            // Number of transpositions  
            double t = 0;

            int point = 0;

            // Count number of occurances  
            // where two characters match but  
            // there is a third matched character  
            // in between the indices  
            for (int i = 0; i < len1; i++)
                if (hash_s1[i] == 1)
                {

                    // Find the next matched character  
                    // in second string  
                    while (hash_s2[point] == 0)
                        point++;

                    if (s1[i] != s2[point++])
                        t++;
                }
            t /= 2;

            // Return the Jaro Similarity  
            return (((double)match) / ((double)len1)
                    + ((double)match) / ((double)len2)
                    + ((double)match - t) / ((double)match))
                / 3.0;
        }

        // Jaro Winkler Similarity  
        static double jaro_Winkler(string s1, string s2)
        {
            double jaro_dist = jaro_distance(s1, s2);

            // If the jaro Similarity is above a threshold  
            if (jaro_dist > 0.7)
            {

                // Find the length of common prefix  
                int prefix = 0;

                for (int i = 0; i < Math.Min(s1.Length,
                                            s2.Length); i++)
                {

                    // If the characters match  
                    if (s1[i] == s2[i])
                        prefix++;

                    // Else break  
                    else
                        break;
                }

                // Maximum of 4 characters are allowed in prefix  
                prefix = Math.Min(4, prefix);

                // Calculate jaro winkler Similarity  
                jaro_dist += 0.1 * prefix * (1 - jaro_dist);
            }
            return jaro_dist;
        }

        // Driver code  
        public static void Main()
        {
            string s1 = "Goethe@en", s2 = "goethe";

            // Print Jaro-Winkler Similarity of two strings  
            Console.WriteLine("Jaro-Winkler Similarity =" +
                                jaro_Winkler(s1.ToLower(), s2.ToLower()));
        }
    }

    // This code is contributed by AnkitRai01 
}
