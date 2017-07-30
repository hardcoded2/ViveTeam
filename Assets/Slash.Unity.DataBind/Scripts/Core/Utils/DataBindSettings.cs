// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataBindSettings.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Slash.Unity.DataBind.Core.Utils
{
    using System.IO;

    using Slash.Unity.DataBind.Core.Data;

    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;

#endif

    /// <summary>
    ///   Settings that can be set for the specific project Data Bind is used in.
    /// </summary>
    public class DataBindSettings : ScriptableObject
    {
        /// <summary>
        ///   Available naming formats for data property fields in <see cref="Context" /> classes.
        /// </summary>
        public enum DataProviderFormats
        {
            /// <summary>
            ///   The data provider name is the same as the data name, but with the first letter being lower case
            ///   and a "Property" postfix.
            /// </summary>
            FirstLetterLowerCase,

            /// <summary>
            ///   The data provider name is the same as the data name,
            ///   but only with the "Property" postfix.
            /// </summary>
            FirstLetterUpperCase
        }

        private const string SettingsPath = "Data Bind Settings.asset";

        private static DataBindSettings instance;

        /// <summary>
        ///   Singleton instance of settings.
        ///   Will create the settings file when executed in editor.
        /// </summary>
        public static DataBindSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<DataBindSettings>(SettingsPath);
#if UNITY_EDITOR
                    const string SettingsAssetPath = "Assets/Slash.Unity.DataBind/Resources/" + SettingsPath;
                    instance = AssetDatabase.LoadAssetAtPath<DataBindSettings>(SettingsAssetPath);
#endif
                    if (instance == null)
                    {
                        instance = CreateInstance<DataBindSettings>();
#if UNITY_EDITOR
                        var settingsDirectoryName = Path.GetDirectoryName(SettingsAssetPath);
                        if (!string.IsNullOrEmpty(settingsDirectoryName))
                        {
                            Directory.CreateDirectory(settingsDirectoryName);
                        }
                        AssetDatabase.CreateAsset(instance, SettingsAssetPath);
                        AssetDatabase.SaveAssets();
#endif
                    }
                }
                return instance;
            }
        }

        #region Data Provider Format

        /// <summary>
        ///   Naming format to use for the data property fields in a <see cref="Context" /> class.
        /// </summary>
        [Header("Data Provider Format")]
        [Tooltip("Indicates how the data provider field/property name is created from the data name")]
        public DataProviderFormats DataProviderFormat;

        /// <summary>
        ///   Indicates if an underscore should be appended to form the data provider field/property name.
        /// </summary>
        [Tooltip("Indicates if an underscore should be appended to form the data provider field/property name")]
        public bool AppendUnderscore;

        #endregion
    }
}