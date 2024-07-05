using System.Diagnostics.CodeAnalysis;

namespace PredictorPatchFramework.Extentions
{
    public static class EnumerableExtentions
    {
        public static bool EmptyOrNull<T>([NotNullWhen(false)] this IEnumerable<T>? collection)
        {
            return collection is null || !collection.Any();
        }
    }
}
