using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace PetManagementSystem.Helpers;

/// <summary>
/// Đọc số thập phân dạng "300000.00" (định dạng của &lt;input type="number"&gt;) theo InvariantCulture.
/// Nếu không dùng, máy có ngôn ngữ vi-VN sẽ hiểu dấu chấm là dấu phân cách hàng nghìn và đọc 300000.00 thành 30.000.000.
/// </summary>
public class InvariantDecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var type = context.Metadata.ModelType;
        return type == typeof(decimal) || type == typeof(decimal?)
            ? new InvariantDecimalModelBinder(type)
            : null;
    }
}

public partial class InvariantDecimalModelBinder(Type modelType) : IModelBinder
{
    [GeneratedRegex(@"^[+-]?\d+(\.\d+)?$")]
    private static partial Regex PlainDecimal();

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);
        var metadata = bindingContext.ModelMetadata;
        var raw = valueResult.FirstValue?.Trim();

        if (string.IsNullOrEmpty(raw))
        {
            if (Nullable.GetUnderlyingType(modelType) is not null)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(
                    bindingContext.ModelName,
                    metadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(metadata.GetDisplayName()));
            }

            return Task.CompletedTask;
        }

        // Chuỗi thuần "123" hoặc "123.45" luôn là định dạng invariant; các dạng khác (vd "1.500.000,5") đọc theo ngôn ngữ hiện tại.
        var culture = PlainDecimal().IsMatch(raw) ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture;
        if (decimal.TryParse(raw, NumberStyles.Number, culture, out var value))
        {
            bindingContext.Result = ModelBindingResult.Success(value);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                metadata.ModelBindingMessageProvider.AttemptedValueIsInvalidAccessor(raw, metadata.GetDisplayName()));
        }

        return Task.CompletedTask;
    }
}
