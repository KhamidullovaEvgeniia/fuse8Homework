using System.Globalization;
using Framework.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Framework.Binders;

/// <summary>
/// Model Binder для парсинга даты
/// </summary>
public class DateOnlyModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        var valueStr = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(valueStr))
        {
            DateOnly? defaultValue = bindingContext.ModelType == typeof(DateOnly) ? null : default(DateOnly);
            SetValue(defaultValue);
            return Task.CompletedTask;
        }

        var parsedDataTime = DateOnly.ParseExact(valueStr, ModelBinderConstans.DateFormat, CultureInfo.CurrentCulture);
        SetValue(parsedDataTime);
        return Task.CompletedTask;

        void SetValue(DateOnly? value)
        {
            bindingContext.Result = ModelBindingResult.Success(value);
        }
    }
}