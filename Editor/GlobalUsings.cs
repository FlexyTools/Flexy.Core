global using System;
global using System.Collections.Generic;
global using System.Globalization;
global using System.Linq;
global using System.Reflection;

global using Cysharp.Threading.Tasks;
global using UnityEngine;
global using UnityEditor;
global using UnityEditor.UIElements;
global using UnityEngine.UIElements;
global using UnityEngine.SceneManagement;

global using Flexy.Core.Extensions;
global using Flexy.Core.GameContexts;
global using Flexy.Core.Binding;
global using Flexy.Core.Actions;

global using Object				= UnityEngine.Object;
global using Binder				= Flexy.Core.Binding.Binder;
global using StaticAttribute	= UnityEngine.RuntimeInitializeOnLoadMethodAttribute;

#if FLEXY_LOG
global using Debug = Flexy.Log.Debug;
#endif

global using static Flexy.Core.RuntimeInit;