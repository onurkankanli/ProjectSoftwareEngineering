using System;

namespace WeatherClient.Extensions.CGTK.Utilities.Singletons
{
	/// <summary> Eager Singleton for <see cref="class"/>es.</summary>
	/// <remarks> Ensured = Uses explicitly created Instance if exists, else creates one. </remarks>
	/// <typeparam name="T"> Type of the Singleton (CRTP). </typeparam>
	public abstract class EnsuredSingleton<T> where T : EnsuredSingleton<T>, new()
	{
		#region Properties

		private static T _internalInstance;

		/// <summary> Static reference to the Instance. </summary>
		public static T Instance => InstanceExists ? _internalInstance : new T();

		/// <summary> Whether there is an instance or not. </summary>
		public static bool InstanceExists => (_internalInstance != null);

		#endregion

		#region Structors

		protected EnsuredSingleton()
		{
			if (InstanceExists) return;

			_internalInstance = (T)this;

			Console.WriteLine($"Singleton {typeof(T).Name} Created");
		}

		#endregion
	}
}