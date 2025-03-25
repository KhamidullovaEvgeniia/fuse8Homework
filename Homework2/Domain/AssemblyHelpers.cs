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
    public static (string BaseTypeName, int InheritorCount)[] GetTypesWithInheritors()
    {
        // Получаем текущее пространство имен, в котором находится класс AssemblyHelpers
        var currentNamespace = typeof(AssemblyHelpers).Namespace;

        // Получаем все классы из текущей Assembly
        var assemblyClassTypes = Assembly.GetAssembly(typeof(AssemblyHelpers)) !.DefinedTypes.Where(p => p.IsClass && p.Namespace == currentNamespace);

        // ToDo: Добавить реализацию

        // Создаем словарь для хранения базовых классов и количества их наследников
        var baseTypesWithInheritors = new Dictionary<Type, int>();
        foreach (var type in assemblyClassTypes)
        {
            // Проверка: если класс абстрактный, то его не нужно учитывать, так как он не может быть создавать экземпляр
            if (type.IsAbstract)
                continue;

            // Получение базового класса
            var baseClass = GetBaseType(type);

            // Если у типа нет базового класса, пропускаем его
            if (baseClass == null)
                continue;

            // Проверка, что базовый класс находится в том же пространстве имён, что и AssemblyHelpers
            if (baseClass.Namespace != currentNamespace)
                continue;

            // Добавляем базовый класс в словарь, если его там нет, либо увеличиваем счётчик наследников
            if (baseTypesWithInheritors.TryAdd(baseClass, 1) is false)
                baseTypesWithInheritors[baseClass]++;
        }

        // Преобразуем словарь в массив кортежей, содержащих имя базового класса и количество его наследников
        return baseTypesWithInheritors.Select(kvp => (BaseTypeName: kvp.Key.Name, InheritorCount: kvp.Value)).ToArray();
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