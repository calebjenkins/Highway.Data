﻿using System.Linq.Expressions;
using System.Data.Entity;
using Highway.Data;

// ReSharper disable CheckNamespace
namespace Highway.Data.QueryObjects
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Extension to allow for mulitple table includes
    /// </summary>
    public static class IncludeExtension
    {
        /// <summary>
        /// Takes the specified number of records
        /// </summary>
        /// <param name="extend">Query to Extend</param>
        /// <param name="propertiesToInclude">Property of related objects to include</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Query with extension applied</returns>
        public static IQuery<T> Include<T>(this IQuery<T> extend, params string[] propertiesToInclude)
        {
            foreach (var s in propertiesToInclude)
            {
                var generics = new[] { typeof(T) };
                var parameters = new Expression[] { Expression.Constant(s) };
                ((IExtendableQuery)extend).AddMethodExpression("Include", generics, parameters);
            }
            return extend;
        }



    }
}