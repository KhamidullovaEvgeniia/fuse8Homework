using System.Reflection;

namespace Fuse8.BackendInternship.Domain;

public static class AssemblyHelpers
{
    /// <summary>
    /// Получает информацию о базовых типах классов из namespace "Fuse8.BackendInternship.Domain", у которых есть наследники.
    /// </summary>
    /// <remarks>
    ///	Информация возвращается только по самым базовым классам.
    /// Информация о промежуточных базовых классах не возвращается
    /// </remarks>
    /// <returns>Список типов с количеством наследников</returns>
    private const string Namespace = "Fuse8.BackendInternship.Domain";

    public static (string BaseTypeName, int InheritorCount)[] GetTypesWithInheritors()
    {
        // Получаем все классы из текущей Assembly
        var assemblyClassTypes =
            Assembly.GetAssembly(typeof(AssemblyHelpers)) !.DefinedTypes.Where(p => p.IsClass && p.Namespace == Namespace);

        // ToDo: Добавить реализацию

        var baseTypesWithInheritors = new Dictionary<Type, HashSet<Type>>();
        foreach (var type in assemblyClassTypes)
        {
            if (type.IsAbstract)
                continue;

            var baseClass = GetBaseType(type);
            if (baseClass == null)
                continue;

            if (baseClass.Namespace != Namespace)
                continue;

            if (baseTypesWithInheritors.ContainsKey(baseClass))
                baseTypesWithInheritors[baseClass].Add(type);
            else
                baseTypesWithInheritors[baseClass] = new HashSet<Type> { type };
        }

        return baseTypesWithInheritors.Select(kvp => (BaseTypeName: kvp.Key.Name, InheritorCount: kvp.Value.Count)).ToArray();
    }

    /// <summary>
    /// Получает базовый тип для класса
    /// </summary>
    /// <param name="type">Тип, для которого необходимо получить базовый тип</param>
    /// <returns>
    /// Первый тип в цепочке наследований. Если наследования нет, возвращает null
    /// </returns>
    /// <example>
    /// Класс A, наследуется от B, B наследуется от C
    /// При вызове GetBaseType(typeof(A)) вернется C
    /// При вызове GetBaseType(typeof(B)) вернется C
    /// При вызове GetBaseType(typeof(C)) вернется C
    /// </example>
    private static Type? GetBaseType(Type type)
    {
        var baseType = type;

        while (baseType.BaseType is not null && baseType.BaseType != typeof(object))
        {
            baseType = baseType.BaseType;
        }

        return baseType == type ? null : baseType;
    }
}