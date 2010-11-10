﻿/*
Copyright (c) 2010 <a href="http://www.gutgames.com">James Craig</a>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Utilities.Reflection.Emit.Commands;
using Utilities.Reflection.Emit.Enums;
#endregion

namespace Utilities.Reflection.Emit.Interfaces
{
    /// <summary>
    /// Interface for methods
    /// </summary>
    public interface IMethodBuilder
    {
        #region Functions

        /// <summary>
        /// Defines a local variable
        /// </summary>
        /// <param name="Name">Name of the local variable</param>
        /// <param name="LocalType">The Type of the local variable</param>
        /// <returns>The LocalBuilder associated with the variable</returns>
        IVariable CreateLocal(string Name, Type LocalType);

        /// <summary>
        /// Constant value
        /// </summary>
        /// <param name="Value">Value of the constant</param>
        /// <returns>The ConstantBuilder associated with the variable</returns>
        IVariable CreateConstant(object Value);

        /// <summary>
        /// Creates new object
        /// </summary>
        /// <param name="Constructor">Constructor</param>
        /// <param name="Variables">Variables to send to the constructor</param>
        IVariable NewObj(ConstructorInfo Constructor, List<IVariable> Variables);

        /// <summary>
        /// Assigns the value to the left hand side variable
        /// </summary>
        /// <param name="LeftHandSide">Left hand side variable</param>
        /// <param name="Value">Value to store (may be constant or IVariable object)</param>
        void Assign(IVariable LeftHandSide, object Value);

        /// <summary>
        /// Returns a specified value
        /// </summary>
        /// <param name="ReturnValue">Variable to return</param>
        void Return(object ReturnValue);

        /// <summary>
        /// Returns from the method (used if void is the return type)
        /// </summary>
        void Return();

        /// <summary>
        /// Calls a function on an object
        /// </summary>
        /// <param name="ObjectCallingOn">Object calling on</param>
        /// <param name="MethodCalling">Method calling</param>
        /// <param name="Parameters">Parameters sending</param>
        /// <returns>The return value</returns>
        IVariable Call(IVariable ObjectCallingOn, MethodInfo MethodCalling, List<IVariable> Parameters);

        /// <summary>
        /// Defines an if statement
        /// </summary>
        /// <param name="ComparisonType">Comparison type</param>
        /// <param name="LeftHandSide">Left hand side of the if statement</param>
        /// <param name="RightHandSide">Right hand side of the if statement</param>
        /// <returns>The if command</returns>
        If If(Comparison ComparisonType, IVariable LeftHandSide, IVariable RightHandSide);

        /// <summary>
        /// Defines the end of an if statement
        /// </summary>
        /// <param name="IfCommand">If command</param>
        void EndIf(If IfCommand);

        /// <summary>
        /// Defines a while statement
        /// </summary>
        /// <param name="ComparisonType">Comparison type</param>
        /// <param name="LeftHandSide">Left hand side of the while statement</param>
        /// <param name="RightHandSide">Right hand side of the while statement</param>
        /// <returns>The while command</returns>
        While While(Comparison ComparisonType, IVariable LeftHandSide, IVariable RightHandSide);

        /// <summary>
        /// Defines the end of a while statement
        /// </summary>
        /// <param name="WhileCommand">While command</param>
        void EndWhile(While WhileCommand);

        #endregion

        #region Properties

        /// <summary>
        /// Method name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Return type
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        /// Parameters
        /// </summary>
        List<ParameterBuilder> Parameters { get; }

        /// <summary>
        /// Attributes for the method
        /// </summary>
        System.Reflection.MethodAttributes Attributes { get; }

        /// <summary>
        /// IL generator for this method
        /// </summary>
        ILGenerator Generator { get; }

        #endregion
    }
}