namespace GitLab.VisualStudio.Shared
{
    using System;
    using System.Collections.Generic;

    public delegate bool EqualityComparison<in T>(T x, T y);

    public static partial class Extensions
    {
        public static bool IsNotNullOrEmpty<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                return false;
            }

            using (var enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
            {
                return true;
            }

            using (var enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, EqualityComparison<TSource> comparer)
        {
            if (first != null && second != null && comparer != null)
            {
                foreach (var f in first)
                {
                    foreach (var s in second)
                    {
                        if (comparer(f, s))
                        {
                            continue;
                        }
                    }

                    yield return f;
                }
            }
        }

        public static IEnumerable<TSource> Each<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            if (source != null && action != null)
            {
                foreach (var item in source)
                {
                    action(item);
                }
            }

            return source;
        }

        public static IEnumerable<TSource> Each<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> action)
        {
            if (source != null && action != null)
            {
                foreach (var item in source)
                {
                    if (!action(item))
                    {
                        break;
                    }
                }
            }

            return source;
        }

        public static IEnumerable<TSource> Each<TSource>(this IEnumerable<TSource> source, Action<int, TSource> action)
        {
            if (source != null && action != null)
            {
                var index = 0;
                foreach (var item in source)
                {
                    action(index, item);
                }
            }

            return source;
        }

        public static IEnumerable<TSource> Each<TSource>(this IEnumerable<TSource> source, Func<int, TSource, bool> action)
        {
            if (source != null && action != null)
            {
                var index = 0;
                foreach (var item in source)
                {
                    if (!action(index, item))
                    {
                        break;
                    }
                }
            }

            return source;
        }

        public static IEnumerable<T[]> Split<T>(this T[] array, int size)
        {
            if (array == null)
            {
                yield break;
            }

            for (var i = 0; i < (float)array.Length / size; i++)
            {
                var actual = Math.Min(array.Length - (i * size), size);
                var dest = new T[actual];

                Array.Copy(array, i * size, dest, 0, actual);

                yield return dest;
            }
        }

        public static IList<T> Shuffle<T>(this IList<T> source)
        {
            if (source == null)
            {
                return null;
            }

            var rnd = new Random();
            for (var i = 1; i < source.Count; i++)
            {
                var position = rnd.Next(i + 1);
                var temp = source[i];
                source[i] = source[position];
                source[position] = temp;
            }

            return source;
        }

        public static T[] Shuffle<T>(this T[] source)
        {
            if (source == null)
            {
                return null;
            }

            var rnd = new Random();
            for (var i = 1; i < source.Length; i++)
            {
                var position = rnd.Next(i + 1);
                var temp = source[i];
                source[i] = source[position];
                source[position] = temp;
            }

            return source;
        }
    }
}