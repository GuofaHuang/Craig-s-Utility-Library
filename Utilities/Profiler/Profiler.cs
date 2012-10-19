﻿/*
Copyright (c) 2012 <a href="http://www.gutgames.com">James Craig</a>

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
using System.Web;
using Utilities.DataTypes.ExtensionMethods;
using System.Text;
using System.Linq;
using Utilities.Caching.ExtensionMethods;
using System.Diagnostics;
using Utilities.Environment.ExtensionMethods;
using System.Reflection;
#endregion

namespace Utilities.Profiler
{
    /// <summary>
    /// Object class used to profile a function.
    /// Create at the beginning of a function in a using statement and it will automatically record the time.
    /// Note that this isn't exact and is based on when the object is destroyed
    /// </summary>
    public class Profiler : IDisposable
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        protected Profiler()
        {
            Setup("");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="FunctionName">Function/identifier</param>
        public Profiler(string FunctionName)
        {
            Setup(FunctionName);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Profiles">Profiles to copy data from</param>
        protected Profiler(IEnumerable<Profiler> Profiles)
        {
            Children = new List<Profiler>();
            Times = new List<long>();
            StopWatch = new StopWatch();
            Running = false;
            foreach (Profiler Profile in Profiles)
            {
                this.Level = Profile.Level;
                this.Function = Profile.Function;
                this.Times.Add(Profile.Times);
                this.Children.Add(Profile.Children);
                this.CalledFrom = Profile.CalledFrom;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Total time that the profiler has taken (in milliseconds)
        /// </summary>
        public virtual List<long> Times { get; protected set; }

        /// <summary>
        /// Children profiler items
        /// </summary>
        public virtual List<Profiler> Children { get; protected set; }

        /// <summary>
        /// Parent profiler item
        /// </summary>
        protected virtual Profiler Parent { get; set; }

        /// <summary>
        /// Function name
        /// </summary>
        public virtual string Function { get; protected set; }

        /// <summary>
        /// Determines if it is running
        /// </summary>
        protected virtual bool Running { get; set; }

        /// <summary>
        /// Level of the profiler
        /// </summary>
        protected virtual int Level { get; set; }

        /// <summary>
        /// Where the profiler was started at
        /// </summary>
        protected virtual string CalledFrom { get; set; }

        /// <summary>
        /// Stop watch
        /// </summary>
        protected virtual StopWatch StopWatch { get; set; }

        /// <summary>
        /// Contains the root profiler
        /// </summary>
        public static Profiler Root
        {
            get
            {
                Profiler ReturnValue = "Root_Profiler".GetFromCache<Profiler>(CachingExtensions.CacheType.Item | CachingExtensions.CacheType.Internal);
                if (ReturnValue == null)
                {
                    ReturnValue = new Profiler("Start");
                    Root = ReturnValue;
                }
                return ReturnValue;
            }
            protected set
            {
                value.Cache("Root_Profiler", CachingExtensions.CacheType.Item | CachingExtensions.CacheType.Internal);
            }
        }

        /// <summary>
        /// Contains the current profiler
        /// </summary>
        public static Profiler Current
        {
            get
            {
                Profiler ReturnValue = "Current_Profiler".GetFromCache<Profiler>(CachingExtensions.CacheType.Item | CachingExtensions.CacheType.Internal);
                if (ReturnValue == null)
                {
                    ReturnValue = "Root_Profiler".GetFromCache<Profiler>(CachingExtensions.CacheType.Item | CachingExtensions.CacheType.Internal);
                    Current = ReturnValue;
                }
                return ReturnValue;
            }
            protected set
            {
                value.Cache("Current_Profiler", CachingExtensions.CacheType.Item | CachingExtensions.CacheType.Internal);
            }
        }

        #endregion

        #region Functions

        #region Dispose

        /// <summary>
        /// Disposes of the object
        /// </summary>
        public virtual void Dispose()
        {
            Stop();
        }

        #endregion

        #region Stop

        /// <summary>
        /// Stops the timer and registers the information
        /// </summary>
        public virtual void Stop()
        {
            if (Running)
            {
                Running = false;
                StopWatch.Stop();
                Times.Add(StopWatch.ElapsedTime);
                Current = Parent;
            }
        }

        #endregion

        #region Start

        /// <summary>
        /// Starts the timer
        /// </summary>
        public virtual void Start()
        {
            if (Running)
                Stop();
            Running = true;
            StopWatch.Start();
            Current = this;
        }

        #endregion

        #region Setup

        /// <summary>
        /// Sets up the profiler
        /// </summary>
        /// <param name="Function">Function/Identification name</param>
        protected virtual void Setup(string Function = "")
        {
            this.Parent = Current;
            if (Parent != null)
                Parent.Children.Add(this);
            this.Function = Function;
            Children = new List<Profiler>();
            Times = new List<long>();
            StopWatch = new StopWatch();
            this.Level = Parent == null ? 0 : Parent.Level + 1;
            this.CalledFrom = new StackTrace().GetMethods(this.GetType().Assembly).ToString<MethodBase>(x => x.DeclaringType.Name + " > " + x.Name, "<br />");
            Running = false;
            Start();
        }

        #endregion

        #region StartProfiling

        /// <summary>
        /// Starts profiling
        /// </summary>
        /// <returns>The root profiler</returns>
        public static Profiler StartProfiling()
        {
            return Root;
        }

        #endregion

        #region StopProfiling

        /// <summary>
        /// Stops profiling
        /// </summary>
        /// <returns>The root profiler</returns>
        public static Profiler StopProfiling()
        {
            return Root;
        }

        #endregion

        #region CompileData

        /// <summary>
        /// Compiles data, combining instances where appropriate
        /// </summary>
        protected virtual void CompileData()
        {
            bool Continue = true;
            while (Continue)
            {
                Continue = false;
                for (int x = 0; x < Children.Count; ++x)
                {
                    IEnumerable<Profiler> Combinables = Children.Where(y => y == Children[x]).ToList();
                    if (Combinables.Count() > 1)
                    {
                        Continue = true;
                        Profiler Temp = new Profiler(Combinables);
                        Combinables.ForEach(y => Children.Remove(y));
                        Children.Add(Temp);
                        break;
                    }
                }
            }
        }

        #endregion

        #region ToString

        /// <summary>
        /// Outputs the information to a table
        /// </summary>
        /// <returns>an html string containing the information</returns>
        public override string ToString()
        {
            CompileData();
            StringBuilder Builder = new StringBuilder();
            Level.Times(x => { Builder.Append("\t"); });
            Builder.AppendLineFormat("{0} ({1} ms)", Function, Times.Sum());
            foreach (Profiler Child in Children)
            {
                Builder.AppendLineFormat(Child.ToString());
            }
            return Builder.ToString();
        }

        #endregion

        #region Equals

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            Profiler Temp = obj as Profiler;
            if (Temp == null)
                return false;
            return Temp == this;
        }

        /// <summary>
        /// Compares the profilers and determines if they are equal
        /// </summary>
        /// <param name="First">First</param>
        /// <param name="Second">Second</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public static bool operator ==(Profiler First, Profiler Second)
        {
            if ((object)First == null && (object)Second == null)
                return true;
            if ((object)First == null)
                return false;
            if ((object)Second == null)
                return false;
            return First.Function == Second.Function;
        }


        /// <summary>
        /// Compares the profilers and determines if they are not equal
        /// </summary>
        /// <param name="First">First</param>
        /// <param name="Second">Second</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public static bool operator !=(Profiler First, Profiler Second)
        {
            return !(First == Second);
        }

        #endregion

        #region GetHashCode

        /// <summary>
        /// Gets the hash code for the profiler
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return Function.GetHashCode();
        }

        #endregion

        #region ToHTML

        /// <summary>
        /// Outputs the profiler information as an HTML table
        /// </summary>
        /// <returns>Table containing profiler information</returns>
        public virtual string ToHTML()
        {
            CompileData();
            StringBuilder Builder = new StringBuilder();
            if (Level == 0)
                Builder.Append("<table><tr><th>Called From</th><th>Function Name</th><th>Total Time</th><th>Max Time</th><th>Min Time</th><th>Average Time</th><th>Times Called</th></tr>");
            Builder.AppendFormat("<tr><td>{0}</td><td>", CalledFrom);
            if (Level == 0)
                Builder.AppendFormat("{0}</td><td>{1}ms</td><td>{2}ms</td><td>{3}ms</td><td>{4}ms</td><td>{5}</td></tr>", Function, 0, 0, 0, 0, Times.Count);
            else
                Builder.AppendFormat("{0}</td><td>{1}ms</td><td>{2}ms</td><td>{3}ms</td><td>{4}ms</td><td>{5}</td></tr>", Function, Times.Sum(), Times.Max(), Times.Min(), string.Format("{0:0.##}", Times.Average()), Times.Count);
            foreach (Profiler Child in Children)
            {
                Builder.AppendLineFormat(Child.ToHTML());
            }
            if (Level == 0)
                Builder.Append("</table>");
            return Builder.ToString();
        }

        #endregion

        #endregion
    }
}