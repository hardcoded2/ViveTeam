namespace Slash.Unity.DataBind.Foundation.Providers.Getters
{
    using Slash.Unity.DataBind.Core.Presentation;
    using UnityEngine;

    /// <summary>
    ///     Base class to get a data value from a bound context.
    /// </summary>
    /// <typeparam name="TContext">Type of bound context.</typeparam>
    /// <typeparam name="TData">Type of data to get from bound context.</typeparam>
    public abstract class ContextDataProvider<TContext, TData> : DataProvider
    {
        /// <summary>
        ///     Data binding for context to get data from.
        /// </summary>
        [Tooltip("Data binding for context to get data from")]
        public DataBinding Context;

        /// <summary>
        ///     Current data value.
        /// </summary>
        private TData data;

        /// <inheritdoc />
        public override object Value
        {
            get
            {
                return this.data;
            }
        }

        /// <inheritdoc />
        protected override void UpdateValue()
        {
            var context = this.Context.GetValue<TContext>();
            var newData = context != null ? this.GetDataValue(context) : default(TData);
            if (!Equals(newData, this.data))
            {
                this.data = newData;
                this.OnValueChanged(this.data);
            }
        }

        /// <summary>
        ///     Unity callback.
        /// </summary>
        protected virtual void Awake()
        {
            this.AddBinding(this.Context);
        }

        /// <summary>
        ///     Unity callback.
        /// </summary>
        protected virtual void OnDestroy()
        {
            this.RemoveBinding(this.Context);
        }

        /// <summary>
        ///     Get the data value from the specified context.
        ///     Context is guaranteed to be not null.
        /// </summary>
        /// <param name="context">Context to get data value from.</param>
        /// <returns>Data value from specified context.</returns>
        protected abstract TData GetDataValue(TContext context);
    }
}