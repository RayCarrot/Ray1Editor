using System;
using RayCarrot.UI;

namespace RayCarrot.Ray1Editor
{
    /// <summary>
    /// Base view model for app views
    /// </summary>
    public abstract class AppViewBaseViewModel : BaseViewModel, IDisposable
    {
        /// <summary>
        /// The app view model
        /// </summary>
        public AppViewModel App => R1EServices.App;

        /// <summary>
        /// Initializes the view model. Gets called once when the view is loaded.
        /// </summary>
        public abstract void Initialize();

        public virtual void Dispose()
        {

        }
    }
}