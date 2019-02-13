using System;
using System.ComponentModel;
using System.Reflection;

namespace DotNetCore.Microservice.Core
{
    public static class ParameterDefaultValues
    {
        public static object[] GetParameterDefaultValues(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            var parameters = methodInfo.GetParameters();
            var values = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                values[i] = GetParameterDefaultValue(parameters[i]);
            }

            return values;
        }

        private static object GetParameterDefaultValue(ParameterInfo parameterInfo)
        {
            TryGetDeclaredParameterDefaultValue(parameterInfo, out var defaultValue);
            if (defaultValue == null && parameterInfo.ParameterType.IsValueType)
            {
                defaultValue = Activator.CreateInstance(parameterInfo.ParameterType);
            }

            return defaultValue;
        }

        public static bool TryGetDeclaredParameterDefaultValue(ParameterInfo parameterInfo, out object defaultValue)
        {
            if (ParameterDefaultValue.TryGetDefaultValue(parameterInfo, out defaultValue))
            {
                return true;
            }

            var defaultValueAttribute = parameterInfo.GetCustomAttribute<DefaultValueAttribute>(inherit: false);
            if (defaultValueAttribute != null)
            {
                defaultValue = defaultValueAttribute.Value;
                return true;
            }

            return false;
        }
    }

    internal class ParameterDefaultValue
    {
        private static readonly Type _nullable = typeof(Nullable<>);

        public static bool TryGetDefaultValue(ParameterInfo parameter, out object defaultValue)
        {
            bool hasDefaultValue;
            var tryToGetDefaultValue = true;
            defaultValue = null;

            try
            {
                hasDefaultValue = parameter.HasDefaultValue;
            }
            catch (FormatException) when (parameter.ParameterType == typeof(DateTime))
            {
                // Workaround for https://github.com/dotnet/corefx/issues/12338
                // If HasDefaultValue throws FormatException for DateTime
                // we expect it to have default value
                hasDefaultValue = true;
                tryToGetDefaultValue = false;
            }

            if (hasDefaultValue)
            {
                if (tryToGetDefaultValue)
                {
                    defaultValue = parameter.DefaultValue;
                }

                // Workaround for https://github.com/dotnet/corefx/issues/11797
                if (defaultValue == null && parameter.ParameterType.IsValueType)
                {
                    defaultValue = Activator.CreateInstance(parameter.ParameterType);
                }

                // Handle nullable enums
                if (defaultValue != null &&
                    parameter.ParameterType.IsGenericType &&
                    parameter.ParameterType.GetGenericTypeDefinition() == _nullable
                    )
                {
                    var underlyingType = Nullable.GetUnderlyingType(parameter.ParameterType);
                    if (underlyingType != null && underlyingType.IsEnum)
                    {
                        defaultValue = Enum.ToObject(underlyingType, defaultValue);
                    }
                }
            }

            return hasDefaultValue;
        }
    }
}
