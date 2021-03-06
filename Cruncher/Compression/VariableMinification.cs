// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VariableMinification.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Represents the way variables should be minified by a Minifier instance.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruncher.Compression
{
    /// <summary>
    /// Represents the way variables should be minified by a Minifier instance.
    /// </summary>
    public enum VariableMinification
    {
        /// <summary>
        /// No minification will take place.
        /// </summary>
        None = 0,

        /// <summary>
        /// Only variables that are local in scope to a function will be minified.
        /// </summary>
        LocalVariablesOnly = 1,

        /// <summary>
        /// Local scope variables will be minified, as will function parameter names. 
        /// This can have a negative impact on some scripts, so test if you use it! 
        /// </summary>
        LocalVariablesAndFunctionArguments = 2
    }
}
