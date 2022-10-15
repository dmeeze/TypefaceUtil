using System;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
sealed class UnicodeCharCodeMapAttributeAttribute : Attribute
{
    public UnicodeCharCodeMapAttributeAttribute()
    {
    }
}