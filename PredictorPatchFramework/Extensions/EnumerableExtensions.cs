using System.Diagnostics.CodeAnalysis;

namespace PredictorPatchFramework.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool EmptyOrNull<T>([NotNullWhen(false)] this IEnumerable<T>? collection)
        {
            return collection is null || !collection.Any();
        }
    }
}
