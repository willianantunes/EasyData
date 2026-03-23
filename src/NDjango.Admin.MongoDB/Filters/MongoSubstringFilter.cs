using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NDjango.Admin.Services;
using Newtonsoft.Json;

namespace NDjango.Admin.MongoDB
{
    /// <summary>
    /// Substring search filter for MongoDB LINQ provider.
    /// Only searches string properties to ensure safe server-side translation.
    /// </summary>
    public sealed class MongoSubstringFilter : EasyFilter
    {
        public const string Class = "__substring";

        private string _filterText;

        public MongoSubstringFilter(MetaData model) : base(model) { }

        public override object Apply(MetaEntity entity, bool isLookup, object data)
        {
            if (string.IsNullOrWhiteSpace(_filterText))
                return data;

            return GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                   .Single(m => m.Name == nameof(ApplyGeneric)
                       && m.IsGenericMethodDefinition)
                   .MakeGenericMethod(entity.ClrType)
                   .Invoke(this, new object[] { entity, isLookup, data });
        }

        private IQueryable<T> ApplyGeneric<T>(MetaEntity entity, bool isLookup, object data) where T : class
        {
            var query = (IQueryable<T>)data;
            var text = _filterText.ToLower();

            var stringProperties = GetSearchProperties<T>(entity, isLookup);
            if (!stringProperties.Any())
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression predicateBody = null;

            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var textConstant = Expression.Constant(text, typeof(string));
            var nullConstant = Expression.Constant(null, typeof(string));

            foreach (var prop in stringProperties) {
                var propertyAccess = Expression.Property(parameter, prop);

                // x.Prop != null && x.Prop.ToLower().Contains(text)
                var notNullCheck = Expression.NotEqual(propertyAccess, nullConstant);
                var toLowerCall = Expression.Call(propertyAccess, toLowerMethod);
                var containsCall = Expression.Call(toLowerCall, containsMethod, textConstant);
                var andExpr = Expression.AndAlso(notNullCheck, containsCall);

                predicateBody = predicateBody == null
                    ? andExpr
                    : Expression.OrElse(predicateBody, andExpr);
            }

            if (predicateBody == null)
                return query;

            var lambda = Expression.Lambda<Func<T, bool>>(predicateBody, parameter);
            return query.Where(lambda);
        }

        private IEnumerable<PropertyInfo> GetSearchProperties<T>(MetaEntity entity, bool isLookup)
        {
            var allStringProps = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(string));

            var searchFields = entity?.SearchFields;
            if (searchFields != null && searchFields.Count > 0) {
                return allStringProps.Where(p => searchFields.Contains(p.Name));
            }

            // Fallback: use all visible string properties from entity attributes
            if (entity != null) {
                return allStringProps.Where(prop => {
                    var attr = entity.FindAttribute(a => a.PropInfo == prop);
                    if (attr == null)
                        return false;

                    if (!attr.ShowOnView)
                        return false;

                    if (isLookup && !attr.ShowInLookup && !attr.IsPrimaryKey)
                        return false;

                    return true;
                });
            }

            return allStringProps;
        }

        public override async Task ReadFromJsonAsync(JsonReader reader, CancellationToken ct = default)
        {
            if (!await reader.ReadAsync(ct).ConfigureAwait(false)
               || reader.TokenType != JsonToken.StartObject) {
                throw new BadJsonFormatException(reader.Path);
            }

            while (await reader.ReadAsync(ct).ConfigureAwait(false)) {
                if (reader.TokenType == JsonToken.PropertyName) {
                    var propName = reader.Value.ToString();
                    switch (propName) {
                        case "value":
                            _filterText = await reader.ReadAsStringAsync(ct).ConfigureAwait(false);
                            break;
                        default:
                            await reader.SkipAsync(ct).ConfigureAwait(false);
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject) {
                    break;
                }
            }
        }
    }
}
