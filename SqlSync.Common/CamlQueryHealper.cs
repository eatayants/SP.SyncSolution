using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using CamlexNET;
using CamlexNET.Impl.Helpers;
using Microsoft.SharePoint;

namespace Roster.Common
{
    public static class CamlQueryHealper
    {
        public static Expression<Func<SPListItem, bool>> GetOperation_Eq(SPField field, string value)
        {
            if (field.Type == SPFieldType.Boolean) {
                bool _val = value == "1";
                return (x => (bool)x[field.InternalName] == _val);
            } else if (field.Type == SPFieldType.Choice || field.Type == SPFieldType.MultiChoice) {
                return (x => x[field.InternalName] == (DataTypes.Choice)value);
            } else if (field.Type == SPFieldType.Counter) {
                return (x => x[field.InternalName] == (DataTypes.Counter)value);
            } else if (field.Type == SPFieldType.Currency) {
                return (x => x[field.InternalName] == (DataTypes.Currency)value);
            } else if (field.Type == SPFieldType.DateTime) {
                DateTime dt;
                DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                return (x => (DateTime)x[field.InternalName] == dt);
            } else if (field.Type == SPFieldType.Integer) {
                return (x => x[field.InternalName] == (DataTypes.Integer)value);
            } else if (field.Type == SPFieldType.Lookup) {
                return (x => x[field.InternalName] == (DataTypes.LookupValue)value);
            } else if (field.Type == SPFieldType.Number) {
                return (x => x[field.InternalName] == (DataTypes.Number)value);
            } else if (field.Type == SPFieldType.User) {
                return (x => x[field.InternalName] == (DataTypes.User)value);
            } else {
                return (x => (string)x[field.InternalName] == value);
            }
        }

        public static Expression<Func<SPListItem, bool>> GetOperation_Neq(SPField field, string value)
        {
            if (field.Type == SPFieldType.Boolean) {
                bool _val = value == "1";
                return (x => (bool)x[field.InternalName] != _val);
            } else if (field.Type == SPFieldType.Choice || field.Type == SPFieldType.MultiChoice) {
                return (x => x[field.InternalName] != (DataTypes.Choice)value);
            } else if (field.Type == SPFieldType.Counter) {
                return (x => x[field.InternalName] != (DataTypes.Counter)value);
            } else if (field.Type == SPFieldType.Currency) {
                return (x => x[field.InternalName] != (DataTypes.Currency)value);
            } else if (field.Type == SPFieldType.DateTime) {
                DateTime dt;
                DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                return (x => (DateTime)x[field.InternalName] != dt);
            } else if (field.Type == SPFieldType.Integer) {
                return (x => x[field.InternalName] != (DataTypes.Integer)value);
            } else if (field.Type == SPFieldType.Lookup) {
                return (x => x[field.InternalName] != (DataTypes.LookupValue)value);
            } else if (field.Type == SPFieldType.Number) {
                return (x => x[field.InternalName] != (DataTypes.Number)value);
            } else if (field.Type == SPFieldType.User) {
                return (x => x[field.InternalName] != (DataTypes.User)value);
            } else {
                return (x => (string)x[field.InternalName] != value);
            }
        }

        public static Expression<Func<SPListItem, bool>> GetOperation_In(SPField field, string[] values)
        {
            if (field.Type == SPFieldType.Boolean) {
                bool[] _values = values.Select(x => x == "1").ToArray();
                return (x => _values.Contains((bool)x[field.InternalName]));
            } else if (field.Type == SPFieldType.Counter || field.Type == SPFieldType.Integer) {
                int[] _ints = values.Select(x => Int32.Parse(x)).ToArray();
                return (x => _ints.Contains((int)x[field.InternalName]));
            } else if (field.Type == SPFieldType.DateTime) {
                DateTime[] dts = values.Select(x => DateTime.ParseExact(x, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None)).ToArray();
                return (x => dts.Contains((DateTime)x[field.InternalName]));
            } else if (field.Type == SPFieldType.User || field.Type == SPFieldType.Number || field.Type == SPFieldType.Lookup ||
                       field.Type == SPFieldType.Currency || field.Type == SPFieldType.Choice || field.Type == SPFieldType.MultiChoice) {
                return GetOperation_OrEq(field, values); // In operator does not used to avoid manual-conversion errors
            } else {
                return (x => values.Contains((string)x[field.InternalName]));
            }
        }

        public static Expression<Func<SPListItem, bool>> GetOperation_OrEqLookupId(SPField field, string[] values)
        {
            var orConditions = new List<Expression<Func<SPListItem, bool>>>();

            foreach (string _val in values)
                orConditions.Add(GetOperation_EqLookupId(field, _val));

            return ExpressionsHelper.CombineOr(orConditions);
        }
        public static Expression<Func<SPListItem, bool>> GetOperation_AndEqLookupId(SPField field, string[] values)
        {
            var andConditions = new List<Expression<Func<SPListItem, bool>>>();

            foreach (string _val in values)
                andConditions.Add(GetOperation_EqLookupId(field, _val));

            return ExpressionsHelper.CombineAnd(andConditions);
        }
        public static Expression<Func<SPListItem, bool>> GetOperation_AndNeqLookupId(SPField field, string[] values)
        {
            var andConditions = new List<Expression<Func<SPListItem, bool>>>();

            foreach (string _val in values)
                andConditions.Add(GetOperation_NeqLookupId(field, _val));

            return ExpressionsHelper.CombineAnd(andConditions);
        }

        public static Expression<Func<SPListItem, bool>> GetOperation_EqLookupId(SPField field, string value)
        {
            return (x => x[field.InternalName] == (DataTypes.LookupId)value);
        }
        public static Expression<Func<SPListItem, bool>> GetOperation_NeqLookupId(SPField field, string value)
        {
            return (x => x[field.InternalName] != (DataTypes.LookupId)value);
        }

        public static Expression<Func<SPListItem, bool>> GetOperation_AndEq(SPField field, string[] values)
        {
            var andConditions = new List<Expression<Func<SPListItem, bool>>>();

            foreach (string _val in values)
                andConditions.Add(GetOperation_Eq(field, _val));

            return ExpressionsHelper.CombineAnd(andConditions);
        }

        public static Expression<Func<SPListItem, bool>> GetOperation_AndNeq(SPField field, string[] values)
        {
            var andNotConditions = new List<Expression<Func<SPListItem, bool>>>();

            foreach (string _val in values)
                andNotConditions.Add(GetOperation_Neq(field, _val));

            return ExpressionsHelper.CombineAnd(andNotConditions);
        }

        public static Expression<Func<SPListItem, bool>> GetOperation_OrEq(SPField field, string[] values)
        {
            var orConditions = new List<Expression<Func<SPListItem, bool>>>();

            foreach (string _val in values)
                orConditions.Add(GetOperation_Eq(field, _val));

            return ExpressionsHelper.CombineOr(orConditions);
        }
    }
}
