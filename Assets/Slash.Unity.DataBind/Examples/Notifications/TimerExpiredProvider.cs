﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerExpiredProvider.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Slash.Unity.DataBind.Examples.Notifications
{
    using Slash.Unity.DataBind.Core.Presentation;

    using UnityEngine;

    public class TimerExpiredProvider : DataProvider
    {
        public DataBinding Duration;

        public DataBinding Running;

        private float remainingDuration = float.MaxValue;

        private bool timerExpired;

        private bool timerRunning;

        /// <inheritdoc />
        public override object Value
        {
            get
            {
                return this.timerExpired;
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            base.Disable();

            this.Running.ValueChanged -= this.OnRunningChanged;
        }

        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();

            this.Running.ValueChanged += this.OnRunningChanged;
            if (this.Running.GetValue<bool>())
            {
                this.StartTimer();
            }
        }

        /// <inheritdoc />
        protected override void UpdateValue()
        {
            this.OnValueChanged(this.Value);
        }

        private void Awake()
        {
            this.AddBinding(this.Duration);
            this.AddBinding(this.Running);
        }

        private void OnDestroy()
        {
            this.RemoveBinding(this.Duration);
            this.RemoveBinding(this.Running);
        }

        private void OnRunningChanged(object newValue)
        {
            var newIsRunning = (bool)newValue;
            if (newIsRunning != this.timerRunning)
            {
                if (newIsRunning)
                {
                    this.StartTimer();
                }
                else
                {
                    this.StopTimer();
                }
            }
        }

        private void OnTimerExpired()
        {
            this.timerRunning = false;
            this.timerExpired = true;
            this.OnValueChanged(true);
        }

        private void StartTimer()
        {
            this.remainingDuration = this.Duration.GetValue<float>();
            this.timerRunning = true;
            this.timerExpired = false;

            this.OnValueChanged(false);
        }

        private void StopTimer()
        {
            this.timerRunning = false;
        }

        private void Update()
        {
            if (!this.timerRunning)
            {
                return;
            }

            this.remainingDuration -= Time.deltaTime;
            if (this.remainingDuration <= 0)
            {
                this.OnTimerExpired();
            }
        }
    }
}