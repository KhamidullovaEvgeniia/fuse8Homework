using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fuse8.BackendInternship.PublicApi.Models;

namespace Fuse8.BackendInternship.PublicApi;

/// <summary>
/// Json-конвектор для получения и записи DateOnly в Json
/// </summary>
public class DateOnlyJsonConvector : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            throw new SerializationException($"Не удалось перевести дату к {nameof(DateOnly)}: пустое значение");
        }

        return DateOnly.ParseExact(
            value,
            ModelBinderConstans.DateFormat,
            CultureInfo.CurrentCulture,
            style: DateTimeStyles.AssumeLocal);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(ModelBinderConstans.DateFormat, CultureInfo.CurrentCulture));
}