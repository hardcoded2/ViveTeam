// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Context.cs" company="Slash Games">
//   Copyright (c) Slash Games. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Slash.Unity.DataBind.Core.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Slash.Unity.DataBind.Core.Utils;
    using UnityEngine;

    /// <summary>
    ///     Base class for a data context which contains properties to bind to.
    /// </summary>
    public abstract class Context
    {
        /// <summary>
        ///     Separator in context paths.
        /// </summary>
        public const char PathSeparator = '.';

        /// <summary>
        ///     Path to use to get context itself instead of a data property from the context.
        /// </summary>
        public const string SelfReferencePath = "#";

        /// <summary>
        ///     Root data node.
        /// </summary>
        private readonly DataNode root;

        /// <summary>
        ///     Constructor.
        /// </summary>
        protected Context()
        {
            this.root = new DataNode(this);
        }

        /// <summary>
        ///     Returns the value at the specified path.
        /// </summary>
        /// <param name="path">Path to get value for.</param>
        /// <returns>Current value at specified path.</returns>
        public object GetValue(string path)
        {
            var node = this.root.FindDescendant(path);
            if (node == null)
            {
                throw new ArgumentException("Invalid path '" + path + "' for type " + this.GetType(), "path");
            }
            return node.Value;
        }

        /// <summary>
        ///     Registers a callback at the specified path of the context.
        /// </summary>
        /// <param name="path">Path to register for.</param>
        /// <param name="onValueChanged">Callback to invoke when value at the specified path changed.</param>
        /// <exception cref="ArgumentException">Thrown if path is invalid for this context.</exception>
        /// <returns>Current value at specified path.</returns>
        public object RegisterListener(string path, Action<object> onValueChanged)
        {
            var node = this.root.FindDescendant(path);
            if (node == null)
            {
                throw new ArgumentException("Invalid path '" + path + "' for type " + this.GetType(), "path");
            }

            // Register for value change.
            node.ValueChanged += onValueChanged;

            if (node.CanValueChange && (node.ParentNode == null || node.ParentNode.Value != null) &&
                node.DataProvider == null)
            {
                Debug.LogWarningFormat(
                    "Expected {2}.{3} in context of type '{1}'\n" +
                    "No data provider found for path '{0}', value will never change.",
                    path,
                    this.root.Value.GetType(),
                    this.root.Value.GetType().Name,
                    DataNode.GetDataProviderName(node.Name));
            }

            return node.Value;
        }

        /// <summary>
        ///     Removes the callback from the specified path of the context.
        /// </summary>
        /// <param name="path">Path to remove callback from.</param>
        /// <param name="onValueChanged">Callback to remove.</param>
        /// <exception cref="ArgumentException">Thrown if path is invalid for this context.</exception>
        public void RemoveListener(string path, Action<object> onValueChanged)
        {
            var node = this.root.FindDescendant(path);
            if (node == null)
            {
                throw new ArgumentException("Invalid path '" + path + "' for type " + this.GetType(), "path");
            }

            // Remove from value change.
            node.ValueChanged -= onValueChanged;
        }

        /// <summary>
        ///     Sets the specified value at the specified path.
        /// </summary>
        /// <param name="path">Path to set the data value at.</param>
        /// <exception cref="ArgumentException">Thrown if path is invalid for this context.</exception>
        /// <exception cref="InvalidOperationException">Thrown if data at specified path can't be changed.</exception>
        /// <param name="value">Value to set.</param>
        public void SetValue(string path, object value)
        {
            var node = this.root.FindDescendant(path);
            if (node == null)
            {
                throw new ArgumentException("Invalid path '" + path + "' for type " + this.GetType(), "path");
            }

            node.SetValue(value);
        }

        /// <summary>
        ///     Wraps a data object in the context tree and makes sure that the registered listeners are informed
        ///     when the data value changed.
        /// </summary>
        private sealed class DataNode
        {
            /// <summary>
            ///     Data provider to get informed if value changes.
            /// </summary>
            private IDataProvider dataProvider;

            private DataNode parentNode;

            private object value;

            private DataNode(NodeTypeInfo typeInfo)
            {
                this.Children = new List<DataNode>();
                this.TypeInfo = typeInfo;
            }

            public DataNode(Context context)
            {
                this.Children = new List<DataNode>();
                this.TypeInfo = new ContextNode(context);
                this.Value = context;
            }

            public IDataProvider DataProvider
            {
                get
                {
                    return this.dataProvider;
                }
                private set
                {
                    if (value == this.dataProvider)
                    {
                        return;
                    }

                    if (this.dataProvider != null)
                    {
                        this.dataProvider.ValueChanged -= this.OnDataProviderValueChanged;
                    }

                    this.dataProvider = value;

                    if (this.dataProvider != null)
                    {
                        this.dataProvider.ValueChanged += this.OnDataProviderValueChanged;
                    }
                }
            }

            /// <summary>
            ///     Indicates if the value of this node can change.
            /// </summary>
            public bool CanValueChange
            {
                get
                {
                    return this.TypeInfo != null && this.TypeInfo.CanValueChange;
                }
            }

            /// <summary>
            ///     Name of data node.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            ///     Parent of this node.
            /// </summary>
            public DataNode ParentNode
            {
                get
                {
                    return this.parentNode;
                }
                private set
                {
                    if (value == this.parentNode)
                    {
                        return;
                    }

                    if (this.parentNode != null)
                    {
                        this.parentNode.ValueChanged -= this.OnParentValueChanged;
                    }

                    this.parentNode = value;

                    if (this.parentNode != null)
                    {
                        this.parentNode.ValueChanged += this.OnParentValueChanged;
                    }
                }
            }

            public object Value
            {
                get
                {
                    return this.value;
                }
                private set
                {
                    if (value == this.value)
                    {
                        return;
                    }

                    // Handle specific changed events for collection.
                    var collectionValue = this.value as Collection;
                    if (collectionValue != null)
                    {
                        collectionValue.Cleared -= this.OnCollectionCleared;
                        collectionValue.ItemAdded -= this.OnCollectionItemAdded;
                        collectionValue.ItemRemoved -= this.OnCollectionItemRemoved;
                    }

                    this.value = value;

                    // Handle specific changed events for collection.
                    collectionValue = this.value as Collection;
                    if (collectionValue != null)
                    {
                        collectionValue.Cleared += this.OnCollectionCleared;
                        collectionValue.ItemAdded += this.OnCollectionItemAdded;
                        collectionValue.ItemRemoved += this.OnCollectionItemRemoved;
                    }

                    this.OnValueChanged(this.value);
                }
            }

            private List<DataNode> Children { get; set; }

            /// <summary>
            ///     Cached type information of the data value this node capsules.
            /// </summary>
            private NodeTypeInfo TypeInfo { get; set; }

            public event Action<object> ValueChanged;

            public DataNode FindDescendant(string path)
            {
                var pointPos = path.IndexOf(PathSeparator);
                var nodeName = path;
                string pathRest = null;
                if (pointPos >= 0)
                {
                    nodeName = path.Substring(0, pointPos);
                    pathRest = path.Substring(pointPos + 1);
                }

                if (nodeName == SelfReferencePath)
                {
                    return this;
                }

                // Get children with name.
                var childNode = this.GetChild(nodeName);
                if (childNode == null)
                {
                    return null;
                }

                return string.IsNullOrEmpty(pathRest) ? childNode : childNode.FindDescendant(pathRest);
            }

            public static string GetDataProviderName(string name)
            {
                var settings = DataBindSettings.Instance;

                string dataProviderName;
                switch (settings.DataProviderFormat)
                {
                    case DataBindSettings.DataProviderFormats.FirstLetterLowerCase:
                        dataProviderName = char.ToLowerInvariant(name[0]) + name.Substring(1);
                        break;
                    case DataBindSettings.DataProviderFormats.FirstLetterUpperCase:
                        dataProviderName = name;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (settings.AppendUnderscore)
                {
                    dataProviderName = "_" + dataProviderName;
                }

                return dataProviderName + "Property";
            }

            public void SetValue(object newValue)
            {
                // Update data value.
                this.TypeInfo.SetValue(this.parentNode != null ? this.parentNode.value : null, newValue);

                // Update cached value.
                this.Value = newValue;
            }

            private DataNode CreateChild(string name)
            {
                // Get type of child.
                var typeInfo = this.GetChildTypeInfo(name);
                if (typeInfo == null)
                {
                    // No child with this name.
                    return null;
                }

                var childNode = new DataNode(typeInfo) {Name = name, ParentNode = this};
                childNode.UpdateContent(this.value);
                this.Children.Add(childNode);
                return childNode;
            }

            private DataNode GetChild(string name)
            {
                var childNode = this.Children.FirstOrDefault(child => child.Name == name) ?? this.CreateChild(name);
                return childNode;
            }

            private NodeTypeInfo GetChildTypeInfo(string name)
            {
                // Get item if collection.
                var type = this.TypeInfo.Type;
                if (type.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    // Check if index provided.
                    int itemIndex;
                    if (int.TryParse(name, out itemIndex))
                    {
                        // Get item type.
                        var itemType = type.GetElementType();
                        if (itemType == null)
                        {
                            if (type.IsGenericType && type.GetGenericArguments().Length > 0)
                            {
                                itemType = type.GetGenericArguments()[0];
                            }
                            else
                            {
                                itemType = typeof(object);
                            }
                        }

                        // Return item.
                        return new EnumerableNode {Type = itemType, Index = itemIndex};
                    }
                }

                // Get property.
                var reflectionProperty = ReflectionUtils.GetPublicProperty(type, name);
                if (reflectionProperty != null)
                {
                    return new PropertyNode {Type = reflectionProperty.PropertyType, Property = reflectionProperty};
                }

                // Get field.
                var reflectionField = ReflectionUtils.GetPublicField(type, name);
                if (reflectionField != null)
                {
                    return new FieldNode {Type = reflectionField.FieldType, Field = reflectionField};
                }

                // Get method.
                var reflectionMethod = ReflectionUtils.GetPublicMethod(type, name);
                if (reflectionMethod != null)
                {
                    return new MethodNode {Type = reflectionMethod.ReturnType, Method = reflectionMethod};
                }

                return null;
            }

            /// <summary>
            ///     Returns the data provider of the specified object and type with the specified name.
            /// </summary>
            /// <param name="obj">Concrete object to get data provider for.</param>
            /// <param name="name">Name of data provider to get.</param>
            /// <returns>Data provider for the specified object and name.</returns>
            private static IDataProvider GetDataProvider(object obj, string name)
            {
                if (obj == null || string.IsNullOrEmpty(name))
                {
                    return null;
                }

                // Go up the type hierarchy of the object.
                // NOTE(co): Private fields of base classes are not reflected for derived types.
                var type = obj.GetType();
                while (type != null)
                {
                    var property = GetDataProvider(type, obj, name);
                    if (property != null)
                    {
                        return property;
                    }
                    type = ReflectionUtils.GetBaseType(type);
                }

                return null;
            }

            /// <summary>
            ///     Returns the data provider of the specified object and type with the specified name.
            /// </summary>
            /// <param name="type">Type to search.</param>
            /// <param name="obj">Concrete object to get data provider for.</param>
            /// <param name="name">Name of data to get data provider for.</param>
            /// <returns>Data provider for the specified object and name, using the specified type for reflection.</returns>
            private static IDataProvider GetDataProvider(Type type, object obj, string name)
            {
                var dataProviderName = GetDataProviderName(name);

                // Check for field.
                var dataProviderField = ReflectionUtils.GetPrivateField(type, dataProviderName);
                if (dataProviderField != null)
                {
                    return dataProviderField.GetValue(obj) as IDataProvider;
                }

                // Check for public property.
                var dataProviderProperty = ReflectionUtils.GetPublicProperty(type, dataProviderName);
                if (dataProviderProperty != null)
                {
                    return dataProviderProperty.GetValue(obj, null) as IDataProvider;
                }

                return null;
            }

            private void OnCollectionCleared()
            {
                this.OnValueChanged(this.value);
            }

            private void OnCollectionItemAdded(object item)
            {
                this.OnValueChanged(this.value);
            }

            private void OnCollectionItemRemoved(object item)
            {
                this.OnValueChanged(this.value);
            }

            private void OnDataProviderValueChanged()
            {
                // Update cached value.
                this.Value = this.DataProvider.Value;
            }

            private void OnParentValueChanged(object newParentValue)
            {
                // Update data provider and value.
                this.UpdateContent(newParentValue);
            }

            private void OnValueChanged(object obj)
            {
                var handler = this.ValueChanged;
                if (handler != null)
                {
                    handler(obj);
                }
            }

            private void UpdateContent(object parentValue)
            {
                // Get provider of the node.
                this.DataProvider = GetDataProvider(parentValue, this.Name);

                // Get object of the node.
                this.Value = this.TypeInfo.GetValue(parentValue);
            }

            private class FieldNode : NodeTypeInfo
            {
                public FieldInfo Field { private get; set; }

                /// <inheritdoc />
                public override bool CanValueChange
                {
                    get
                    {
                        return true;
                    }
                }

                public override object GetValue(object obj)
                {
                    if (obj == null)
                    {
                        return null;
                    }

                    // Get field value.
                    if (this.Field != null)
                    {
                        return this.Field.GetValue(obj);
                    }

                    return null;
                }

                public override void SetValue(object obj, object value)
                {
                    if (obj == null)
                    {
                        return;
                    }

                    if (this.Field == null)
                    {
                        return;
                    }

                    // Set field value.
                    this.Field.SetValue(obj, value);
                }
            }

            private class PropertyNode : NodeTypeInfo
            {
                /// <inheritdoc />
                public override bool CanValueChange
                {
                    get
                    {
                        return this.Property != null && this.Property.CanWrite;
                    }
                }

                public PropertyInfo Property { private get; set; }

                public override object GetValue(object obj)
                {
                    if (obj == null)
                    {
                        return null;
                    }

                    // Get property value.
                    if (this.Property != null)
                    {
                        return this.Property.GetValue(obj, null);
                    }

                    return null;
                }

                public override void SetValue(object obj, object value)
                {
                    if (obj == null)
                    {
                        return;
                    }

                    if (this.Property == null)
                    {
                        return;
                    }

                    // Set property value.
                    if (this.Property.CanWrite)
                    {
                        this.Property.SetValue(obj, value, null);
                    }
                    else
                    {
                        throw new InvalidOperationException("Property '" + this.Property.Name + "' is read-only.");
                    }
                }
            }

            private class MethodNode : NodeTypeInfo
            {
                /// <inheritdoc />
                public override bool CanValueChange
                {
                    get
                    {
                        return false;
                    }
                }

                public MethodInfo Method { private get; set; }

                public override object GetValue(object obj)
                {
                    if (obj == null)
                    {
                        return null;
                    }

                    // Get delegate.
                    if (this.Method != null)
                    {
                        var args = new List<Type>(this.Method.GetParameters().Select(p => p.ParameterType));
                        var delegateType = Expression.GetActionType(args.ToArray());
                        return ReflectionUtils.CreateDelegate(delegateType, obj, this.Method);
                    }

                    return null;
                }
            }

            private abstract class NodeTypeInfo
            {
                /// <summary>
                ///     Indicates if this the value of this node can change.
                /// </summary>
                public abstract bool CanValueChange { get; }

                public Type Type { get; set; }

                /// <summary>
                ///     Returns the value using the type info on the specified object.
                /// </summary>
                /// <param name="obj">Object to use the type info on.</param>
                /// <returns>Current value when using type info on specified object.</returns>
                public abstract object GetValue(object obj);

                public virtual void SetValue(object obj, object value)
                {
                    throw new InvalidOperationException("Data node of type '" + this.Type + "' is read-only.");
                }
            }

            private class ContextNode : NodeTypeInfo
            {
                private readonly Context context;

                /// <inheritdoc />
                public ContextNode(Context context)
                {
                    this.context = context;
                    this.Type = context.GetType();
                }

                /// <inheritdoc />
                public override bool CanValueChange
                {
                    get
                    {
                        return true;
                    }
                }

                /// <inheritdoc />
                public override object GetValue(object obj)
                {
                    return this.context;
                }
            }

            private class EnumerableNode : NodeTypeInfo
            {
                /// <inheritdoc />
                public override bool CanValueChange
                {
                    get
                    {
                        return true;
                    }
                }

                public int Index { get; set; }

                public override object GetValue(object obj)
                {
                    // Check if enumerable.
                    var enumerable = obj as IEnumerable;
                    if (enumerable == null)
                    {
                        return null;
                    }

                    var index = 0;
                    foreach (var item in enumerable)
                    {
                        if (index == this.Index)
                        {
                            return item;
                        }
                        ++index;
                    }

                    return null;
                }
            }
        }
    }
}