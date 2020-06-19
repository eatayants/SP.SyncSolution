using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    [DataContract, Serializable]
    public enum messageLevelEnum
    {
        [EnumMemberAttribute]
        none = 0,
        [EnumMemberAttribute]
        information = 1,
        [EnumMemberAttribute]
        warning = 2,
        [EnumMemberAttribute]
        error = 3,
        [EnumMemberAttribute]
        critical = 4
    }

    [DataContract, Serializable]
    public enum logicalJoinEnum
    {
        [EnumMemberAttribute]
        none = 0,
        [EnumMemberAttribute]
        and = 1,
        [EnumMemberAttribute]
        or = 2
    }

    [DataContract, Serializable]
    public enum numericComparators
    {
        [EnumMemberAttribute]
        Less = 0,
        [EnumMemberAttribute]
        LessOrEqual = 1,
        [EnumMemberAttribute]
        Equal = 2,
        [EnumMemberAttribute]
        GreaterOrEqual = 3,
        [EnumMemberAttribute]
        Greater = 4
    }

    [DataContract, Serializable]
    public enum dateComparators
    {
        [EnumMemberAttribute]
        Less = 0,
        [EnumMemberAttribute]
        LessOrEqual = 1,
        [EnumMemberAttribute]
        Equal = 2,
        [EnumMemberAttribute]
        GreaterOrEqual = 3,
        [EnumMemberAttribute]
        Greater = 4,
        [EnumMemberAttribute]
        InRange = 5
    }

    [DataContract, Serializable]
    public enum comparatorEnum
    {
        [EnumMemberAttribute]
        none = 0,       
        [EnumMemberAttribute]
        less = 1,   
        [EnumMemberAttribute]
        lessOrEqual = 2,       
        [EnumMemberAttribute]
        equal = 3,       
        [EnumMemberAttribute]
        notEqual = 4,      
        [EnumMemberAttribute]
        greaterOrEqual = 5,       
        [EnumMemberAttribute]
        greater = 6,
    }
}
