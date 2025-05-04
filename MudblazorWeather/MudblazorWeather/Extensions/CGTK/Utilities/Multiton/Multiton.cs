using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace WeatherClient.Extensions.CGTK.Utilities.Multiton
{
	/// <summary> Multiton for <see cref="class"/>es, (HashSet Collection Type)</summary>
	/// <typeparam name="T"> Type of the Multiton (CRTP). </typeparam>
	public abstract class Multiton<T>
		where T : Multiton<T>
	{
		#region Fields & Properties
        
		public static HashSet<T> Instances { get; private set; } = new HashSet<T>();

		public static bool HasInstances => (Instances.Count > 0);

		#endregion
        
		#region Structors
        
		protected Multiton() => Register();
		~Multiton() => Unregister();

		#endregion
        
		#region Methods

		public static void ClearAll() => Instances.Clear();

		internal void Register()
		{
			//if(Instances.Contains(item: this as T)) return;
            
			//Debug.Log(message: "Registered", context: this);
			Instances.Add(item: this as T);
		}
        
		internal void Unregister()
		{
			if(!Instances.Contains(item: this as T)) return;

			//Debug.Log(message: "<i>Un</i>registered", context: this);
			Instances.Remove(item: this as T);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Foreach(in Action action)
		{
			for (int __index = 0; __index < Instances.Count; __index++)
				action?.Invoke();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Foreach(in Action<T> action)
		{
			foreach (T __instance in Instances)
				action?.Invoke(obj: __instance);
		}
		
		#endregion

	}
}