namespace QueryPack.RestApi.Mvc.Model
{
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Humanizer;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public static class WebModelExtensions
    {
        public static ValueProviderResult GetValue(IValueProvider valueProvider, string name)
        {
            // try origin name
            var result = valueProvider.GetValue(name);
            if (result != ValueProviderResult.None) return result;

            result = valueProvider.GetValue(name.ToLower());
            if (result != ValueProviderResult.None) return result;

            result = valueProvider.GetValue(name.ToUpper());
            if (result != ValueProviderResult.None) return result;

            // camel case  
            result = valueProvider.GetValue(name.Camelize());
            if (result != ValueProviderResult.None) return result;

            // snake case
            result = valueProvider.GetValue(name.Underscore());
            if (result != ValueProviderResult.None) return result;

            result = valueProvider.GetValue(name.Kebaberize());
            if (result != ValueProviderResult.None) return result;

            return result;
        }

        public static ValueProviderResult GetValue(IValueProvider valueProvider, string pattern, params string[] args)
        {
            // try origin name
            var result = valueProvider.GetValue(string.Format(pattern, args));
            if (result != ValueProviderResult.None) return result;

            result = valueProvider.GetValue(string.Format(pattern.ToLower(), args.Select(e => e.ToLower()).ToArray()));
            if (result != ValueProviderResult.None) return result;

            result = valueProvider.GetValue(string.Format(pattern.ToUpper(), args.Select(e => e.ToUpper()).ToArray()));
            if (result != ValueProviderResult.None) return result;

            // camel case  
            result = valueProvider.GetValue(string.Format(pattern.Camelize(), args.Select(e => e.Camelize()).ToArray()));
            if (result != ValueProviderResult.None) return result;

            // snake case
            result = valueProvider.GetValue(string.Format(pattern.Underscore(), args.Select(e => e.Underscore()).ToArray()));
            if (result != ValueProviderResult.None) return result;

            result = valueProvider.GetValue(string.Format(pattern.Kebaberize(), args.Select(e => e.Kebaberize()).ToArray()));
            if (result != ValueProviderResult.None) return result;

            return result;
        }
        public static bool TryConvert(this Type type, string value, out object result)
        {
            try
            {
                result = TypeDescriptor.GetConverter(type).ConvertFromString(value);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static bool TryConvertEnum(this Type type, string value, out object result)
        {
            var enumTypeMap = new Dictionary<string, object>();
            foreach (var member in type.GetMembers())
            {
                var attr = member.GetCustomAttributes<EnumMemberAttribute>().FirstOrDefault();
                if (attr != null)
                {
                    if (!string.IsNullOrEmpty(attr.Value))
                    {
                        string attrValue = attr.Value;

                        foreach (var enumValue in Enum.GetValues(type))
                        {
                            if (Enum.GetName(type, enumValue) == member.Name)
                            {
                                enumTypeMap[attrValue] = enumValue;
                            }
                        }
                    }
                }
            }

            if (enumTypeMap.TryGetValue(value, out result))
            {
                return true;
            }
            else if (Enum.TryParse(type, value, true, out result))
            {
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
}