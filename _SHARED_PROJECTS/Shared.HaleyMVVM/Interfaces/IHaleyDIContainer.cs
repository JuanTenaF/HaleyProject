﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Haley.Abstractions
{
    public interface IHaleyDIContainer
    {
        bool ignore_if_registered { get; set; }
        bool overwrite_if_registered { get; set; }
        (bool status, Type registered_type, string message) checkIfRegistered(Type input_type);
        /// <summary>
        /// To register a given abstractable type or interface and its implementation
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <typeparam name="TConcrete"></typeparam>
        /// <param name="is_singleton"></param>
        void Register<TContract, TConcrete>(bool is_singleton = false) where TConcrete : class, TContract;  //TConcrete should either implement or inherit from TContract
        void Register<TContract, TConcrete>(TConcrete instance) where TConcrete : class, TContract;  //TImplementation should either implement or inherit from TContract
        void Register<TConcrete>(TConcrete instance = null) where TConcrete : class;  //TImplementation should either implement or inherit from TContract
        T Resolve<T>(bool generate_new_instance = false);
        object Resolve(Type input_type, bool generate_new_instance = false);
    }
}