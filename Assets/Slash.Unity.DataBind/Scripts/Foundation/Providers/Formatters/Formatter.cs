namespace Slash.Unity.DataBind.Foundation.Providers.Formatters
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Slash.Unity.DataBind.Core.Presentation;

    using UnityEngine;

    /// <summary>
    ///   Base class for a formatter that adjusts a bound target by the bound data.
    /// </summary>
    /// <typeparam name="TTarget">Type of target to format.</typeparam>
    /// <typeparam name="TData">Type of data to use for formatting.</typeparam>
    public abstract class Formatter<TTarget, TData> : DataProvider
    {
        /// <summary>
        ///   Data to bind to.
        /// </summary>
        public DataBinding Data;

        /// <summary>
        ///   Target to format.
        /// </summary>
        public DataBinding Target;

        private TData dataValue;

        private TTarget targetValue;

        /// <inheritdoc />
        public override object Value
        {
            get
            {
                return this.targetValue;
            }
        }

        /// <summary>
        ///   Unity callback.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMemberHiearchy.Global")]
        protected virtual void Awake()
        {
            this.AddBinding(this.Target);
            this.AddBinding(this.Data);
        }

        /// <summary>
        ///   Unity callback.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMemberHiearchy.Global")]
        protected virtual void OnDestroy()
        {
            this.RemoveBinding(this.Target);
            this.RemoveBinding(this.Data);
        }

        /// <inheritdoc />
        public override void Disable()
        {
            base.Disable();

            this.Target.ValueChanged -= this.OnTargetValueChanged;
            this.Data.ValueChanged -= this.OnDataValueChanged;
        }

        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();

            this.Target.ValueChanged += this.OnTargetValueChanged;
            if (this.Target.IsInitialized)
            {
                this.OnTargetValueChanged(this.Target.Value);
            }

            this.Data.ValueChanged += this.OnDataValueChanged;
            if (this.Data.IsInitialized)
            {
                this.OnDataValueChanged(this.Data.Value);
            }
        }

        /// <summary>
        ///   Called when target has to be updated with a new value.
        /// </summary>
        /// <param name="target">Target to update.</param>
        /// <param name="value">New data value.</param>
        /// <returns>True if target changed; otherwise, false.</returns>
        protected abstract bool UpdateTarget(TTarget target, TData value);

        /// <inheritdoc />
        protected override void UpdateValue()
        {
            this.UpdateTarget();
        }

        /// <summary>
        ///   Called when the data binding value changed.
        /// </summary>
        /// <param name="newValue">New data value.</param>
        private void OnDataValueChanged(object newValue)
        {
            TData value;
            if (newValue is TData)
            {
                value = (TData)newValue;
            }
            else
            {
                try
                {
                    value = this.Data.GetValue<TData>();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(
                        string.Format(
                            "Couldn't convert new value '{0}' to type '{1}', using default value: {2}",
                            newValue,
                            typeof(TData),
                            e.Message),
                        this);
                    value = default(TData);
                }
            }

            if (Equals(value, this.dataValue))
            {
                return;
            }

            this.dataValue = value;
            this.UpdateTarget();
        }

        private void OnTargetValueChanged(object newValue)
        {
            TTarget value;
            if (newValue is TTarget)
            {
                value = (TTarget)newValue;
            }
            else
            {
                try
                {
                    value = this.Data.GetValue<TTarget>();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(
                        string.Format(
                            "Couldn't convert new value '{0}' to type '{1}', using default value: {2}",
                            newValue,
                            typeof(TTarget),
                            e.Message),
                        this);
                    value = default(TTarget);
                }
            }

            if (Equals(value, this.targetValue))
            {
                return;
            }

            this.targetValue = value;
            this.UpdateTarget();
        }

        private void UpdateTarget()
        {
            if (this.targetValue == null)
            {
                return;
            }

            if (this.UpdateTarget(this.targetValue, this.dataValue))
            {
                this.OnValueChanged(this.targetValue);
            }
        }
    }
}