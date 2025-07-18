﻿using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace General.Binders;

/// <summary>
/// Задает ModelBinder для специфичного парсинга значений в запросах
/// </summary>
public class DateOnlyBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var modelValueType = context.Metadata.ModelType;
        if (modelValueType == typeof(DateOnly) || modelValueType == typeof(DateOnly?))
        {
            return new DateOnlyModelBinder();
        }

        return null;
    }
}