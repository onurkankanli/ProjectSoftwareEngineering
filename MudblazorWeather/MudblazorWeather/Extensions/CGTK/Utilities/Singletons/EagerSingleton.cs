using System;

namespace WeatherClient.Extensions.CGTK.Utilities.Singletons
{
	/// <summary> Eager Singleton for <see cref="class"/>es.</summary>
	/// <remarks> Eager = Only explicit creation. </remarks>
	/// <typeparam name="T"> Type of the Singleton (CRTP). </typeparam>
	public abstract class EagerSingleton<T> where T : EagerSingleton<T>
	{
		#region Properties

		/// <summary> Static reference to the Instance. </summary>
		public static T Instance { get; private set; }

		/// <summary> Whether there is an instance or not. </summary>
		public static bool InstanceExists => (Instance != null);

		#endregion

		#region Structors

		protected EagerSingleton()
		{
			Console.WriteLine($"Singleton {typeof(T).Name} Created");
			if (!InstanceExists) Instance = (T) this;
		}
		
		~EagerSingleton()
		{
			Instance = null;
		}

		#endregion
	}
}